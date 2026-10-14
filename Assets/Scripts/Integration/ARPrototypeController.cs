using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using RiftHuntersAR.AR;
using RiftHuntersAR.Combat;
using RiftHuntersAR.Enemies;
using RiftHuntersAR.Game;

namespace RiftHuntersAR.Integration
{
    public sealed class ARPrototypeController : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager planes;
        [SerializeField] private ARRaycastManager raycasts;
        [SerializeField] private ARAnchorManager anchors;
        [SerializeField] private PortalSpawnPoint portalPrefab;
        [SerializeField] private EnemyHealth enemyPrefab;
        [SerializeField] private PlasmaWeapon weapon;
        [SerializeField] private EnlaceCombateAR enlaceCombate;
        [SerializeField] private ScoreManager score;
        [SerializeField] private Text statusText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button shootButton;
        private readonly List<ARRaycastHit> hits = new();
        private readonly List<RaycastResult> uiHits = new();
        private bool placed;
        private PlayerHealth player;
        private EnemySpawner spawner;
        private PortalSpawnPoint activePortal;
        private Coroutine loop;
        private Text playerText, ammoText, hitMarker;
        private Button retryButton, reloadButton;
        private float feedbackUntil;
        private float playerFlashUntil;
        private string state = "SCANNING";
        private int wave;

        // Presentation reads the existing state; these properties do not change gameplay.
        public PlayerHealth Player => player;
        public string CurrentState => state;
        public int CurrentWave => wave;
        public int AliveEnemies => EnemigosRestantes;

        private Button[] botonesArmas;
        private bool aplicacionPausada;
        private bool pruebaEscritorio;
        private bool pausaPrueba;
        public bool SeguimientoDisponible => !aplicacionPausada && !pausaPrueba
            && (pruebaEscritorio || ARSession.state == ARSessionState.SessionTracking);
        public Camera CamaraJuego => weapon.AimCamera;
        public string EstadoPartida => state;
        public int OleadaActual => wave;
        public int PuntosActuales => score.Score;
        public float VidaJugador => player != null ? player.CurrentHealth : 100;
        public int CantidadEnemigos => EnemigosRestantes;

        public void HabilitarPruebaEscritorio()
        {
#if UNITY_EDITOR
            pruebaEscritorio = true;
            weapon.HabilitarPruebaEscritorio();
#endif
        }

        public void IniciarPruebaEscritorio(Vector3 posicion)
        {
#if UNITY_EDITOR
            if (!pruebaEscritorio || placed || portalPrefab == null) return;
            var origen = new GameObject("Origen del portal de prueba");
            origen.transform.position = posicion;
            PortalSpawnPoint portal = Instantiate(portalPrefab, posicion, Quaternion.identity, origen.transform);
            ComenzarPartida(portal);
#endif
        }

        public void CambiarPausaPrueba()
        {
#if UNITY_EDITOR
            if (!pruebaEscritorio) return;
            pausaPrueba = !pausaPrueba;
            if (enlaceCombate != null) enlaceCombate.Pausar(!SeguimientoDisponible || state != "WAVE");
#endif
        }

        public void DispararPrueba()
        {
#if UNITY_EDITOR
            if (pruebaEscritorio && SeguimientoDisponible && state == "WAVE") weapon.Shoot();
#endif
        }

        public void RecargarPrueba()
        {
#if UNITY_EDITOR
            if (pruebaEscritorio && SeguimientoDisponible) Reload();
#endif
        }

        public void ReintentarPrueba()
        {
#if UNITY_EDITOR
            if (pruebaEscritorio && (state == "GAME_OVER" || state == "VICTORY")) Retry();
#endif
        }

        private void ComenzarPartida(PortalSpawnPoint portal)
        {
            placed = true;
            activePortal = portal;
            state = "PORTAL_PLACED";
            loop = StartCoroutine(Spawn(portal));
        }
        private int EnemigosRestantes => enlaceCombate != null ? enlaceCombate.EnemigosVivos : spawner.AliveCount;

