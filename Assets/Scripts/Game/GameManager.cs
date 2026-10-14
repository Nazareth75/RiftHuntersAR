using UnityEngine;

namespace RiftHunters.Game
{
    // Dueño de la partida: estado, reloj, vivos y avisos al HUD.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] float countdownSeconds = 3f;

        [SerializeField] ScoreManager scoreManager;
        [SerializeField] PlayerLives playerLives;
        [SerializeField] WaveManager waveManager;

        float matchTime;
        float countdownLeft;
        int alive;
        bool portalPlaced;
        bool resetting;
        bool suppressHookDeath;
        bool advancing;

        string weaponLine = "—";
        float bossCurrent;
        float bossMax;

        public GameState State { get; private set; } = GameState.Waiting;
        public WaveManager Waves => waveManager;
        public bool PortalWasPlaced => portalPlaced;
        public float MatchSeconds => matchTime;
        public float CountdownLeft => countdownLeft;
        public string WeaponLine => weaponLine;
        public float BossCurrent => bossCurrent;
        public float BossMax => bossMax;

        public int Score => scoreManager != null ? scoreManager.Score : 0;
        public int Lives => playerLives != null ? playerLives.Current : 0;
        public bool PlayerIsInvulnerable => playerLives != null && playerLives.IsInvulnerable;

        // Countdown y oleadas. En Waiting, Victory y GameOver los hooks de combate no cuentan.
        public bool IsRunning =>
            State == GameState.Countdown ||
            State == GameState.Wave1 ||
            State == GameState.Wave2 ||
            State == GameState.Wave3 ||
            State == GameState.Boss;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            if (scoreManager == null)
                scoreManager = GetComponent<ScoreManager>();
            if (playerLives == null)
                playerLives = GetComponent<PlayerLives>();
            if (waveManager == null)
                waveManager = GetComponent<WaveManager>();

            if (countdownSeconds < 0.5f)
                countdownSeconds = 3f;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (IsRunning)
                matchTime += Time.deltaTime;

            if (State == GameState.Countdown)
            {
                countdownLeft -= Time.deltaTime;
                if (countdownLeft <= 0f)
                    ChangeState(GameState.Wave1);
            }

            if (waveManager != null)
                waveManager.Tick(Time.deltaTime);
        }

        public void SetSpawnPoint(Transform portal)
        {
            if (waveManager != null)
                waveManager.SetSpawnPoint(portal);
        }

        public void SetPlayerTarget(Transform cameraOrPlayer)
        {
            if (waveManager != null)
                waveManager.SetPlayerTarget(cameraOrPlayer);
        }

        // Se guarda, pero la prueba puede empezar sin portal.
        public void PortalPlaced()
        {
            portalPlaced = true;
        }

        public void StartMatch()
        {
            if (State == GameState.Countdown || IsWave(State))
                return;

            if (State != GameState.Waiting)
                ReturnToWaiting();

            if (scoreManager != null)
                scoreManager.ResetScore();
            if (playerLives != null)
                playerLives.ResetLives();

            matchTime = 0f;
            weaponLine = "—";
            bossCurrent = 0f;
            bossMax = 0f;
            alive = 0;
            suppressHookDeath = false;
            countdownLeft = countdownSeconds;
            ChangeState(GameState.Countdown);
        }

        // MENÚ: vuelve al título sin recargar la escena.
        public void ReturnToWaiting()
        {
            resetting = true;
            if (waveManager != null)
                waveManager.ClearBattlefield();
            resetting = false;

            alive = 0;
            suppressHookDeath = false;
            matchTime = 0f;
            countdownLeft = 0f;
            weaponLine = "—";
            bossCurrent = 0f;
            bossMax = 0f;

            if (scoreManager != null)
                scoreManager.ResetScore();
            if (playerLives != null)
                playerLives.ResetLives();

            State = GameState.Waiting;
        }

        // REINTENTAR: limpia y arranca otra cuenta atrás.
        public void Retry()
        {
            ReturnToWaiting();
            StartMatch();
        }

        public void EnterGameOver()
        {
            if (State == GameState.GameOver || State == GameState.Victory)
                return;

            ChangeState(GameState.GameOver);
        }

        public void PlayerDamaged(int amount)
        {
            if (!IsRunning)
                return;

            if (playerLives != null)
                playerLives.Damage(amount);
        }

        public void EnemyDefeated(int score)
        {
            // Si el placeholder ya avisó la muerte, no restamos otra vez.
            bool alreadyCounted = suppressHookDeath;
            suppressHookDeath = false;

            if (!IsRunning)
                return;

            if (scoreManager != null)
                scoreManager.Add(score);

            if (!alreadyCounted && alive > 0)
                alive--;

            TryAdvance();
        }

        public void ReportBossHealth(float current, float max)
        {
            if (State == GameState.Waiting)
                return;

            bossCurrent = current;
            bossMax = max;
        }

        public void ReportAmmo(string weaponName, int current, int max)
        {
            if (!IsRunning)
                return;

            if (string.IsNullOrEmpty(weaponName))
                weaponName = "ARMA";

            weaponLine = weaponName + " " + current + "/" + max;
        }

        public void RegisterSpawn()
        {
            if (resetting)
                return;

            alive++;
        }

        public void RegisterDeath(bool willReportScore)
        {
            if (resetting)
                return;

            if (alive > 0)
                alive--;

            // El hook de puntos llega enseguida y es quien pasa de oleada,
            // para que el puntaje entre antes del cambio de estado.
            if (willReportScore)
            {
                suppressHookDeath = true;
                return;
            }

            TryAdvance();
        }

        void ChangeState(GameState next)
        {
            State = next;

            if (IsWave(next) && waveManager != null)
                waveManager.Begin(next);

            if (next == GameState.GameOver || next == GameState.Victory)
            {
                if (waveManager != null)
                    waveManager.Stop();
            }
        }

        void TryAdvance()
        {
            if (advancing || resetting)
                return;

            if (!IsWave(State))
                return;

            if (alive > 0)
                return;

            if (waveManager != null && waveManager.PendingSpawns > 0)
                return;

            advancing = true;

            if (State == GameState.Wave1)
                ChangeState(GameState.Wave2);
            else if (State == GameState.Wave2)
                ChangeState(GameState.Wave3);
            else if (State == GameState.Wave3)
                ChangeState(GameState.Boss);
            else if (State == GameState.Boss)
                ChangeState(GameState.Victory);

            advancing = false;
        }

        static bool IsWave(GameState state)
        {
            return state == GameState.Wave1 ||
                   state == GameState.Wave2 ||
                   state == GameState.Wave3 ||
                   state == GameState.Boss;
        }
    }
}
