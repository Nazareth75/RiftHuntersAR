using UnityEngine;

namespace RiftHunters.Game
{
    // Único puente con el resto del equipo.
    // Delega en GameManager. Si todavía no existe, no hace nada.
    // Los de combate también se ignoran si la partida no está en curso
    // (eso lo decide GameManager).
    public static class GameplayHooks
    {
        // Persona 1: dónde aparecen los monstruos.
        public static void SetSpawnPoint(Transform portal)
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.SetSpawnPoint(portal);
        }

        // Persona 1: hacia dónde caminan (cámara o jugador).
        public static void SetPlayerTarget(Transform cameraOrPlayer)
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.SetPlayerTarget(cameraOrPlayer);
        }

        // Persona 1: el portal ya quedó colocado.
        // En la escena de prueba esto no frena el juego.
        public static void PortalPlaced()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.PortalPlaced();
        }

        // Menú o persona 1: empieza la cuenta atrás.
        public static void StartMatch()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.StartMatch();
        }

        // Persona 2: un enemigo murió. Suma puntos. No llama esto si solo llegó al jugador.
        public static void EnemyDefeated(int score)
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.EnemyDefeated(score);
        }

        // Persona 2: un enemigo alcanzó al jugador.
        public static void PlayerDamaged(int amount)
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.PlayerDamaged(amount);
        }

        // Persona 2 o 4: actualiza la barra del jefe.
        public static void ReportBossHealth(float current, float max)
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.ReportBossHealth(current, max);
        }

        // Persona 2, opcional. Si nadie llama, el HUD muestra "—".
        public static void ReportAmmo(string weaponName, int current, int max)
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.ReportAmmo(weaponName, current, max);
        }
    }
}
