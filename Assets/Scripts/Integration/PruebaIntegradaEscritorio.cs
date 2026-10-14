using UnityEngine;
using UnityEngine.InputSystem;

namespace RiftHuntersAR.Integration
{
    // Usa el controlador real; solo sustituye tracking y entrada del telefono.
    [DefaultExecutionOrder(-1000)]
    public class PruebaIntegradaEscritorio : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private ARPrototypeController controlador;
        private float giroHorizontal;
        private float giroVertical;

        private void Awake()
        {
            controlador.HabilitarPruebaEscritorio();
            Camera camara = controlador.CamaraJuego;
            camara.transform.SetParent(null, true);
            camara.transform.SetPositionAndRotation(new Vector3(0, 1, -6), Quaternion.identity);
            camara.clearFlags = CameraClearFlags.SolidColor;
            camara.backgroundColor = new Color(0.04f, 0.06f, 0.1f);
        }

        private void Update()
        {
            var teclado = Keyboard.current;
            if (teclado == null) return;
            if (teclado.enterKey.wasPressedThisFrame)
            {
                controlador.IniciarPruebaEscritorio(Vector3.zero);
                controlador.ReintentarPrueba();
            }
            if (teclado.pKey.wasPressedThisFrame) controlador.CambiarPausaPrueba();
            if (teclado.spaceKey.wasPressedThisFrame) controlador.DispararPrueba();
            if (teclado.rKey.wasPressedThisFrame) controlador.RecargarPrueba();
            var enlace = controlador.GetComponent<EnlaceCombateAR>();
            if (enlace != null && enlace.Armas != null)
            {
                if (teclado.digit1Key.wasPressedThisFrame) enlace.SeleccionarArma(0);
                if (teclado.digit2Key.wasPressedThisFrame) enlace.SeleccionarArma(1);
                if (teclado.digit3Key.wasPressedThisFrame) enlace.SeleccionarArma(2);
            }
            float horizontal = (teclado.rightArrowKey.isPressed ? 1 : 0) - (teclado.leftArrowKey.isPressed ? 1 : 0);
            float vertical = (teclado.downArrowKey.isPressed ? 1 : 0) - (teclado.upArrowKey.isPressed ? 1 : 0);
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                Vector2 movimiento = Mouse.current.delta.ReadValue();
                giroHorizontal += movimiento.x * 0.15f;
                giroVertical -= movimiento.y * 0.15f;
            }
            giroHorizontal = Mathf.Clamp(giroHorizontal + horizontal * 60 * Time.deltaTime, -90, 90);
            giroVertical = Mathf.Clamp(giroVertical + vertical * 60 * Time.deltaTime, -60, 60);
            controlador.CamaraJuego.transform.rotation = Quaternion.Euler(giroVertical, giroHorizontal, 0);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 620, 55),
                "PARTIDA INTEGRADA EN ESCRITORIO (no valida AR real)\nEnter: portal/reintento | Espacio: disparar | R: recarga | 1/2/3: armas | P: pausa | Flechas: mira");
        }
#endif
    }
}
