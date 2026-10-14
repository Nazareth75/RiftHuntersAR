using System.Collections.Generic;
using UnityEngine;

namespace RiftHunters.Combate
{
    public class ArmaElectrica : Arma
    {
        [UnityEngine.Serialization.FormerlySerializedAs("chainRadius")]
        [SerializeField, Min(0.1f)] private float radioCadena = 2.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("maxTargets")]
        [SerializeField, Range(1, 3)] private int maximoObjetivos = 3;
        [UnityEngine.Serialization.FormerlySerializedAs("beamMaterial")]
        [SerializeField] private Material materialRayo;
        public override string NombreVisible => "Electrica";


        public void ConfigurarValoresIniciales()
        {
            dano = 35;
            capacidadCargador = 8;
            intervaloDisparo = 0.8f;
            duracionRecarga = 2f;
            ReiniciarArma();
        }

        protected override void AplicarImpacto(RaycastHit impacto)
        {
            Enemigo actual = impacto.collider.GetComponentInParent<Enemigo>();
            if (actual == null || actual.Vida <= 0)
            {
                UltimoResultado = "La electricidad necesita impactar un enemigo";
                return;
            }
            var alcanzados = new HashSet<Enemigo>();
            Vector3 inicioRayo = impacto.point + Vector3.up * 0.2f;
            while (actual != null && alcanzados.Count < maximoObjetivos)
            {
                Vector3 posicion = actual.transform.position;
                alcanzados.Add(actual);
                DibujarRayo(inicioRayo, posicion);
                actual.RecibirDano(dano);
                inicioRayo = posicion;
                actual = BuscarMasCercano(posicion, alcanzados);
            }
            UltimoResultado = $"Cadena electrica: {alcanzados.Count} enemigos | {dano} dano a cada uno";
        }

        private Enemigo BuscarMasCercano(Vector3 origen, HashSet<Enemigo> alcanzados)
        {
            Enemigo masCercano = null;
            float menorDistancia = float.PositiveInfinity;
            foreach (Collider colisionador in Physics.OverlapSphere(origen, radioCadena, capasImpacto, QueryTriggerInteraction.Ignore))
            {
                Enemigo enemigo = colisionador.GetComponentInParent<Enemigo>();
                if (enemigo == null || !enemigo.isActiveAndEnabled || enemigo.Vida <= 0 || alcanzados.Contains(enemigo)) continue;
                float distancia = (enemigo.transform.position - origen).sqrMagnitude;
                if (distancia >= menorDistancia) continue;
                menorDistancia = distancia;
                masCercano = enemigo;
            }
            return masCercano;
        }

        private void DibujarRayo(Vector3 desde, Vector3 hasta)
        {
            if (materialRayo == null) return;
            var efecto = new GameObject("Arco electrico");
            var linea = efecto.AddComponent<LineRenderer>();
            linea.sharedMaterial = materialRayo;
            linea.positionCount = 2;
            linea.startWidth = linea.endWidth = 0.04f;
            linea.SetPosition(0, desde);
            linea.SetPosition(1, hasta);
            Destroy(efecto, 0.15f);
        }

        public void AsignarMaterialRayo(Material material) => materialRayo = material;
    }
}
