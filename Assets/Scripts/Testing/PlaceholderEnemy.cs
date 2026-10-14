using RiftHunters.Game;
using UnityEngine;

namespace RiftHunters.Testing
{
    // Cápsula de prueba. No es el enemigo final.
    // La persona 2 imita las fases del jefe en el prefab real:
    // a 300 de vida la velocidad pasa de 0.8 a 1.6;
    // a 150 llama WaveManager.SpawnExtra(EnemyKind.Normal, 2).
    public class PlaceholderEnemy : MonoBehaviour
    {
        const float ReachDistance = 1.2f;

        EnemyKind kind;
        Transform target;
        int health;
        int maxHealth;
        int damage;
        int points;
        float speed;
        float bodyScale = 1f;
        bool speedBoosted;
        bool addsSpawned;
        bool left;

        public void Initialize(EnemyKind enemyKind, Transform player)
        {
            kind = enemyKind;
            target = player;
            ApplyStats();

            transform.localScale = new Vector3(bodyScale, bodyScale, bodyScale);
            Vector3 position = transform.position;
            // La cápsula mide 2 de alto. Con la escala, el centro debe quedar a bodyScale del suelo.
            position.y += bodyScale;
            transform.position = position;

            Paint(ColorFor(kind));
            gameObject.name = NameFor(kind);

            if (WaveManager.Instance != null)
                WaveManager.Instance.NotifySpawned();

            if (kind == EnemyKind.Boss)
                GameplayHooks.ReportBossHealth(health, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (left || amount <= 0)
                return;

            health -= amount;

            if (kind == EnemyKind.Boss)
            {
                GameplayHooks.ReportBossHealth(Mathf.Max(health, 0), maxHealth);
                RunBossPhases();
            }

            if (health <= 0)
                Die();
        }

        void Update()
        {
            if (left || target == null)
                return;

            // En victoria, derrota o el menú se quedan quietos.
            if (GameManager.Instance != null && !GameManager.Instance.IsRunning)
                return;

            Vector3 destination = target.position;
            destination.y = transform.position.y;
            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination) <= ReachDistance)
                ReachPlayer();
        }

        void OnDestroy()
        {
            if (left)
                return;

            left = true;
            if (WaveManager.Instance != null)
                WaveManager.Instance.NotifyDied(false);
        }

        // Solo en este placeholder. El jefe de verdad tiene que repetir estas dos fases.
        void RunBossPhases()
        {
            if (!speedBoosted && health <= 300)
            {
                speedBoosted = true;
                speed *= 2f;
            }

            if (!addsSpawned && health <= 150)
            {
                addsSpawned = true;
                if (WaveManager.Instance != null)
                    WaveManager.Instance.SpawnExtra(EnemyKind.Normal, 2);
            }
        }

        void Die()
        {
            if (left)
                return;

            left = true;

            if (kind == EnemyKind.Boss)
                GameplayHooks.ReportBossHealth(0f, maxHealth);

            // true: EnemyDefeated viene enseguida y no debe descontar el vivo otra vez.
            if (WaveManager.Instance != null)
                WaveManager.Instance.NotifyDied(true);

            GameplayHooks.EnemyDefeated(points);
            Destroy(gameObject);
        }

        void ReachPlayer()
        {
            if (left)
                return;

            left = true;
            GameplayHooks.PlayerDamaged(damage);

            if (WaveManager.Instance != null)
                WaveManager.Instance.NotifyDied(false);

            Destroy(gameObject);
        }

        void ApplyStats()
        {
            if (kind == EnemyKind.Fast)
            {
                maxHealth = 30;
                speed = 2f;
                damage = 1;
                points = 150;
                bodyScale = 1f;
            }
            else if (kind == EnemyKind.Tank)
            {
                maxHealth = 200;
                speed = 0.5f;
                damage = 2;
                points = 300;
                bodyScale = 1.5f;
            }
            else if (kind == EnemyKind.Boss)
            {
                maxHealth = 500;
                speed = 0.8f;
                damage = 2;
                points = 1000;
                bodyScale = 2f;
            }
            else
            {
                maxHealth = 50;
                speed = 1f;
                damage = 1;
                points = 100;
                bodyScale = 1f;
            }

            health = maxHealth;
        }

        static Color ColorFor(EnemyKind enemyKind)
        {
            if (enemyKind == EnemyKind.Fast)
                return new Color(0.95f, 0.85f, 0.15f);
            if (enemyKind == EnemyKind.Tank)
                return new Color(0.85f, 0.18f, 0.15f);
            if (enemyKind == EnemyKind.Boss)
                return new Color(0.55f, 0.22f, 0.82f);
            return new Color(0.2f, 0.85f, 0.25f);
        }

        static string NameFor(EnemyKind enemyKind)
        {
            if (enemyKind == EnemyKind.Fast)
                return "Enemigo Rapido";
            if (enemyKind == EnemyKind.Tank)
                return "Enemigo Tanque";
            if (enemyKind == EnemyKind.Boss)
                return "Enemigo Jefe";
            return "Enemigo Normal";
        }

        void Paint(Color color)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend == null)
                return;

            Material material = rend.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }
    }
}
