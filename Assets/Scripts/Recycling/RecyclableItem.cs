using UnityEngine;

namespace RiftHuntersAR.Recycling
{
    public enum RecyclingCategory { Plastic, Paper, Metal, Other }

    // Category data only. Collection, scoring and classification are deliberately separate.
    public sealed class RecyclableItem : MonoBehaviour
    {
        [SerializeField] RecyclingCategory category;
        public RecyclingCategory Category => category;
        public bool Collected { get; private set; }
        public bool IsSettled => GetComponent<ResidueLanding>() == null;
        public bool Classifiable => category != RecyclingCategory.Other && !Collected;
        public void MarkCollected() { Collected = true; SetSelected(false); }
        public void SetSelected(bool selected)
        {
            // Per-renderer tint; shared approved materials stay untouched.
            var block = new MaterialPropertyBlock();
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
                for (int i=0;i<renderer.sharedMaterials.Length;i++)
                {
                    renderer.GetPropertyBlock(block,i);
                    var material=renderer.sharedMaterials[i];
                    block.SetColor("_BaseColor", selected ? Color.Lerp(material.GetColor("_BaseColor"), new Color(.2f,1,.7f),.65f) : material.GetColor("_BaseColor"));
                    renderer.SetPropertyBlock(block,i);
                }
        }
        public void Configure(RecyclingCategory value) => category = value;
    }
}
