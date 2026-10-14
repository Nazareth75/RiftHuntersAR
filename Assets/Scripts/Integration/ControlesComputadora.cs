using UnityEngine;
using UnityEngine.InputSystem;

namespace RiftHuntersAR.Integration
{
    // Controles de laptop para ARGame. En el celular no se añade.
    public sealed class ControlesComputadora : MonoBehaviour
    {
        private const float SensibilidadRaton = 0.12f;
        private const float VelocidadFlechas = 70f;

        private ARPrototypeController controlador;
        private float giroHorizontal;
        private float giroVertical;
        private bool giroListo;
        private GUIStyle estiloAyuda;

        public void Unir(ARPrototypeController valor) => controlador = valor;

        private bool EnCombate => controlador.CurrentState == "WAVE" && !controlador.EnPausa;
        private bool Terminada => controlador.CurrentState == "GAME_OVER" || controlador.CurrentState == "VICTORY";

        private void Update()
        {
            if (controlador == null || !controlador.ModoComputadora) return;
            Keyboard teclado = Keyboard.current;
            Mouse raton = Mouse.current;

            if (teclado != null)
            {
                if (teclado.pKey.wasPressedThisFrame || teclado.escapeKey.wasPressedThisFrame)
                    controlador.CambiarPausaPrueba();
                if (Terminada && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame))
                    controlador.ReintentarPrueba();
            }

            if (!EnCombate)
            {
                SoltarCursor();
                return;
            }

            bool capturado = Cursor.lockState == CursorLockMode.Locked;
            if (!capturado)
            {
                if (raton != null && raton.leftButton.wasPressedThisFrame && Application.isFocused)
                    CapturarCursor();
            }
            else if (raton != null)
            {
                if (raton.leftButton.wasPressedThisFrame) controlador.DispararPrueba();
                Vector2 movimiento = raton.delta.ReadValue();
                giroHorizontal += movimiento.x * SensibilidadRaton;
                giroVertical -= movimiento.y * SensibilidadRaton;
                float rueda = raton.scroll.ReadValue().y;
                if (rueda > 0.1f) controlador.CambiarArma(1);
                else if (rueda < -0.1f) controlador.CambiarArma(-1);
            }

            if (teclado != null)
            {
                if (teclado.spaceKey.wasPressedThisFrame) controlador.DispararPrueba();
                if (teclado.rKey.wasPressedThisFrame) controlador.RecargarPrueba();
                if (teclado.digit1Key.wasPressedThisFrame || teclado.numpad1Key.wasPressedThisFrame) controlador.SeleccionarArma(0);
                if (teclado.digit2Key.wasPressedThisFrame || teclado.numpad2Key.wasPressedThisFrame) controlador.SeleccionarArma(1);
                if (teclado.digit3Key.wasPressedThisFrame || teclado.numpad3Key.wasPressedThisFrame) controlador.SeleccionarArma(2);
                float horizontal = (teclado.rightArrowKey.isPressed || teclado.dKey.isPressed ? 1 : 0)
                    - (teclado.leftArrowKey.isPressed || teclado.aKey.isPressed ? 1 : 0);
                float vertical = (teclado.upArrowKey.isPressed || teclado.wKey.isPressed ? 1 : 0)
                    - (teclado.downArrowKey.isPressed || teclado.sKey.isPressed ? 1 : 0);
                giroHorizontal += horizontal * VelocidadFlechas * Time.deltaTime;
                giroVertical -= vertical * VelocidadFlechas * Time.deltaTime;
            }

            Girar();
        }

        private void Girar()
        {
            Transform camara = controlador.CamaraJuego.transform;
            if (!giroListo)
            {
                Vector3 angulos = camara.eulerAngles;
                giroHorizontal = angulos.y;
                giroVertical = angulos.x > 180f ? angulos.x - 360f : angulos.x;
                giroListo = true;
                return;
            }
            giroVertical = Mathf.Clamp(giroVertical, -60f, 60f);
            camara.rotation = Quaternion.Euler(giroVertical, giroHorizontal, 0f);
        }

        private static void CapturarCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void SoltarCursor()
        {
            if (Cursor.lockState == CursorLockMode.None) return;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable() => SoltarCursor();

        private void OnGUI()
        {
            if (controlador == null || !controlador.ModoComputadora) return;
            estiloAyuda ??= new GUIStyle(GUI.skin.box) { fontSize = 14, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            string texto = controlador.CurrentState switch
            {
                "SCANNING" => "Enter o clic: colocar el portal",
                "GAME_OVER" or "VICTORY" => "Enter: reintentar",
                "PORTAL_PLACED" or "COUNTDOWN" => "Prepárate: al empezar, haz clic para apuntar con el mouse",
                _ => Cursor.lockState == CursorLockMode.Locked
                    ? "Mouse o flechas/WASD: apuntar | Clic o Espacio: disparar | R: recargar | 1/2/3 o rueda: arma | P o Esc: pausa"
                    : controlador.EnPausa ? "PAUSA | P o Esc: seguir" : "Haz clic en el juego para apuntar con el mouse",
            };
            GUI.Box(new Rect(10, Screen.height - 50, Mathf.Min(Screen.width - 20, 820), 40), texto, estiloAyuda);
        }
    }
}
