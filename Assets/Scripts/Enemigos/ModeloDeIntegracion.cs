using UnityEngine;
using RiftHuntersAR.Enemies;
using RiftHuntersAR.UI.Visual;

namespace RiftHunters.Combate
{
    public sealed class ModeloDeIntegracion : MonoBehaviour
    {
        [SerializeField] GameObject modelo;
        [SerializeField, Min(.05f)] float alturaVisual = .28f;
        [SerializeField] GameObject efectoDano;
        [SerializeField] GameObject efectoMuerte;
        [SerializeField] GameObject efectoAparicion;

        public static bool MedirVisible(GameObject objeto, Transform referencia, out Bounds limites)
        {
            limites = default;
            bool primero = true;
            foreach (MeshRenderer renderer in objeto.GetComponentsInChildren<MeshRenderer>())
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                MeshFilter filtro = renderer.GetComponent<MeshFilter>();
                if (filtro == null || filtro.sharedMesh == null) continue;
                Bounds mesh = filtro.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 esquina = mesh.center + Vector3.Scale(mesh.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 punto = referencia.InverseTransformPoint(filtro.transform.TransformPoint(esquina));
                    if (primero) { limites = new Bounds(punto, Vector3.zero); primero = false; }
                    else limites.Encapsulate(punto);
                }
            }
            return !primero;
        }

        private void ApoyarModelo(GameObject instancia)
        {
            if (!MedirVisible(instancia, transform, out Bounds limites) || limites.size.y < .001f) return;
            instancia.transform.localScale *= alturaVisual / (limites.size.y * Mathf.Abs(transform.lossyScale.y));
            MedirVisible(instancia, transform, out limites);
            instancia.transform.localPosition -= Vector3.up * limites.min.y;
            MedirVisible(instancia, transform, out limites);
            var capsule = GetComponent<CapsuleCollider>();
            if (capsule == null) return;
            capsule.center = limites.center;
            capsule.height = limites.size.y;
            capsule.radius = Mathf.Clamp(Mathf.Min(limites.extents.x, limites.extents.z), .025f, capsule.height * .5f);
        }

        private static void Retirar(Behaviour componente)
        {
            componente.enabled = false;
            if (Application.isPlaying) Destroy(componente);
            else DestroyImmediate(componente);
        }

        void Awake()
        {
            Transform placeholder = transform.Find("Visual - reemplazable por modelo 3D");
            if (modelo != null)
            {
                if (placeholder != null) placeholder.gameObject.SetActive(false);
                GameObject instancia = Instantiate(modelo, transform);
                instancia.transform.localPosition = Vector3.zero;
                instancia.transform.localRotation = Quaternion.identity;
                // La orientación permanece separada del nuevo pivote procedural visual.
                foreach (VisualMotion movimiento in instancia.GetComponentsInChildren<VisualMotion>())
                    movimiento.animarFragmentos = false;
                VisualMotion orientacion = instancia.GetComponent<VisualMotion>();
                if (orientacion == null) orientacion = instancia.AddComponent<VisualMotion>();
                orientacion.animarFragmentos = false;
                orientacion.faceCamera = true;
                ApoyarModelo(instancia);
                Collider[] colliders = instancia.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
                EnemyHealth[] vidas = instancia.GetComponentsInChildren<EnemyHealth>(true);
                for (int i = 0; i < vidas.Length; i++) Retirar(vidas[i]);
                EnemyMovement[] movimientos = instancia.GetComponentsInChildren<EnemyMovement>(true);
                for (int i = 0; i < movimientos.Length; i++) Retirar(movimientos[i]);
                EnemyVisualEffects[] efectos = instancia.GetComponentsInChildren<EnemyVisualEffects>(true);
                for (int i = 0; i < efectos.Length; i++) Retirar(efectos[i]);
                if (Application.isPlaying)
                    RiftHuntersAR.Recycling.EnemyProceduralVisual.Attach(GetComponent<Enemigo>(), instancia, modelo.name, alturaVisual);
            }

            if (Application.isPlaying && efectoAparicion != null)
                Instantiate(efectoAparicion, transform.position + Vector3.up * (alturaVisual * .5f), Quaternion.identity);

            Enemigo enemigo = GetComponent<Enemigo>();
            if (enemigo == null) return;
            enemigo.VidaCambiada += () =>
            {
                if (efectoDano != null && enemigo.Vida > 0)
                    Instantiate(efectoDano, transform.position + Vector3.up * (alturaVisual * .5f), Quaternion.identity);
            };
            enemigo.Murio += _ =>
            {
                if (efectoMuerte != null)
                    Instantiate(efectoMuerte, transform.position + Vector3.up * (alturaVisual * .5f), Quaternion.identity);
            };
        }
    }
}
