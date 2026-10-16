using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace RiftHuntersAR.AR
{
    // Suelo horizontal unido al ancla. El contorno solo limita la aparición.
    // No representa paredes, muebles ni escalones.
    public sealed class SueloCombateAR : MonoBehaviour
    {
        private Vector2[] contorno;
        public void Configurar(ARPlane plano)
        {
            contorno = new Vector2[plano.boundary.Length];
            for (int i = 0; i < contorno.Length; i++)
            {
                Vector2 punto = plano.boundary[i];
                Vector3 local = transform.InverseTransformPoint(plano.transform.TransformPoint(new Vector3(punto.x, 0, punto.y)));
                contorno[i] = new Vector2(local.x, local.z);
            }
        }
        public Vector3 Apoyar(Vector3 posicion)
        {
            Vector3 local = transform.InverseTransformPoint(posicion);
            local.y = 0f;
            return transform.TransformPoint(local);
        }
        public Vector3 PosicionAparicion(Vector3 posicion)
        {
            Vector3 local = transform.InverseTransformPoint(posicion);
            Vector2 punto = new(local.x, local.z);
            if (contorno == null || contorno.Length < 3) return Apoyar(posicion);
            bool dentro = false;
            float menor = float.PositiveInfinity;
            Vector2 cercano = punto;
            for (int i = 0, j = contorno.Length - 1; i < contorno.Length; j = i++)
            {
                Vector2 a = contorno[j], b = contorno[i];
                if ((a.y > punto.y) != (b.y > punto.y)
                    && punto.x < (b.x - a.x) * (punto.y - a.y) / (b.y - a.y) + a.x) dentro = !dentro;
                Vector2 lado = b - a;
                Vector2 candidato = a + lado * Mathf.Clamp01(Vector2.Dot(punto - a, lado) / Mathf.Max(.000001f, lado.sqrMagnitude));
                float distancia = (candidato - punto).sqrMagnitude;
                if (distancia < menor) { menor = distancia; cercano = candidato; }
            }
            if (!dentro) punto = Vector2.MoveTowards(cercano, Vector2.zero, .1f);
            return transform.TransformPoint(new Vector3(punto.x, 0f, punto.y));
        }
    }
}
