using UnityEngine;
using UnityEngine.SceneManagement;
namespace RiftHuntersAR.UI.Visual
{
    public sealed class VisualMainMenu : MonoBehaviour
    {
        public void StartGame() { SceneManager.LoadScene("ARGame"); }
        public void Quit() { Application.Quit(); }
    }
}
