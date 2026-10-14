using UnityEngine;

namespace RiftHunters.Combate
{
    [RequireComponent(typeof(Enemigo))]
    public class RespuestaVisualEnemigo : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float duracionDestello = 0.12f;
        private Enemigo enemigo;
        private Renderer[] superficies;
        private MaterialPropertyBlock[] originales;
        private float finDestello;
        private bool mostrando;

        private void Awake()
        {
            enemigo = GetComponent<Enemigo>();
            superficies = GetComponentsInChildren<Renderer>();
            originales = new MaterialPropertyBlock[superficies.Length];
            for (int i = 0; i < superficies.Length; i++)
            {
                originales[i] = new MaterialPropertyBlock();
                superficies[i].GetPropertyBlock(originales[i]);
            }
        }

        private void OnEnable() => enemigo.VidaCambiada += MostrarDestello;

        private void MostrarDestello()
        {
            finDestello = Time.time + duracionDestello;
            mostrando = true;
            for (int i = 0; i < superficies.Length; i++)
            {
                if (superficies[i] == null) continue;
                var propiedades = new MaterialPropertyBlock();
                superficies[i].GetPropertyBlock(propiedades);
                propiedades.SetColor("_BaseColor", Color.white);
                superficies[i].SetPropertyBlock(propiedades);
            }
        }

        private void Update()
        {
            if (mostrando && Time.time >= finDestello) RestaurarColor();
        }

        private void RestaurarColor()
        {
            mostrando = false;
            for (int i = 0; i < superficies.Length; i++)
                if (superficies[i] != null) superficies[i].SetPropertyBlock(originales[i]);
        }

        private void OnDisable()
        {
            enemigo.VidaCambiada -= MostrarDestello;
            RestaurarColor();
        }
    }
}
