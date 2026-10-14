using System;

namespace RiftHunters.Game
{
    public interface IPuntuacion
    {
        int Score { get; }
        void AgregarPuntos(int cantidad);
        void AddEnemyScore();
        void ResetScore();
    }

    public interface IVidaJugador
    {
        float CurrentHealth { get; }
        bool IsDead { get; }
        event Action Changed;
        event Action Died;
        void TakeDamage(float damage);
        void ResetHealth();
        void Retry();
    }

    public interface IGeneradorOleada
    {
        int EnemigosVivos { get; }
        bool EstaGenerando { get; }
        event Action EnemigosAgotados;
        bool GenerarGrupo(TipoEnemigo tipo, int cantidad, float intervalo);
        void Limpiar();
        void Pausar(bool pausa);
    }
}
