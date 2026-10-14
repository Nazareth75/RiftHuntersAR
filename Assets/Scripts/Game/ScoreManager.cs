using System;
using UnityEngine;
namespace RiftHuntersAR.Game
{
    public sealed class ScoreManager : MonoBehaviour
    {
        public int Score { get; private set; }
        public event Action<int> Changed;
        public void AddEnemyScore() { Score += 100; Changed?.Invoke(Score); }
    }
}
