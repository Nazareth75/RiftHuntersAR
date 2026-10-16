using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using RiftHunters.Game;
using RiftHuntersAR.AR;
using RiftHuntersAR.Combat;
using RiftHuntersAR.Game;

namespace RiftHuntersAR.Integration
{
    public sealed class ARPrototypeController : MonoBehaviour
    {
        public const string TextoEscanear = "Mueve lentamente el teléfono para reconocer el entorno.";
        public const string TextoColocar = "Toca para colocar el portal.";
        public const string TextoEditorColocar = "ENTER O CLIC PARA COLOCAR EL PORTAL";

        [SerializeField] private ARPlaneManager planes;
        [SerializeField] private ARRaycastManager raycasts;
        [SerializeField] private ARAnchorManager anchors;
        [SerializeField] private PortalSpawnPoint portalPrefab;
        [SerializeField] private PlasmaWeapon weapon;
        [SerializeField] private EnlaceCombateAR enlaceCombate;
        [SerializeField] private ScoreManager score;
        [SerializeField] private Text statusText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button shootButton;
        private readonly List<RaycastResult> uiHits = new();
        private bool placed;
        private PlayerHealth player;
        private PortalSpawnPoint activePortal;
        private ARAnchor activeAnchor;
        private ColocacionPortalAR colocacion;
        private float seguimientoEstable;
        private bool SeguimientoFisico => ARSession.state == ARSessionState.SessionTracking
            && (!placed || (activeAnchor != null && activeAnchor.trackingState == TrackingState.Tracking));
        private float tiempoSinSeguimiento;
        private const float RetardoMensajePerdida = 0.5f;

        private Text playerText, ammoText, hitMarker;
        private Button retryButton, reloadButton;
        private float feedbackUntil;
        private float playerFlashUntil;
        private string state = "SCANNING";
        private int wave;
        private int armaIndice;
        private bool aplicacionPausada;
        private bool pruebaEscritorio;
        private bool pausaPrueba;
        private AdaptadorGenerador adaptador;
        private bool ganchosListos;

        public bool SeguimientoDisponible => !aplicacionPausada && !pausaPrueba
            && (pruebaEscritorio
                || (SeguimientoFisico && seguimientoEstable >= .2f));
        public Camera CamaraJuego => weapon.AimCamera;
        public PortalSpawnPoint PortalActivo => activePortal;
        public PlayerHealth Player => player;
        public string CurrentState => state;
        public int CurrentWave => wave;
        public bool EsJefe => GameManager.Instance != null && GameManager.Instance.EsJefe;
        public int AliveEnemies => enlaceCombate != null ? enlaceCombate.EnemigosVivos : 0;
        public int EnemigosDerrotados => enlaceCombate != null ? enlaceCombate.Derrotados : 0;
        public float TiempoPartida => GameManager.Instance != null ? GameManager.Instance.MatchSeconds : 0f;
        public string TextoCuenta => GameManager.Instance != null ? GameManager.Instance.TextoCuenta : "";
        public string AnuncioOleada => GameManager.Instance != null ? GameManager.Instance.Anuncio : "";
        public bool AnunciandoJefe => GameManager.Instance != null && GameManager.Instance.AnunciandoJefe;
        public float FraccionJefe => adaptador != null ? adaptador.FraccionJefe : -1f;
        public RiftHunters.Combate.NucleosJefe NucleosJefe => adaptador != null ? adaptador.Nucleos : null;
        public RiftHunters.Combate.GestorArmas Armas => enlaceCombate != null ? enlaceCombate.Armas : null;
        public RiftHunters.Combate.GeneradorEnemigos Generador => enlaceCombate != null ? enlaceCombate.Generador : null;
        public PlasmaWeapon Arma => weapon;
        public static bool EsComputadora => Application.isEditor || !Application.isMobilePlatform;
        public bool ModoComputadora => pruebaEscritorio;
        public bool EnPausa => pausaPrueba;

