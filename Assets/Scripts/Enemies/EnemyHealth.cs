using System;
using UnityEngine;

namespace RiftHuntersAR.Enemies
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private float maxHealth = 100;
        [SerializeField] private float currentHealth;
        public float CurrentHealth => currentHealth;
        public event Action<EnemyHealth> Died;
        private bool dead;
        private void Awake() { currentHealth = maxHealth; }
        public void TakeDamage(float damage)
        {
            if (dead || damage <= 0 || float.IsNaN(damage)) return;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            Debug.Log($"[Rift] Hit: {currentHealth}/{maxHealth} HP", this);
            if (currentHealth <= 0) Die();
        }
        public void Die()
        {
            if (dead) return;
            dead = true;
            Died?.Invoke(this);
            gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(gameObject);
        }
    }
}
