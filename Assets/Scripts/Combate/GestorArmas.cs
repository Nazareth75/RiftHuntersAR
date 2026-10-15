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
            ActualizarArmasGuardadas();
            ArmaCambiada?.Invoke();
        }

        private void Start() => ActualizarArmasGuardadas();

        private void ActualizarArmasGuardadas()
        {
            if (armas == null) return;
            for (int i = 0; i < armas.Length; i++)
                if (armas[i] != null) armas[i].Guardar(i != seleccionada);
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
            if (seleccionada == indice) return;
            seleccionada = indice;
            ActualizarArmasGuardadas();
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
            ActualizarArmasGuardadas();
            ArmaCambiada?.Invoke();
        }
    }
}
