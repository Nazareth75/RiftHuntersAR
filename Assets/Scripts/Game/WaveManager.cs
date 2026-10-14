using System.Collections.Generic;
using RiftHunters.Testing;
using UnityEngine;

namespace RiftHunters.Game
{
    // Spawnea la oleada. La persona 4 arrastra prefabs a los huecos.
    // Si un hueco está vacío, crea un PlaceholderEnemy de prueba.
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Tooltip("Segundos entre un enemigo y el siguiente.")]
        [SerializeField] float spawnInterval = 0.8f;

        [Tooltip("Vacío = cápsula de prueba.")]
        [SerializeField] GameObject normalPrefab;
        [SerializeField] GameObject fastPrefab;
        [SerializeField] GameObject tankPrefab;
        [SerializeField] GameObject bossPrefab;

        readonly Queue<EnemyKind> queue = new Queue<EnemyKind>();
        readonly List<GameObject> spawned = new List<GameObject>();

        Transform spawnPoint;
        Transform playerTarget;
        Transform folder;
        bool running;
        float spawnTimer;
        int spawnSerial;

        public int PendingSpawns => queue.Count;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            if (spawnInterval < 0.05f)
                spawnInterval = 0.8f;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetSpawnPoint(Transform portal)
        {
            spawnPoint = portal;
        }

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        public void Begin(GameState state)
        {
            queue.Clear();
            spawnSerial = 0;
            spawnTimer = 0f;
            running = true;

            if (state == GameState.Wave1)
            {
                Enqueue(EnemyKind.Normal, 5);
            }
            else if (state == GameState.Wave2)
            {
                Enqueue(EnemyKind.Normal, 6);
                Enqueue(EnemyKind.Fast, 2);
            }
            else if (state == GameState.Wave3)
            {
                Enqueue(EnemyKind.Normal, 5);
                Enqueue(EnemyKind.Fast, 2);
                Enqueue(EnemyKind.Tank, 1);
            }
            else if (state == GameState.Boss)
            {
                Enqueue(EnemyKind.Boss, 1);
            }
            else
            {
                running = false;
            }
        }

        // El primero sale en el mismo frame en que empieza la oleada.
        // Después, uno cada spawnInterval.
        public void Tick(float deltaTime)
        {
            if (!running)
                return;

            if (queue.Count == 0)
                return;

            spawnTimer -= deltaTime;
            if (spawnTimer > 0f)
                return;

            EnemyKind kind = queue.Dequeue();
            SpawnOne(kind);
            spawnTimer = spawnInterval;
        }

        public void Stop()
        {
            running = false;
            queue.Clear();
        }

        // Fases del jefe de prueba: a 150 de vida salen 2 normales ya, sin esperar el intervalo.
        public void SpawnExtra(EnemyKind kind, int count)
        {
            if (count <= 0)
                return;

            if (GameManager.Instance != null && !GameManager.Instance.IsRunning)
                return;

            for (int i = 0; i < count; i++)
                SpawnOne(kind);
        }

        // El placeholder avisa al aparecer. Los prefabs no: aquí se cuenta al instanciarlos.
        public void NotifySpawned()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.RegisterSpawn();
        }

        // willReportScore: el placeholder llama EnemyDefeated justo después
        // y no hay que restar el vivo dos veces.
        public void NotifyDied(bool willReportScore = false)
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.RegisterDeath(willReportScore);
        }

        public void ClearBattlefield()
        {
            Stop();

            // Inmediato: el OnDestroy del placeholder corre ahora, con la partida en reset.
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] != null)
                    DestroyImmediate(spawned[i]);
            }

            spawned.Clear();
        }

        void Enqueue(EnemyKind kind, int count)
        {
            for (int i = 0; i < count; i++)
                queue.Enqueue(kind);
        }

        void SpawnOne(EnemyKind kind)
        {
            Vector3 position = SpawnPosition();
            GameObject prefab = PrefabFor(kind);

            if (prefab != null)
            {
                GameObject body = Instantiate(prefab, position, Quaternion.identity, Folder());
                spawned.Add(body);
                NotifySpawned();
                return;
            }

            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(Folder(), true);
            capsule.transform.position = position;
            spawned.Add(capsule);
            capsule.AddComponent<PlaceholderEnemy>().Initialize(kind, playerTarget);
        }

        GameObject PrefabFor(EnemyKind kind)
        {
            if (kind == EnemyKind.Fast)
                return fastPrefab;
            if (kind == EnemyKind.Tank)
                return tankPrefab;
            if (kind == EnemyKind.Boss)
                return bossPrefab;
            return normalPrefab;
        }

        // El primero de cada oleada sale al centro, para poder dispararle con la mira.
        Vector3 SpawnPosition()
        {
            Vector3 origin = spawnPoint != null
                ? spawnPoint.position
                : new Vector3(0f, 0f, 9f);

            float[] lanes = { 0f, -1.6f, 1.6f, -3.2f, 3.2f };
            float side = lanes[spawnSerial % lanes.Length];
            spawnSerial++;
            return new Vector3(origin.x + side, origin.y, origin.z);
        }

        Transform Folder()
        {
            if (folder == null)
            {
                GameObject go = new GameObject("Enemigos");
                folder = go.transform;
            }

            return folder;
        }
    }
}
