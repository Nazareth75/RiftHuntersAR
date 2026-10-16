using UnityEngine;
namespace RiftHunters.Game
{
    // Balance concentrado: dificultad por oleada, sin multiplicar objetos en escena.
    public static class BalancePartida
    {
        public const int TotalOleadas = 12;
        public const int MaximosSimultaneos = 6;
        public const int MaximosPorOleada = 14;
        public const int MaximosResiduos = 10;
        public static bool EsJefe(int oleada) => oleada > 0 && oleada % 4 == 0;
        public static float Vida(int oleada) => 1f + .13f * Mathf.Max(0, oleada - 1);
        public static float Velocidad(int oleada) => 1.15f * (1f + .02f * Mathf.Max(0, oleada - 1));
        public static float FuerzaJefe(int oleada) => 1f + .45f * Mathf.Max(0, oleada / 4 - 1);
    }
}
