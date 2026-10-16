using System.Collections.Generic;
using UnityEngine;
using RiftHuntersAR.AR;

namespace RiftHuntersAR.Recycling
{
    // Tres contenedores sobre el suelo del portal, confirmados una vez por el jugador.
    public sealed class ColocacionContenedores : MonoBehaviour
    {
        readonly float[] escalas = { .4f, .3f, .24f };
        readonly Vector3[] posiciones = new Vector3[3];
        readonly List<RecycleStation> estaciones = new();
        Camera camara;
        PortalSpawnPoint portal;
        SueloCombateAR suelo;
        RecyclingVisualCatalog catalogo;
        float siguienteConsulta;
        public Transform Raiz { get; private set; }
        public IReadOnlyList<RecycleStation> Estaciones => estaciones;
        public bool Guardado { get; private set; }
        public bool Compacto { get; private set; }
        public bool Buscando { get; private set; }
        public bool PuedeColocar { get; private set; }
        public string Mensaje { get; private set; } = "";

        public void Preparar(PortalSpawnPoint origen, Camera vista, RecyclingVisualCatalog recursos)
        {
            if (origen == null || vista == null || recursos == null) return;
            if (portal != origen && (Raiz != null || Guardado || Compacto))
            {
                if (Raiz != null) Destroy(Raiz.gameObject);
                Raiz = null; estaciones.Clear(); Guardado = Compacto = false;
            }
            portal = origen; camara = vista; catalogo = recursos;
            suelo = portal.GetComponentInParent<SueloCombateAR>();
            if (Guardado && Raiz != null) { Raiz.gameObject.SetActive(true); Buscando = false; return; }
            if (Compacto) return;
            Reubicar();
        }
        public void Reubicar()
        {
            Guardado = Compacto = PuedeColocar = false; Buscando = true; siguienteConsulta = 0;
            if (Raiz != null) Raiz.gameObject.SetActive(false);
            Mensaje = "APUNTA AL SUELO PARA COLOCAR LOS TRES CONTENEDORES";
        }
        public void Ocultar()
        {
            Buscando = PuedeColocar = false;
            if (Raiz != null) Raiz.gameObject.SetActive(false);
        }
        public void UsarBotones()
        {
            Ocultar(); Guardado = false; Compacto = true;
        }
        public bool Confirmar()
        {
            if (!Buscando || !PuedeColocar || Raiz == null) return false;
            Guardado = true; Buscando = false;
            foreach (var estacion in estaciones)
            {
                foreach (var colision in estacion.GetComponentsInChildren<Collider>()) colision.enabled = true;
                foreach (var dibujo in estacion.GetComponentsInChildren<Renderer>()) dibujo.SetPropertyBlock(null);
            }
            return true;
        }
        public void Actualizar(bool seguimiento)
        {
            if (!Buscando) return;
            if (!seguimiento)
            {
                PuedeColocar = false;
                if (Raiz != null) Raiz.gameObject.SetActive(false);
                return;
            }
            if (Time.unscaledTime < siguienteConsulta) return;
            siguienteConsulta = Time.unscaledTime + .1f;
            PuedeColocar = false;
            if (Raiz != null) Raiz.gameObject.SetActive(false);
            if (portal == null || camara == null) return;
            Vector3 arriba = suelo != null ? suelo.transform.up : portal.transform.up;
            var plano = new Plane(arriba, suelo != null ? suelo.transform.position : portal.transform.position);
            Ray rayo = camara.ViewportPointToRay(new Vector3(.5f, .5f));
            Mensaje = "APUNTA AL SUELO RECONOCIDO, DELANTE O A UN LADO DEL PORTAL";
            if (!plano.Raycast(rayo, out float distancia) || distancia < .55f || distancia > 3f) return;
            Vector3 centro = rayo.GetPoint(distancia);
            Vector3 frente = Vector3.ProjectOnPlane(camara.transform.position - centro, arriba).normalized;
            if (frente.sqrMagnitude < .01f) return;
            Vector3 lado = Vector3.Cross(frente, arriba).normalized;
            foreach (float escala in escalas)
            {
                if (!ValidarFila(centro, frente, lado, arriba, escala)) continue;
                if (!Crear()) { Mensaje = "NO SE ENCUENTRAN LOS PREFABS DE CONTENEDORES"; return; }
                Raiz.gameObject.SetActive(true);
                var bloque = new MaterialPropertyBlock(); bloque.SetColor("_BaseColor", new Color(.3f, 1f, .7f));
                for (int i = 0; i < 3; i++)
                {
                    var estacion = estaciones[i];
                    estacion.transform.SetPositionAndRotation(posiciones[i], Quaternion.LookRotation(frente, arriba));
                    estacion.transform.localScale = catalogo.Station(i).transform.localScale * escala;
                    foreach (var colision in estacion.GetComponentsInChildren<Collider>()) colision.enabled = false;
                    foreach (var dibujo in estacion.GetComponentsInChildren<Renderer>()) dibujo.SetPropertyBlock(bloque);
                }
                PuedeColocar = true; Mensaje = "TOCA PARA FIJAR LOS CONTENEDORES AQUÍ\nESTA POSICIÓN SE CONSERVARÁ"; return;
            }
            Mensaje = "AQUÍ NO CABEN: APUNTA A OTRO PUNTO DEL SUELO\nO ELIGE USAR BOTONES";
        }
        bool ValidarFila(Vector3 centro, Vector3 frente, Vector3 lado, Vector3 arriba, float escala)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 punto = centro + lado * ((i - 1) * (.65f * escala + .045f));
                posiciones[i] = punto;
                Vector3 vista = camara.WorldToViewportPoint(punto + arriba * (.4f * escala));
                if (vista.z < .45f || vista.x < .06f || vista.x > .94f || vista.y < .08f || vista.y > .9f) return false;
                if (Vector3.ProjectOnPlane(punto - portal.transform.position, arriba).magnitude < .45f
                    || Vector3.ProjectOnPlane(punto - portal.SpawnPoint.position, arriba).magnitude < .3f) return false;
                // Evitar colocar la fila detrás del portal visto desde el jugador.
                if (Vector3.Dot(punto - portal.transform.position, camara.transform.forward) > -.15f) return false;
                for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    if (suelo != null && !suelo.Contiene(punto + lado * (x * (.238f * escala + .02f))
                        + frente * (z * (.22f * escala + .02f)))) return false;
            }
            return true;
        }
        bool Crear()
        {
            if (Raiz != null && estaciones.Count == 3) return true;
            for (int i = 0; i < 3; i++)
                if (catalogo.Station(i) == null || catalogo.Station(i).GetComponent<RecycleStation>() == null) return false;
            estaciones.Clear();
            Raiz = new GameObject("Contenedores - posición guardada").transform;
            Raiz.SetParent(portal.transform.parent, false);
            for (int i = 0; i < 3; i++) estaciones.Add(Instantiate(catalogo.Station(i), Raiz).GetComponent<RecycleStation>());
            return true;
        }
    }
}
