using UnityEngine;
using RiftHuntersAR.Enemies;
namespace RiftHuntersAR.UI.Visual
{
    public sealed class EnemyVisualEffects : MonoBehaviour
    {
        public EnemyHealth health;
        public Transform aimPoint;
        public GameObject damage, death, plasmaHit;
        private float previousHP;
        private void OnEnable() { if(health!=null) { previousHP=health.CurrentHealth; health.Died+=Death; } }
        private void OnDisable() { if(health!=null) health.Died-=Death; }
        private Vector3 Position => aimPoint == null ? transform.position+Vector3.up*.3f : aimPoint.position;
        private void LateUpdate()
        {
            if(health==null) return;
            if(health.CurrentHealth<previousHP && health.CurrentHealth>0 && damage!=null) Instantiate(damage,Position,Quaternion.identity);
            previousHP=health.CurrentHealth;
        }
        private void Death(EnemyHealth enemy)
        {
            if(death!=null) Instantiate(death,Position,Quaternion.identity);
            if(plasmaHit!=null) Instantiate(plasmaHit,Position,Quaternion.identity);
        }
    }
}
