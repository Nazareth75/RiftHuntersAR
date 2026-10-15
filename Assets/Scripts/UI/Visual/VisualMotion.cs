using UnityEngine;
namespace RiftHuntersAR.UI.Visual
{
    public sealed class VisualMotion : MonoBehaviour
    {
        public Transform[] fragments = System.Array.Empty<Transform>();
        public Renderer[] energy = System.Array.Empty<Renderer>();
        public bool faceCamera;
        public bool animarFragmentos = true;
        private Vector3[] posiciones;
        private Quaternion[] rotaciones;
        private Material[][] materiales;
        private MaterialPropertyBlock bloque;
        private Camera camara;
        private float siguientePulso;
        private static readonly int Emision = Shader.PropertyToID("_EmissionColor");
        private void Start()
        {
            camara = Camera.main;
            posiciones = new Vector3[fragments.Length]; rotaciones = new Quaternion[fragments.Length];
            for (int i=0;i<fragments.Length;i++)
                if (fragments[i] != null) { posiciones[i]=fragments[i].localPosition; rotaciones[i]=fragments[i].localRotation; }
            materiales = new Material[energy.Length][];
            for (int i=0;i<energy.Length;i++) materiales[i]=energy[i] != null ? energy[i].sharedMaterials : System.Array.Empty<Material>();
            bloque = new MaterialPropertyBlock();
        }
        private void LateUpdate()
        {
            if (faceCamera && camara != null)
            {
                Vector3 direccion=camara.transform.position-transform.position; direccion.y=0;
                if (direccion.sqrMagnitude>.001f)
                    transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(direccion),180f*Time.deltaTime);
            }
            if (Time.unscaledTime < siguientePulso) return;
            siguientePulso = Time.unscaledTime + .1f;
            if (animarFragmentos)
                for (int i=0;i<fragments.Length;i++)
                {
                    if (fragments[i] == null) continue;
                    fragments[i].localPosition=posiciones[i]+Vector3.up*Mathf.Sin(Time.time*.7f+i)*.012f;
                    fragments[i].localRotation=rotaciones[i]*Quaternion.Euler(0,Mathf.Sin(Time.time*.3f+i)*4,0);
                }
            float pulso=1+.12f*Mathf.Sin(Time.time*1.4f);
            for (int i=0;i<energy.Length;i++)
                for (int j=0;j<materiales[i].Length;j++)
                {
                    Material material=materiales[i][j];
                    if (energy[i] == null || material == null || !material.HasProperty(Emision)) continue;
                    bloque.Clear(); bloque.SetColor(Emision,material.GetColor(Emision)*pulso);
                    energy[i].SetPropertyBlock(bloque,j);
                }
        }
    }
}
