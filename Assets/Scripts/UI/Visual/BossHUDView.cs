using UnityEngine;
using UnityEngine.UI;
namespace RiftHuntersAR.UI.Visual
{
    public sealed class BossHUDView : MonoBehaviour
    {
        public Image healthFill, left, core, right;
        // Persona 2/3 supplies real boss data later. This component only paints it.
        public void Present(float healthFraction,bool leftActive,bool coreActive,bool rightActive)
        {
            healthFill.fillAmount = Mathf.Clamp01(healthFraction);
            left.color = leftActive ? Color.white : Color.gray;
            core.color = coreActive ? Color.white : Color.gray;
            right.color = rightActive ? Color.white : Color.gray;
        }
    }
}
