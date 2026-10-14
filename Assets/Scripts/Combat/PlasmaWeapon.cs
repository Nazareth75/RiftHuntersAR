using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using RiftHuntersAR.Enemies;
namespace RiftHuntersAR.Combat
{
    public sealed class PlasmaWeapon : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float damage = 25;
        [SerializeField] private float range = 20;
        [SerializeField] private int magazineSize = 30;
        public Camera AimCamera => aimCamera;
        public int CurrentAmmo { get; private set; } = 30;
        public int MagazineSize => magazineSize;
        public bool Ready { get; set; }
        public event Action<string> Feedback;
        private float nextShot;
        private int lastShotFrame = -1;
        public void Reload() { CurrentAmmo = magazineSize; Feedback?.Invoke("RELOADED"); }
        public void Shoot()
        {
            Fire(ARSession.state == ARSessionState.SessionTracking, Time.unscaledTime, Time.frameCount);
        }
        internal void Fire(bool tracking, float now, int frame)
        {
            if (!Ready || aimCamera == null || !tracking) return;
            if (lastShotFrame == frame || now < nextShot) return;
            if (CurrentAmmo <= 0) { Feedback?.Invoke("EMPTY - RELOAD"); return; }
            lastShotFrame = frame;
            nextShot = now + .2f;
            CurrentAmmo--;
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Feedback?.Invoke(enemy.CurrentHealth <= 0 ? "ENEMY DESTROYED +100" : $"HIT -{enemy.CurrentHealth:0} HP");
                    Debug.Log($"[Rift Shot] frame={Time.frameCount} target={enemy.GetInstanceID()} damage={damage} hp={enemy.CurrentHealth} ammo={CurrentAmmo}");
                    return;
                }
            }
            Feedback?.Invoke("MISS");
        }
    }
}
