using System;
using UnityEngine;
using RiftHunters.Combate;
using RiftHunters.Game;
using RiftHuntersAR.AR;

namespace RiftHuntersAR.Integration
{
    public sealed class AdaptadorGenerador : MonoBehaviour, IGeneradorOleada
    {
        GeneradorEnemigos generador;
        CatalogoPartida catalogo;
        Transform punto;
        Transform camara;
        Transform contenedor;
        Enemigo jefe;
        NucleosJefe nucleos;
        TipoEnemigo pedido;

        public NucleosJefe Nucleos => jefe != null ? nucleos : null;

        public int EnemigosVivos => generador != null ? generador.EnemigosVivos : 0;
        public bool EstaGenerando => generador != null && generador.EstaGenerando;
        public event Action EnemigosAgotados;

        public float FraccionJefe
        {
            get
            {
                if (jefe == null || jefe.VidaMaxima <= 0) return -1f;
                if (nucleos != null) return nucleos.Fraccion;
                return (float)jefe.Vida / jefe.VidaMaxima;
            }
        }

        public void Unir(GeneradorEnemigos fuente, CatalogoPartida datos)
        {
            if (generador != null)
            {
                generador.EnemigosAgotados -= AlAgotarse;
                generador.EnemigoGenerado -= AlGenerar;
            }
            generador = fuente;
            catalogo = datos;
            if (generador == null) return;
            generador.EnemigosAgotados += AlAgotarse;
            generador.EnemigoGenerado += AlGenerar;
        }

        public void UsarPortal(PortalSpawnPoint portal, Transform camaraJugador)
        {
            if (portal == null) return;
            punto = portal.SpawnPoint;
            camara = camaraJugador;
            contenedor = portal.transform.parent;
        }

        public bool GenerarGrupo(TipoEnemigo tipo, int cantidad, float intervalo)
        {
            if (generador == null || punto == null || camara == null || generador.EstaGenerando || generador.EstaPausado)
                return false;
            Enemigo prefab = Prefab(tipo);
            if (prefab == null) return false;
            pedido = tipo;
            generador.Configurar(prefab, punto, camara, contenedor);
            generador.GenerarGrupo(cantidad, intervalo);
            return generador.EstaGenerando;
        }

        public void Limpiar()
        {
            jefe = null;
            nucleos = null;
            if (generador != null) generador.Limpiar();
        }

        public void Pausar(bool pausa)
        {
            if (generador != null) generador.Pausar(pausa);
        }

        void AlAgotarse() => EnemigosAgotados?.Invoke();

        void AlGenerar(Enemigo enemigo)
        {
            int numero = GameManager.Instance != null ? GameManager.Instance.NumeroOleada : 1;
            enemigo.ConfigurarEstadisticas(Mathf.CeilToInt(enemigo.VidaMaxima * BalancePartida.Vida(numero)),
                enemigo.Velocidad * BalancePartida.Velocidad(numero), enemigo.DanoAtaque, enemigo.Puntos);
            if (pedido != TipoEnemigo.Jefe) return;
            jefe = enemigo;
            nucleos = enemigo.gameObject.AddComponent<NucleosJefe>();
            nucleos.ConfigurarFuerza(BalancePartida.FuerzaJefe(numero));
            nucleos.Inicializar(camara);
        }

        Enemigo Prefab(TipoEnemigo tipo)
        {
            if (catalogo == null) return null;
            if (tipo == TipoEnemigo.Rapido) return catalogo.rapido;
            if (tipo == TipoEnemigo.Tanque) return catalogo.tanque;
            if (tipo == TipoEnemigo.Jefe) return catalogo.jefe;
            return catalogo.normal;
        }
    }
}
