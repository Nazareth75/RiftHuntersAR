using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RiftHuntersAR.Combat;
using RiftHuntersAR.Game;
using RiftHuntersAR.Integration;
using RiftHuntersAR.Audio;

namespace RiftHuntersAR.UI.Visual
{
    // Presentation adapter: all combat/retry actions invoke the existing buttons' events.
    public sealed class VisualHUD : MonoBehaviour
    {
        public ARPrototypeController controller;
        public PlasmaWeapon weapon;
        public ScoreManager score;
        public Canvas legacyCanvas;
        public Button legacyShoot;
        public Text legacyStatus;
        public Button shoot, reload, retry, mainMenu;
        public Image crosshair, message;
        public Image[] hearts;
        public Sprite crosshairNormal, crosshairHit, heartFull, heartEmpty, heartDamaged;
        public Sprite scanMessage, placeMessage, killMessage, bossMessage;
        public Text scoreValue, ammoValue, waveValue, enemyValue, stateValue, resultTitle;
        public Text tiempoValue, detalleFin;
        public GameObject resultPanel;
        public BossHUDView bossHUD;
        public Transform muzzle;
        public GameObject plasmaShot, plasmaHit;
        private Button legacyReload, legacyRetry;
        private float hitUntil, killUntil, damageUntil;
        private float previousHP = 100;
        private bool preview;
        private Color? colorGameOver;
        private static readonly Color ColorVictoria = new(0.35f, 1f, 0.75f);
        private void OnEnable()
        {
            shoot.onClick.AddListener(Shoot); reload.onClick.AddListener(Reload);
            retry.onClick.AddListener(Retry); mainMenu.onClick.AddListener(Menu);
            if (weapon != null) weapon.Feedback += Feedback;
        }
        private void OnDisable()
        {
            shoot.onClick.RemoveListener(Shoot); reload.onClick.RemoveListener(Reload);
            retry.onClick.RemoveListener(Retry); mainMenu.onClick.RemoveListener(Menu);
            if (weapon != null) weapon.Feedback -= Feedback;
        }
        private void Start()
        {
            ammoValue.resizeTextForBestFit=true;
            ammoValue.resizeTextMinSize=22;
            ammoValue.resizeTextMaxSize=ammoValue.fontSize;
            if (GetComponent<WeaponVisuals>() == null && ammoValue != null && ammoValue.GetComponent<Button>() == null)
            {
                ammoValue.raycastTarget = true;
                var cambiar = ammoValue.gameObject.AddComponent<Button>();
                cambiar.targetGraphic = ammoValue;
                cambiar.onClick.AddListener(() => { if (controller != null) controller.CambiarArma(); });
            }
            if (preview || legacyCanvas == null) return;
            legacyReload = legacyCanvas.transform.Find("RELOAD")?.GetComponent<Button>();
            legacyRetry = legacyCanvas.transform.Find("RETRY")?.GetComponent<Button>();
            if (legacyReload == null || legacyRetry == null) Debug.LogError("[Rift Visual] Legacy reload/retry references missing", this);
            // Keep legacy objects/events/state alive; only hide their presentation and raycasts.
            legacyCanvas.enabled = false;
            var raycaster = legacyCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null) raycaster.enabled = false;
        }
        private void Shoot() { if (legacyShoot != null && legacyShoot.interactable) legacyShoot.onClick.Invoke(); }
        private void Reload() { if (legacyReload != null) legacyReload.onClick.Invoke(); }
        private void Retry()
        {
            SonidoJuego.Reproducir("ui_confirmar", 0.8f);
            if (controller != null) controller.ReintentarPartida();
            else if (legacyRetry != null) legacyRetry.onClick.Invoke();
        }
        private void Menu()
        {
            SonidoJuego.Reproducir("ui_click", 0.8f);
            SceneManager.LoadScene("MainMenu");
        }
        private void Feedback(string text)
        {
            bool hit = text.StartsWith("HIT") || text.StartsWith("ENEMY DESTROYED")
                || text.StartsWith("Impacto:") || text.StartsWith("Enemigo eliminado")
                || text.StartsWith("Cadena electrica:") || text.StartsWith("Explosion:");
            if (hit) hitUntil = Time.unscaledTime + .16f;
            if (text.StartsWith("ENEMY DESTROYED") || text.StartsWith("Enemigo eliminado")) killUntil = Time.unscaledTime + 1.2f;
            bool recarga = text.StartsWith("Recarg") || text == "RELOADED" || text.StartsWith("Sin municion") || text.StartsWith("EMPTY");
            bool shot = !recarga && (hit || text == "MISS" || text.StartsWith("Disparo")
                || text.StartsWith("Impacto") || text.StartsWith("La electricidad"));
            if (shot && plasmaShot != null && muzzle != null) Instantiate(plasmaShot, muzzle.position, muzzle.rotation);
            // Cosmetic query only. Never applies damage, ammo or score.
            if ((text.StartsWith("HIT") || text.StartsWith("Impacto:")) && plasmaHit != null && weapon != null && weapon.AimCamera != null)
            {
                var ray = weapon.AimCamera.ViewportPointToRay(new Vector3(.5f,.5f,0));
                if (Physics.Raycast(ray, out var result, 20, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    Instantiate(plasmaHit, result.point, Quaternion.LookRotation(result.normal));
            }
        }
        private void LateUpdate()
        {
            if (preview || controller == null || controller.Player == null) return;
            float hp = controller.Player.CurrentHealth;
            if (hp < previousHP) damageUntil = Time.unscaledTime + .3f;
            previousHP = hp;
            string state = controller.CurrentState;
            bool finished = state == "GAME_OVER" || state == "VICTORY";
            string status = legacyStatus == null ? "" : legacyStatus.text;
            if (controller.CurrentState == "SCANNING" || controller.CurrentState == "COUNTDOWN")
                status = legacyStatus == null ? status : legacyStatus.text;
            Draw(controller.CurrentState,hp,score.Score,weapon.CurrentAmmo,controller.CurrentWave,controller.AliveEnemies,
                 status, Time.unscaledTime < damageUntil,
                 Time.unscaledTime < hitUntil, Time.unscaledTime < killUntil);
            shoot.interactable = legacyShoot != null && legacyShoot.interactable;
            reload.interactable = state == "WAVE";
            resultPanel.SetActive(finished);
        }
        private void Draw(string state,float hp,int points,int ammo,int wave,int alive,string status,bool damaged,bool hit,bool killed)
        {
            AsegurarTextos();
            int cargador = weapon != null ? weapon.MagazineSize : 30;
            string nombreArma = weapon != null ? weapon.NombreArma : "ARMA";
            scoreValue.text = points.ToString();
            ammoValue.text = $"{nombreArma} {ammo}/{cargador}";
            bool jefe = controller != null && controller.EsJefe;
            waveValue.text = jefe ? "JEFE" : wave > 0 ? $"{wave} / 4" : "— / 4";
            enemyValue.text = alive.ToString();
            if (tiempoValue != null)
                tiempoValue.text = FormatoTiempo(controller != null ? controller.TiempoPartida : 0f);
            int full = Mathf.CeilToInt(hp / (100f / 3f));
            if (full > hearts.Length) full = hearts.Length;
            for(int i=0;i<hearts.Length;i++) hearts[i].sprite = i < full ? (damaged && i == full-1 ? heartDamaged : heartFull) : heartEmpty;
            crosshair.sprite = hit ? crosshairHit : crosshairNormal;
            crosshair.gameObject.SetActive(state!="GAME_OVER" && state!="VICTORY");
            bool colocar = status == ARPrototypeController.TextoColocar;
            bool editorSinPlano = status == ARPrototypeController.TextoEditorColocar;
            float fraccion = controller != null ? controller.FraccionJefe : -1f;
            bool mostrarJefe = jefe && fraccion >= 0f;
            bool avisoJefe = state == "WAVE" && controller != null && controller.AnunciandoJefe && bossMessage != null;
            // El cartel de eliminado comparte altura con el HUD del jefe; no se muestra mientras el jefe está en pantalla.
            bool mostrarEliminado = killed && !mostrarJefe;
            message.gameObject.SetActive((mostrarEliminado || avisoJefe || state == "SCANNING") && !editorSinPlano);
            message.sprite = avisoJefe ? bossMessage : mostrarEliminado ? killMessage : colocar ? placeMessage : scanMessage;
            var nucleos = controller != null ? controller.NucleosJefe : null;
            string anuncio = controller != null ? controller.AnuncioOleada : "";
            if (state == "COUNTDOWN" && controller != null) stateValue.text = controller.TextoCuenta;
            else if (state == "SCANNING") stateValue.text = status;
            else if (controller != null && !controller.SeguimientoDisponible && state == "WAVE") stateValue.text = status;
            else if (state == "WAVE" && controller != null && controller.Generador != null && controller.Generador.EsperandoDistancia)
                stateValue.text = "Aléjate un poco del portal para que salgan los enemigos.";
            else if (state == "WAVE" && !string.IsNullOrEmpty(anuncio)) stateValue.text = anuncio;
            else if (jefe && state == "WAVE" && nucleos != null) stateValue.text = nucleos.Pista;
            else stateValue.text = "";
            bool finished = state == "GAME_OVER" || state == "VICTORY";
            bool victoria = state == "VICTORY";
            colorGameOver ??= resultTitle.color;
            resultTitle.text = victoria ? "VICTORIA" : "GAME OVER";
            resultTitle.color = victoria ? ColorVictoria : colorGameOver.Value;
            if (detalleFin != null)
            {
                int derrotados = controller != null ? controller.EnemigosDerrotados : 0;
                detalleFin.text = $"PUNTOS: {points}\nENEMIGOS: {derrotados}\nTIEMPO: {FormatoTiempo(controller != null ? controller.TiempoPartida : 0f)}";
                detalleFin.gameObject.SetActive(finished);
            }
            if (bossHUD != null)
            {
                bossHUD.gameObject.SetActive(mostrarJefe);
                if (mostrarJefe)
                {
                    if (nucleos != null) bossHUD.Present(fraccion, nucleos.IzquierdoVivo, nucleos.CentralExpuesto && nucleos.CentralVivo, nucleos.DerechoVivo);
                    else bossHUD.Present(fraccion, false, false, false);
                }
            }
        }

        private void AsegurarTextos()
        {
            // Junto a los corazones: entre Heart_2 (termina en x=309) y ScorePanel incluso en pantallas 20:9.
            if (tiempoValue == null && scoreValue != null && resultPanel != null)
                tiempoValue = BuscarOCrearTexto(resultPanel.transform.parent, "Tiempo", scoreValue,
                    new Vector2(0f, 1f), new Vector2(415f, -92f), new Vector2(190f, 70f), 40, scoreValue.color);
            // Entre ResultTitle (y 220..340 del panel) y Retry (y -190..-20): tres líneas sin desbordar.
            if (detalleFin == null && resultTitle != null && resultPanel != null)
                detalleFin = BuscarOCrearTexto(resultPanel.transform, "DetalleFin", resultTitle,
                    new Vector2(0.5f, 1f), new Vector2(0f, -265f), new Vector2(800f, 200f), 46, new Color(0.92f, 0.96f, 1f));
        }

        private static Text BuscarOCrearTexto(Transform padre, string nombre, Text estilo, Vector2 ancla,
            Vector2 posicion, Vector2 tamano, int tamanoFuente, Color color)
        {
            Transform existente = padre.Find(nombre);
            if (existente != null && existente.TryGetComponent(out Text encontrado)) return encontrado;

            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = ancla;
            rt.anchorMax = ancla;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = tamano;
            rt.anchoredPosition = posicion;
            var texto = go.AddComponent<Text>();
            texto.font = estilo.font;
            texto.fontSize = tamanoFuente;
            texto.resizeTextForBestFit = true;
            texto.resizeTextMinSize = 20;
            texto.resizeTextMaxSize = tamanoFuente;
            texto.lineSpacing = 1.1f;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.color = color;
            texto.raycastTarget = false;
            texto.horizontalOverflow = HorizontalWrapMode.Wrap;
            texto.verticalOverflow = VerticalWrapMode.Truncate;
            return texto;
        }

        private static string FormatoTiempo(float segundos)
        {
            if (segundos < 0f || float.IsNaN(segundos)) segundos = 0f;
            int total = Mathf.FloorToInt(segundos);
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }
        // Explicit editor-review fixture: synthetic values, never used to advance game state.
        public void ShowReview(string state,float hp,int points,int ammo,int wave,int alive,string status="")
        {
            preview = true; Draw(state,hp,points,ammo,wave,alive,status,false,false,false);
            shoot.interactable = state == "WAVE"; reload.interactable = state == "WAVE";
            resultPanel.SetActive(state == "GAME_OVER" || state == "VICTORY");
        }
    }
}
