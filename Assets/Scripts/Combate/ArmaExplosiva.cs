using System.Collections.Generic;
using UnityEngine;

namespace RiftHunters.Combate
{
    public class ArmaExplosiva : Arma
    {
        [UnityEngine.Serialization.FormerlySerializedAs("blastRadius")]
        [SerializeField, Min(0.1f)] private float radioExplosion = 2f;
        [UnityEngine.Serialization.FormerlySerializedAs("areaDamage")]
        [SerializeField, Min(1)] private int danoArea = 40;
        [UnityEngine.Serialization.FormerlySerializedAs("explosionPrefab")]
        [SerializeField] private GameObject prefabExplosion;
        [UnityEngine.Serialization.FormerlySerializedAs("pulseMaterial")]
        [SerializeField] private Material materialPulso;
        public override string NombreVisible => "Explosiva";

        public void ConfigurarValoresIniciales()
        {
            dano = 80;
            capacidadCargador = 5;
            intervaloDisparo = 1f;
            duracionRecarga = 2.5f;
            ReiniciarArma();
        }

        protected override void AplicarImpacto(RaycastHit impacto)
        {
            // Un impacto directo recibe 80; el resto recibe 40, una sola vez.
            var danados = new HashSet<Enemigo>();
            Enemigo directo = impacto.collider.GetComponentInParent<Enemigo>();
            if (directo != null && directo.isActiveAndEnabled && directo.Vida > 0)
            {
                danados.Add(directo);
                directo.RecibirDano(dano);
            }
            foreach (Collider colisionador in Physics.OverlapSphere(
                impacto.point, radioExplosion, capasImpacto, QueryTriggerInteraction.Ignore))
            {
                Enemigo enemigo = colisionador.GetComponentInParent<Enemigo>();
                if (enemigo == null || !enemigo.isActiveAndEnabled || enemigo.Vida <= 0 || !danados.Add(enemigo))
                    continue;
                enemigo.RecibirDano(Mathf.CeilToInt(danoArea * FactorDano));
            }

            MostrarExplosion(impacto.point);
            UltimoResultado = $"Explosion: {danados.Count} enemigos | Directo: {dano} | Area: {danoArea}";
        }

        private void MostrarExplosion(Vector3 posicion)
        {
            // Arte puede asignar un prefab de particulas sin modificar el dano.
            if (prefabExplosion != null)
            {
                Destroy(Instantiate(prefabExplosion, posicion, Quaternion.identity), 3f);
                return;
            }
            if (materialPulso == null) return;
            var efecto = new GameObject("Pulso de explosion");
            efecto.transform.position = posicion;
            efecto.AddComponent<PulsoExplosion>().Inicializar(radioExplosion, materialPulso);
        }

        public void AsignarMaterialPulso(Material material) => materialPulso = material;
    }
}
