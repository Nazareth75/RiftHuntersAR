using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RiftHuntersAR.Combat;
using RiftHuntersAR.Game;
using RiftHuntersAR.Integration;

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
        public Sprite scanMessage, placeMessage, killMessage;
        public Text scoreValue, ammoValue, waveValue, enemyValue, stateValue, resultTitle;
        public GameObject resultPanel;
        public BossHUDView bossHUD;
        public Transform muzzle;
        public GameObject plasmaShot, plasmaHit;
        private Button legacyReload, legacyRetry;
        private float hitUntil, killUntil, damageUntil;
        private float previousHP = 100;
        private bool preview;
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
        private void Retry() { if (legacyRetry != null) legacyRetry.onClick.Invoke(); }
        private void Menu() { SceneManager.LoadScene("MainMenu"); }
        private void Feedback(string text)
        {
            bool hit = text.StartsWith("HIT") || text.StartsWith("ENEMY DESTROYED");
            if (hit) hitUntil = Time.unscaledTime + .16f;
            if (text.StartsWith("ENEMY DESTROYED")) killUntil = Time.unscaledTime + 1.2f;
            bool shot = hit || text == "MISS";
            if (shot && plasmaShot != null && muzzle != null) Instantiate(plasmaShot, muzzle.position, muzzle.rotation);
            // Cosmetic query only. Never applies damage, ammo or score.
            if (text.StartsWith("HIT") && plasmaHit != null && weapon != null && weapon.AimCamera != null)
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
            Draw(state,hp,score.Score,weapon.CurrentAmmo,controller.CurrentWave,controller.AliveEnemies,
                 legacyStatus == null ? "" : legacyStatus.text, Time.unscaledTime < damageUntil,
                 Time.unscaledTime < hitUntil, Time.unscaledTime < killUntil);
            shoot.interactable = legacyShoot != null && legacyShoot.interactable;
            reload.interactable = state == "WAVE";
            resultPanel.SetActive(finished);
        }
        private void Draw(string state,float hp,int points,int ammo,int wave,int alive,string status,bool damaged,bool hit,bool killed)
        {
            scoreValue.text = points.ToString(); ammoValue.text = $"{ammo} / 30";
            waveValue.text = wave > 0 ? $"{Mathf.Clamp(wave,1,3)} / 3" : "— / 3";
            enemyValue.text = alive.ToString();
            int full = Mathf.CeilToInt(hp / (100f/3));
            for(int i=0;i<hearts.Length;i++) hearts[i].sprite = i < full ? (damaged && i == full-1 ? heartDamaged : heartFull) : heartEmpty;
            crosshair.sprite = hit ? crosshairHit : crosshairNormal;
            message.gameObject.SetActive(killed || state == "SCANNING");
            message.sprite = killed ? killMessage : status == "TAP TO PLACE PORTAL" ? placeMessage : scanMessage;
            stateValue.text = state switch { "COUNTDOWN" => "PREPARATE", "WAVE_COMPLETE" => "OLEADA COMPLETADA", "NEXT_WAVE" => "SIGUIENTE OLEADA", _ => "" };
            resultTitle.text = state == "VICTORY" ? "VICTORIA" : "GAME OVER";
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
