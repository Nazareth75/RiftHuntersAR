using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RiftHunters.Combate
{
    public class GeneradorEnemigos : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("enemyPrefab")]
        [SerializeField] private Enemigo prefabEnemigo;
        [UnityEngine.Serialization.FormerlySerializedAs("spawnPoint")]
        [SerializeField] private Transform puntoAparicion;
        [UnityEngine.Serialization.FormerlySerializedAs("target")]
        [SerializeField] private Transform objetivo;
        [UnityEngine.Serialization.FormerlySerializedAs("spawnSpacing")]
        [SerializeField, Min(0)] private float separacionAparicion = 0.35f;
        private Transform contenedorEnemigos;
        private Coroutine aparicion;
        private readonly List<Enemigo> generados = new List<Enemigo>();
        private bool esperandoFinal;
        public event Action<Enemigo> EnemigoGenerado;
        public event Action EnemigosAgotados;
        public bool EsperandoDistancia { get; private set; }
        public bool EstaGenerando => aparicion != null;
        public bool EstaPausado { get; private set; }
        public int EnemigosVivos => generados.Count;

        public void Configurar(Enemigo prefab, Transform origen, Transform jugador, Transform contenedor = null)
        {
            prefabEnemigo = prefab;
            puntoAparicion = origen;
            objetivo = jugador;
            contenedorEnemigos = contenedor;
        }

        public void GenerarGrupo(int cantidad, float intervalo)
        {
            if (!isActiveAndEnabled || EstaPausado || EstaGenerando || cantidad <= 0) return;
            if (prefabEnemigo == null || puntoAparicion == null || objetivo == null)
            {
                Debug.LogWarning("Generador: asigna prefab, origen y objetivo.", this);
                return;
            }
            esperandoFinal = true;
            aparicion = StartCoroutine(RutinaAparicion(cantidad, Mathf.Max(0.05f, intervalo)));
        }

        private IEnumerator RutinaAparicion(int cantidad, float intervalo)
        {
            Enemigo prefabGrupo = prefabEnemigo;
            Transform objetivoGrupo = objetivo;
            Transform origenGrupo = puntoAparicion;
            Transform contenedorGrupo = contenedorEnemigos;
            // Permite guardar la coroutine antes de generar el primer enemigo.
            yield return null;
            for (int i = 0; i < cantidad; i++)
            {
                while (EstaPausado || generados.Count >= RiftHunters.Game.BalancePartida.MaximosSimultaneos) yield return null;
                if (origenGrupo == null || objetivoGrupo == null) break;
                Vector3 desplazamiento = origenGrupo.right * ((i % 3 - 1) * separacionAparicion);
                Vector3 posicion = origenGrupo.position + desplazamiento;
                var suelo = contenedorGrupo != null ? contenedorGrupo.GetComponent<RiftHuntersAR.AR.SueloCombateAR>() : null;
                if (suelo != null) posicion = suelo.PosicionAparicion(posicion);
                // Esperar si el jugador se acerca al portal, sin crear enemigos encima.
                while (suelo != null)
                {
                    if (origenGrupo == null || objetivoGrupo == null) { aparicion = null; yield break; }
                    posicion = suelo.PosicionAparicion(origenGrupo.position + desplazamiento);
                    Vector3 diferencia = posicion - objetivoGrupo.position;
                    diferencia.y = 0f;
                    if (!EstaPausado && generados.Count < RiftHunters.Game.BalancePartida.MaximosSimultaneos && diferencia.sqrMagnitude >= .8f * .8f) break;
                    EsperandoDistancia = true;
                    yield return null;
                }
                EsperandoDistancia = false;
                Enemigo enemigo = Instantiate(prefabGrupo, posicion, origenGrupo.rotation, contenedorGrupo);
                enemigo.AsignarObjetivo(objetivoGrupo);
                generados.Add(enemigo);
                enemigo.Retirado += AlRetirarEnemigo;
                enemigo.gameObject.SetActive(true);
                EnemigoGenerado?.Invoke(enemigo);
                if (!esperandoFinal) yield break;
                // Parejas cortas y un respiro; conservar la cantidad y el intervalo medio.
                if (i == cantidad - 1 && cantidad == 1) continue;
                float restante = i == cantidad - 1 ? .7f : intervalo * (i % 2 == 0 ? .65f : 1.35f);
                while (restante > 0)
                {
                    yield return null;
                    if (!EstaPausado) restante -= Time.deltaTime;
                }
            }
            aparicion = null;
            VerificarFinalizacion();
        }

        private void AlRetirarEnemigo(Enemigo enemigo)
        {
            enemigo.Retirado -= AlRetirarEnemigo;
            generados.Remove(enemigo);
            VerificarFinalizacion();
        }

        private void VerificarFinalizacion()
        {
            if (!esperandoFinal || EstaGenerando || generados.Count > 0) return;
            esperandoFinal = false;
            EnemigosAgotados?.Invoke();
        }

        public void Pausar(bool pausa)
        {
            EstaPausado = pausa;
            foreach (Enemigo enemigo in generados)
                if (enemigo != null) enemigo.Pausar(pausa);
        }

        public void Limpiar()
        {
            EsperandoDistancia = false;
            esperandoFinal = false; // Cancelar no cuenta como completar una oleada.
            if (aparicion != null) StopCoroutine(aparicion);
            aparicion = null;
            EstaPausado = false;
            foreach (Enemigo enemigo in generados)
            {
                if (enemigo == null) continue;
                enemigo.Retirado -= AlRetirarEnemigo;
                enemigo.gameObject.SetActive(false);
                Destroy(enemigo.gameObject);
            }
            generados.Clear();
        }

        private void OnDisable() => Limpiar();
    }
}
