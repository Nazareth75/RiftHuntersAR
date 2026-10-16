using UnityEngine;
using RiftHunters.Game;
using RiftHunters.Combate;
using RiftHuntersAR.Recycling;
namespace RiftHuntersAR.Integration
{
    // Compras de la partida actual. No modifica los prefabs ni usa dinero real.
    public sealed class TiendaECO : MonoBehaviour
    {
        private readonly int[,] niveles = new int[3, 3];
        private ARPrototypeController controlador;
        private GameManager partida;
        private RecyclingGameplay reciclaje;
        public bool Abierta { get; private set; }
        public string Mensaje { get; private set; } = "";
        public int Saldo => reciclaje != null ? reciclaje.EcoPoints : 0;
        public bool Disponible => partida != null && partida.EntreOleadas && controlador != null && controlador.Player != null && !controlador.Player.IsDead;
        public int PrecioCuracion => 200 + 50 * Mathf.Max(0, ((partida != null ? partida.NumeroOleada : 1) - 1) / 4);
        public void Configurar(ARPrototypeController control, GameManager manager, RecyclingGameplay residuos)
        { controlador = control; partida = manager; reciclaje = residuos; }
        public Arma Arma(int indice) => controlador != null && controlador.Armas != null ? controlador.Armas.ObtenerArma(indice) : null;
        public int Nivel(int arma, int atributo) => arma >= 0 && arma < 3 && atributo >= 0 && atributo < 3 ? niveles[arma, atributo] : 3;
        public int Precio(int arma, int atributo) => Nivel(arma, atributo) >= 3 ? 0 : (atributo == 0 ? 250 : 200) * (1 << Nivel(arma, atributo));
        public bool PuedeComprar(int arma, int atributo) => Disponible && Abierta && Arma(arma) != null && Nivel(arma, atributo) < 3 && Saldo >= Precio(arma, atributo);
        public void Abrir() { if (Disponible) { Abierta = true; Mensaje = "Elige en qué arma invertir tu ECO."; } }
        public void Cerrar() { Abierta = false; }
        public bool Comprar(int arma, int atributo)
        {
            if (!PuedeComprar(arma, atributo) || !reciclaje.IntentarGastarECO(Precio(arma, atributo))) return false;
            niveles[arma, atributo]++;
            Arma(arma).AplicarMejoras(niveles[arma, 0], niveles[arma, 1], niveles[arma, 2]);
            Mensaje = "Mejora instalada."; return true;
        }
        public bool PuedeCurar => Disponible && Abierta && controlador.Player.CurrentHealth < 100 && Saldo >= PrecioCuracion;
        public bool ComprarVida()
        {
            if (!PuedeCurar || !reciclaje.IntentarGastarECO(PrecioCuracion)) return false;
            controlador.Player.Curar(34); Mensaje = "Vida recuperada: +34 HP (máximo 100)."; return true;
        }
        public void Reiniciar()
        {
            Abierta = false; Mensaje = "";
            for (int i = 0; i < 3; i++)
            { for (int j = 0; j < 3; j++) niveles[i, j] = 0; Arma(i)?.AplicarMejoras(0, 0, 0); }
        }
    }
}
