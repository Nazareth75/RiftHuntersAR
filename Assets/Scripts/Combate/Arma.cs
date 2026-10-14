using System;
using UnityEngine;

namespace RiftHunters.Combate
{
    public abstract class Arma : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("aimCamera")]
        [SerializeField] private Camera camaraApuntado;
        [UnityEngine.Serialization.FormerlySerializedAs("damage")]
        [SerializeField, Min(1)] protected int dano = 20;
        [UnityEngine.Serialization.FormerlySerializedAs("shotInterval")]
        [SerializeField, Min(0.01f)] protected float intervaloDisparo = 0.25f;
        [UnityEngine.Serialization.FormerlySerializedAs("range")]
        [SerializeField, Min(1)] private float alcance = 30f;
        [UnityEngine.Serialization.FormerlySerializedAs("hitMask")]
        [SerializeField] protected LayerMask capasImpacto = ~0;
        [UnityEngine.Serialization.FormerlySerializedAs("magazineSize")]
        [SerializeField, Min(1)] protected int capacidadCargador = 30;
        [UnityEngine.Serialization.FormerlySerializedAs("reloadDuration")]
        [SerializeField, Min(0.01f)] protected float duracionRecarga = 1.5f;
        private float siguienteDisparo;
        private float finRecarga;
        private float inicioPausa;
        public bool EstaPausada { get; private set; }
        public int Municion { get; private set; }
        public int CapacidadCargador => capacidadCargador;
        public bool EstaRecargando { get; private set; }
        public string UltimoResultado { get; protected set; } = "Apunta y dispara";
        public abstract string NombreVisible { get; }
        public event Action MunicionCambiada;
        public event Action<Vector3, Vector3> DisparoRealizado;

        protected virtual void Awake() => ReiniciarArma();

        protected virtual void Update()
        {
            if (EstaPausada || !EstaRecargando || Time.time < finRecarga) return;
            Municion = capacidadCargador;
            EstaRecargando = false;
            UltimoResultado = "Recarga completada";
            MunicionCambiada?.Invoke();
        }

        public void Recargar()
        {
            if (!isActiveAndEnabled || EstaPausada || EstaRecargando || Municion >= capacidadCargador) return;
            EstaRecargando = true;
            finRecarga = Time.time + duracionRecarga;
            UltimoResultado = "Recargando...";
            MunicionCambiada?.Invoke();
        }

        public void Disparar()
        {
            if (!isActiveAndEnabled || EstaPausada || camaraApuntado == null || EstaRecargando || Time.time < siguienteDisparo) return;
            if (Municion <= 0)
            {
                UltimoResultado = "Sin municion. Pulsa R o RECARGAR";
                return;
            }
            siguienteDisparo = Time.time + intervaloDisparo;
            Municion--;
            MunicionCambiada?.Invoke();
            Ray rayo = camaraApuntado.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            bool acerto = Physics.Raycast(rayo, out RaycastHit impacto,
                alcance, capasImpacto, QueryTriggerInteraction.Ignore);
            Vector3 inicio = camaraApuntado.transform.TransformPoint(new Vector3(0.15f, -0.1f, 0.3f));
            Vector3 fin = acerto ? impacto.point : rayo.GetPoint(alcance);
            DisparoRealizado?.Invoke(inicio, fin);
            if (!acerto)
            {
                UltimoResultado = "Disparo sin impacto";
                return;
            }
            AplicarImpacto(impacto);
        }

        public Enemigo ObtenerEnemigoApuntado()
        {
            if (camaraApuntado == null) return null;
            Ray rayo = camaraApuntado.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (!Physics.Raycast(rayo, out RaycastHit impacto, alcance,
                capasImpacto, QueryTriggerInteraction.Ignore)) return null;
            Enemigo enemigo = impacto.collider.GetComponentInParent<Enemigo>();
            return enemigo != null && enemigo.Vida > 0 ? enemigo : null;
        }

        protected abstract void AplicarImpacto(RaycastHit impacto);

        public void Pausar(bool pausa)
        {
            if (EstaPausada == pausa) return;
            if (pausa) inicioPausa = Time.time;
            else
            {
                float duracionPausa = Time.time - inicioPausa;
                siguienteDisparo += duracionPausa;
                finRecarga += duracionPausa;
            }
            EstaPausada = pausa;
        }

        public void CancelarRecarga()
        {
            if (!EstaRecargando) return;
            EstaRecargando = false;
            UltimoResultado = "Recarga cancelada";
            MunicionCambiada?.Invoke();
        }

        public void ReiniciarArma()
        {
            EstaPausada = false;
            capacidadCargador = Mathf.Max(1, capacidadCargador);
            Municion = capacidadCargador;
            EstaRecargando = false;
            siguienteDisparo = 0;
            UltimoResultado = "Apunta y dispara";
            MunicionCambiada?.Invoke();
        }

        protected virtual void OnDisable() => CancelarRecarga();

        public void AsignarCamara(Camera camara) => camaraApuntado = camara;
    }
}
