using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ReproductorAnimacion : MonoBehaviour
{
    [SerializeField] private string escenaSiguiente = "PartidaIntegrada";
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private bool permitirSaltar = true;

    private bool yaCargando = false;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += AlTerminar;
    }

    void Update()
    {
        if (!permitirSaltar || yaCargando) return;

        bool clic = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool toque = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (clic || toque)
            CargarSiguiente();
    }

    void AlTerminar(VideoPlayer vp)
    {
        CargarSiguiente();
    }

    void CargarSiguiente()
    {
        if (yaCargando) return;
        yaCargando = true;

        videoPlayer.loopPointReached -= AlTerminar;
        SceneManager.LoadScene(escenaSiguiente);
    }
}