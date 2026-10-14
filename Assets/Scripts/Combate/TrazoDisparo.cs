using UnityEngine;

namespace RiftHunters.Combate
{
    public class TrazoDisparo : MonoBehaviour
    {
        [SerializeField] private Arma arma;
        [SerializeField] private Material materialTrazo;
        [SerializeField, Min(0.01f)] private float duracion = 0.12f;

        public void Configurar(Arma origen, Material material)
        {
            if (arma != null) arma.DisparoRealizado -= Mostrar;
            arma = origen;
            materialTrazo = material;
            if (isActiveAndEnabled && arma != null) arma.DisparoRealizado += Mostrar;
        }

        private void OnEnable()
        {
            if (arma != null) arma.DisparoRealizado += Mostrar;
        }

        private void OnDisable()
        {
            if (arma != null) arma.DisparoRealizado -= Mostrar;
        }

        private void Mostrar(Vector3 inicio, Vector3 fin)
        {
            if (materialTrazo == null) return;
            var efecto = new GameObject("Trazo de disparo");
            var linea = efecto.AddComponent<LineRenderer>();
            linea.sharedMaterial = materialTrazo;
            linea.useWorldSpace = true;
            linea.positionCount = 2;
            linea.startWidth = 0.025f;
            linea.endWidth = 0.01f;
            linea.SetPosition(0, inicio);
            linea.SetPosition(1, fin);
            Destroy(efecto, duracion);
        }
    }
}
