using UnityEngine;
namespace RiftHuntersAR.UI.Visual
{
    public sealed class VisualMotion : MonoBehaviour
    {
        public Transform[] fragments;
        public Renderer[] energy;
        public bool faceCamera;
        private Vector3[] positions;
        private Quaternion[] rotations;
        private MaterialPropertyBlock block;
        private void Start()
        {
            positions = new Vector3[fragments.Length]; rotations = new Quaternion[fragments.Length];
            for(int i=0;i<fragments.Length;i++) { positions[i]=fragments[i].localPosition; rotations[i]=fragments[i].localRotation; }
            block = new MaterialPropertyBlock();
        }
        private void LateUpdate()
        {
            if(faceCamera && Camera.main != null)
            {
                Vector3 direction = Camera.main.transform.position-transform.position; direction.y=0;
                if(direction.sqrMagnitude>.001f) transform.rotation=Quaternion.LookRotation(direction);
            }
            for(int i=0;i<fragments.Length;i++)
            {
                fragments[i].localPosition=positions[i]+Vector3.up*Mathf.Sin(Time.time*.7f+i)*.012f;
                fragments[i].localRotation=rotations[i]*Quaternion.Euler(0,Mathf.Sin(Time.time*.3f+i)*4,0);
            }
            foreach(var renderer in energy)
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    if(!materials[i].HasProperty("_EmissionColor")) continue;
                    block.Clear(); block.SetColor("_EmissionColor",materials[i].GetColor("_EmissionColor")*(1+.12f*Mathf.Sin(Time.time*1.4f)));
                    renderer.SetPropertyBlock(block,i);
                }
            }
        }
    }
}
