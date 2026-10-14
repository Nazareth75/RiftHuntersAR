namespace RiftHunters.Game
{
    // Estados de una partida.
    // Waiting -> Countdown -> Wave1 -> Wave2 -> Wave3 -> Boss -> Victory.
    // Con 0 vidas se pasa a GameOver y se corta el spawn.
    public enum GameState
    {
        Waiting,
        Countdown,
        Wave1,
        Wave2,
        Wave3,
        Boss,
        Victory,
        GameOver
    }

    // Tipos que spawnea WaveManager.
    // Si el prefab de ese tipo está vacío, sale un PlaceholderEnemy.
    public enum EnemyKind
    {
        Normal,
        Fast,
        Tank,
        Boss
    }
}
