using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RiftHuntersAR.UI.Visual
{
    // Solo gestiona la pulsación; el arma sigue decidiendo cadencia, munición y daño.
    public sealed class BotonDisparoSostenido : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Button boton;
        private Action disparar;
        private bool pulsado;
        private int puntero;
        private float omitirHasta = -1f;
        public bool OmitirClick => pulsado || Time.unscaledTime <= omitirHasta;
        public void Configurar(Button control, Action accion) { boton = control; disparar = accion; }
        public void OnPointerDown(PointerEventData evento)
        {
            if (pulsado || evento.button != PointerEventData.InputButton.Left || boton == null || !boton.IsInteractable()) return;
            puntero = evento.pointerId; pulsado = true;
            disparar?.Invoke();
        }
        public void OnPointerUp(PointerEventData evento) { if (pulsado && evento.pointerId == puntero) Cancelar(); }
        public void OnPointerExit(PointerEventData evento) { if (pulsado && evento.pointerId == puntero) Cancelar(); }
        private void Update()
        {
            if (!pulsado) return;
            if (boton == null || !boton.IsActive() || !boton.IsInteractable()) { Cancelar(); return; }
            disparar?.Invoke();
        }
        public void Cancelar()
        {
            if (pulsado) omitirHasta = Time.unscaledTime + .2f;
            pulsado = false;
        }
        private void OnDisable() => Cancelar();
        private void OnApplicationFocus(bool foco) { if (!foco) Cancelar(); }
        private void OnApplicationPause(bool pausa) { if (pausa) Cancelar(); }
    }
}
