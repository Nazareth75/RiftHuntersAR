using UnityEngine;
using RiftHuntersAR.Integration;

namespace RiftHuntersAR.Recycling
{
    // One bounded container per AR surface; preserve waste after combat, clear on Retry.
    public sealed class PollutionZone : MonoBehaviour
    {
        public const int MaxResidues=RiftHunters.Game.BalancePartida.MaximosResiduos;
        ARPrototypeController controller;
        string previousState;
        private readonly System.Collections.Generic.HashSet<RecyclableItem> residuos = new();
        public bool HasSpace => residuos.Count < MaxResidues;
        public int Cantidad => residuos.Count;
        public void Registrar(RecyclableItem item) { if (item != null) residuos.Add(item); }
        public void Retirar(RecyclableItem item) { residuos.Remove(item); }
        void Start() { controller=FindFirstObjectByType<ARPrototypeController>(); }
        void Update()
        {
            if(controller==null) return;
            string state=controller.CurrentState;
            if((previousState=="GAME_OVER" || previousState=="VICTORY") && state=="COUNTDOWN") Clear();
            previousState=state;
        }
        public void Clear()
        {
            residuos.Clear();
            for(int i=transform.childCount-1;i>=0;i--) { var hijo = transform.GetChild(i).gameObject; hijo.SetActive(false); Destroy(hijo); }
        }
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
