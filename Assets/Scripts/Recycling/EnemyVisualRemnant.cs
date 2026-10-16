using UnityEngine;
namespace RiftHuntersAR.Recycling
{
    // A brief visual-only tail after the functional enemy has already been removed.
    public sealed class EnemyVisualRemnant : MonoBehaviour
    {
        Quaternion rotation;
        Vector3 position, scale;
        float time;
        bool death;
        public void Begin(bool died)
        {
            death=died;rotation=transform.localRotation;position=transform.localPosition;scale=transform.localScale;
        }
        void Update()
        {
            time+=Time.deltaTime;float t=Mathf.Clamp01(time/(death?.3f:.16f));
            transform.localRotation=rotation*Quaternion.Euler(death?70*t:Mathf.Sin(t*Mathf.PI)*-12,0,death?25*t:0);
            transform.localScale=scale*(death?Mathf.Lerp(1,.08f,t):1-t*.15f);
            transform.localPosition=position+Vector3.up*(death?-.03f*t:0);
            if(t>=1) Destroy(gameObject);
        }
    }
}
