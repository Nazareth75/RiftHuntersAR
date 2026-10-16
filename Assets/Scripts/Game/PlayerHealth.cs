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
        public bool Curar(float cantidad)
        {
            if (IsDead || cantidad <= 0 || float.IsNaN(cantidad) || CurrentHealth >= 100) return false;
            CurrentHealth = Mathf.Min(100, CurrentHealth + cantidad); Changed?.Invoke(); return true;
        }
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
