using UnityEngine;
using UnityEngine.SceneManagement;
using RiftHuntersAR.Audio;
namespace RiftHuntersAR.UI.Visual
{
    public sealed class VisualMainMenu : MonoBehaviour
    {
        public void StartGame()
        {
            SonidoJuego.Reproducir("ui_confirmar", 0.9f);
            SceneManager.LoadScene("ARGame");
        }
        public void Quit()
        {
            SonidoJuego.Reproducir("ui_click", 0.8f);
            Application.Quit();
        }
    }
}
