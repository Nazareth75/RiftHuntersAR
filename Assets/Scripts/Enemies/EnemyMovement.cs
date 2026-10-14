using UnityEngine;
using UnityEngine.XR.ARFoundation;
using RiftHuntersAR.Game;
namespace RiftHuntersAR.Enemies
{
    public sealed class EnemyMovement : MonoBehaviour
    {
        [SerializeField, Min(0)] private float speed = .25f;
        [SerializeField, Min(.1f)] private float attackDistance = .65f;
        [SerializeField, Min(0)] private float damage = 34;
        [SerializeField, Min(.1f)] private float attackCooldown = 2;
        private Transform target;
        private PlayerHealth player;
        private float nextAttack;
        public bool Running { get; set; }
        public void Initialize(Transform camera, PlayerHealth health) { target = camera; player = health; Running = true; nextAttack = Time.time + attackCooldown; }
        private void Update()
        {
            Advance(ARSession.state == ARSessionState.SessionTracking, Time.time, Time.deltaTime);
        }
        internal void Advance(bool tracking, float now, float deltaTime)
        {
            if (!Running || target == null || player == null || player.IsDead || !tracking) return;
            Vector3 delta = target.position - transform.position; delta.y = 0;
            float distance = delta.magnitude;
            if (distance > attackDistance + .01f)
            {
                transform.position += delta.normalized * Mathf.Min(speed * deltaTime, distance - attackDistance);
                if (delta.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(delta);
            }
            else if (now >= nextAttack)
            {
                nextAttack = now + attackCooldown;
                player.TakeDamage(damage);
            }
        }
    }
}
