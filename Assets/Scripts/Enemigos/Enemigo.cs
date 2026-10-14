using System;
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
        private bool finalizado;
        private bool pausado;
        // 0: ataca una vez y desaparece. Mayor que 0: se queda y repite el ataque (jefe).
        private float intervaloAtaque;
        private float siguienteAtaque;
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

        public void Pausar(bool pausa) => pausado = pausa;
        private void OnDestroy() => Retirado?.Invoke(this);

        public void AsignarObjetivo(Transform valor) => objetivo = valor;

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
                finalizado = true;
                // El receptor decide como aplicar vida, escudo o derrota.
                AtacoJugador?.Invoke(danoAtaque);
                Destroy(gameObject);
                return;
            }
            transform.position = Vector3.MoveTowards(
                transform.position, destino, velocidad * Time.deltaTime);
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
            Debug.Log($"{name}: {Vida}/{vidaMaxima} HP", this);
            if (Vida > 0) return;
            finalizado = true;
            Murio?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
