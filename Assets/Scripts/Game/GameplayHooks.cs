using System;

namespace RiftHunters.Game
{
    public static class GameplayHooks
    {
        public static event Action<bool> Pausar;
        public static event Action DetenerCombate;
        public static event Action ReiniciarArmas;

        public static void SolicitarPausa(bool pausa) => Pausar?.Invoke(pausa);
        public static void SolicitarDetenerCombate() => DetenerCombate?.Invoke();
        public static void SolicitarReiniciarArmas() => ReiniciarArmas?.Invoke();

        public static void IniciarPartida()
        {
            if (GameManager.Instance != null) GameManager.Instance.IniciarPartida();
        }

        public static void Murio(int puntos)
        {
            if (GameManager.Instance != null) GameManager.Instance.Murio(puntos);
        }

        public static void AtacoJugador(int unidades)
        {
            if (GameManager.Instance != null) GameManager.Instance.AtacoJugador(unidades);
        }

        public static void EnemigosAgotados()
        {
            if (GameManager.Instance != null) GameManager.Instance.EnemigosAgotados();
        }
    }
}
