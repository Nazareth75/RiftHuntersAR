using UnityEngine;
namespace RiftHuntersAR.Recycling
{
    public sealed class RecyclingCollection : MonoBehaviour
    {
        Vector3 start,end,scale;
        float elapsed;
        GameObject effect;
        public void Begin(Vector3 destination,GameObject positiveEffect)
        {
            start=transform.localPosition;end=transform.parent.InverseTransformPoint(destination);scale=transform.localScale;effect=positiveEffect;
            foreach(var collider in GetComponentsInChildren<Collider>())collider.enabled=false;
        }
        void Update()
        {
            elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed/.38f);
            transform.localPosition=Vector3.Lerp(start,end,t)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.10f);
            transform.localScale=scale*(1-t*.95f);
            if(t<1)return;
            if(effect!=null)
            {
                var fx=Instantiate(effect,transform.position,Quaternion.identity,transform.parent);
                var ps=fx.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=ps.main;main.startColor=new Color(.1f,1,.5f,.7f);main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
                var color=ps.colorOverLifetime;color.enabled=false;ps.Play();
            }
            Destroy(gameObject);
        }
    }
}
