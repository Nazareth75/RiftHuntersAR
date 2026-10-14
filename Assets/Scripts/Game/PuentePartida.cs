using System;
using UnityEngine;
using RiftHuntersAR.Game;

namespace RiftHunters.Game
{
    public sealed class PuentePuntuacion : MonoBehaviour, IPuntuacion
    {
        ScoreManager fuente;
        public int Score => fuente != null ? fuente.Score : 0;
        public void Unir(ScoreManager valor) => fuente = valor;
        public void AgregarPuntos(int cantidad) { if (fuente != null) fuente.AgregarPuntos(cantidad); }
        public void AddEnemyScore() { if (fuente != null) fuente.AddEnemyScore(); }
        public void ResetScore() { if (fuente != null) fuente.ResetScore(); }
    }

    public sealed class PuenteVida : MonoBehaviour, IVidaJugador
    {
        PlayerHealth fuente;
        public float CurrentHealth => fuente != null ? fuente.CurrentHealth : 0f;
        public bool IsDead => fuente != null && fuente.IsDead;
        public event Action Changed;
        public event Action Died;

        public void Unir(PlayerHealth valor)
        {
            if (fuente != null)
            {
                fuente.Changed -= AvisarCambio;
                fuente.Died -= AvisarMuerte;
            }
            fuente = valor;
            if (fuente == null) return;
            fuente.Changed += AvisarCambio;
            fuente.Died += AvisarMuerte;
        }

        void OnDestroy() => Unir(null);
        void AvisarCambio() => Changed?.Invoke();
        void AvisarMuerte() => Died?.Invoke();
        public void TakeDamage(float damage) { if (fuente != null) fuente.TakeDamage(damage); }
        public void ResetHealth() { if (fuente != null) fuente.ResetHealth(); }
        public void Retry() { if (fuente != null) fuente.Retry(); }
    }
}
