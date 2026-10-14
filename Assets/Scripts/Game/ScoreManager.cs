using System;
using UnityEngine;
namespace RiftHuntersAR.Game
{
    public sealed class ScoreManager : MonoBehaviour
    {
        public int Score { get; private set; }
        public event Action<int> Changed;
        public void ResetScore() { Score=0; Changed?.Invoke(Score); }
        public void AddEnemyScore() => AgregarPuntos(100);
        public void AgregarPuntos(int cantidad)
        {
            if (cantidad <= 0) return;
            Score += cantidad;
            Changed?.Invoke(Score);
        }
    }
}
