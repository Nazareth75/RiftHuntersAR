using System;
using UnityEngine;
using RiftHuntersAR.Audio;

namespace RiftHunters.Combate
{
    // Jefe de tres núcleos: primero los dos brazos, después el núcleo central.
    // El cuerpo es invulnerable; el jefe muere al romper el núcleo central.
    [RequireComponent(typeof(Enemigo))]
    [DefaultExecutionOrder(100)]
    public sealed class NucleosJefe : MonoBehaviour
    {
        private const int VidaBrazo = 100;
        private const int VidaCentral = 150;
        private static readonly Color ColorBrazo = new(0.25f, 0.9f, 1f);
        private static readonly Color ColorCentral = new(1f, 0.32f, 0.08f);
        private static readonly Color ColorProtegido = new(0.35f, 0.12f, 0.1f);

        private sealed class Nucleo
        {
            public Enemigo enemigo;
            public Transform transform;
            public Material material;
            public Color color;
            public float escalaBase;
            public float golpeHasta;
            public bool vivo;
            public Renderer objetivoVisual;
        }

        private Enemigo jefe;
        private Transform camara;
        private Nucleo izquierdo, derecho, central;
        private float siguienteSonidoEscudo;
        private bool centralExpuesto;
        private bool terminado;
        private Renderer cuerpoVisual;

        public bool IzquierdoVivo => izquierdo != null && izquierdo.vivo;
        public bool DerechoVivo => derecho != null && derecho.vivo;
        public bool CentralVivo => central != null && central.vivo;
        public bool CentralExpuesto => centralExpuesto;
        public event Action Cambio;

        public float Fraccion
        {
            get
            {
                float total = VidaBrazo * 2 + VidaCentral;
                float vida = Vida(izquierdo) + Vida(derecho) + Vida(central);
                return Mathf.Clamp01(vida / total);
            }
        }

        public string Pista => centralExpuesto ? "¡DISPARA AL NÚCLEO CENTRAL!" : "ROMPE LOS NÚCLEOS DE LOS BRAZOS";

        public void Inicializar(Transform camaraJugador)
        {
            camara = camaraJugador;
            jefe = GetComponent<Enemigo>();
            jefe.ConfigurarEstadisticas(jefe.VidaMaxima, 0.35f, 1, jefe.Puntos);
            jefe.ConfigurarAtaqueContinuo(4.5f, 1.4f);
            jefe.Invulnerable = true;
            jefe.ImpactoBloqueado += AlBloquear;
            if(camara!=null)
            {
                var frente=camara.position-transform.position; frente.y=0;
                if(frente.sqrMagnitude>.001f) transform.rotation=Quaternion.LookRotation(frente,Vector3.up);
            }
            CrearNucleos();
            SonidoJuego.Reproducir("jefe_aparece", 1f);
        }

        private void CrearNucleos()
        {
            Quaternion rotacionOriginal = transform.rotation;
            transform.rotation = Quaternion.identity;
            Bounds limites = LimitesLocales();
            transform.rotation = rotacionOriginal;

            float alto = Mathf.Clamp(limites.size.y, 1f, 4f);
            float ancho = Mathf.Clamp(limites.extents.x, 0.5f, 2f);
            float frente = Mathf.Clamp(limites.max.z, 0.3f, 1.5f);
            float radio = Mathf.Clamp(alto * 0.09f, 0.14f, 0.3f);
            float alturaBrazos = limites.min.y + alto * 0.55f;
            float alturaPecho = limites.min.y + alto * 0.62f;

            izquierdo = CrearNucleo("Nucleo brazo izquierdo", new Vector3(-ancho * 0.85f, alturaBrazos, frente * 0.45f), radio, ColorBrazo, VidaBrazo);
            derecho = CrearNucleo("Nucleo brazo derecho", new Vector3(ancho * 0.85f, alturaBrazos, frente * 0.45f), radio, ColorBrazo, VidaBrazo);
            central = CrearNucleo("Nucleo central", new Vector3(0f, alturaPecho, frente + radio * 0.4f), radio * 1.25f, ColorCentral, VidaCentral);
            central.enemigo.Invulnerable = true;
            central.enemigo.ImpactoBloqueado += AlBloquear;
            AlinearConModelo(izquierdo, "WeakPoint_Left");
            AlinearConModelo(derecho, "WeakPoint_Right");
            AlinearConModelo(central, "WeakPoint_Core");
            foreach(var renderer in GetComponentsInChildren<Renderer>())
                if(renderer.name=="Body_Mesh") cuerpoVisual=renderer;
            AjustarColliderCuerpo();
        }

