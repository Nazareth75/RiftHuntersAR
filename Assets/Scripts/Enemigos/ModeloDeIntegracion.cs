using UnityEngine;
using RiftHuntersAR.Enemies;
using RiftHuntersAR.UI.Visual;

namespace RiftHunters.Combate
{
    public sealed class ModeloDeIntegracion : MonoBehaviour
    {
        [SerializeField] GameObject modelo;
        [SerializeField] GameObject efectoDano;
        [SerializeField] GameObject efectoMuerte;
        [SerializeField] GameObject efectoAparicion;

        void Awake()
        {
            Transform placeholder = transform.Find("Visual - reemplazable por modelo 3D");
            if (modelo != null)
            {
                if (placeholder != null) placeholder.gameObject.SetActive(false);
                GameObject instancia = Instantiate(modelo, transform);
                instancia.transform.localPosition = Vector3.zero;
                instancia.transform.localRotation = Quaternion.identity;
                Collider[] colliders = instancia.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
                EnemyHealth[] vidas = instancia.GetComponentsInChildren<EnemyHealth>(true);
                for (int i = 0; i < vidas.Length; i++) Destroy(vidas[i]);
                EnemyMovement[] movimientos = instancia.GetComponentsInChildren<EnemyMovement>(true);
                for (int i = 0; i < movimientos.Length; i++) Destroy(movimientos[i]);
                EnemyVisualEffects[] efectos = instancia.GetComponentsInChildren<EnemyVisualEffects>(true);
                for (int i = 0; i < efectos.Length; i++) Destroy(efectos[i]);
            }

            if (efectoAparicion != null)
                Instantiate(efectoAparicion, transform.position + Vector3.up, Quaternion.identity);

            Enemigo enemigo = GetComponent<Enemigo>();
            if (enemigo == null) return;
            enemigo.VidaCambiada += () =>
            {
                if (efectoDano != null && enemigo.Vida > 0)
                    Instantiate(efectoDano, transform.position + Vector3.up, Quaternion.identity);
            };
            enemigo.Murio += _ =>
            {
                if (efectoMuerte != null)
                    Instantiate(efectoMuerte, transform.position + Vector3.up, Quaternion.identity);
            };
        }
    }
}
