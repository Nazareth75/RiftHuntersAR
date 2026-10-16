using UnityEngine;
namespace RiftHunters.Game
{
    public readonly struct GrupoOleada
    {
        public readonly TipoEnemigo Tipo;
        public readonly int Cantidad;
        public readonly float Intervalo;

        public GrupoOleada(TipoEnemigo tipo, int cantidad, float intervalo)
        {
            Tipo = tipo;
            Cantidad = cantidad;
            Intervalo = intervalo;
        }
    }

    // Los grupos de cada oleada salen en orden; el siguiente empieza cuando el anterior terminó de aparecer.
    public static class PlanOleadas
    {
        public static GrupoOleada[] De(int numero)
        {
            if (numero < 1 || numero > BalancePartida.TotalOleadas) return new GrupoOleada[0];
            if (BalancePartida.EsJefe(numero)) return new[] { new GrupoOleada(TipoEnemigo.Jefe, 1, 1f) };
            int total = Mathf.Min(BalancePartida.MaximosPorOleada, 4 + numero + (numero > 1 ? 1 : 0));
            int tanques = numero < 3 ? 0 : Mathf.Min(3, 1 + numero / 5);
            int rapidos = numero < 2 ? 0 : Mathf.Min(6, 1 + numero / 2);
            int normales = total - tanques - rapidos;
            return new[] {
                new GrupoOleada(TipoEnemigo.Normal, normales, Mathf.Max(.85f, 1.8f - numero * .06f)),
                new GrupoOleada(TipoEnemigo.Rapido, rapidos, Mathf.Max(.7f, 1.15f - numero * .025f)),
                new GrupoOleada(TipoEnemigo.Tanque, tanques, 1.7f)
            };
        }

        public static float Espera(GameState estado)
        {
            if (estado == GameState.Wave1) return 1.5f;
            if (estado == GameState.Boss) return 3f;
            return 2.5f;
        }
    }
}
