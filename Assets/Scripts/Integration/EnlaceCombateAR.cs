using UnityEngine;
using RiftHunters.Combate;
using RiftHuntersAR.AR;
using RiftHuntersAR.Game;

namespace RiftHuntersAR.Integration
{
    public class EnlaceCombateAR : MonoBehaviour
    {
        [SerializeField] private GestorArmas prefabSistemaArmas;
        [SerializeField] private Enemigo[] prefabsEnemigos;
        // Vida compartida: 100 HP. Un ataque normal resta 34, un tanque 68.
        [SerializeField, Min(1)] private int danoPorUnidad = 34;
        private GeneradorEnemigos generador;
        private PlayerHealth jugador;
        private ScoreManager puntuacion;
        private Camera camara;
        public GestorArmas Armas { get; private set; }
        public GeneradorEnemigos Generador => generador;
        public int EnemigosVivos => generador != null ? generador.EnemigosVivos : 0;
        public int Derrotados { get; private set; }

        public void CompletarSiFalta(CatalogoPartida catalogo)
        {
            if (catalogo == null) return;
            if (prefabSistemaArmas == null) prefabSistemaArmas = catalogo.sistemaArmas;
            if (prefabsEnemigos != null && prefabsEnemigos.Length == 3 && prefabsEnemigos[0] != null) return;
            prefabsEnemigos = new[] { catalogo.normal, catalogo.rapido, catalogo.tanque };
        }

        public bool Inicializar(Camera camaraAR, PlayerHealth vida, ScoreManager puntos)
        {
            if (prefabSistemaArmas == null || prefabsEnemigos == null || prefabsEnemigos.Length != 3 || camaraAR == null)
            {
                Debug.LogError("Enlace AR: asigna sistema de armas, normal/rapido/tanque y camara.", this);
                return false;
            }
            foreach (Enemigo prefab in prefabsEnemigos)
                if (prefab == null) return false;
            camara = camaraAR;
            jugador = vida;
            puntuacion = puntos;
            Armas = Instantiate(prefabSistemaArmas, transform);
            Armas.AsignarCamara(camaraAR);
            generador = gameObject.AddComponent<GeneradorEnemigos>();
            generador.EnemigoGenerado += ConectarEnemigo;
            Pausar(true);
            return true;
        }

        private void ConectarEnemigo(Enemigo enemigo)
        {
            enemigo.Murio += AlEliminarEnemigo;
            enemigo.AtacoJugador += AlAtacarJugador;
            if (enemigo.GetComponent<RespuestaVisualEnemigo>() == null)
                enemigo.gameObject.AddComponent<RespuestaVisualEnemigo>();
        }

        private void AlEliminarEnemigo(Enemigo enemigo)
        {
            Derrotados++;
            puntuacion.AgregarPuntos(enemigo.Puntos);
        }
        private void AlAtacarJugador(int unidades) => jugador.TakeDamage(unidades * danoPorUnidad);

        public void GenerarEnemigo(PortalSpawnPoint portal, int oleada, int posicion)
        {
            if (generador == null || portal == null || portal.SpawnPoint == null || jugador.IsDead) return;
            int tipo = 0;
            if (oleada >= 2 && posicion % 3 == 2) tipo = 1;
            if (oleada >= 3 && posicion == 6) tipo = 2;
            // El ancla conserva la relacion espacial con el portal.
            generador.Configurar(prefabsEnemigos[tipo], portal.SpawnPoint, camara.transform, portal.transform.parent);
            generador.GenerarGrupo(1, 0.05f);
        }

        public void SeleccionarArma(int indice)
        {
            if (Armas != null) Armas.SeleccionarArma(indice);
        }

        public void Pausar(bool pausa)
        {
            if (Armas != null) Armas.Pausar(pausa);
            if (generador != null) generador.Pausar(pausa);
        }

        public void Detener()
        {
            if (Armas != null) Armas.DetenerCombate();
            if (generador != null) generador.Limpiar();
        }

        public void Reiniciar()
        {
            Derrotados = 0;
            if (generador != null) generador.Limpiar();
            if (Armas != null) Armas.ReiniciarArmas();
        }

        private void OnDisable() => Detener();
    }
}
