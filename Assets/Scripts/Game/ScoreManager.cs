using UnityEngine;

namespace RiftHunters.Game
{
    // Los puntos solo entran por EnemyDefeated.
    public class ScoreManager : MonoBehaviour
    {
        public int Score { get; private set; }

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            Score += amount;
        }

        public void ResetScore()
        {
            Score = 0;
        }
    }
}
