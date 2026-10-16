using System.Collections.Generic;
using UnityEngine;

namespace RiftHunters.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] float countdownSeconds = 3f;
        [SerializeField] int danoPorUnidad = 34;

        // EnlaceCombateAR ya suma puntos y aplica el daño. Estas dos quedan apagadas para no contar doble.
        [SerializeField] bool aplicarPuntosEnPartida = false;
        [SerializeField] bool aplicarDanoEnPartida = false;

        readonly Queue<GrupoOleada> pendientes = new Queue<GrupoOleada>();

        IPuntuacion puntuacion;
        IVidaJugador vida;
        IGeneradorOleada generador;

        float matchTime;
        float countdownLeft;
        float esperaOleada;
        bool cerrando;
        bool enAviso;
        bool invasion;
        RiftHuntersAR.Integration.ARPrototypeController controlador;

        public GameState State { get; private set; } = GameState.Waiting;
        public float MatchSeconds => matchTime;
        public float CountdownLeft => countdownLeft;
        public string TextoCuenta { get; private set; } = "";
        public string Anuncio { get; private set; } = "";
        public bool AnunciandoJefe => State == GameState.Boss && esperaOleada > 0f;
        public int Puntos => puntuacion != null ? puntuacion.Score : 0;
        public float VidaActual => vida != null ? vida.CurrentHealth : 0f;
        public bool EsJefe => State == GameState.Boss;

        public int NumeroOleada
        {
            get
            {
                if (State == GameState.Wave1) return 1;
                if (State == GameState.Wave2) return 2;
                if (State == GameState.Wave3) return 3;
                if (State == GameState.Wave4) return 4;
                return 0;
            }
        }

        public bool IsRunning => State == GameState.Countdown || EsOleada(State);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            controlador = GetComponent<RiftHuntersAR.Integration.ARPrototypeController>();
            if (countdownSeconds < 0.5f) countdownSeconds = 3f;
            if (danoPorUnidad < 1) danoPorUnidad = 34;
            ResolverFuentes();
        }

        void OnEnable()
        {
            if (vida != null) vida.Died += AlMorirJugador;
            if (generador != null) generador.EnemigosAgotados += AlAgotarseEnemigos;
        }

        void OnDisable()
        {
            if (vida != null) vida.Died -= AlMorirJugador;
            if (generador != null) generador.EnemigosAgotados -= AlAgotarseEnemigos;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Consultar en cada frame: tampoco avanzan anuncios ni cuenta atrás sin tracking.
            if (controlador != null && !controlador.SeguimientoDisponible) return;
            if (IsRunning) matchTime += Time.deltaTime;
            if (EsOleada(State))
            {
                if (esperaOleada > 0f)
                {
                    esperaOleada -= Time.deltaTime;
                    if (esperaOleada > 0f) return;
                    Anuncio = "";
                }
                PedirSiguienteGrupo();
            }
            if (State != GameState.Countdown) return;

            countdownLeft -= Time.deltaTime;
            if (!invasion)
            {
                int segundos = Mathf.CeilToInt(countdownLeft);
                if (segundos > 3) segundos = 3;
                if (segundos < 1) segundos = 1;
                TextoCuenta = segundos.ToString();
                if (countdownLeft <= 0f)
                {
                    invasion = true;
                    countdownLeft = 1f;
                    TextoCuenta = "¡INVASIÓN!";
                }
                return;
            }

            TextoCuenta = "¡INVASIÓN!";
            if (countdownLeft <= 0f) CambiarEstado(GameState.Wave1);
        }

        public void IniciarPartida()
        {
            if (State == GameState.Countdown || EsOleada(State)) return;
            if (State != GameState.Waiting) VolverAlMenu();

            ResolverFuentes();
            if (puntuacion != null) puntuacion.ResetScore();
            if (vida != null) vida.Retry();

            matchTime = 0f;
            invasion = false;
            countdownLeft = countdownSeconds;
            TextoCuenta = "3";
            GameplayHooks.SolicitarReiniciarArmas();
            CambiarEstado(GameState.Countdown);
        }

        public void VolverAlMenu()
        {
            cerrando = true;
            CambiarEstado(GameState.Waiting);
            matchTime = 0f;
            countdownLeft = 0f;
            invasion = false;
            TextoCuenta = "";
            if (puntuacion != null) puntuacion.ResetScore();
            if (vida != null) vida.Retry();
            cerrando = false;
        }

        public void Reintentar()
        {
            VolverAlMenu();
            IniciarPartida();
        }

        public void Murio(int puntos)
        {
            if (!IsRunning || !aplicarPuntosEnPartida || puntuacion == null) return;
            puntuacion.AgregarPuntos(puntos);
        }

        public void AtacoJugador(int unidades)
        {
            if (!IsRunning || !aplicarDanoEnPartida || unidades <= 0 || vida == null) return;
            vida.TakeDamage(unidades * danoPorUnidad);
        }

        public void EnemigosAgotados() => AlAgotarseEnemigos();

        void ResolverFuentes()
        {
            if (puntuacion == null) puntuacion = GetComponent<IPuntuacion>();
            if (vida == null) vida = GetComponent<IVidaJugador>();
            if (generador == null) generador = GetComponent<IGeneradorOleada>();
        }

        void CambiarEstado(GameState siguiente)
        {
            State = siguiente;
            bool oleada = EsOleada(siguiente);
            bool fin = siguiente == GameState.Victory || siguiente == GameState.GameOver;
            if (!oleada)
            {
                pendientes.Clear();
                esperaOleada = 0f;
                Anuncio = "";
            }
            if (!oleada && siguiente != GameState.Countdown) TextoCuenta = "";

            if (generador != null)
            {
                if (fin || siguiente == GameState.Waiting) generador.Limpiar();
                generador.Pausar(!oleada);
            }

            GameplayHooks.SolicitarPausa(!oleada);
            if (fin) GameplayHooks.SolicitarDetenerCombate();
            if (oleada) ComenzarOleada(siguiente);
        }

        void ComenzarOleada(GameState oleada)
        {
            pendientes.Clear();
            GrupoOleada[] grupos = PlanOleadas.De(oleada);
            for (int i = 0; i < grupos.Length; i++) pendientes.Enqueue(grupos[i]);
            Anuncio = PlanOleadas.Anuncio(oleada);
            esperaOleada = PlanOleadas.Espera(oleada);
        }

        void PedirSiguienteGrupo()
        {
            if (esperaOleada > 0f || pendientes.Count == 0 || generador == null || generador.EstaGenerando) return;
            GrupoOleada grupo = pendientes.Peek();
            if (!generador.GenerarGrupo(grupo.Tipo, grupo.Cantidad, grupo.Intervalo)) return;
            pendientes.Dequeue();
        }

        void AlAgotarseEnemigos()
        {
            if (enAviso || cerrando || !EsOleada(State)) return;
            enAviso = true;
            if (generador != null && (generador.EstaGenerando || generador.EnemigosVivos > 0))
            {
                enAviso = false;
                return;
            }

            if (pendientes.Count > 0)
            {
                PedirSiguienteGrupo();
                enAviso = false;
                return;
            }

            Avanzar();
            enAviso = false;
        }

        void Avanzar()
        {
            if (State == GameState.Wave1) CambiarEstado(GameState.Wave2);
            else if (State == GameState.Wave2) CambiarEstado(GameState.Wave3);
            else if (State == GameState.Wave3) CambiarEstado(GameState.Wave4);
            else if (State == GameState.Wave4) CambiarEstado(GameState.Boss);
            else if (State == GameState.Boss) CambiarEstado(GameState.Victory);
        }

        void AlMorirJugador()
        {
            if (cerrando) return;
            if (State == GameState.Waiting || State == GameState.Victory || State == GameState.GameOver) return;
            CambiarEstado(GameState.GameOver);
        }

        static bool EsOleada(GameState estado)
        {
            return estado == GameState.Wave1 || estado == GameState.Wave2 || estado == GameState.Wave3
                || estado == GameState.Wave4 || estado == GameState.Boss;
        }
    }
}
