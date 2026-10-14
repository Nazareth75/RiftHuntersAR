using System;
using UnityEngine;
namespace RiftHuntersAR.Game
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        public float CurrentHealth { get; private set; } = 100;
        public bool IsDead => CurrentHealth <= 0;
        public event Action Changed;
        public event Action Died;
        public void ResetHealth() { CurrentHealth = 100; Changed?.Invoke(); }
        public void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0 || float.IsNaN(damage)) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            Changed?.Invoke();
            if (IsDead) Die();
        }
        public void Die()
        {
            if (CurrentHealth > 0) { CurrentHealth = 0; Changed?.Invoke(); }
            if (deathReported) return;
            deathReported = true;
            Died?.Invoke();
        }
        private bool deathReported;
        public void Retry() { deathReported = false; ResetHealth(); }
    }
}
