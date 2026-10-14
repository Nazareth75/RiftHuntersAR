using RiftHunters.Game;
using RiftHunters.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RiftHunters.Testing
{
    // Se cuelga de un GameObject vacío. Al dar Play arma la prueba sola.
    public class GameTestBootstrap : MonoBehaviour
    {
        bool built;

        void Awake()
        {
            if (built)
                return;

            built = true;
            Build();
        }

        void Build()
        {
            Camera camera = CreateCamera();
            SilenceOtherCameras(camera);
            CreateLight();
            CreateFloor();
            GameObject player = CreatePlayer();
            Transform spawn = CreatePortal();
            CreateManagers();
            CreateUi();

            GameplayHooks.SetPlayerTarget(player.transform);
            GameplayHooks.SetSpawnPoint(spawn);
            // La prueba no espera un portal de verdad.
            GameplayHooks.PortalPlaced();
        }

        static Camera CreateCamera()
        {
            GameObject go = new GameObject("Camara");
            go.tag = "MainCamera";
            Camera camera = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            go.transform.position = new Vector3(0f, 4.2f, -7.5f);
            go.transform.LookAt(new Vector3(0f, 1f, 4f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            return camera;
        }

        static void SilenceOtherCameras(Camera mine)
        {
#if UNITY_2023_1_OR_NEWER
            Camera[] all = FindObjectsByType<Camera>(FindObjectsSortMode.None);
#else
            Camera[] all = FindObjectsOfType<Camera>();
#endif
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == null || all[i] == mine)
                    continue;

                all[i].enabled = false;
                AudioListener listener = all[i].GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = false;
            }
        }

        static void CreateLight()
        {
            GameObject go = new GameObject("Luz");
            Light light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void CreateFloor()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Suelo";
            floor.transform.position = new Vector3(0f, -0.5f, 2f);
            floor.transform.localScale = new Vector3(24f, 1f, 24f);
            Paint(floor, new Color(0.25f, 0.27f, 0.3f));

            // Sin collider: las cápsulas se mueven por código y no deben trabarse.
            Collider collider = floor.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        static GameObject CreatePlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Jugador";
            player.transform.position = new Vector3(0f, 0.5f, 0f);
            player.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            Paint(player, new Color(0.2f, 0.45f, 0.95f));
            return player;
        }

        static Transform CreatePortal()
        {
            // Marca violeta en el suelo. No es el portal de realidad aumentada.
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Portal";
            marker.transform.position = new Vector3(0f, 0.06f, 9f);
            marker.transform.localScale = new Vector3(2.2f, 0.03f, 2.2f);
            Paint(marker, new Color(0.62f, 0.25f, 0.9f));
            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
                Destroy(markerCollider);

            // Donde mira el centro de la cámara, para apuntar el disparo de prueba.
            GameObject aim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            aim.name = "MarcaDeMira";
            aim.transform.position = new Vector3(0f, 0.04f, 4f);
            aim.transform.localScale = new Vector3(0.45f, 0.02f, 0.45f);
            Paint(aim, Color.white);
            Collider aimCollider = aim.GetComponent<Collider>();
            if (aimCollider != null)
                Destroy(aimCollider);

            GameObject spawn = new GameObject("PuntoDeSpawn");
            spawn.transform.position = new Vector3(0f, 0f, 9f);
            return spawn.transform;
        }

        static void CreateManagers()
        {
            GameObject systems = new GameObject("Sistemas");
            systems.AddComponent<ScoreManager>();
            systems.AddComponent<PlayerLives>();
            systems.AddComponent<WaveManager>();
            systems.AddComponent<GameManager>();
        }

        static void CreateUi()
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            CreateEventSystem();

            GameObject hudGo = new GameObject("HUD", typeof(RectTransform));
            hudGo.transform.SetParent(canvasGo.transform, false);
            Stretch(hudGo.GetComponent<RectTransform>());
            hudGo.AddComponent<HUDController>();

            GameObject menuGo = new GameObject("Menu", typeof(RectTransform));
            menuGo.transform.SetParent(canvasGo.transform, false);
            Stretch(menuGo.GetComponent<RectTransform>());
            menuGo.AddComponent<MenuController>();

            GameObject shotGo = new GameObject("DisparoDePrueba");
            shotGo.transform.SetParent(canvasGo.transform, false);
            shotGo.AddComponent<DebugShot>();
        }

        static void CreateEventSystem()
        {
#if UNITY_2023_1_OR_NEWER
            if (FindAnyObjectByType<EventSystem>() != null)
                return;
#else
            if (FindObjectOfType<EventSystem>() != null)
                return;
#endif
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        static void Paint(GameObject go, Color color)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (rend == null)
                return;

            Material material = rend.material;
            material.color = color;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
        }
    }
}
