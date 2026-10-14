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

    public static class PlanOleadas
    {
        public const float Intervalo = 0.8f;
        public const float IntervaloRapido = 0.4f;

        public static GrupoOleada[] De(GameState estado)
        {
            if (estado == GameState.Wave1)
                return new[] { new GrupoOleada(TipoEnemigo.Normal, 5, Intervalo) };

            if (estado == GameState.Wave2)
            {
                return new[]
                {
                    new GrupoOleada(TipoEnemigo.Normal, 8, Intervalo),
                    new GrupoOleada(TipoEnemigo.Rapido, 2, Intervalo)
                };
            }

            if (estado == GameState.Wave3)
                return new[] { new GrupoOleada(TipoEnemigo.Rapido, 6, IntervaloRapido) };

            if (estado == GameState.Wave4)
            {
                return new[]
                {
                    new GrupoOleada(TipoEnemigo.Normal, 4, Intervalo),
                    new GrupoOleada(TipoEnemigo.Tanque, 1, Intervalo)
                };
            }

            if (estado == GameState.Boss)
                return new[] { new GrupoOleada(TipoEnemigo.Jefe, 1, Intervalo) };

            return new GrupoOleada[0];
        }
    }
}
