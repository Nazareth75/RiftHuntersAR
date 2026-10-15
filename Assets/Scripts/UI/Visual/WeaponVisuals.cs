using System;
using UnityEngine;
using UnityEngine.UI;
using RiftHunters.Combate;

namespace RiftHuntersAR.UI.Visual
{
    // Presentation only: selection, damage and magazines belong to GestorArmas.
    public sealed class WeaponVisuals : MonoBehaviour
    {
        public VisualHUD hud;
        public Transform viewRoot;
        public GameObject plasmaView;
        public GameObject[] prefabs;
        public Sprite[] icons;
        public GameObject[] Views { get; private set; }
        public Button[] Selectors { get; private set; }
        GestorArmas manager;
        Arma selected;
        void Start()
        {
            Views = new GameObject[3]; Views[0] = plasmaView;
            for (int i=1;i<3;i++)
            {
                Views[i]=Instantiate(prefabs[i],viewRoot,false);
                Views[i].name=prefabs[i].name;
            }
            Selectors=new Button[3];
            var parent=hud.resultPanel.transform.parent;
            string[] labels={"PLASMA","ELECTRIC","ENERGY"};
            for(int i=0;i<3;i++)
            {
                int index=i;
                var go=new GameObject("Select_"+labels[i],typeof(RectTransform),typeof(Image),typeof(Button));
                go.transform.SetParent(parent,false);
                var rt=go.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=new Vector2(.5f,0);
                rt.sizeDelta=new Vector2(140,140); rt.anchoredPosition=new Vector2((i-1)*170,390);
                var image=go.GetComponent<Image>(); image.sprite=icons[i]; image.preserveAspect=true;
                var button=go.GetComponent<Button>(); button.targetGraphic=image;
                button.onClick.AddListener(()=>hud.controller.SeleccionarArma(index)); Selectors[i]=button;
                var caption=new GameObject("Label",typeof(RectTransform),typeof(Text)); caption.transform.SetParent(go.transform,false);
                var cr=caption.GetComponent<RectTransform>(); cr.anchorMin=cr.anchorMax=new Vector2(.5f,0);
                cr.sizeDelta=new Vector2(155,38); cr.anchoredPosition=new Vector2(0,-21);
                var text=caption.GetComponent<Text>(); text.font=hud.ammoValue.font; text.fontSize=23;
                text.alignment=TextAnchor.MiddleCenter; text.text=labels[i]; text.raycastTarget=false;
            }
            Refresh();
        }
        void Update()
        {
            var current=hud.controller!=null?hud.controller.Armas:null;
            if(manager!=current)
            {
                if(manager!=null) manager.ArmaCambiada-=Refresh;
                manager=current; if(manager!=null) manager.ArmaCambiada+=Refresh;
                Refresh();
            }
            if(manager!=null && selected!=manager.Actual) Refresh();
            bool usable=hud.controller!=null && hud.controller.CurrentState=="WAVE" && manager!=null && !manager.EstaPausado;
            bool finished=hud.controller!=null && (hud.controller.CurrentState=="GAME_OVER" || hud.controller.CurrentState=="VICTORY");
            viewRoot.gameObject.SetActive(!finished);
            if(Selectors!=null) foreach(var button in Selectors)
            {
                button.gameObject.SetActive(hud.controller!=null && hud.controller.CurrentState=="WAVE");
                button.interactable=usable && !manager.Actual.EstaRecargando;
            }
        }
        public void Refresh()
        {
            selected=manager!=null?manager.Actual:null;
            int index=selected is ArmaElectrica?1:selected is ArmaExplosiva?2:0;
            if(Views!=null) for(int i=0;i<Views.Length;i++) Views[i].SetActive(i==index);
            if(Selectors!=null) for(int i=0;i<Selectors.Length;i++) Selectors[i].GetComponent<Image>().color=i==index?Color.white:new Color(.45f,.55f,.65f);
        }
        void OnDestroy() { if(manager!=null) manager.ArmaCambiada-=Refresh; }
    }
}