        void AlinearConModelo(Nucleo nucleo, string nombre)
        {
            foreach(var renderer in GetComponentsInChildren<Renderer>())
            {
                if(renderer.transform.name != nombre && renderer.transform.name != nombre+"_Mesh") continue;
                nucleo.objetivoVisual=renderer;
                nucleo.transform.GetComponent<Renderer>().enabled=false;
                SeguirModelo(nucleo); return;
            }
        }

        void SeguirModelo(Nucleo nucleo)
        {
            if(nucleo==null || !nucleo.vivo || nucleo.transform==null || nucleo.objetivoVisual==null) return;
            var bounds=nucleo.objetivoVisual.bounds;
            nucleo.transform.position=bounds.center;
            float diametro=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*1.15f;
            var escala=transform.lossyScale;
            nucleo.transform.localScale=new Vector3(diametro/Mathf.Abs(escala.x),diametro/Mathf.Abs(escala.y),diametro/Mathf.Abs(escala.z));
        }

        void AjustarColliderCuerpo()
        {
            // The old generic capsule covered the art's arm cores. Keep a shielded
            // body collider, fitted to the actual torso instead of the whole boss.
            if(cuerpoVisual==null || izquierdo?.objetivoVisual==null || derecho?.objetivoVisual==null || central?.objetivoVisual==null) return;
            var capsule=GetComponent<CapsuleCollider>(); if(capsule==null) return;
            var mesh=cuerpoVisual.GetComponent<MeshFilter>(); if(mesh==null || mesh.sharedMesh==null) return;
            var source=mesh.sharedMesh.bounds;
            var local=new Bounds(transform.InverseTransformPoint(cuerpoVisual.transform.TransformPoint(source.center)),Vector3.zero);
            for(int i=0;i<8;i++)
            {
                var corner=source.center+Vector3.Scale(source.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                local.Encapsulate(transform.InverseTransformPoint(cuerpoVisual.transform.TransformPoint(corner)));
            }
            capsule.center=local.center; capsule.height=local.size.y;
            capsule.radius=Mathf.Max(.05f,Mathf.Min(local.extents.x,Mathf.Min(local.extents.y,local.extents.z)));
        }
        void LateUpdate() { AjustarColliderCuerpo(); SeguirModelo(izquierdo); SeguirModelo(derecho); SeguirModelo(central); }

        private Bounds LimitesLocales()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            bool hay = false;
            var limites = new Bounds(transform.position + Vector3.up * 1.05f, new Vector3(1.4f, 2.1f, 1.4f));
            foreach (Renderer r in renderers)
            {
                if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer) continue;
                if (!hay) { limites = r.bounds; hay = true; }
                else limites.Encapsulate(r.bounds);
            }
            limites.center -= transform.position;
            return limites;
        }

