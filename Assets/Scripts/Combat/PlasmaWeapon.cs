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
        public bool Ready { get; set; }
        public event Action<string> Feedback;
        public void Shoot()
        {
            if (!Ready || aimCamera == null || ARSession.state != ARSessionState.SessionTracking) return;
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Feedback?.Invoke($"HIT: {enemy.CurrentHealth:0} HP");
                    return;
                }
            }
            Feedback?.Invoke("MISS");
        }
    }
}
