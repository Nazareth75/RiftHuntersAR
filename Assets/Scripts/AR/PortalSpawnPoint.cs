using UnityEngine;
namespace RiftHuntersAR.AR
{
    public sealed class PortalSpawnPoint : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        public Transform SpawnPoint => spawnPoint;
        private void Awake()
        {
            if (!RiftHunters.Combate.ModeloDeIntegracion.MedirVisible(gameObject, transform, out Bounds limites)
                || limites.size.y < .001f) return;
            transform.localScale *= .65f / limites.size.y;
            RiftHunters.Combate.ModeloDeIntegracion.MedirVisible(gameObject, transform, out limites);
            transform.position -= transform.up * (limites.min.y * transform.lossyScale.y);
        }
    }
}
