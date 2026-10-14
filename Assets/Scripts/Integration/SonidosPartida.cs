using UnityEngine;
using RiftHunters.Combate;
using RiftHuntersAR.Audio;

namespace RiftHuntersAR.Integration
{
    // Solo escucha eventos y estados de la partida; no cambia daño, vida ni puntos.
    public sealed class SonidosPartida : MonoBehaviour
    {
        private ARPrototypeController controlador;
        private GestorArmas armas;
        private string estadoAnterior = "SCANNING";
        private string cuentaAnterior = "";
        private int oleadaAnterior;
        private float vidaAnterior = 100f;
        private bool recargandoAntes;
        private Arma armaAntes;

        public void Unir(ARPrototypeController valor)
        {
            controlador = valor;
            armas = controlador.Armas;
            if (armas != null)
            {
                foreach (Arma arma in armas.GetComponentsInChildren<Arma>(true))
                {
                    Arma propia = arma;
                    arma.DisparoRealizado += (_, _) => AlDisparar(propia);
                }
                armas.ArmaCambiada += AlCambiarArma;
            }
            if (controlador.Generador != null) controlador.Generador.EnemigoGenerado += AlGenerarEnemigo;
            if (controlador.Arma != null) controlador.Arma.Feedback += AlRecibirMensaje;
            if (controlador.Player != null)
            {
                vidaAnterior = controlador.Player.CurrentHealth;
                controlador.Player.Changed += AlCambiarVida;
            }
        }

        private void OnDestroy()
        {
            if (controlador == null) return;
            if (armas != null) armas.ArmaCambiada -= AlCambiarArma;
            if (controlador.Generador != null) controlador.Generador.EnemigoGenerado -= AlGenerarEnemigo;
            if (controlador.Arma != null) controlador.Arma.Feedback -= AlRecibirMensaje;
            if (controlador.Player != null) controlador.Player.Changed -= AlCambiarVida;
        }

        private void AlDisparar(Arma arma)
        {
            switch (arma)
            {
                case ArmaElectrica:
                    SonidoJuego.ReproducirVariante("disparo_electrica", 2, 0.75f, 0.05f);
                    break;
                case ArmaExplosiva:
                    SonidoJuego.Reproducir("disparo_explosiva", 0.7f, 0.04f, 0f);
                    Invoke(nameof(SonarExplosion), 0.12f);
                    break;
                default:
                    SonidoJuego.ReproducirVariante("disparo_plasma", 3, 0.55f, 0.06f);
                    break;
            }
        }

        private void SonarExplosion() => SonidoJuego.Reproducir("explosion", 0.9f, 0.05f, 0f);

        private void AlCambiarArma()
        {
            if (controlador.CurrentState == "WAVE") SonidoJuego.Reproducir("cambio_arma", 0.6f);
        }

        private void AlGenerarEnemigo(Enemigo enemigo)
        {
            if (enemigo.GetComponent<NucleosJefe>() != null) return;
            enemigo.VidaCambiada += () =>
            {
                if (enemigo != null && enemigo.Vida > 0) SonidoJuego.ReproducirVariante("impacto", 3, 0.55f, 0.1f);
            };
            enemigo.Murio += _ => SonidoJuego.ReproducirVariante("muerte_enemigo", 2, 0.8f, 0.08f);
        }

        private void AlRecibirMensaje(string mensaje)
        {
            if (mensaje.StartsWith("Sin municion") || mensaje.StartsWith("EMPTY"))
                SonidoJuego.Reproducir("sin_municion", 0.6f, 0f, 0.15f);
        }

        private void AlCambiarVida()
        {
            float vida = controlador.Player.CurrentHealth;
            if (vida < vidaAnterior && controlador.CurrentState == "WAVE")
            {
                SonidoJuego.Reproducir("dano_jugador", 0.9f, 0.05f);
                SonidoJuego.Reproducir("dano_jugador_glitch", 0.5f);
            }
            vidaAnterior = vida;
        }

        private void Update()
        {
            if (controlador == null) return;
            VigilarRecarga();
            VigilarEstado();
        }

        private void VigilarRecarga()
        {
            Arma actual = armas != null ? armas.Actual : null;
            bool recargando = actual != null && actual.EstaRecargando;
            if (actual == armaAntes && recargando != recargandoAntes)
            {
                if (recargando) SonidoJuego.Reproducir("recarga_inicio", 0.7f);
                else if (actual.Municion == actual.CapacidadCargador) SonidoJuego.Reproducir("recarga_fin", 0.8f);
            }
            armaAntes = actual;
            recargandoAntes = recargando;
        }

        private void VigilarEstado()
        {
            string estado = controlador.CurrentState;
            string cuenta = controlador.TextoCuenta;
            int oleada = controlador.CurrentWave;

            if (estadoAnterior == "SCANNING" && estado != "SCANNING")
                SonidoJuego.Reproducir("portal", 0.9f);

            if (estado == "COUNTDOWN" && cuenta != cuentaAnterior)
            {
                if (cuenta == "¡INVASIÓN!") SonidoJuego.Reproducir("invasion", 1f);
                else if (!string.IsNullOrEmpty(cuenta)) SonidoJuego.Reproducir("cuenta", 0.8f, 0f, 0.2f);
            }

            if (estado == "WAVE" && oleada > 1 && oleada != oleadaAnterior)
                SonidoJuego.Reproducir("invasion", 0.6f);

            if (estado != estadoAnterior)
            {
                if (estado == "VICTORY") Invoke(nameof(SonarVictoria), 0.8f);
                else if (estado == "GAME_OVER") SonidoJuego.Reproducir("game_over", 1f);
            }

            estadoAnterior = estado;
            cuentaAnterior = cuenta;
            oleadaAnterior = oleada;
        }

        private void SonarVictoria() => SonidoJuego.Reproducir("victoria", 1f);
    }
}
