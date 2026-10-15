using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace RiftHuntersAR.AR
{
    // Valida un punto bajo la mira; no exige escanear toda la habitación.
    public sealed class ColocacionPortalAR : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float distanciaMinima = 1.1f;
        [SerializeField, Min(1f)] private float distanciaMaxima = 4f;
        [SerializeField, Min(0.1f)] private float radioPortal = 0.18f;
        private readonly List<ARRaycastHit> impactos = new();
        private ARPlaneManager planos;
        private ARRaycastManager rayos;
        private Camera camara;
        private LineRenderer indicador;
        private Material material;
        private Vector3 anterior;
        private TrackableId planoAnterior;
        private float estable;
        private float siguienteConsulta;
        private float ultimaConsulta;
        private readonly Vector3[] puntos = new Vector3[24];
        public ARPlane Plano { get; private set; }
        public Pose Pose { get; private set; }
        public bool PuedeColocar { get; private set; }
        public string Mensaje { get; private set; }

        public void Configurar(ARPlaneManager planosAR, ARRaycastManager rayosAR, Camera camaraAR)
        {
            planos = planosAR; rayos = rayosAR; camara = camaraAR;
            if (planos != null) planos.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            var gestorCamara = camara.GetComponent<ARCameraManager>();
            if (gestorCamara != null) gestorCamara.autoFocusRequested = true;
            var objeto = new GameObject("Indicador de colocación");
            objeto.transform.SetParent(transform, false);
            indicador = objeto.AddComponent<LineRenderer>();
            indicador.loop = true; indicador.useWorldSpace = true;
            indicador.positionCount = puntos.Length; indicador.widthMultiplier = 0.008f;
            indicador.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            indicador.receiveShadows = false;
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            indicador.sharedMaterial = material;
            indicador.enabled = false;
        }

        public void Actualizar(bool seguimiento)
        {
            if (!seguimiento) { Ocultar(); return; }
            if (Time.unscaledTime < siguienteConsulta) return;
            float delta = Mathf.Min(.15f, Time.unscaledTime - ultimaConsulta);
            ultimaConsulta = Time.unscaledTime;
            siguienteConsulta = Time.unscaledTime + .1f;
            PuedeColocar = false;
            Plano = null;
            Mensaje = "Apunta al suelo y mueve lentamente el teléfono.";
            if (!seguimiento || planos == null || rayos == null
                || !rayos.Raycast(new Vector2(camara.pixelWidth * .5f, camara.pixelHeight * .5f),
                    impactos, TrackableType.PlaneWithinPolygon))
            { Ocultar(); return; }
            ARPlane plano = planos.GetPlane(impactos[0].trackableId);
            if (plano == null || plano.trackingState != TrackingState.Tracking
                || plano.alignment != PlaneAlignment.HorizontalUp)
            { Ocultar(); return; }
            Plano = plano;
            Pose = impactos[0].pose;
            Vector3 diferencia = camara.transform.position - Pose.position;
            float distancia = new Vector2(diferencia.x, diferencia.z).magnitude;
            bool valido = false;
            if (distancia < distanciaMinima) Mensaje = "Apunta un poco más lejos (mínimo 1,1 m).";
            else if (distancia > distanciaMaxima) Mensaje = "Demasiado lejos: apunta a una zona más cercana.";
            else if (diferencia.y < .65f) Mensaje = "Baja la mira hacia el piso; evita mesas y asientos.";
            else if (HayPlanoInferior(plano)) Mensaje = "Hay una superficie más baja: apunta al piso.";
            else if (!CabePortal(plano, Pose.position)) Mensaje = "Busca una zona más amplia o aléjate del borde.";
            else valido = true;
            if (!valido || plano.trackableId != planoAnterior || Vector3.Distance(anterior, Pose.position) > .12f)
                estable = 0f;
            else estable += delta;
            anterior = Pose.position; planoAnterior = plano.trackableId;
            PuedeColocar = valido && estable >= .2f;
            if (valido) Mensaje = PuedeColocar ? "Toca para colocar el portal." : "Mantén la mira aquí un instante.";
            indicador.enabled = true;
            material.SetColor("_BaseColor", PuedeColocar ? Color.green : new Color(1f, .65f, .1f));
            for (int i = 0; i < indicador.positionCount; i++)
            {
                float angulo = i * Mathf.PI * 2f / indicador.positionCount;
                puntos[i] = Pose.position + plano.transform.up * .015f
                    + plano.transform.right * (Mathf.Cos(angulo) * radioPortal)
                    + plano.transform.forward * (Mathf.Sin(angulo) * radioPortal);
            }
            indicador.SetPositions(puntos);
        }

        private bool HayPlanoInferior(ARPlane candidato)
        {
            foreach (ARPlane plano in planos.trackables)
                if (plano.trackingState == TrackingState.Tracking && plano.alignment == PlaneAlignment.HorizontalUp
                    && plano.transform.position.y < candidato.transform.position.y - .2f
                    && plano.size.x * plano.size.y > .15f) return true;
            return false;
        }

        private bool CabePortal(ARPlane plano, Vector3 centro)
        {
            var borde = plano.boundary;
            if (borde.Length < 3) return false;
            Vector3 local = plano.transform.InverseTransformPoint(centro);
            Vector2 punto = new(local.x, local.z);
            // El contorno de un ARPlane es convexo: exigir margen a todas sus aristas.
            for (int i = 0; i < borde.Length; i++)
            {
                Vector2 a = borde[i], b = borde[(i + 1) % borde.Length];
                Vector2 lado = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(punto - a, lado) / Mathf.Max(.000001f, lado.sqrMagnitude));
                if (Vector2.Distance(punto, a + lado * t) < radioPortal) return false;
            }
            return true;
        }

        public void Ocultar()
        {
            estable = 0f; PuedeColocar = false;
            if (indicador != null) indicador.enabled = false;
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
