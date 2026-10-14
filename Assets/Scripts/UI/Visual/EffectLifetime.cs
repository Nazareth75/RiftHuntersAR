using UnityEngine;
namespace RiftHuntersAR.UI.Visual
{
    public sealed class EffectLifetime : MonoBehaviour
    {
        public float seconds=1;
        private void OnEnable() { if(Application.isPlaying) Destroy(gameObject,seconds); }
    }
}
