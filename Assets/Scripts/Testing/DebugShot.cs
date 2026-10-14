using RiftHunters.Game;
using RiftHunters.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RiftHunters.Testing
{
    // Disparo solo para esta prueba. No es el sistema de armas.
    // La persona 2 lo reemplaza y, al matar o al golpear, llama a los mismos hooks.
    public class DebugShot : MonoBehaviour
    {
        public const int Damage = 20;

        [SerializeField] Button fireButton;
        Text crosshair;
        int lastShotFrame = -1;

        void Start()
        {
            if (fireButton == null)
                CreateButton();
        }

        void Update()
        {
            if (crosshair != null && GameManager.Instance != null)
                crosshair.gameObject.SetActive(GameManager.Instance.IsRunning);

            if (!ClickedThisFrame())
                return;

            // El botón ya dispara solo. Si el clic cae en la UI, no tiremos otro rayo.
            if (PointerOverUi())
                return;

            Fire();
        }

        public void Fire()
        {
            if (lastShotFrame == Time.frameCount)
                return;

            lastShotFrame = Time.frameCount;

            Camera cam = Camera.main;
            if (cam == null)
                return;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit[] hits = Physics.RaycastAll(ray, 200f);
            float best = float.MaxValue;
            PlaceholderEnemy chosen = null;

            for (int i = 0; i < hits.Length; i++)
            {
                PlaceholderEnemy enemy = hits[i].collider.GetComponentInParent<PlaceholderEnemy>();
                if (enemy == null)
                    continue;
                if (hits[i].distance >= best)
                    continue;

                best = hits[i].distance;
                chosen = enemy;
            }

            if (chosen != null)
                chosen.TakeDamage(Damage);
        }

        void CreateButton()
        {
            Transform parent = UiParent();

            GameObject go = new GameObject("DISPARAR DE PRUEBA", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.sizeDelta = new Vector2(320f, 64f);
            rt.anchoredPosition = new Vector2(-24f, 24f);

            Image image = go.AddComponent<Image>();
            image.color = new Color(0.45f, 0.18f, 0.18f);

            fireButton = go.AddComponent<Button>();
            fireButton.targetGraphic = image;
            fireButton.onClick.AddListener(Fire);

            GameObject textGo = new GameObject("Texto", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text label = textGo.AddComponent<Text>();
            label.font = HUDController.BuiltinFont();
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = "DISPARAR DE PRUEBA";

            GameObject mark = new GameObject("Mira", typeof(RectTransform));
            mark.transform.SetParent(parent, false);
            RectTransform markRt = mark.GetComponent<RectTransform>();
            markRt.anchorMin = new Vector2(0.5f, 0.5f);
            markRt.anchorMax = new Vector2(0.5f, 0.5f);
            markRt.pivot = new Vector2(0.5f, 0.5f);
            markRt.sizeDelta = new Vector2(64f, 64f);
            markRt.anchoredPosition = Vector2.zero;

            crosshair = mark.AddComponent<Text>();
            crosshair.font = HUDController.BuiltinFont();
            crosshair.fontSize = 42;
            crosshair.alignment = TextAnchor.MiddleCenter;
            crosshair.color = Color.white;
            crosshair.raycastTarget = false;
            crosshair.text = "+";
            crosshair.gameObject.SetActive(false);
        }

        Transform UiParent()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                return canvas.transform;

#if UNITY_2023_1_OR_NEWER
            Canvas any = FindAnyObjectByType<Canvas>();
#else
            Canvas any = FindObjectOfType<Canvas>();
#endif
            if (any != null)
                return any.transform;

            return transform;
        }

        static bool ClickedThisFrame()
        {
            bool clicked = false;

#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                clicked = true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (!clicked && Input.GetMouseButtonDown(0))
                clicked = true;
#endif

            return clicked;
        }

        static bool PointerOverUi()
        {
            EventSystem events = EventSystem.current;
            if (events == null)
                return false;

            if (events.IsPointerOverGameObject())
                return true;

#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && events.IsPointerOverGameObject(mouse.deviceId))
                return true;
#endif

            return false;
        }
    }
}
