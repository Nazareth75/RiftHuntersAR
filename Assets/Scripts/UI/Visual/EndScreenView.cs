using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RiftHuntersAR.UI.Visual
{
    // Presentation only: the existing HUD owns result state and button callbacks.
    [DefaultExecutionOrder(100)]
    public sealed class EndScreenView : MonoBehaviour
    {
        public VisualHUD hud;
        public Sprite[] gameOverSprites, victorySprites;
        public TMP_FontAsset font;
        public RectTransform card;
        public TextMeshProUGUI title, points, enemies, elapsed;
        Image heading, stats, portal, crystals;
        Sprite redPortal, bluePortal;
        readonly System.Collections.Generic.Dictionary<Graphic,bool> hidden=new();
        bool built;
        RectTransform Rect(string name,Transform parent,Vector2 center,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform;rt.anchorMin=center-size/2;rt.anchorMax=center+size/2;rt.offsetMin=rt.offsetMax=Vector2.zero;return rt;
        }
        Image Picture(string name,Transform parent,Vector2 center,Vector2 size,Sprite sprite=null)
        {
            var image=Rect(name,parent,center,size).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;
        }
        TextMeshProUGUI Label(string name,Transform parent,Vector2 center,Vector2 size,int fontSize,string text)
        {
            var tmp=Rect(name,parent,center,size).gameObject.AddComponent<TextMeshProUGUI>();tmp.font=font;tmp.fontSize=fontSize;tmp.enableAutoSizing=true;tmp.fontSizeMin=16;tmp.fontSizeMax=fontSize;tmp.alignment=TextAlignmentOptions.Center;tmp.text=text;tmp.raycastTarget=false;tmp.color=new Color(.92f,.98f,1);return tmp;
        }
        void Position(RectTransform rt,Transform parent,Vector2 center,Vector2 size)
        {rt.SetParent(parent,false);rt.anchorMin=center-size/2;rt.anchorMax=center+size/2;rt.offsetMin=rt.offsetMax=Vector2.zero;rt.localScale=Vector3.one;}
        void Button(Button button,Vector2 center,string label)
        {
            Position((RectTransform)button.transform,card,center,new Vector2(.86f,.105f));
            foreach(var graphic in button.GetComponentsInChildren<Graphic>(true))if(graphic!=button.targetGraphic)graphic.enabled=false;
            var cover=Picture("CaptionBacking",button.transform,new Vector2(.665f,.5f),new Vector2(.53f,.61f));cover.color=new Color(.008f,.014f,.025f); Label(label,button.transform,new Vector2(.665f,.5f),new Vector2(.53f,.61f),43,label);
        }
        void Build()
        {
            if(built || hud==null || font==null)return;built=true;
            var root=(RectTransform)hud.resultPanel.transform;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            foreach(Transform child in root)if(child!=hud.retry.transform && child!=hud.mainMenu.transform && child!=hud.resultTitle.transform && child!=hud.detalleFin.transform)child.gameObject.SetActive(false);
            var backdrop=hud.resultPanel.GetComponent<Image>();if(backdrop!=null){backdrop.sprite=null;backdrop.color=new Color(.008f,.012f,.024f,.76f);backdrop.raycastTarget=true;}
            redPortal=CropPortal(gameOverSprites[2]);bluePortal=CropPortal(victorySprites[2]);
            card=Rect("EndScreenCard",root,new Vector2(.5f,.5f),new Vector2(.94f,.94f));
            var fit=card.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=1000f/1450;
            portal=Picture("Portal",card,new Vector2(.5f,.105f),new Vector2(.98f,.24f));
            crystals=Picture("Crystals",card,new Vector2(.5f,.83f),new Vector2(.98f,.31f));crystals.color=Color.clear;
            heading=Picture("TitleFrame",card,new Vector2(.5f,.84f),new Vector2(.98f,.22f));
            // Mask baked atlas lettering; live TMP supplies all title/stat text.
            var mask=Picture("TitleTextBacking",heading.transform,new Vector2(.5f,.5f),new Vector2(.77f,.52f));mask.color=new Color(.02f,.005f,.015f);
            title=Label("DynamicTitle",heading.transform,new Vector2(.5f,.5f),new Vector2(.77f,.6f),88,"");title.fontStyle=FontStyles.Bold;
            stats=Picture("StatsFrame",card,new Vector2(.5f,.585f),new Vector2(.90f,.285f));
            mask=Picture("StatsTextBacking",stats.transform,new Vector2(.58f,.5f),new Vector2(.62f,.72f));mask.color=new Color(.014f,.012f,.026f);
            string[] names={"PUNTOS:","ENEMIGOS:","TIEMPO:"};TextMeshProUGUI[] values=new TextMeshProUGUI[3];
            for(int i=0;i<3;i++){float y=.73f-i*.24f;var label=Label(names[i],stats.transform,new Vector2(.44f,y),new Vector2(.34f,.17f),37,names[i]);label.alignment=TextAlignmentOptions.Left;values[i]=Label("Value"+i,stats.transform,new Vector2(.76f,y),new Vector2(.26f,.17f),42,"");}
            points=values[0];enemies=values[1];elapsed=values[2];
            Button(hud.retry,new Vector2(.5f,.345f),"REINTENTAR");Button(hud.mainMenu,new Vector2(.5f,.205f),"MENÚ PRINCIPAL");
            // Keep legacy public fields for existing adapters/tests; suppress their rendering.
            Position((RectTransform)hud.resultTitle.transform,card,new Vector2(.5f,.84f),new Vector2(.76f,.12f));hud.resultTitle.enabled=false;
            Position((RectTransform)hud.detalleFin.transform,card,new Vector2(.5f,.585f),new Vector2(.66f,.24f));hud.detalleFin.enabled=false;
        }
        Sprite CropPortal(Sprite source)
        {
            // The supplied decorative crop includes an atlas-button edge at its top.
            // Use only the portal region; keep the source PNG unchanged and versioned.
            var rect=source.rect;rect.height*=.89f;
            return Sprite.Create(source.texture,rect,new Vector2(.5f,.5f),source.pixelsPerUnit);
        }
        void OnDestroy(){if(redPortal!=null)Destroy(redPortal);if(bluePortal!=null)Destroy(bluePortal);}
        void LateUpdate()
        {
            Build();if(!built)return;
            bool finished=hud.resultPanel.activeSelf;
            if(finished){hud.resultPanel.transform.SetAsLastSibling();foreach(var graphic in hud.GetComponentsInChildren<Graphic>(true)){if(graphic.transform.IsChildOf(hud.resultPanel.transform))continue;if(!hidden.ContainsKey(graphic))hidden[graphic]=graphic.enabled;graphic.enabled=false;}}
            else if(hidden.Count>0){foreach(var entry in hidden)if(entry.Key!=null)entry.Key.enabled=entry.Value;hidden.Clear();}
            if(!finished)return;
            bool win=hud.resultTitle.text=="VICTORIA";var sprites=win?victorySprites:gameOverSprites;
            if(sprites==null || sprites.Length!=6)return;
            heading.sprite=sprites[0];stats.sprite=sprites[1];portal.sprite=win?bluePortal:redPortal;crystals.sprite=sprites[3];
            foreach(var pair in new[]{hud.retry,hud.mainMenu}){pair.image.color=Color.white;pair.image.type=Image.Type.Simple;}
            hud.retry.image.sprite=sprites[4];hud.mainMenu.image.sprite=sprites[5];
            // Cover baked button captions while preserving icon/frame and real Button component.
            title.text=hud.resultTitle.text;title.color=win?new Color(.57f,1,.76f):new Color(1,.24f,.38f);
            var c=hud.controller;points.text=(hud.score!=null?hud.score.Score:0).ToString();enemies.text=(c!=null?c.EnemigosDerrotados:0).ToString();
            int seconds=Mathf.Max(0,Mathf.FloorToInt(c!=null?c.TiempoPartida:0));elapsed.text=$"{seconds/60:00}:{seconds%60:00}";
        }
    }
}
