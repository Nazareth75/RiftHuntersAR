using UnityEngine;
using UnityEngine.UI;
using RiftHuntersAR.UI.Visual;

namespace RiftHuntersAR.Recycling
{
    [DefaultExecutionOrder(200)]
    public sealed class RecyclingHUD : MonoBehaviour
    {
        RecyclingGameplay flow;
        VisualHUD hud;
        public Text EcoLabel {get;private set;}
        public Text InstructionLabel {get;private set;}
        public Text FeedbackLabel {get;private set;}
        public Text PendingLabel {get;private set;}
        Button rescan;
        public void Configure(RecyclingGameplay value,VisualHUD presentation)
        {
            flow=value;hud=presentation;
            var safe=hud.resultPanel.transform.parent;
            EcoLabel=Label(safe,"EcoPoints",new Vector2(0,-355),new Vector2(460,34),30,new Color(.3f,1,.65f));
            PendingLabel=Label(safe,"RecoveryPending",new Vector2(0,-425),new Vector2(750,60),38,Color.white);
            InstructionLabel=Label(safe,"RecoveryInstruction",new Vector2(0,-660),new Vector2(860,140),44,new Color(.35f,1,.7f));
            FeedbackLabel=Label(safe,"RecoveryFeedback",new Vector2(0,-810),new Vector2(820,110),40,Color.white);
            var buttonRoot=new GameObject("RecoveryRescan",typeof(RectTransform),typeof(Image),typeof(Button));buttonRoot.transform.SetParent(safe,false);
            var rt=buttonRoot.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,0);rt.anchoredPosition=new Vector2(0,560);rt.sizeDelta=new Vector2(660,100);
            buttonRoot.GetComponent<Image>().color=new Color(.025f,.12f,.15f,.9f);
            rescan=buttonRoot.GetComponent<Button>();rescan.onClick.AddListener(()=>UnityEngine.SceneManagement.SceneManager.LoadScene("ARGame"));
            var caption=Label(buttonRoot.transform,"Label",Vector2.zero,new Vector2(620,90),35,Color.white);caption.rectTransform.anchorMin=caption.rectTransform.anchorMax=new Vector2(.5f,.5f);caption.text="VOLVER A ESCANEAR (REINICIA PARTIDA)";
            buttonRoot.SetActive(false);
        }
        Text Label(Transform parent,string name,Vector2 position,Vector2 size,int font,Color color)
        {
            var root=new GameObject(name,typeof(RectTransform),typeof(Text));root.transform.SetParent(parent,false);
            var rt=root.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.anchoredPosition=position;rt.sizeDelta=size;
            var text=root.GetComponent<Text>();text.font=hud.scoreValue.font;text.fontSize=font;text.alignment=TextAnchor.MiddleCenter;text.color=color;text.raycastTarget=false;
            text.resizeTextForBestFit=true;text.resizeTextMinSize=24;text.resizeTextMaxSize=font;text.verticalOverflow=VerticalWrapMode.Truncate;
            var shadow=root.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.95f);shadow.effectDistance=new Vector2(2,-2);
            return text;
        }
        void LateUpdate()
        {
            if(flow==null || hud==null || EcoLabel==null)return;
            EcoLabel.text="ECO "+flow.EcoPoints;EcoLabel.gameObject.SetActive(hud.controller.CurrentState!="SCANNING");
            bool active=flow.Active;
            rescan.gameObject.SetActive(active && flow.InputAvailable && flow.Stations.Count==0 && flow.PendingCount>0);
            InstructionLabel.gameObject.SetActive(active);PendingLabel.gameObject.SetActive(active);FeedbackLabel.gameObject.SetActive(active);
            if(active)
            {
                InstructionLabel.text=flow.Instruction;PendingLabel.text="RESIDUOS PENDIENTES: "+flow.PendingCount;
                FeedbackLabel.text=flow.Feedback;
                FeedbackLabel.color=flow.Feedback.StartsWith("CONTENEDOR")?new Color(1,.5f,.3f):new Color(.4f,1,.7f);
                hud.stateValue.text="";hud.message.gameObject.SetActive(false);
            }
            hud.shoot.gameObject.SetActive(!active);hud.reload.gameObject.SetActive(!active);
        }
    }
}
