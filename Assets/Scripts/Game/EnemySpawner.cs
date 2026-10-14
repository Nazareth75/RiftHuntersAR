using System.Collections.Generic;
using UnityEngine;
using RiftHuntersAR.AR;
using RiftHuntersAR.Enemies;
namespace RiftHuntersAR.Game
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        private readonly HashSet<EnemyHealth> alive = new();
        public int AliveCount => alive.Count;
        public event System.Action EnemyKilled;
        public void Spawn(EnemyHealth prefab, PortalSpawnPoint portal, Transform camera, PlayerHealth player)
        {
            var enemy = Instantiate(prefab, portal.SpawnPoint.position, portal.SpawnPoint.rotation, portal.transform.parent);
            alive.Add(enemy);
            enemy.Died += OnDied;
            var movement = enemy.GetComponent<EnemyMovement>();
            if (movement == null) movement = enemy.gameObject.AddComponent<EnemyMovement>();
            movement.Initialize(camera, player);
        }
        private void OnDied(EnemyHealth enemy) { enemy.Died -= OnDied; if (alive.Remove(enemy)) EnemyKilled?.Invoke(); }
        public void Clear()
        {
            foreach (var enemy in alive) if (enemy != null) { enemy.Died -= OnDied; enemy.gameObject.SetActive(false); Destroy(enemy.gameObject); }
            alive.Clear();
        }
        public void StopMovement() { foreach (var enemy in alive) if (enemy != null) { var movement = enemy.GetComponent<EnemyMovement>(); if (movement != null) movement.Running = false; } }
    }
}
