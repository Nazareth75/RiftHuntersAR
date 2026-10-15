using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RiftHuntersAR.Audio
{
    // Los clips viven en Assets/Resources/Sonidos. Sobrevive a los cambios de escena
    // para que el clic de "Iniciar" no se corte al cargar ARGame.
    public sealed class SonidoJuego : MonoBehaviour
    {
        private const int Canales = 12;
        private static SonidoJuego instancia;
        private static readonly Dictionary<string, AudioClip> clips = new();
        private static readonly Dictionary<string, float> ultimaVez = new();
        private readonly List<AudioSource> fuentes = new();
        private AudioListener oyentePropio;
        private int siguiente;

        public static float Volumen { get; set; } = 1f;
#if UNITY_EDITOR
        public static event System.Action<string, AudioClip> Reproducido;
#endif

        public static void Reproducir(string nombre, float volumen = 1f, float variacionTono = 0f, float separacionMinima = 0.03f)
        {
            if (!Application.isPlaying) return;
            AudioClip clip = Cargar(nombre);
            if (clip == null) return;
            if (ultimaVez.TryGetValue(nombre, out float antes) && Time.unscaledTime - antes < separacionMinima) return;
            ultimaVez[nombre] = Time.unscaledTime;
            Obtener().Tocar(clip, volumen, variacionTono);
#if UNITY_EDITOR
            Reproducido?.Invoke(nombre, clip);
#endif
        }

        public static void ReproducirVariante(string prefijo, int cantidad, float volumen = 1f, float variacionTono = 0.05f)
        {
            Reproducir($"{prefijo}_{Random.Range(1, cantidad + 1)}", volumen, variacionTono, 0f);
        }

        private static AudioClip Cargar(string nombre)
        {
            if (clips.TryGetValue(nombre, out AudioClip clip)) return clip;
            clip = Resources.Load<AudioClip>("Sonidos/" + nombre);
            if (clip == null) Debug.LogWarning($"Sonido no encontrado: Sonidos/{nombre}");
            clips[nombre] = clip;
            return clip;
        }

        private static SonidoJuego Obtener()
        {
            if (instancia != null) return instancia;
            var go = new GameObject("Sonido del juego");
            DontDestroyOnLoad(go);
            instancia = go.AddComponent<SonidoJuego>();
            return instancia;
        }

        private void Awake()
        {
            for (int i = 0; i < Canales; i++)
            {
                AudioSource fuente = gameObject.AddComponent<AudioSource>();
                fuente.playOnAwake = false;
                fuente.spatialBlend = 0f;
                fuente.ignoreListenerPause = true;
                fuentes.Add(fuente);
            }
            SceneManager.sceneLoaded += AlCargarEscena;
            AsegurarOyente();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            if (instancia == this) instancia = null;
        }

        private void AlCargarEscena(Scene escena, LoadSceneMode modo) => AsegurarOyente();

        private void AsegurarOyente()
        {
            if (oyentePropio == null) oyentePropio = gameObject.AddComponent<AudioListener>();
            oyentePropio.enabled = false;
            AudioListener[] oyentes = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            bool hayOtro = false;
            foreach (AudioListener oyente in oyentes)
                if (oyente != oyentePropio && oyente.isActiveAndEnabled) { hayOtro = true; break; }
            oyentePropio.enabled = !hayOtro;
        }

        private void Tocar(AudioClip clip, float volumen, float variacionTono)
        {
            AudioSource fuente = null;
            for (int i = 0; i < fuentes.Count; i++)
            {
                AudioSource candidata = fuentes[(siguiente + i) % fuentes.Count];
                if (candidata.isPlaying) continue;
                fuente = candidata;
                siguiente = (siguiente + i + 1) % fuentes.Count;
                break;
            }
            if (fuente == null)
            {
                fuente = fuentes[siguiente];
                siguiente = (siguiente + 1) % fuentes.Count;
            }
            fuente.Stop();
            fuente.clip = clip;
            fuente.volume = Mathf.Clamp01(volumen * Volumen);
            fuente.pitch = 1f + Random.Range(-variacionTono, variacionTono);
            fuente.Play();
        }
    }
}
