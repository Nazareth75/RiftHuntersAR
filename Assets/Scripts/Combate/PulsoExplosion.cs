using UnityEngine;

namespace RiftHunters.Combate
{
    // Visual provisional sin colisionador: no interviene en el raycast ni en el dano.
    public class PulsoExplosion : MonoBehaviour
    {
        private const float Duracion = 0.35f;
        private const int Segmentos = 48;
        private float tiempoTranscurrido;
        private float radio;
        private readonly LineRenderer[] anillos = new LineRenderer[3];

        public void Inicializar(float radioExplosion, Material material)
        {
            radio = radioExplosion;
            for (int i = 0; i < anillos.Length; i++)
            {
                var anillo = new GameObject("Anillo de explosion");
                anillo.transform.SetParent(transform, false);
                LineRenderer linea = anillo.AddComponent<LineRenderer>();
                linea.sharedMaterial = material;
                linea.useWorldSpace = false;
                linea.loop = true;
                linea.positionCount = Segmentos;
                for (int p = 0; p < Segmentos; p++)
                {
                    float angulo = p * Mathf.PI * 2 / Segmentos;
                    float x = Mathf.Cos(angulo);
                    float y = Mathf.Sin(angulo);
                    linea.SetPosition(p, i == 0 ? new Vector3(x, y, 0)
                        : i == 1 ? new Vector3(x, 0, y) : new Vector3(0, x, y));
                }
                anillos[i] = linea;
            }
            transform.localScale = Vector3.one * 0.05f;
        }

        private void Update()
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempoTranscurrido / Duracion);
            transform.localScale = Vector3.one * Mathf.Lerp(0.05f, radio, progreso);
            foreach (LineRenderer linea in anillos)
                if (linea != null) linea.startWidth = linea.endWidth = Mathf.Lerp(0.06f, 0.005f, progreso);
            if (tiempoTranscurrido >= Duracion) Destroy(gameObject);
        }
    }
}
