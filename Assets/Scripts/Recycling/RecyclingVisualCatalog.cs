using UnityEngine;
namespace RiftHuntersAR.Recycling
{
    [CreateAssetMenu(menuName="Rift Hunters/Recycling visual catalog")]
    public sealed class RecyclingVisualCatalog : ScriptableObject
    {
        public GameObject plasticBottle, metalCan, paperCardboard, contaminationVFX;
        public GameObject plasticStation, paperStation, metalStation;
        public GameObject Station(int index) => index == 1 ? paperStation : index == 2 ? metalStation : plasticStation;
        public GameObject Waste(int index) => index == 1 ? paperCardboard : index == 2 ? metalCan : plasticBottle;
    }
}
