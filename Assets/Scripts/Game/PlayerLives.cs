using UnityEngine;

namespace RiftHunters.Game
{
    // 3 vidas. Tras un golpe hay 0.6 s en los que no entra otro.
    public class PlayerLives : MonoBehaviour
    {
        [SerializeField] int startingLives = 3;
        [SerializeField] float invulnerabilitySeconds = 0.6f;

        float invulnerableUntil;

        public int Current { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        void Awake()
        {
            Current = startingLives;
        }

        public void ResetLives()
        {
            Current = startingLives;
            invulnerableUntil = 0f;
        }

        // amount es el daño del enemigo (1 o 2), no el de un arma.
        public void Damage(int amount)
        {
            if (Current <= 0)
                return;

            if (amount <= 0)
                return;

            if (Time.time < invulnerableUntil)
                return;

            Current -= amount;
            if (Current < 0)
                Current = 0;

            invulnerableUntil = Time.time + invulnerabilitySeconds;

            if (Current == 0 && GameManager.Instance != null)
                GameManager.Instance.EnterGameOver();
        }
    }
}
