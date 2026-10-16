using UnityEngine;
using RiftHuntersAR.Integration;

namespace RiftHuntersAR.Recycling
{
    // One bounded container per AR surface; preserve waste after combat, clear on Retry.
    public sealed class PollutionZone : MonoBehaviour
    {
        public const int MaxResidues=32;
        ARPrototypeController controller;
        string previousState;
        public bool HasSpace => GetComponentsInChildren<RecyclableItem>().Length < MaxResidues;
        void Start() { controller=FindFirstObjectByType<ARPrototypeController>(); }
        void Update()
        {
            if(controller==null) return;
            string state=controller.CurrentState;
            if((previousState=="GAME_OVER" || previousState=="VICTORY") && state=="COUNTDOWN") Clear();
            previousState=state;
        }
        public void Clear() { for(int i=transform.childCount-1;i>=0;i--) Destroy(transform.GetChild(i).gameObject); }
        public static PollutionZone For(Transform surface)
        {
            if(surface!=null)
            {
                var existing=surface.Find("Pollution - visual residues");
                if(existing!=null) return existing.GetComponent<PollutionZone>();
            }
            var root=new GameObject("Pollution - visual residues");root.transform.SetParent(surface,false);
            return root.AddComponent<PollutionZone>();
        }
    }
}
