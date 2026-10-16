using UnityEngine;
namespace RiftHuntersAR.Recycling
{
    // Scripted short arc: no Rigidbody, forces, collision response or shooting targets.
    public sealed class ResidueLanding : MonoBehaviour
    {
        Vector3 start, end;
        Quaternion startRotation, endRotation;
        float elapsed;
        GameObject impactVFX;
        public void SetImpact(GameObject prefab) => impactVFX=prefab;
        public void Begin(Vector3 from, Vector3 to, Quaternion settledRotation)
        {
            start=from; end=to; startRotation=transform.localRotation; endRotation=settledRotation;
            transform.localPosition=start;
        }
        void Update()
        {
            elapsed += Time.deltaTime;
            float t=Mathf.Clamp01(elapsed/.42f);
            transform.localPosition=Vector3.Lerp(start,end,t)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.045f);
            transform.localRotation=Quaternion.Slerp(startRotation,endRotation,t);
            if(t>=1)
            {
                if(impactVFX!=null) Instantiate(impactVFX,transform.position,Quaternion.identity,transform.parent);
                Destroy(this);
            }
        }
    }
}
