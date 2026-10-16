using UnityEngine;
namespace RiftHuntersAR.Recycling
{
    public sealed class RecycleStation : MonoBehaviour
    {
        [SerializeField] RecyclingCategory category;
        public RecyclingCategory Category => category;
        public void Configure(RecyclingCategory value) => category = value;
    }
}