        public void HabilitarPruebaEscritorio()
        {
            if (!EsComputadora || pruebaEscritorio) return;
            pruebaEscritorio = true;
            weapon.HabilitarPruebaEscritorio();
            foreach (Behaviour componente in weapon.AimCamera.GetComponents<Behaviour>())
                if (componente.GetType().Name == "TrackedPoseDriver") componente.enabled = false;
            if (FindFirstObjectByType<PruebaIntegradaEscritorio>() == null && GetComponent<ControlesComputadora>() == null)
                gameObject.AddComponent<ControlesComputadora>().Unir(this);
        }

        static bool SeguimientoARReal()
        {
            return ARSession.state == ARSessionState.SessionTracking;
        }

        bool PulsoColocarSimulado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame))
                return true;
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return false;
            return !OverUI(Mouse.current.position.ReadValue());
        }

        void ColocarPortalSimulado()
        {
            if (!pruebaEscritorio || placed || portalPrefab == null) return;
            Camera camara = weapon != null ? weapon.AimCamera : Camera.main;
            Vector3 posicion = new Vector3(0f, 0f, 2.2f);
            Quaternion rotacion = Quaternion.identity;
            if (camara != null)
            {
                Vector3 adelante = camara.transform.forward;
                adelante.y = 0f;
                if (adelante.sqrMagnitude < 0.001f) adelante = Vector3.forward;
                adelante.Normalize();
                posicion = camara.transform.position + adelante * 2.2f;
                rotacion = Quaternion.LookRotation(-adelante, Vector3.up);
            }
            var origen = new GameObject("Origen del portal de prueba");
            origen.transform.SetPositionAndRotation(posicion, rotacion);
            PortalSpawnPoint portal = Instantiate(portalPrefab, posicion, rotacion, origen.transform);
            ComenzarPartida(portal);
        }

        public void IniciarPruebaEscritorio(Vector3 posicion)
        {
            if (!pruebaEscritorio || placed || portalPrefab == null) return;
            var origen = new GameObject("Origen del portal de prueba");
            origen.transform.position = posicion;
            PortalSpawnPoint portal = Instantiate(portalPrefab, posicion, Quaternion.identity, origen.transform);
            ComenzarPartida(portal);
        }

        public void CambiarPausaPrueba()
        {
            if (!pruebaEscritorio || !placed) return;
            if (state == "GAME_OVER" || state == "VICTORY") return;
            pausaPrueba = !pausaPrueba;
        }

        public void DispararPrueba()
        {
            if (pruebaEscritorio && SeguimientoDisponible && state == "WAVE") weapon.Shoot();
        }

        public void RecargarPrueba()
        {
            if (pruebaEscritorio && SeguimientoDisponible) Reload();
        }

        public void ReintentarPrueba()
        {
            if (pruebaEscritorio && (state == "GAME_OVER" || state == "VICTORY")) ReintentarPartida();
        }

        public void CambiarArma() => CambiarArma(1);

        public void CambiarArma(int paso)
        {
            if (enlaceCombate == null || enlaceCombate.Armas == null || state != "WAVE" || !SeguimientoDisponible) return;
            armaIndice = ((armaIndice + paso) % 3 + 3) % 3;
            enlaceCombate.SeleccionarArma(armaIndice);
        }

        public void SeleccionarArma(int indice)
        {
            if (enlaceCombate == null || enlaceCombate.Armas == null || state != "WAVE" || !SeguimientoDisponible) return;
            armaIndice = Mathf.Clamp(indice, 0, 2);
            enlaceCombate.SeleccionarArma(armaIndice);
        }

        public void ReintentarPartida()
        {
            pausaPrueba = false;
            if (retryButton != null) retryButton.gameObject.SetActive(false);
            if (GameManager.Instance != null) GameManager.Instance.Reintentar();
        }

        private void Awake()
        {
            player = gameObject.AddComponent<PlayerHealth>();
            player.Changed += PlayerChanged;
            PrepararCombate();
            colocacion = gameObject.AddComponent<ColocacionPortalAR>();
            colocacion.Configurar(planes, raycasts, weapon.AimCamera);
            var parent = statusText.transform.parent;
            playerText = MakeLabel(parent, "Player", new Vector2(0, -280), new Vector2(.5f, 1));
            ammoText = MakeLabel(parent, "Ammo", new Vector2(0, 450), new Vector2(.5f, 0));
            hitMarker = MakeLabel(parent, "HitMarker", Vector2.zero, new Vector2(.5f, .5f));
            retryButton = MakeButton(parent, "RETRY", new Vector2(0, 650), ReintentarPartida);
            reloadButton = MakeButton(parent, "RELOAD", new Vector2(0, 50), Reload);
            retryButton.gameObject.SetActive(false);
            PlayerChanged();
        }

        private void PrepararCombate()
        {
            if (enlaceCombate == null) enlaceCombate = GetComponent<EnlaceCombateAR>();
            if (enlaceCombate == null) enlaceCombate = gameObject.AddComponent<EnlaceCombateAR>();
            CatalogoPartida catalogo = Resources.Load<CatalogoPartida>("CatalogoPartida");
            enlaceCombate.CompletarSiFalta(catalogo);
            if (!enlaceCombate.Inicializar(weapon.AimCamera, player, score))
            {
                enabled = false;
                Debug.LogError("Partida: falta el sistema de armas o los prefabs normal, rápido y tanque.", this);
                return;
            }
            weapon.ConectarSistema(enlaceCombate.Armas);
            adaptador = gameObject.AddComponent<AdaptadorGenerador>();
            adaptador.Unir(enlaceCombate.Generador, catalogo);
            gameObject.AddComponent<SonidosPartida>().Unir(this);
            PuentePuntuacion puntos = gameObject.AddComponent<PuentePuntuacion>();
            puntos.Unir(score);
            PuenteVida vida = gameObject.AddComponent<PuenteVida>();
            vida.Unir(player);
            gameObject.AddComponent<GameManager>();
            gameObject.AddComponent<RiftHuntersAR.Recycling.RecyclingGameplay>();
            if (!ganchosListos)
            {
                GameplayHooks.Pausar += AlPedirPausa;
                GameplayHooks.DetenerCombate += AlDetenerCombate;
                GameplayHooks.ReiniciarArmas += AlReiniciarArmas;
                ganchosListos = true;
            }
        }

        private void OnDestroy()
        {
            if (!ganchosListos) return;
            GameplayHooks.Pausar -= AlPedirPausa;
            GameplayHooks.DetenerCombate -= AlDetenerCombate;
            GameplayHooks.ReiniciarArmas -= AlReiniciarArmas;
        }

        private void AlPedirPausa(bool pausa)
        {
            if (enlaceCombate != null) enlaceCombate.Pausar(pausa);
        }

        private void AlDetenerCombate()
        {
            weapon.Ready = false;
            if (enlaceCombate != null) enlaceCombate.Detener();
        }

        private void AlReiniciarArmas()
        {
            armaIndice = 0;
            if (enlaceCombate != null) enlaceCombate.Reiniciar();
        }

        private void ComenzarPartida(PortalSpawnPoint portal)
        {
            placed = true;
            activePortal = portal;
            state = "PORTAL_PLACED";
            if (adaptador != null) adaptador.UsarPortal(portal, weapon.AimCamera.transform);
            GameplayHooks.IniciarPartida();
            CopiarEstado();
        }

        private Text MakeLabel(Transform parent, string name, Vector2 position, Vector2 anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(1000, 80);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private Button MakeButton(Transform parent, string name, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(500, 90);
            go.GetComponent<Image>().color = new Color(.1f, .15f, .2f, .95f);
            var label = MakeLabel(go.transform, name + " Label", Vector2.zero, new Vector2(.5f, .5f));
            label.text = name;
            label.rectTransform.sizeDelta = new Vector2(500, 90);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(action);
            return button;
        }

        private void OnApplicationPause(bool pausa)
        {
            aplicacionPausada = pausa;
            seguimientoEstable = 0f;
            if (pausa && enlaceCombate != null) enlaceCombate.Pausar(true);
        }

        private float vidaPresentada = 100f;
        private void PlayerChanged()
        {
            int hearts = Mathf.CeilToInt(player.CurrentHealth / (100f / 3f));
            playerText.text = $"LIFE {hearts}/3 | {player.CurrentHealth:0} HP";
            bool recibioDano = player.CurrentHealth < vidaPresentada;
            vidaPresentada = player.CurrentHealth;
            if (recibioDano)
            {
                playerText.color = Color.red;
                playerFlashUntil = Time.unscaledTime + .35f;
                UpdateFeedback("PLAYER HIT");
            }
        }

        private void Reload()
        {
            if (state == "WAVE") weapon.Reload();
        }

        private void OnEnable()
        {
            score.Changed += UpdateScore;
            weapon.Feedback += UpdateFeedback;
            shootButton.onClick.RemoveListener(weapon.Shoot);
            shootButton.onClick.AddListener(weapon.Shoot);
        }

        private void OnDisable()
        {
            score.Changed -= UpdateScore;
            weapon.Feedback -= UpdateFeedback;
            shootButton.onClick.RemoveListener(weapon.Shoot);
        }

        private void Start()
        {
            Application.targetFrameRate = 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            UpdateScore(score.Score);
        }

        private void UpdateScore(int value) { scoreText.text = $"SCORE: {value}"; }

        private void UpdateFeedback(string value)
        {
            feedbackText.text = value;
            feedbackUntil = Time.unscaledTime + 1.5f;
            if (hitMarker == null) return;
            bool marca = value.StartsWith("HIT") || value.StartsWith("Impacto:") || value.StartsWith("Enemigo eliminado")
                || value.StartsWith("Cadena electrica:") || value.StartsWith("Explosion:");
            hitMarker.text = marca ? "X" : "";
            hitMarker.color = Color.cyan;
        }

        private bool TouchPosition(out Vector2 position)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                position = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }
