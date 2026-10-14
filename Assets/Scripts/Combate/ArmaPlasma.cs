using UnityEngine;

namespace RiftHunters.Combate
{
    public class ArmaPlasma : Arma
    {
        public override string NombreVisible => "Plasma";

        protected override void AplicarImpacto(RaycastHit impacto)
        {
            Enemigo enemigo = impacto.collider.GetComponentInParent<Enemigo>();
            if (enemigo == null)
            {
                UltimoResultado = "Impacto en escenario";
                return;
            }
            if (enemigo.Invulnerable)
            {
                enemigo.RecibirDano(dano);
                UltimoResultado = "Escudo: rompe los nucleos";
                return;
            }
            enemigo.RecibirDano(dano);
            UltimoResultado = enemigo.Vida > 0 ? $"Impacto: {enemigo.Vida} HP restantes" : "Enemigo eliminado";
        }
    }
}