        private void Awake()
        {
            player = gameObject.AddComponent<PlayerHealth>();
            spawner = gameObject.AddComponent<EnemySpawner>();
            player.Died += GameOver;
            player.Changed += PlayerChanged;
            spawner.EnemyKilled += score.AddEnemyScore;
            if (enlaceCombate != null)
            {
                if (!enlaceCombate.Inicializar(weapon.AimCamera, player, score))
                {
                    enabled = false;
                    return;
                }
                weapon.ConectarSistema(enlaceCombate.Armas);
            }
            var parent = statusText.transform.parent;
            playerText = MakeLabel(parent, "Player", new Vector2(0,-280), new Vector2(.5f,1));
            ammoText = MakeLabel(parent, "Ammo", new Vector2(0,450), new Vector2(.5f,0));
            hitMarker = MakeLabel(parent, "HitMarker", Vector2.zero, new Vector2(.5f,.5f));
            retryButton = MakeButton(parent, "RETRY", new Vector2(0,650), Retry);
            reloadButton = MakeButton(parent, "RELOAD", new Vector2(0,50), Reload);
            retryButton.gameObject.SetActive(false);
            if (enlaceCombate != null)
            {
                CrearBotonesArmas(parent);
                var rectDisparo = shootButton.GetComponent<RectTransform>();
                rectDisparo.anchoredPosition = new Vector2(0, 290);
                rectDisparo.sizeDelta = new Vector2(600, 100);
                ammoText.rectTransform.anchoredPosition = new Vector2(0, 540);
                feedbackText.rectTransform.anchoredPosition = new Vector2(0, 430);
            }
            PlayerChanged();
        }
        private Text MakeLabel(Transform parent, string name, Vector2 position, Vector2 anchor)
        {
            var go = new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=anchor; rect.anchoredPosition=position; rect.sizeDelta=new Vector2(1000,80);
            var text=go.GetComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=36; text.color=Color.white; text.alignment=TextAnchor.MiddleCenter; text.raycastTarget=false;
            return text;
        }
        private Button MakeButton(Transform parent,string name,Vector2 position,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=new Vector2(.5f,0); rect.anchoredPosition=position; rect.sizeDelta=new Vector2(500,90);
            go.GetComponent<Image>().color=new Color(.1f,.15f,.2f,.95f);
            var label=MakeLabel(go.transform,name+" Label",Vector2.zero,new Vector2(.5f,.5f)); label.text=name; label.rectTransform.sizeDelta=new Vector2(500,90);
            var button=go.GetComponent<Button>(); button.onClick.AddListener(action); return button;
        }
        private void CrearBotonesArmas(Transform contenedor)
        {
            string[] nombres = { "PLASMA", "ELECTRICA", "EXPLOSIVA" };
            botonesArmas = new Button[3];
            for (int i = 0; i < nombres.Length; i++)
            {
                int indice = i;
                Button boton = MakeButton(contenedor, nombres[i],
                    new Vector2((i - 1) * 330, 160), () => enlaceCombate.SeleccionarArma(indice));
                boton.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 75);
                boton.GetComponentInChildren<Text>().rectTransform.sizeDelta = new Vector2(300, 75);
                botonesArmas[i] = boton;
            }
        }

        private void OnApplicationPause(bool pausa)
        {
            aplicacionPausada = pausa;
            if (enlaceCombate != null && pausa) enlaceCombate.Pausar(true);
        }

        private void PlayerChanged()
        {
            int hearts=Mathf.CeilToInt(player.CurrentHealth/(100f/3));
            playerText.text=$"LIFE {hearts}/3 | {player.CurrentHealth:0} HP";
            if(player.CurrentHealth<100) { playerText.color=Color.red; playerFlashUntil=Time.unscaledTime+.35f; UpdateFeedback("PLAYER HIT"); }
        }
        private void Reload() { if(state=="WAVE") weapon.Reload(); }
        private void GameOver()
        {
            state="GAME_OVER"; weapon.Ready=false; spawner.StopMovement();
            if (enlaceCombate != null) enlaceCombate.Detener();
            if(loop!=null) StopCoroutine(loop);
            retryButton.gameObject.SetActive(true);
            UpdateFeedback("GAME OVER");
        }
        private void Retry()
        {
            if(loop!=null) StopCoroutine(loop);
            pausaPrueba = false;
            spawner.Clear();
            if (enlaceCombate != null) enlaceCombate.Reiniciar();
            player.Retry(); score.ResetScore(); weapon.RestaurarMunicion();
            retryButton.gameObject.SetActive(false);
            loop=StartCoroutine(Spawn(activePortal));
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
        private void UpdateFeedback(string value) { feedbackText.text = value; feedbackUntil=Time.unscaledTime+1.5f; if(hitMarker!=null) { hitMarker.text=(value.StartsWith("HIT") || value.StartsWith("Impacto:") || value.StartsWith("Enemigo eliminado") || value.StartsWith("Cadena electrica:") || value.StartsWith("Explosion:")) ? "X" : ""; hitMarker.color=Color.cyan; } }
        private bool TouchPosition(out Vector2 position)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            { position = Touchscreen.current.primaryTouch.position.ReadValue(); return true; }
#if UNITY_EDITOR
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            { position = Mouse.current.position.ReadValue(); return true; }
#endif
            position = default; return false;
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
            if(Time.unscaledTime>playerFlashUntil) playerText.color=Color.white;
            ammoText.text=$"{weapon.NombreArma.ToUpperInvariant()} {weapon.CurrentAmmo} / {weapon.MagazineSize}";
            if(Time.unscaledTime>feedbackUntil) { hitMarker.text=""; feedbackText.text=""; }
            bool tracking = SeguimientoDisponible;
            if (enlaceCombate != null)
            {
                enlaceCombate.Pausar(!tracking || state != "WAVE");
                if (botonesArmas != null)
                    foreach (Button boton in botonesArmas)
                        boton.interactable = tracking && state == "WAVE" && !enlaceCombate.Armas.Actual.EstaRecargando;
                reloadButton.interactable = tracking && state == "WAVE"
                    && !enlaceCombate.Armas.Actual.EstaRecargando
                    && enlaceCombate.Armas.Actual.Municion < enlaceCombate.Armas.Actual.CapacidadCargador;
            }
            shootButton.interactable = tracking && weapon.Ready
                && (enlaceCombate == null || (!enlaceCombate.Armas.Actual.EstaRecargando && weapon.CurrentAmmo > 0));
            if (!tracking)
            {
                if (pruebaEscritorio) { statusText.text = "PAUSA DE PRUEBA: P PARA REANUDAR"; return; }
                statusText.text = ARSession.state == ARSessionState.Unsupported ? "AR NOT SUPPORTED" : "SCAN ENVIRONMENT";
                return;
            }
            if (placed)
            {
                if (planes != null) foreach (ARPlane plane in planes.trackables) { var r = plane.GetComponent<MeshRenderer>(); if (r != null) r.enabled = false; }
                statusText.text = state == "WAVE" ? $"WAVE {wave} | ENEMIES {EnemigosRestantes}" : state;
                return;
            }
            if (pruebaEscritorio)
            {
                statusText.text = "PRUEBA DE ESCRITORIO: ENTER PARA COLOCAR PORTAL";
                return;
            }
            bool hasPlane = false;
            foreach (ARPlane plane in planes.trackables)
                if (plane.trackingState == TrackingState.Tracking) { hasPlane = true; break; }
            statusText.text = hasPlane ? "TAP TO PLACE PORTAL" : "SCAN ENVIRONMENT";
            if (!TouchPosition(out Vector2 position) || OverUI(position)) return;
            if (!raycasts.Raycast(position, hits, TrackableType.PlaneWithinPolygon)) return;
            ARPlane selected = planes.GetPlane(hits[0].trackableId);
            if (selected == null || selected.trackingState != TrackingState.Tracking) return;
            Pose pose = hits[0].pose;
            ARAnchor anchor = anchors.AttachAnchor(selected, pose);
            if (anchor == null) { UpdateFeedback("PLACEMENT FAILED: TAP AGAIN"); return; }
            PortalSpawnPoint portal = Instantiate(portalPrefab, pose.position, pose.rotation, anchor.transform);
            placed = true;
            foreach (ARPlane plane in planes.trackables)
            {
                var renderer = plane.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            // Keep tracking active; the anchor owns the placed content during plane refinements.

            ComenzarPartida(portal);
        }
        private IEnumerator TrackedWait(float seconds)
        {
            while(seconds>0) { if(SeguimientoDisponible) seconds-=Time.deltaTime; yield return null; }
        }
        private IEnumerator Spawn(PortalSpawnPoint portal)
        {
            weapon.Ready=false;
            state="COUNTDOWN";
            for(int n=3;n>0;n--) { UpdateFeedback($"START IN {n}"); yield return TrackedWait(1); }
            int[] counts={3,5,7};
            for(wave=1;wave<=3;wave++)
            {
                state="WAVE"; weapon.Ready=true;
                for(int i=0;i<counts[wave-1];i++)
                {
                    while(!SeguimientoDisponible) yield return null;
                    if(player.IsDead || portal==null) yield break;
                    if (enlaceCombate != null)
                    {
                        // La coroutine puede reanudarse antes del siguiente Update.
                        enlaceCombate.Pausar(false);
                        enlaceCombate.GenerarEnemigo(portal, wave, i);
                    }
                    else spawner.Spawn(enemyPrefab,portal,weapon.AimCamera.transform,player);
                    yield return TrackedWait(2);
                }
                while(EnemigosRestantes>0) yield return null;
                weapon.Ready=false; state="WAVE_COMPLETE"; UpdateFeedback("WAVE COMPLETE");
                yield return TrackedWait(3);
                if(wave<3) { state="NEXT_WAVE"; weapon.RestaurarMunicion(); yield return TrackedWait(1); }
            }
            state="VICTORY"; weapon.Ready=false; retryButton.gameObject.SetActive(true); UpdateFeedback("VICTORY");
        }
    }
}
