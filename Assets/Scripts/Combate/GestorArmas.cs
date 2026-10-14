using System;
using UnityEngine;

namespace RiftHunters.Combate
{
    public class GestorArmas : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("weapons")]
        [SerializeField] private Arma[] armas;
        private int seleccionada;
        public bool EstaPausado { get; private set; }
        public Arma Actual => armas != null && armas.Length > 0 ? armas[seleccionada] : null;
        public event Action ArmaCambiada;

        public void Configurar(params Arma[] disponibles)
        {
            armas = disponibles;
            seleccionada = 0;
            ArmaCambiada?.Invoke();
        }

        public void AsignarCamara(Camera camara)
        {
            if (armas == null) return;
            foreach (Arma arma in armas)
                if (arma != null) arma.AsignarCamara(camara);
        }

        public void SeleccionarArma(int indice)
        {
            if (EstaPausado || armas == null || indice < 0 || indice >= armas.Length || armas[indice] == null) return;
            // Evita cambiar para saltarse la recarga. Cada arma conserva su cargador.
            if (Actual != null && Actual.EstaRecargando) return;
            seleccionada = indice;
            ArmaCambiada?.Invoke();
        }

        public void Disparar()
        {
            if (isActiveAndEnabled && !EstaPausado) Actual?.Disparar();
        }
        public void Recargar()
        {
            if (isActiveAndEnabled && !EstaPausado) Actual?.Recargar();
        }

        public void Pausar(bool pausa)
        {
            EstaPausado = pausa;
            if (armas == null) return;
            foreach (Arma arma in armas)
                if (arma != null) arma.Pausar(pausa);
        }

        public void DetenerCombate()
        {
            Pausar(true);
            if (armas == null) return;
            foreach (Arma arma in armas)
                if (arma != null) arma.CancelarRecarga();
        }

        public void ReiniciarArmas()
        {
            EstaPausado = false;
            if (armas == null) return;
            foreach (Arma arma in armas)
                if (arma != null) arma.ReiniciarArma();
            seleccionada = 0;
            ArmaCambiada?.Invoke();
        }
    }
}
