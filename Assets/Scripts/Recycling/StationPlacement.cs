using UnityEngine;
using RiftHuntersAR.AR;

namespace RiftHuntersAR.Recycling
{
    public static class StationPlacement
    {
        private static readonly float[] tamanos = { .55f, .4f, .3f, .24f };
        private static readonly float[] distancias = { .3f, .5f, .75f, 1f, 1.3f };

        // Buscar una distribución completa: nunca amontonar centros en el borde.
        public static bool TryFind(SueloCombateAR floor, Vector3 portal, Vector3 spawn, Camera camera,
            out Vector3[] positions, out Quaternion rotation, out float scale)
        {
            positions = null; rotation = Quaternion.identity; scale = 1f;
            if (camera == null) return false;
            Vector3 up = floor != null ? floor.transform.up : Vector3.up;
            Vector3 haciaJugador = Vector3.ProjectOnPlane(camera.transform.position - portal, up).normalized;
            if (haciaJugador.sqrMagnitude < .1f) haciaJugador = -Vector3.ProjectOnPlane(camera.transform.forward, up).normalized;
            if (haciaJugador.sqrMagnitude < .1f) haciaJugador = Vector3.ProjectOnPlane(camera.transform.up, up).normalized;
            Vector3 derecha = Vector3.Cross(up, haciaJugador).normalized;
            var residuos = Object.FindObjectsByType<RecyclableItem>(FindObjectsSortMode.None);
            var candidato = new Vector3[3];
            foreach (float tamano in tamanos)
            for (int distribucion = 0; distribucion < 2; distribucion++)
            foreach (float distancia in distancias)
            for (int paso = 0; paso < 8; paso++)
            {
                float angulo = paso * Mathf.PI / 4f;
                Vector3 centro = portal + (haciaJugador * Mathf.Cos(angulo) + derecha * Mathf.Sin(angulo)) * distancia;
                if (floor != null) centro = floor.Apoyar(centro);
                Vector3 frente = Vector3.ProjectOnPlane(camera.transform.position - centro, up).normalized;
                if (frente.sqrMagnitude < .1f) continue;
                Vector3 lado = Vector3.Cross(frente, up).normalized;
                float separacion = .65f * tamano + .045f;
                bool valido = true;
                for (int i = 0; i < 3 && valido; i++)
                {
                    Vector2 desplazamiento = distribucion == 0 ? new Vector2(i - 1, 0)
                        : i == 0 ? new Vector2(-.5f, -.289f) : i == 1 ? new Vector2(.5f, -.289f) : new Vector2(0, .577f);
                    Vector3 punto = centro + lado * (desplazamiento.x * separacion) + frente * (desplazamiento.y * separacion);
                    candidato[i] = punto;
                    Vector3 vista = camera.WorldToViewportPoint(punto + up * (.4f * tamano));
                    if (vista.z <= .1f || Vector3.ProjectOnPlane(camera.transform.position - punto, up).magnitude < .45f
                        || Vector3.ProjectOnPlane(portal - punto, up).magnitude < .24f
                        || Vector3.ProjectOnPlane(spawn - punto, up).magnitude < .2f) { valido = false; break; }
                    // Las cuatro esquinas y un margen deben caber en la superficie guardada.
                    for (int x = -1; x <= 1; x += 2)
                    for (int z = -1; z <= 1; z += 2)
                        if (floor != null && !floor.Contiene(punto + lado * (x * (.238f * tamano + .02f))
                            + frente * (z * (.22f * tamano + .02f)))) valido = false;
                    var volumen = new Bounds(punto + up * (.41f * tamano), new Vector3(.68f, .82f, .68f) * tamano);
                    foreach (var residuo in residuos)
                    {
                        if (!valido) break;
                        if (!residuo.Classifiable) continue;
                        var collider = residuo.GetComponent<BoxCollider>();
                        if (collider == null) continue;
                        Vector3 destino = collider.bounds.center;
                        var rayo = new Ray(camera.transform.position, destino - camera.transform.position);
                        if (volumen.IntersectRay(rayo, out float impacto) && impacto < Vector3.Distance(camera.transform.position, destino)) valido = false;
                    }
                }
                if (!valido) continue;
                positions = candidato; rotation = Quaternion.LookRotation(frente, up); scale = tamano; return true;
            }
            // La interfaz ofrece clasificación compacta si no caben tres objetos físicos.
            return false;
        }
    }
}
