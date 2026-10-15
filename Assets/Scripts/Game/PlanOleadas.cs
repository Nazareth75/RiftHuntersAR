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
        public const float Intervalo = 1.8f;
        public const float IntervaloRapido = 1f;

        public static GrupoOleada[] De(GameState estado)
        {
            // Oleada 1: introducción, solo básicos.
            if (estado == GameState.Wave1)
                return new[] { new GrupoOleada(TipoEnemigo.Normal, 5, Intervalo) };

            // Oleada 2: aparecen los rápidos intercalados con básicos.
            if (estado == GameState.Wave2)
            {
                return new[]
                {
                    new GrupoOleada(TipoEnemigo.Normal, 3, Intervalo),
                    new GrupoOleada(TipoEnemigo.Rapido, 2, Intervalo),
                    new GrupoOleada(TipoEnemigo.Normal, 2, Intervalo),
                    new GrupoOleada(TipoEnemigo.Rapido, 2, Intervalo)
                };
            }

            // Oleada 3: mezcla de los anteriores con mayoría de rápidos.
            if (estado == GameState.Wave3)
            {
                return new[]
                {
                    new GrupoOleada(TipoEnemigo.Rapido, 3, IntervaloRapido),
                    new GrupoOleada(TipoEnemigo.Normal, 3, Intervalo),
                    new GrupoOleada(TipoEnemigo.Rapido, 4, IntervaloRapido)
                };
            }

            // Oleada 4: el tanque abre la oleada para presentarlo como enemigo pesado.
            if (estado == GameState.Wave4)
            {
                return new[]
                {
                    new GrupoOleada(TipoEnemigo.Tanque, 1, Intervalo),
                    new GrupoOleada(TipoEnemigo.Normal, 3, Intervalo),
                    new GrupoOleada(TipoEnemigo.Rapido, 2, IntervaloRapido),
                    new GrupoOleada(TipoEnemigo.Tanque, 1, Intervalo)
                };
            }

            if (estado == GameState.Boss)
                return new[] { new GrupoOleada(TipoEnemigo.Jefe, 1, Intervalo) };

            return new GrupoOleada[0];
        }

        public static string Anuncio(GameState estado)
        {
            if (estado == GameState.Wave1) return "OLEADA 1: ENEMIGOS BÁSICOS";
            if (estado == GameState.Wave2) return "OLEADA 2: ¡LLEGAN LOS RÁPIDOS!";
            if (estado == GameState.Wave3) return "OLEADA 3: ¡ENJAMBRE MIXTO!";
            if (estado == GameState.Wave4) return "OLEADA 4: ¡CUIDADO CON EL TANQUE!";
            if (estado == GameState.Boss) return "¡EL RIFT GUARDIAN SE ACERCA!";
            return "";
        }

        public static float Espera(GameState estado)
        {
            if (estado == GameState.Wave1) return 1.5f;
            if (estado == GameState.Boss) return 3f;
            return 2.5f;
        }
    }
}
