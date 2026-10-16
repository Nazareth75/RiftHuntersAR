using UnityEngine;
using UnityEngine.InputSystem;

namespace RiftHunters.Combate
{
    // Este HUD y la vida son solo para pruebas. El juego usara los de persona 3.
    public class ControlesPruebaCombate : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("weapon")]
        [SerializeField] private ArmaPlasma arma;
        [SerializeField] private GestorArmas prefabSistemaArmas;
        private GestorArmas gestorArmas;
        private Enemigo enemigoApuntado;
        [SerializeField] private Enemigo[] tiposEnemigo;
        private int enemigoSeleccionado;
        private Transform origenAparicion;
        private GeneradorEnemigos generador;
        private Camera camaraApuntado;
        private bool pausado;
        private bool zonaDespejada;
        private int vidas = 3;
        private int eliminados;
        private int puntos;
        private float giroHorizontal;
        private float giroVertical;
        public void AsignarTiposEnemigo(params Enemigo[] prefabs) => tiposEnemigo = prefabs;
        public void AsignarSistemaArmas(GestorArmas prefab) => prefabSistemaArmas = prefab;
        public void AsignarArma(ArmaPlasma valor) => arma = valor;

        private void Start()
        {
            camaraApuntado = Camera.main;
            Enemigo plantilla = FindFirstObjectByType<Enemigo>();
            if (plantilla == null || camaraApuntado == null)
            {
                Debug.LogError("PruebaCombate necesita camara y enemigo de prueba.", this);
                enabled = false;
                return;
            }
            if (tiposEnemigo == null || tiposEnemigo.Length != 3)
            {
                Debug.LogError("Asigna los tres prefabs de enemigos en el Inspector.", this);
                enabled = false;
                return;
            }
            foreach (Enemigo tipo in tiposEnemigo)
            {
                if (tipo != null) continue;
                Debug.LogError("Falta una referencia en Tipos Enemigo. Asigna los prefabs de Assets/Prefabs/Enemigos.", this);
                enabled = false;
                return;
            }
            if (prefabSistemaArmas == null)
            {
                Debug.LogError("Falta SistemaArmas. Asigna Assets/Prefabs/Armas/SistemaArmas.prefab en el Inspector.", this);
                enabled = false;
                return;
            }
            // La prueba utiliza el mismo prefab que se entregara para AR.
            gestorArmas = Instantiate(prefabSistemaArmas, transform);
            gestorArmas.AsignarCamara(camaraApuntado);
            if (arma != null) arma.enabled = false; // Componente conservado de la primera escena.
            plantilla.gameObject.SetActive(false);
            var origen = new GameObject("Punto de aparicion de prueba");
            origen.transform.SetPositionAndRotation(new Vector3(plantilla.transform.position.x, 0, plantilla.transform.position.z), plantilla.transform.rotation);
            origenAparicion = origen.transform;
            origen.transform.SetParent(transform);
            generador = gameObject.AddComponent<GeneradorEnemigos>();
            generador.Configurar(tiposEnemigo[0], origen.transform, camaraApuntado.transform);
            generador.EnemigoGenerado += ConectarEnemigo;
            generador.EnemigosAgotados += AlDespejarZona;
            generador.GenerarGrupo(1, 1);
        }

        private void ConectarEnemigo(Enemigo enemigo)
        {
            zonaDespejada = false;
            if (enemigo.GetComponent<RespuestaVisualEnemigo>() == null)
                enemigo.gameObject.AddComponent<RespuestaVisualEnemigo>();
            enemigo.Murio += AlMorirEnemigo;
            enemigo.AtacoJugador += AlRecibirAtaque;
        }

        private void AlMorirEnemigo(Enemigo enemigo)
        {
            eliminados++;
            puntos += enemigo.Puntos;
        }

        private void SeleccionarEnemigo(int indice)
        {
            enemigoSeleccionado = indice;
            generador.Configurar(tiposEnemigo[indice], origenAparicion, camaraApuntado.transform);
        }

        private void AlDespejarZona() => zonaDespejada = true;

        private void AlternarPausa()
        {
            pausado = !pausado;
            gestorArmas.Pausar(pausado);
            generador.Pausar(pausado);
        }

        private void AlRecibirAtaque(int dano)
        {
            vidas = Mathf.Max(0, vidas - dano);
            if (vidas == 0)
            {
                gestorArmas.DetenerCombate();
                generador.Limpiar();
            }
        }

        private void Update()
        {
            if (generador == null || vidas == 0) return;
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame) AlternarPausa();
            if (pausado) return;
            enemigoApuntado = gestorArmas.Actual.ObtenerEnemigoApuntado();
            var teclado = Keyboard.current;
            if (teclado != null)
            {
                float horizontal = (teclado.rightArrowKey.isPressed ? 1 : 0)
                    - (teclado.leftArrowKey.isPressed ? 1 : 0);
                float vertical = (teclado.downArrowKey.isPressed ? 1 : 0)
                    - (teclado.upArrowKey.isPressed ? 1 : 0);
                giroHorizontal = Mathf.Clamp(giroHorizontal + horizontal * 60 * Time.deltaTime, -80, 80);
                giroVertical = Mathf.Clamp(giroVertical + vertical * 60 * Time.deltaTime, -45, 45);
                camaraApuntado.transform.rotation = Quaternion.Euler(giroVertical, giroHorizontal, 0);
                if (teclado.spaceKey.wasPressedThisFrame) gestorArmas.Disparar();
                if (teclado.rKey.wasPressedThisFrame) gestorArmas.Recargar();
                if (teclado.digit1Key.wasPressedThisFrame) gestorArmas.SeleccionarArma(0);
                if (teclado.digit2Key.wasPressedThisFrame) gestorArmas.SeleccionarArma(1);
                if (teclado.digit3Key.wasPressedThisFrame) gestorArmas.SeleccionarArma(2);
            }
            // Los clics sobre botones se gestionan exclusivamente en OnGUI.
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                float escala = Mathf.Max(0.7f, Screen.height / 720f);
                Vector2 raton = Mouse.current.position.ReadValue();
                if (raton.y > 180 * escala && raton.y < Screen.height - 320 * escala)
                    gestorArmas.Disparar();
            }
        }

        private void OnGUI()
        {
            if (generador == null) return;
            float escala = Mathf.Max(0.7f, Screen.height / 720f);
            Matrix4x4 matrizAnterior = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * escala);
            float ancho = Screen.width / escala;
            float alto = Screen.height / escala;
            GUI.Box(new Rect(20, 20, 580, 115),
                $"PRUEBA DE COMBATE | Vida: {vidas}/3 | Eliminados: {eliminados} | Puntos: {puntos}\n{gestorArmas.Actual.NombreVisible}: {gestorArmas.Actual.Municion}/{gestorArmas.Actual.CapacidadCargador} | R: recargar\nEspacio / clic / DISPARAR | Flechas: apuntar\nEnemigos activos: {generador.EnemigosVivos} | P: pausar/reanudar");
            GUI.Label(new Rect(25, 145, 460, 30),
                vidas == 0 ? "FIN DE PRUEBA. Pulsa REINICIAR." : pausado ? "PAUSA: pulsa P o REANUDAR." : gestorArmas.Actual.UltimoResultado);
            Color colorAnterior = GUI.color;
            GUI.color = vidas > 0 && enemigoApuntado != null ? Color.green : Color.white;
            GUI.Label(new Rect(ancho / 2 - 8, alto / 2 - 12, 30, 30), "+");
            GUI.color = colorAnterior;
            if (vidas > 0 && enemigoApuntado != null)
                GUI.Box(new Rect(ancho / 2 - 100, alto / 2 + 20, 200, 30),
                    $"Vida del objetivo: {enemigoApuntado.Vida}/{enemigoApuntado.VidaMaxima}");
            GUI.enabled = vidas > 0 && !pausado && !gestorArmas.Actual.EstaRecargando && gestorArmas.Actual.Municion > 0;
            if (GUI.Button(new Rect(ancho / 2 - 100, alto - 85, 200, 60), "DISPARAR"))
                gestorArmas.Disparar();
            GUI.enabled = vidas > 0 && !pausado && !gestorArmas.Actual.EstaRecargando && gestorArmas.Actual.Municion < gestorArmas.Actual.CapacidadCargador;
            if (GUI.Button(new Rect(ancho / 2 - 100, alto - 155, 200, 55),
                gestorArmas.Actual.EstaRecargando ? "RECARGANDO..." : "RECARGAR (R)"))
                gestorArmas.Recargar();
            GUI.enabled = vidas > 0 && !pausado && !generador.EstaGenerando;
            if (GUI.Button(new Rect(ancho - 190, alto - 85, 170, 60), "GRUPO DE 3"))
                generador.GenerarGrupo(3, 0.2f);
            GUI.enabled = true;
            if (GUI.Button(new Rect(20, alto - 85, 170, 60), "REINICIAR"))
            {
                generador.Limpiar();
                gestorArmas.ReiniciarArmas();
                pausado = false;
                zonaDespejada = false;
                vidas = 3;
                eliminados = 0;
                puntos = 0;
                SeleccionarEnemigo(0);
                giroHorizontal = giroVertical = 0;
                camaraApuntado.transform.rotation = Quaternion.identity;
                generador.GenerarGrupo(1, 1);
            }
            GUI.enabled = vidas > 0 && !pausado;
            if (GUI.Button(new Rect(20, 190, 150, 45), "1 - PLASMA"))
                gestorArmas.SeleccionarArma(0);
            if (GUI.Button(new Rect(180, 190, 150, 45), "2 - ELECTRICA"))
                gestorArmas.SeleccionarArma(1);
            if (GUI.Button(new Rect(340, 190, 150, 45), "3 - EXPLOSIVA"))
                gestorArmas.SeleccionarArma(2);
            GUI.enabled = true;
            GUI.enabled = vidas > 0 && !pausado && !generador.EstaGenerando;
            if (GUI.Button(new Rect(20, 245, 150, 45), "NORMAL: 50 HP"))
                SeleccionarEnemigo(0);
            if (GUI.Button(new Rect(180, 245, 150, 45), "RAPIDO: 30 HP"))
                SeleccionarEnemigo(1);
            if (GUI.Button(new Rect(340, 245, 150, 45), "TANQUE: 200 HP"))
                SeleccionarEnemigo(2);
            GUI.enabled = true;
            GUI.Label(new Rect(20, 295, 480, 30),
                $"Siguiente grupo: {tiposEnemigo[enemigoSeleccionado].name}. Pulsa GRUPO DE 3.");
            GUI.enabled = vidas > 0;
            if (GUI.Button(new Rect(ancho - 190, alto - 155, 170, 55),
                pausado ? "REANUDAR (P)" : "PAUSAR (P)"))
                AlternarPausa();
            GUI.enabled = true;
            if (zonaDespejada && !generador.EstaGenerando && vidas > 0)
                GUI.Label(new Rect(ancho / 2 - 130, alto - 195, 300, 30), "ZONA DESPEJADA: crea otro grupo.");
            GUI.matrix = matrizAnterior;
        }
    }
}
