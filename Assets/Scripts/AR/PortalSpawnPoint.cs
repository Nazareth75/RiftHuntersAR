using UnityEngine;
namespace RiftHuntersAR.AR
{
    public sealed class PortalSpawnPoint : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        public Transform SpawnPoint => spawnPoint;
    }
}
