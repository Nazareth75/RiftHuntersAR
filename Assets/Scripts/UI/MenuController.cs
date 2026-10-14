using RiftHunters.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RiftHunters.UI
{
    // Título, fin de partida y victoria. No recarga la escena.
    public class MenuController : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Text titleText;
        [SerializeField] Text scoreText;
        [SerializeField] Button playButton;
        [SerializeField] Button retryButton;
        [SerializeField] Button menuButton;

        void Awake()
        {
            EnsureUi();
            Refresh();
        }

        void Update()
        {
            Refresh();
        }

        void EnsureUi()
        {
            if (panel != null)
                return;

            panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(700f, 440f);
            panelRt.anchoredPosition = Vector2.zero;

            Image background = panel.AddComponent<Image>();
            background.color = new Color(0.06f, 0.07f, 0.1f, 0.94f);

            titleText = CreateText(panel.transform, "Titulo", "RIFT HUNTERS AR", 42, FontStyle.Bold,
                new Vector2(0f, 120f), new Vector2(640f, 80f));
            scoreText = CreateText(panel.transform, "Puntaje", "PUNTOS 0", 32, FontStyle.Normal,
                new Vector2(0f, 40f), new Vector2(640f, 50f));

            playButton = CreateButton(panel.transform, "JUGAR",
                new Color(0.15f, 0.55f, 0.32f), new Vector2(0f, -50f));
            retryButton = CreateButton(panel.transform, "REINTENTAR",
                new Color(0.55f, 0.38f, 0.12f), new Vector2(0f, -40f));
            menuButton = CreateButton(panel.transform, "MENÚ",
                new Color(0.28f, 0.3f, 0.36f), new Vector2(0f, -120f));

            playButton.onClick.AddListener(OnPlay);
            retryButton.onClick.AddListener(OnRetry);
            menuButton.onClick.AddListener(OnMenu);
        }

        void Refresh()
        {
            if (panel == null)
                return;

            GameManager game = GameManager.Instance;
            GameState state = game != null ? game.State : GameState.Waiting;
            bool end = state == GameState.Victory || state == GameState.GameOver;
            bool show = state == GameState.Waiting || end;
            panel.SetActive(show);

            if (!show)
                return;

            if (titleText != null)
            {
                if (state == GameState.Victory)
                    titleText.text = "VICTORIA";
                else if (state == GameState.GameOver)
                    titleText.text = "FIN DE LA PARTIDA";
                else
                    titleText.text = "RIFT HUNTERS AR";
            }

            if (scoreText != null)
            {
                int points = game != null ? game.Score : 0;
                scoreText.text = "PUNTOS " + points;
                scoreText.gameObject.SetActive(end);
            }

            if (playButton != null)
                playButton.gameObject.SetActive(state == GameState.Waiting);
            if (retryButton != null)
                retryButton.gameObject.SetActive(end);
            if (menuButton != null)
                menuButton.gameObject.SetActive(end);
        }

        void OnPlay()
        {
            GameplayHooks.StartMatch();
        }

        void OnRetry()
        {
            if (GameManager.Instance == null)
            {
                ReloadActiveScene();
                return;
            }

            GameManager.Instance.Retry();
        }

        void OnMenu()
        {
            if (GameManager.Instance == null)
            {
                ReloadActiveScene();
                return;
            }

            GameManager.Instance.ReturnToWaiting();
        }

        static void ReloadActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0)
                SceneManager.LoadScene(scene.buildIndex);
            else if (!string.IsNullOrEmpty(scene.name))
                SceneManager.LoadScene(scene.name);
        }

        static Text CreateText(
            Transform parent,
            string name,
            string content,
            int size,
            FontStyle style,
            Vector2 position,
            Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = position;

            Text label = go.AddComponent<Text>();
            label.font = HUDController.BuiltinFont();
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = content;
            return label;
        }

        static Button CreateButton(Transform parent, string label, Color color, Vector2 position)
        {
            GameObject go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300f, 64f);
            rt.anchoredPosition = position;

            Image image = go.AddComponent<Image>();
            image.color = color;

            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;

            GameObject textGo = new GameObject("Texto", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text text = textGo.AddComponent<Text>();
            text.font = HUDController.BuiltinFont();
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            text.text = label;
            return button;
        }
    }
}
