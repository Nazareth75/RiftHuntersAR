using System;
using System.Collections.Generic;
using UnityEngine;

namespace RiftHunters.Combate
{
    public class Enemigo : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxHealth")]
        [SerializeField, Min(1)] private int vidaMaxima = 50;
        [UnityEngine.Serialization.FormerlySerializedAs("score")]
        [SerializeField, Min(0)] private int puntos = 100;
        [UnityEngine.Serialization.FormerlySerializedAs("speed")]
        [SerializeField, Min(0)] private float velocidad = 0.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("attackDistance")]
        [SerializeField, Min(0.1f)] private float distanciaAtaque = 0.8f;
        [UnityEngine.Serialization.FormerlySerializedAs("attackDamage")]
        [SerializeField, Min(1)] private int danoAtaque = 1;
        private Transform objetivo;
        private RiftHuntersAR.AR.SueloCombateAR suelo;
        private bool finalizado;
        private bool pausado;
        // 0: ataca una vez y desaparece. Mayor que 0: se queda y repite el ataque (jefe).
        private float intervaloAtaque;
        private float siguienteAtaque;
        // Pausa antes del golpe: el jugador aún puede eliminarlo sin recibir daño.
        private const float PreparacionAtaque = 0.8f;
        private const float RadioSeparacion = 0.3f;
        private static readonly List<Enemigo> enMovimiento = new();
        private float preparacion;
        public bool PreparandoAtaque => preparacion > 0f;
        public int Vida { get; private set; }
        public int Puntos => puntos;
        public int VidaMaxima => vidaMaxima;
        public bool Invulnerable { get; set; }
        public event Action VidaCambiada;
        public event Action<Enemigo> Murio;
        public event Action<int> AtacoJugador;
        public event Action<Enemigo> Retirado;
        public event Action ImpactoBloqueado;

        private void Awake() => Vida = vidaMaxima;

        public void ConfigurarEstadisticas(int vida, float velocidadMovimiento, int danoJugador, int valorPuntos)
        {
            vidaMaxima = Mathf.Max(1, vida);
            velocidad = Mathf.Max(0, velocidadMovimiento);
            danoAtaque = Mathf.Max(1, danoJugador);
            puntos = Mathf.Max(0, valorPuntos);
            Vida = vidaMaxima;
            finalizado = false;
        }

        public void ConfigurarAtaqueContinuo(float intervalo, float distancia)
        {
            intervaloAtaque = Mathf.Max(0, intervalo);
            distanciaAtaque = Mathf.Max(0.1f, distancia);
        }

        public void Pausar(bool pausa)
        {
            if (pausado == pausa) return;
            pausado = pausa;
            // Dar un margen al recuperar tracking; no golpear inmediatamente al reanudar.
            if (!pausa) { preparacion = 0f; siguienteAtaque = Time.time + .8f; }
        }
        private void OnDestroy()
        {
            enMovimiento.Remove(this);
            Retirado?.Invoke(this);
        }

        public void AsignarObjetivo(Transform valor)
        {
            objetivo = valor;
            suelo = GetComponentInParent<RiftHuntersAR.AR.SueloCombateAR>();
            if (!enMovimiento.Contains(this)) enMovimiento.Add(this);
        }

        private void Update()
        {
            if (pausado || finalizado || objetivo == null) return;
            Vector3 destino = new Vector3(objetivo.position.x, transform.position.y, objetivo.position.z);
            if (Vector3.Distance(transform.position, destino) <= distanciaAtaque)
            {
                if (intervaloAtaque > 0)
                {
                    if (Time.time < siguienteAtaque) return;
                    siguienteAtaque = Time.time + intervaloAtaque;
                    AtacoJugador?.Invoke(danoAtaque);
                    return;
                }
                preparacion += Time.deltaTime;
                if (preparacion < PreparacionAtaque) return;
                finalizado = true;
                // El receptor decide como aplicar vida, escudo o derrota.
                AtacoJugador?.Invoke(danoAtaque);
                Destroy(gameObject);
                return;
            }
            preparacion = 0f;
            Vector3 siguiente = Vector3.MoveTowards(
                transform.position, destino, velocidad * Time.deltaTime);
            // Cerca del jugador prima una trayectoria legible sobre el empuje lateral del grupo.
            float separacion = Mathf.InverseLerp(distanciaAtaque + .1f, 1.5f, Vector3.Distance(transform.position, destino));
            Vector3 posicion = siguiente + Separacion() * (velocidad * separacion * Time.deltaTime);
            transform.position = suelo != null ? suelo.Apoyar(posicion) : posicion;
        }

        private Vector3 Separacion()
        {
            Vector3 empuje = Vector3.zero;
            foreach (Enemigo otro in enMovimiento)
            {
                if (otro == this || otro == null || otro.finalizado) continue;
                Vector3 diferencia = transform.position - otro.transform.position;
                diferencia.y = 0f;
                float distancia = diferencia.magnitude;
                if (distancia >= RadioSeparacion) continue;
                Vector3 direccion = distancia > 0.001f
                    ? diferencia / distancia
                    : transform.right * (GetInstanceID() > otro.GetInstanceID() ? 1f : -1f);
                empuje += direccion * (1f - distancia / RadioSeparacion);
            }
            return Vector3.ClampMagnitude(empuje, 1f);
        }

        public void RecibirDano(int dano)
        {
            if (pausado || finalizado || Vida <= 0 || dano <= 0) return;
            if (Invulnerable)
            {
                ImpactoBloqueado?.Invoke();
                return;
            }
            Vida = Mathf.Max(0, Vida - dano);
            VidaCambiada?.Invoke();
#if UNITY_EDITOR
            Debug.Log($"{name}: {Vida}/{vidaMaxima} HP", this);
#endif
            if (Vida > 0) return;
            finalizado = true;
            Murio?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
