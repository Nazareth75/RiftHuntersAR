using RiftHunters.Game;
using UnityEngine;
using UnityEngine.UI;

namespace RiftHunters.UI
{
    // Arma el HUD si nadie le pasó textos en el Inspector.
    public class HUDController : MonoBehaviour
    {
        [SerializeField] Text waveText;
        [SerializeField] Text livesText;
        [SerializeField] Text scoreText;
        [SerializeField] Text timeText;
        [SerializeField] Text weaponText;
        [SerializeField] Slider bossSlider;
        [SerializeField] GameObject bossRoot;

        public static Font BuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        void Awake()
        {
            EnsureUi();
        }

        void Update()
        {
            GameManager game = GameManager.Instance;
            if (game == null)
                return;

            if (waveText != null)
                waveText.text = WaveLabel(game);

            if (livesText != null)
            {
                livesText.text = Hearts(game.Lives);
                livesText.color = game.PlayerIsInvulnerable
                    ? new Color(1f, 0.85f, 0.35f)
                    : Color.white;
            }

            if (scoreText != null)
                scoreText.text = "PUNTOS " + game.Score;

            if (timeText != null)
                timeText.text = "TIEMPO " + FormatTime(game.MatchSeconds);

            if (weaponText != null)
                weaponText.text = game.WeaponLine;

            bool showBoss = game.State == GameState.Boss;
            if (bossRoot != null)
                bossRoot.SetActive(showBoss);

            if (showBoss && bossSlider != null && game.BossMax > 0f)
                bossSlider.value = Mathf.Clamp01(game.BossCurrent / game.BossMax);
        }

        void EnsureUi()
        {
            if (waveText != null)
                return;

            waveText = CreateLabel(
                "Oleada", "", 32, TextAnchor.MiddleLeft, FontStyle.Bold,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -24f), new Vector2(560f, 48f));

            scoreText = CreateLabel(
                "Puntos", "PUNTOS 0", 28, TextAnchor.MiddleLeft, FontStyle.Normal,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -78f), new Vector2(560f, 40f));

            weaponText = CreateLabel(
                "Arma", "—", 24, TextAnchor.MiddleLeft, FontStyle.Normal,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -122f), new Vector2(560f, 36f));

            livesText = CreateLabel(
                "Vidas", "❤ ❤ ❤", 34, TextAnchor.MiddleRight, FontStyle.Bold,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-28f, -24f), new Vector2(360f, 48f));

            timeText = CreateLabel(
                "Tiempo", "TIEMPO 00:00", 30, TextAnchor.MiddleCenter, FontStyle.Bold,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -24f), new Vector2(280f, 48f));

            CreateBossBar();
        }

        void CreateBossBar()
        {
            bossRoot = new GameObject("Jefe", typeof(RectTransform));
            bossRoot.transform.SetParent(transform, false);
            RectTransform root = bossRoot.GetComponent<RectTransform>();
            root.anchorMin = new Vector2(0.5f, 0f);
            root.anchorMax = new Vector2(0.5f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.sizeDelta = new Vector2(440f, 72f);
            root.anchoredPosition = new Vector2(0f, 36f);

            Text caption = CreateLabel(
                "TituloJefe", "VIDA DEL JEFE", 20, TextAnchor.MiddleCenter, FontStyle.Bold,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 0f), new Vector2(440f, 28f));
            caption.transform.SetParent(bossRoot.transform, false);

            GameObject barGo = new GameObject("Barra", typeof(RectTransform));
            barGo.transform.SetParent(bossRoot.transform, false);
            RectTransform barRt = barGo.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.5f, 0f);
            barRt.anchorMax = new Vector2(0.5f, 0f);
            barRt.pivot = new Vector2(0.5f, 0f);
            barRt.sizeDelta = new Vector2(440f, 28f);
            barRt.anchoredPosition = Vector2.zero;

            Image background = barGo.AddComponent<Image>();
            background.color = new Color(0.15f, 0.15f, 0.18f);

            bossSlider = barGo.AddComponent<Slider>();

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(barGo.transform, false);
            RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0.2f);
            fillAreaRt.anchorMax = new Vector2(1f, 0.8f);
            fillAreaRt.offsetMin = new Vector2(6f, 0f);
            fillAreaRt.offsetMax = new Vector2(-6f, 0f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.58f, 0.25f, 0.82f);

            bossSlider.fillRect = fillRt;
            bossSlider.targetGraphic = background;
            bossSlider.minValue = 0f;
            bossSlider.maxValue = 1f;
            bossSlider.value = 1f;
            bossSlider.interactable = false;
            bossRoot.SetActive(false);
        }

        Text CreateLabel(
            string name,
            string content,
            int size,
            TextAnchor align,
            FontStyle style,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 position,
            Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = position;

            Text label = go.AddComponent<Text>();
            label.font = BuiltinFont();
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = align;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            label.text = content;

            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return label;
        }

        static string WaveLabel(GameManager game)
        {
            if (game.State == GameState.Wave1)
                return "OLEADA 1/3";
            if (game.State == GameState.Wave2)
                return "OLEADA 2/3";
            if (game.State == GameState.Wave3)
                return "OLEADA 3/3";
            if (game.State == GameState.Boss)
                return "JEFE";
            if (game.State == GameState.Countdown)
            {
                int seconds = Mathf.CeilToInt(game.CountdownLeft);
                if (seconds < 1)
                    seconds = 1;
                return "COMIENZA EN " + seconds;
            }
            if (game.State == GameState.Victory)
                return "VICTORIA";
            if (game.State == GameState.GameOver)
                return "FIN";
            return "";
        }

        static string Hearts(int lives)
        {
            if (lives <= 0)
                return "";
            if (lives > 10)
                lives = 10;

            string text = "❤";
            for (int i = 1; i < lives; i++)
                text += " ❤";
            return text;
        }

        static string FormatTime(float seconds)
        {
            if (seconds < 0f)
                seconds = 0f;

            int total = Mathf.FloorToInt(seconds);
            int minutes = total / 60;
            int secs = total % 60;
            return minutes.ToString("00") + ":" + secs.ToString("00");
        }
    }
}