        private Nucleo CrearNucleo(string nombre, Vector3 posicion, float radio, Color color, int vida)
        {
            GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            esfera.name = nombre;
            esfera.transform.SetParent(transform, false);
            esfera.transform.localPosition = posicion;
            esfera.transform.localScale = Vector3.one * radio * 2f;

            var renderer = esfera.GetComponent<Renderer>();
            Shader sombreador = Shader.Find("Universal Render Pipeline/Unlit");
            if (sombreador == null) sombreador = Shader.Find("Unlit/Color");
            var material = new Material(sombreador);
            Pintar(material, color);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Enemigo enemigo = esfera.AddComponent<Enemigo>();
            enemigo.ConfigurarEstadisticas(vida, 0f, 1, 0);
            var nucleo = new Nucleo
            {
                enemigo = enemigo, transform = esfera.transform, material = material,
                color = color, escalaBase = radio * 2f, vivo = true,
            };
            enemigo.VidaCambiada += () => AlGolpear(nucleo);
            enemigo.Murio += _ => AlRomper(nucleo);
            return nucleo;
        }

        private static void Pintar(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static float Vida(Nucleo nucleo) => nucleo != null && nucleo.vivo && nucleo.enemigo != null ? nucleo.enemigo.Vida : 0f;

        private void AlGolpear(Nucleo nucleo)
        {
            nucleo.golpeHasta = Time.time + 0.12f;
            if (nucleo.enemigo.Vida > 0) SonidoJuego.ReproducirVariante("impacto", 3, 0.8f, 0.08f);
            Cambio?.Invoke();
        }

        private void AlRomper(Nucleo nucleo)
        {
            nucleo.vivo = false;
            if(nucleo.objetivoVisual!=null) nucleo.objetivoVisual.enabled=false;
            SonidoJuego.Reproducir("nucleo_roto", 1f, 0.05f);
            if (nucleo == central)
            {
                Derrotar();
                return;
            }
            if (!centralExpuesto && !IzquierdoVivo && !DerechoVivo)
            {
                centralExpuesto = true;
                central.enemigo.Invulnerable = false;
                SonidoJuego.Reproducir("nucleo_expuesto", 0.9f);
            }
            Cambio?.Invoke();
        }

        private void Derrotar()
        {
            if (terminado) return;
            terminado = true;
            SonidoJuego.Reproducir("jefe_muere", 1f);
            Cambio?.Invoke();
            jefe.Invulnerable = false;
            jefe.RecibirDano(Mathf.Max(1, jefe.Vida));
        }

        private void AlBloquear()
        {
            if (Time.time < siguienteSonidoEscudo) return;
            siguienteSonidoEscudo = Time.time + 0.3f;
            SonidoJuego.Reproducir("escudo", 0.45f, 0.06f);
        }

        private void Update()
        {
            if (camara != null)
            {
                Vector3 haciaJugador = camara.position - transform.position;
                haciaJugador.y = 0f;
                if (haciaJugador.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(haciaJugador.normalized, Vector3.up), Time.deltaTime * 3f);
            }
            Animar(izquierdo, 6f);
            Animar(derecho, 6f);
            Animar(central, centralExpuesto ? 9f : 2f);
        }

        private void Animar(Nucleo nucleo, float velocidad)
        {
            if (nucleo == null || !nucleo.vivo || nucleo.transform == null) return;
            bool golpe = Time.time < nucleo.golpeHasta;
            float pulso = 1f + Mathf.Sin(Time.time * velocidad) * 0.08f;
            nucleo.transform.localScale = Vector3.one * nucleo.escalaBase * (golpe ? 1.2f : pulso);
            Color color = golpe ? Color.white
                : nucleo == central && !centralExpuesto ? ColorProtegido
                : nucleo.color * (0.85f + 0.15f * Mathf.Sin(Time.time * velocidad));
            Pintar(nucleo.material, color);
        }

        private void OnDestroy()
        {
            if (jefe != null) jefe.ImpactoBloqueado -= AlBloquear;
            DestroyMaterial(izquierdo);
            DestroyMaterial(derecho);
            DestroyMaterial(central);
        }

        private static void DestroyMaterial(Nucleo nucleo)
        {
            if (nucleo != null && nucleo.material != null) Destroy(nucleo.material);
        }
    }
}