#if UNITY_EDITOR
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }
#endif
            position = default;
            return false;
        }

        private bool OverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = position };
            uiHits.Clear();
            EventSystem.current.RaycastAll(data, uiHits);
            return uiHits.Count > 0;
        }

        private void Update()
        {
            seguimientoEstable = SeguimientoFisico && !aplicacionPausada
                ? seguimientoEstable + Time.unscaledDeltaTime : 0f;
            RefrescarPresentacion();
        }

        private void LateUpdate()
        {
            CopiarEstado();
            if (enlaceCombate != null)
                enlaceCombate.Pausar(!SeguimientoDisponible || state != "WAVE");
        }

        private void RefrescarPresentacion()
        {
            CopiarEstado();
            if (Time.unscaledTime > playerFlashUntil && playerText != null) playerText.color = Color.white;
            if (ammoText != null)
                ammoText.text = $"{weapon.NombreArma.ToUpperInvariant()} {weapon.CurrentAmmo} / {weapon.MagazineSize}";
            if (state != "COUNTDOWN" && Time.unscaledTime > feedbackUntil)
            {
                hitMarker.text = "";
                feedbackText.text = "";
            }
            if (state == "COUNTDOWN" && !string.IsNullOrEmpty(TextoCuenta))
                feedbackText.text = TextoCuenta;

            if (EsComputadora && !SeguimientoARReal())
                HabilitarPruebaEscritorio();
            bool tracking = SeguimientoDisponible;
            var armaActual = enlaceCombate != null && enlaceCombate.Armas != null ? enlaceCombate.Armas.Actual : null;
            if (reloadButton != null)
                reloadButton.interactable = tracking && state == "WAVE" && armaActual != null
                    && !armaActual.EstaRecargando && armaActual.Municion < armaActual.CapacidadCargador;
            bool puedeDisparar = tracking && state == "WAVE" && weapon.Ready && armaActual != null
                && !armaActual.EstaRecargando && weapon.CurrentAmmo > 0;
            shootButton.interactable = puedeDisparar;

            if (!tracking)
            {
                tiempoSinSeguimiento += Time.unscaledDeltaTime;
                colocacion.Ocultar();
                if (pruebaEscritorio) { statusText.text = "PAUSA: P O ESC PARA SEGUIR"; return; }
                if (!placed || tiempoSinSeguimiento >= RetardoMensajePerdida)
                    statusText.text = MensajeSinSeguimiento();
                return;
            }
            tiempoSinSeguimiento = 0f;
            if (placed)
            {
                if (state == "COUNTDOWN") statusText.text = TextoCuenta;
                else if (state == "WAVE" && !string.IsNullOrEmpty(AnuncioOleada)) statusText.text = AnuncioOleada;
                else if (state == "WAVE") statusText.text = EsJefe
                    ? (NucleosJefe != null ? $"JEFE: {NucleosJefe.Pista}" : "JEFE")
                    : $"OLEADA {wave}/12";
                else statusText.text = state == "VICTORY" ? "VICTORIA" : state == "GAME_OVER" ? "GAME OVER" : state;
                if (state == "VICTORY" || state == "GAME_OVER") retryButton.gameObject.SetActive(true);
                return;
            }

            if (!placed && pruebaEscritorio && !SeguimientoARReal())
            {
                statusText.text = TextoEditorColocar;
                if (PulsoColocarSimulado())
                    ColocarPortalSimulado();
                return;
            }

            colocacion.Actualizar(tracking);
            statusText.text = colocacion.Mensaje;
            if (!TouchPosition(out Vector2 position) || OverUI(position)) return;
            if (!colocacion.PuedeColocar) return;
            ARPlane selected = colocacion.Plano;
            Pose pose = colocacion.Pose;
            ARAnchor anchor = anchors.AttachAnchor(selected, pose);
            if (anchor == null) { UpdateFeedback("No se pudo anclar: vuelve a tocar."); return; }
            activeAnchor = anchor;
            anchor.gameObject.AddComponent<SueloCombateAR>().Configurar(selected);
            // Vertical y mirando al jugador; no heredar la orientación arbitraria del plano.
            Vector3 frente = weapon.AimCamera.transform.position - pose.position;
            frente.y = 0f;
            Quaternion orientacion = frente.sqrMagnitude > .001f ? Quaternion.LookRotation(frente) : Quaternion.identity;
            PortalSpawnPoint portal = Instantiate(portalPrefab, pose.position, orientacion, anchor.transform);
            colocacion.Ocultar();
            foreach (ARPlane plane in planes.trackables)
            {
                var renderer = plane.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            planes.requestedDetectionMode = PlaneDetectionMode.None;
            ComenzarPartida(portal);
        }
        private string MensajeSinSeguimiento()
        {
            if (ARSession.state == ARSessionState.Unsupported) return "AR NOT SUPPORTED";
            if (activeAnchor != null && activeAnchor.trackingState != TrackingState.Tracking)
                return "PORTAL PERDIDO: MIRA DONDE LO COLOCASTE";
            switch (ARSession.notTrackingReason)
            {
                case NotTrackingReason.ExcessiveMotion: return "TRACKING PERDIDO: MUEVE EL TELEFONO DESPACIO";
                case NotTrackingReason.InsufficientLight: return "POCA LUZ: ENCIENDE UNA LUZ";
                case NotTrackingReason.InsufficientFeatures: return "APUNTA A UNA SUPERFICIE CON DETALLES";
                case NotTrackingReason.Initializing:
                case NotTrackingReason.Relocalizing: return "RECUPERANDO TRACKING: MIRA TU ENTORNO";
                default: return placed ? "Estabilizando seguimiento: mira el entorno del portal." : TextoEscanear;
            }
        }
        private void CopiarEstado()
        {
            GameManager partida = GameManager.Instance;
            if (partida == null || !placed) return;
            if (partida.RecoveryInProgress)
            {
                state = "RECOVERY";
                weapon.Ready = false;
            }
            else if (partida.State == GameState.Countdown)
            {
                state = "COUNTDOWN";
                wave = 0;
                weapon.Ready = false;
            }
            else if (partida.EsJefe)
            {
                state = "WAVE";
                wave = partida.NumeroOleada;
                weapon.Ready = true;
            }
            else if (partida.IsRunning && partida.NumeroOleada > 0)
            {
                state = "WAVE";
                wave = partida.NumeroOleada;
                weapon.Ready = true;
            }
            else if (partida.State == GameState.Victory)
            {
                state = "VICTORY";
                weapon.Ready = false;
            }
            else if (partida.State == GameState.GameOver)
            {
                state = "GAME_OVER";
                weapon.Ready = false;
            }
        }
    }
}
