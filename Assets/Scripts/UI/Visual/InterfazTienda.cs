using UnityEngine;
using UnityEngine.UI;
using RiftHunters.Game;
using RiftHuntersAR.Integration;
using RiftHuntersAR.Audio;

namespace RiftHuntersAR.UI.Visual
{
    // Panel de la partida: conserva tipografía, colores y los iconos existentes.
    [DefaultExecutionOrder(300)]
    public sealed class InterfazTienda : MonoBehaviour
    {
        private TiendaECO tienda;
        private VisualHUD hud;
        private GameManager partida;
        private GameObject decisiones, escaparate;
        private Font fuente;
        private int arma;
        private Text tituloPausa, saldo, mensaje;
        private readonly Text[] atributos = new Text[3], precios = new Text[3];
        private readonly Button[] compras = new Button[3], pestanas = new Button[3];
        private Button curacion;
        private Text textoCuracion;
        private static readonly Color Fondo = new(.025f, .065f, .10f, .98f);
        private static readonly Color Cian = new(.2f, .85f, 1f);

        public void Configurar(TiendaECO modelo, VisualHUD vista)
        {
            tienda = modelo; hud = vista; partida = GameManager.Instance;
            fuente = hud.scoreValue.font;
            Transform safe = hud.resultPanel.transform.parent;
            decisiones = Pantalla(safe, "Fin de oleada");
            tituloPausa = Texto(decisiones.transform, "Título", "", new Vector2(0, 180), new Vector2(900, 120), 52, Cian);
            Texto(decisiones.transform, "Ayuda", "Invierte tu ECO o continúa con la siguiente oleada.", new Vector2(0, 65), new Vector2(850, 100), 34, Color.white);
            Boton(decisiones.transform, "TIENDA", new Vector2(-225, -100), new Vector2(420, 110), () => tienda.Abrir());
            Boton(decisiones.transform, "SIGUIENTE", new Vector2(225, -100), new Vector2(420, 110), () => partida.SiguienteOleada());

            escaparate = Pantalla(safe, "Tienda ECO");
            var panel = Caja(escaparate.transform, "Marco", Vector2.zero, new Vector2(940, 1280), Fondo);
            panel.gameObject.AddComponent<Outline>().effectColor = Cian;
            Texto(panel, "Título", "TIENDA ECO", new Vector2(0, 555), new Vector2(850, 85), 54, Cian);
            saldo = Texto(panel, "Saldo", "", new Vector2(0, 465), new Vector2(850, 65), 38, new Color(.4f, 1f, .65f));
            Texto(panel, "Ayuda", "Mejoras de esta partida · máximo 3 niveles por atributo", new Vector2(0, 400), new Vector2(870, 55), 27, Color.white);
            string[] nombres = { "PLASMA", "ELECTRIC", "ENERGY" };
            var visuales = hud.GetComponent<WeaponVisuals>();
            for (int i = 0; i < 3; i++)
            {
                int indice = i;
                pestanas[i] = Boton(panel, nombres[i], new Vector2((i - 1) * 285, 310), new Vector2(265, 95), () => arma = indice);
                if (visuales != null && visuales.icons != null && visuales.icons.Length > i)
                {
                    var icono = Caja(panel, "Icono " + i, new Vector2((i-1)*285, 235), new Vector2(64, 64), Color.white).gameObject;
                    Image imagen = icono.GetComponent<Image>(); imagen.sprite = visuales.icons[i]; imagen.preserveAspect = true; imagen.raycastTarget = false;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                int atributo = i;
                var fila = Caja(panel, "Mejora " + i, new Vector2(0, 125 - i * 160), new Vector2(870, 140), new Color(.04f, .12f, .17f));
                atributos[i] = Texto(fila, "Atributo", "", new Vector2(-160, 0), new Vector2(490, 125), 32, Color.white);
                compras[i] = Boton(fila, "", new Vector2(265, 0), new Vector2(290, 100), () => { if (tienda.Comprar(arma, atributo)) SonidoJuego.Reproducir("ui_confirmar", .7f); });
                precios[i] = compras[i].GetComponentInChildren<Text>();
            }
            curacion = Boton(panel, "", new Vector2(0, -365), new Vector2(870, 95), () => { if (tienda.ComprarVida()) SonidoJuego.Reproducir("ui_confirmar", .7f); });
            textoCuracion = curacion.GetComponentInChildren<Text>();
            mensaje = Texto(panel, "Mensaje", "", new Vector2(0, -465), new Vector2(850, 80), 30, Color.white);
            Boton(panel, "VOLVER", new Vector2(0, -565), new Vector2(420, 95), () => tienda.Cerrar());
            decisiones.SetActive(false); escaparate.SetActive(false);
        }

        private GameObject Pantalla(Transform padre, string nombre)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image)); go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0, .015f, .03f, .85f); return go;
        }
        private RectTransform Caja(Transform padre, string nombre, Vector2 posicion, Vector2 tamano, Color color)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image)); go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = tamano; rect.anchoredPosition = posicion;
            go.GetComponent<Image>().color = color; return rect;
        }
        private Text Texto(Transform padre, string nombre, string contenido, Vector2 posicion, Vector2 tamano, int medida, Color color)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Text)); go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = tamano; rect.anchoredPosition = posicion;
            var texto = go.GetComponent<Text>(); texto.font = fuente; texto.text = contenido; texto.fontSize = medida; texto.color = color;
            texto.alignment = TextAnchor.MiddleCenter; texto.raycastTarget = false; texto.resizeTextForBestFit = true; texto.resizeTextMinSize = 22; texto.resizeTextMaxSize = medida;
            return texto;
        }
        private Button Boton(Transform padre, string contenido, Vector2 posicion, Vector2 tamano, UnityEngine.Events.UnityAction accion)
        {
            RectTransform rect = Caja(padre, contenido, posicion, tamano, new Color(.03f, .24f, .3f));
            var borde = rect.gameObject.AddComponent<Outline>(); borde.effectColor = Cian; borde.effectDistance = new Vector2(1, -1);
            var boton = rect.gameObject.AddComponent<Button>(); boton.targetGraphic = rect.GetComponent<Image>(); boton.onClick.AddListener(accion);
            Texto(rect, "Texto", contenido, Vector2.zero, tamano - new Vector2(16, 8), 32, Color.white); return boton;
        }
        private void LateUpdate()
        {
            if (tienda == null || decisiones == null) return;
            bool disponible = tienda.Disponible;
            decisiones.SetActive(disponible && !tienda.Abierta); escaparate.SetActive(disponible && tienda.Abierta);
            if (!disponible) return;
            tituloPausa.text = $"OLEADA {partida.NumeroOleada}/12 COMPLETADA";
            if (!tienda.Abierta) return;
            saldo.text = $"SALDO: {tienda.Saldo} ECO";
            var actual = tienda.Arma(arma);
            for (int i = 0; i < 3; i++)
            {
                pestanas[i].GetComponent<Image>().color = i == arma ? new Color(.05f, .5f, .6f) : new Color(.03f, .15f, .2f);
                int nivel = tienda.Nivel(arma, i);
                string nombre = i == 0 ? "DAÑO" : i == 1 ? "CARGADOR" : "RECARGA";
                string valor = actual == null ? "" : i == 0 ? actual.Dano.ToString() : i == 1 ? actual.CapacidadCargador + " balas" : actual.TiempoRecarga.ToString("0.00") + " s";
                atributos[i].text = $"{nombre} · NIVEL {nivel}/3\n{valor}";
                precios[i].text = nivel >= 3 ? "MÁXIMO" : $"MEJORAR\n{tienda.Precio(arma, i)} ECO";
                compras[i].interactable = tienda.PuedeComprar(arma, i);
            }
            textoCuracion.text = $"RECUPERAR 34 HP · {tienda.PrecioCuracion} ECO   |   VIDA: {hud.controller.Player.CurrentHealth:0}/100";
            curacion.interactable = tienda.PuedeCurar; mensaje.text = tienda.Mensaje;
        }
    }
}
