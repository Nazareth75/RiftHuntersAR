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
        private float recargaGuardada;
        private bool guardada;
        private readonly RaycastHit[] candidatos = new RaycastHit[32];
        public float ProgresoRecarga => !EstaRecargando ? 0f : 1f - Mathf.Clamp01(
            (guardada ? recargaGuardada : finRecarga - (EstaPausada ? inicioPausa : Time.time)) / duracionRecarga);
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
            if (EstaPausada || guardada || !EstaRecargando || Time.time < finRecarga) return;
            Municion = capacidadCargador;
            EstaRecargando = false;
            UltimoResultado = "Recarga completada";
            MunicionCambiada?.Invoke();
        }

        public void Recargar()
        {
            if (!isActiveAndEnabled || EstaPausada || guardada || EstaRecargando || Municion >= capacidadCargador) return;
            EstaRecargando = true;
            finRecarga = Time.time + duracionRecarga;
            UltimoResultado = "Recargando...";
            MunicionCambiada?.Invoke();
        }

        public void Disparar()
        {
            if (!isActiveAndEnabled || EstaPausada || guardada || camaraApuntado == null || EstaRecargando || Time.time < siguienteDisparo) return;
            if (Municion <= 0)
            {
                UltimoResultado = "Sin municion. Pulsa R o RECARGAR";
                return;
            }
            siguienteDisparo = Time.time + intervaloDisparo;
            Municion--;
            MunicionCambiada?.Invoke();
            Ray rayo = camaraApuntado.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            bool acerto = BuscarImpacto(rayo, out RaycastHit impacto);
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
            if (!BuscarImpacto(rayo, out RaycastHit impacto)) return null;
            Enemigo enemigo = impacto.collider.GetComponentInParent<Enemigo>();
            return enemigo != null && enemigo.Vida > 0 ? enemigo : null;
        }

        private bool BuscarImpacto(Ray rayo, out RaycastHit impacto)
        {
            bool directo = Physics.Raycast(rayo, out impacto, alcance, capasImpacto, QueryTriggerInteraction.Ignore);
            if (directo)
            {
                Enemigo enemigo = impacto.collider.GetComponentInParent<Enemigo>();
                // Un acierto exacto tiene prioridad, incluso si se apunta al centro protegido.
                if (enemigo != null && enemigo.Vida > 0) return true;
            }
            // Margen de 3,5 cm y 1,5 grados. No mueve la cámara ni atraviesa obstáculos.
            int cantidad = Physics.SphereCastNonAlloc(rayo, .035f, candidatos, alcance, capasImpacto, QueryTriggerInteraction.Ignore);
            float mejorAngulo = 1.5f;
            bool asistido = false;
            for (int i = 0; i < cantidad; i++)
            {
                Collider colisionador = candidatos[i].collider;
                Enemigo enemigo = colisionador.GetComponentInParent<Enemigo>();
                if (enemigo == null || !enemigo.isActiveAndEnabled || enemigo.Vida <= 0 || enemigo.Invulnerable) continue;
                Vector3 centro = colisionador.bounds.center;
                float distancia = Vector3.Dot(centro - rayo.origin, rayo.direction);
                if (distancia <= 0f || distancia > alcance) continue;
                Vector3 punto = colisionador.ClosestPoint(rayo.GetPoint(distancia));
                // Entrar ligeramente al collider evita fallos numéricos en un rayo tangente.
                punto = Vector3.MoveTowards(punto, centro, .003f);
                Vector3 haciaPunto = punto - rayo.origin;
                float angulo = Vector3.Angle(rayo.direction, haciaPunto);
                if (angulo >= mejorAngulo) continue;
                if (!Physics.Raycast(rayo.origin, haciaPunto.normalized, out RaycastHit visible,
                    Mathf.Min(alcance, haciaPunto.magnitude + .01f), capasImpacto, QueryTriggerInteraction.Ignore)) continue;
                if (visible.collider.GetComponentInParent<Enemigo>() != enemigo) continue;
                impacto = visible; mejorAngulo = angulo; asistido = true;
            }
            return asistido || directo;
        }

        public void Guardar(bool valor)
        {
            if (guardada == valor) return;
            if (EstaRecargando)
            {
                if (valor) recargaGuardada = Mathf.Max(0f, finRecarga - Time.time);
                else finRecarga = Time.time + recargaGuardada;
            }
            guardada = valor;
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
            guardada = false;
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
