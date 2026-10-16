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
        Button confirmar, usarBotones, reubicar;
        readonly Button[] categorias = new Button[3];
        public void Configure(RecyclingGameplay value,VisualHUD presentation)
        {
            flow=value;hud=presentation;
            var safe=hud.resultPanel.transform.parent;
            EcoLabel=Label(safe,"EcoPoints",new Vector2(0,-355),new Vector2(460,34),30,new Color(.3f,1,.65f));
            PendingLabel=Label(safe,"RecoveryPending",new Vector2(0,-425),new Vector2(750,60),38,Color.white);
            InstructionLabel=Label(safe,"RecoveryInstruction",new Vector2(0,-660),new Vector2(860,140),44,new Color(.35f,1,.7f));
            FeedbackLabel=Label(safe,"RecoveryFeedback",new Vector2(0,-810),new Vector2(820,110),40,Color.white);
            string[] nombres = { "PLÁSTICO", "PAPEL", "METAL" };
            for (int i = 0; i < categorias.Length; i++)
            {
                int indice = i;
                var objeto = new GameObject("Clasificar_"+i, typeof(RectTransform), typeof(Image), typeof(Button));
                objeto.transform.SetParent(safe, false);
                var rect = objeto.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0);
                rect.anchoredPosition = new Vector2((i-1)*280, 480); rect.sizeDelta = new Vector2(260, 100);
                objeto.GetComponent<Image>().color = new Color(.025f, .15f, .18f, .95f);
                categorias[i] = objeto.GetComponent<Button>();
                categorias[i].onClick.AddListener(() => flow.ClasificarCategoria((RecyclingCategory)indice));
                var texto = Label(objeto.transform, "Nombre", Vector2.zero, new Vector2(250, 90), 32, Color.white);
                texto.rectTransform.anchorMin = texto.rectTransform.anchorMax = new Vector2(.5f, .5f);
                objeto.SetActive(false);
            }
            confirmar=CrearBoton(safe,"COLOCAR AQUÍ",480,()=>flow.ConfirmarContenedores());
            usarBotones=CrearBoton(safe,"USAR BOTONES",350,()=>flow.UsarBotonesClasificacion());
            reubicar=CrearBoton(safe,"REUBICAR CONTENEDORES",350,()=>flow.ReubicarContenedores());
        }
        Button CrearBoton(Transform padre,string nombre,float altura,UnityEngine.Events.UnityAction accion)
        {
            var objeto=new GameObject(nombre,typeof(RectTransform),typeof(Image),typeof(Button));
            objeto.transform.SetParent(padre,false);
            var rect=objeto.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);
            rect.anchoredPosition=new Vector2(0,altura);rect.sizeDelta=new Vector2(680,100);
            objeto.GetComponent<Image>().color=new Color(.025f,.22f,.27f,.97f);
            objeto.AddComponent<Outline>().effectColor=new Color(.2f,.85f,1f);
            var texto=Label(objeto.transform,"Texto",Vector2.zero,new Vector2(650,90),32,Color.white);
            texto.rectTransform.anchorMin=texto.rectTransform.anchorMax=new Vector2(.5f,.5f);
            var boton=objeto.GetComponent<Button>();boton.onClick.AddListener(accion);objeto.SetActive(false);return boton;
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
            confirmar.gameObject.SetActive(flow.EsperandoColocacion);
            confirmar.interactable=flow.PuedeConfirmarContenedores;
            usarBotones.gameObject.SetActive(flow.EsperandoColocacion);
            usarBotones.interactable=hud.controller.SeguimientoDisponible;
            reubicar.gameObject.SetActive(active && flow.InputAvailable && flow.PendingCount>0);
            reubicar.interactable=hud.controller.SeguimientoDisponible;
            foreach (Button boton in categorias)
            {
                boton.gameObject.SetActive(flow.ClasificacionCompacta && flow.PendingCount > 0);
                boton.interactable = flow.InputAvailable && flow.Selected != null;
            }
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
