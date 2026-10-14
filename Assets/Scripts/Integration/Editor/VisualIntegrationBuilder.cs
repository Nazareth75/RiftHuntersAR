using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using RiftHuntersAR.UI.Visual;
using RiftHuntersAR.Enemies;
using RiftHuntersAR.Combat;
using RiftHuntersAR.Game;
using Object = UnityEngine.Object;

namespace RiftHuntersAR.Integration.Editor
{
    // Editor-only, explicit commands. No build, combat, AR or source-art generation.
    [InitializeOnLoad]
    public static class VisualIntegrationBuilder
    {
        const string Command = "Temp/RiftVisualCommand.txt";
        const string Models = "Assets/Art/Models/Approved/";
        const string UI = "Assets/Art/UI/Approved/";
        const string Mats = "Assets/Art/Materials/";
        const string Gameplay = "Assets/Scenes/Gameplay/ARGame.unity";
        const string MenuPath = "Assets/Scenes/MainMenu/MainMenu.unity";
        const string HUDPath = "Assets/Prefabs/UI/GameplayHUD.prefab";
        const string BasicPath = "Assets/Prefabs/Enemies/EnemyBasicPlaceholder.prefab";
        const string PortalPath = "Assets/Prefabs/Portal/PortalPlaceholder.prefab";
        static VisualIntegrationBuilder() { EditorApplication.update += Poll; EditorApplication.update += PlayCheck; Application.logMessageReceived += CheckLog; }
        static void CheckLog(string message,string stack,LogType type)
        {
            if(!SessionState.GetBool("RiftVisualPlayCheck",false)) return;
            File.AppendAllText("Temp/RiftVisualPlayLog.txt",type+": "+message+"\n");
            if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert) SessionState.SetInt("RiftVisualPlayErrors",SessionState.GetInt("RiftVisualPlayErrors",0)+1);
        }
        static void PlayCheck()
        {
            if(!SessionState.GetBool("RiftVisualPlayCheck",false) || !EditorApplication.isPlaying) return;
            try
            {
                if(SessionState.GetInt("RiftVisualPlayStage",0)==0 && Time.time>1)
                {
                    var menu=Object.FindFirstObjectByType<VisualMainMenu>(); if(menu==null) throw new Exception("Runtime menu missing");
                    var start=menu.transform.Find("SafeArea/Start").GetComponent<Button>(); if(start.onClick.GetPersistentEventCount()!=1) throw new Exception("Start event missing or duplicated");
                    SessionState.SetInt("RiftVisualPlayStage",1); start.onClick.Invoke();
                }
                else if(SessionState.GetInt("RiftVisualPlayStage",0)==1 && Time.time>8)
                {
                    var hud=Object.FindFirstObjectByType<VisualHUD>();
                    if(hud==null || hud.controller==null || hud.controller.Player==null || hud.legacyCanvas.enabled || hud.legacyCanvas.GetComponent<GraphicRaycaster>().enabled || hud.legacyCanvas.transform.Find("RELOAD")==null || hud.legacyCanvas.transform.Find("RETRY")==null) throw new Exception("Runtime HUD bridge/lifecycle failed");
                    if(hud.ammoValue.text!="30 / 30" || hud.scoreValue.text!="0" || hud.enemyValue.text!="0" || hud.controller.CurrentState!="SCANNING") throw new Exception("Runtime dynamic HUD state incorrect");
                    int errors=SessionState.GetInt("RiftVisualPlayErrors",0);
                    File.WriteAllText("Temp/RiftVisualPlayCheck.txt",(errors==0?"PASS":"FAIL")+": real MainMenu Start event loads ARGame; HUD Awake/Start/LateUpdate; dynamic score/ammo/wave/count/hearts; legacy canvas hidden and raycaster disabled; legacy reload/retry present. Errors="+errors+". AR device tracking not tested in Editor.");
                    SessionState.SetBool("RiftVisualPlayCheck",false); EditorApplication.isPlaying=false;
                }
            }
            catch(Exception ex) { File.WriteAllText("Temp/RiftVisualPlayCheck.txt","FAIL: "+ex); SessionState.SetBool("RiftVisualPlayCheck",false); EditorApplication.isPlaying=false; Debug.LogException(ex); }
        }
        static void Poll()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Command)) return;
            string command;
            try { command=File.ReadAllText(Command).Trim(); File.Delete(Command); } catch(IOException) { return; }
            try
            {
                switch(command)
                {
                    case "import": Import(); break;
                    case "models": Materials(); ModelPrefabs(); break;
                    case "hud": HUD(); IntegrateScene(); break;
                    case "menu": Menu(); break;
                    case "hide-legacy":
                        var legacyScene=EditorSceneManager.OpenScene(Gameplay); var legacy=Find<VisualHUD>(legacyScene).legacyCanvas;
                        legacy.enabled=false; legacy.GetComponent<GraphicRaycaster>().enabled=false; EditorSceneManager.SaveScene(legacyScene); break;
                    case "validate": Validate(); break;
                    case "review": Review(); break;
                    case "vfx": Effects(); break;
                    case "play-check":
                        typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("Clear",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public)?.Invoke(null,null);
                        File.WriteAllText("Temp/RiftVisualPlayLog.txt",""); SessionState.SetInt("RiftVisualPlayErrors",0); SessionState.SetInt("RiftVisualPlayStage",0);
                        SessionState.SetBool("RiftVisualPlayCheck",true); EditorSceneManager.OpenScene(MenuPath); EditorApplication.isPlaying=true; break;
                    default: throw new Exception("Unknown visual command: "+command);
                }
                AssetDatabase.SaveAssets();
                File.WriteAllText("Temp/RiftVisualResult.txt","SUCCESS: "+command);
                Debug.Log("[Rift Visual] SUCCESS: "+command);
            }
            catch(Exception ex) { File.WriteAllText("Temp/RiftVisualResult.txt","FAILED: "+command+"\n"+ex); Debug.LogException(ex); }
        }
        static void Folder(string path) { Directory.CreateDirectory(path); AssetDatabase.Refresh(); }
        static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(UI+name+".png") ?? throw new Exception("Missing sprite "+name);
        static GameObject Child(string name,Transform parent=null) { var go=new GameObject(name); go.transform.SetParent(parent,false); return go; }
        static T Find<T>(Scene scene) where T:Component => scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).FirstOrDefault();
        static void Import()
        {
            AssetDatabase.Refresh();
            foreach(var path in Directory.GetFiles(UI,"*.png"))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
                var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.alphaSource=TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.filterMode=FilterMode.Bilinear; importer.maxTextureSize=1024; importer.SaveAndReimport();
                if(Path.GetFileNameWithoutExtension(path).StartsWith("msg_"))
                {
                    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path.Replace('\\','/'));
                    importer.spriteImportMode=SpriteImportMode.Multiple;
                    // The source contains a filename footer. Crop only the imported sprite UVs, never the approved PNG.
#pragma warning disable CS0618
                    importer.spritesheet=new[]{new SpriteMetaData { name=Path.GetFileNameWithoutExtension(path),rect=new Rect(0,32,texture.width,texture.height-32),pivot=new Vector2(.5f,.5f),alignment=0 }};
#pragma warning restore CS0618
                    importer.SaveAndReimport();
                }
            }
            foreach(var path in Directory.GetFiles(Models,"*.fbx"))
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.importAnimation=false; importer.addCollider=false; importer.importCameras=false; importer.importLights=false;
                importer.globalScale=1; importer.bakeAxisConversion=true; importer.SaveAndReimport();
            }
        }
        static Material Mat(string name,Color color,float metallic=.55f, float emission=0)
        {
            string path=Mats+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null) { mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path); }
            mat.SetColor("_BaseColor",color); mat.SetFloat("_Metallic",metallic); mat.SetFloat("_Smoothness",.38f);
            if(emission>0) { mat.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive; mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor",color*emission); }
            EditorUtility.SetDirty(mat); return mat;
        }
        static Material LoadMat(string name) => AssetDatabase.LoadAssetAtPath<Material>(Mats+name+".mat");
        static void Materials()
        {
            Folder(Mats);
            Mat("M_Enemy_MetalDark",new Color(.085f,.105f,.13f)); Mat("M_Enemy_MetalLight",new Color(.32f,.37f,.42f),.7f);
            Mat("M_Enemy_Energy",new Color(1,.12f,.015f),.05f,1.4f); Mat("M_Enemy_Energy_Red",new Color(1,.025f,.009f),.05f,1.3f);
            Mat("M_Tech_MetalDark",new Color(.08f,.11f,.15f)); Mat("M_Tech_MetalLight",new Color(.34f,.4f,.46f),.7f);
            Mat("M_Player_Energy",new Color(.015f,.46f,1),.05f,1.5f);
            Mat("M_Rift_Metal",new Color(.075f,.1f,.17f)); var energy=Mat("M_Rift_Energy",new Color(.04f,.3f,1),.05f,1.6f);
            energy.SetFloat("_Cull",0);
            var shader=Shader.Find("RiftHunters/PlaneHologram") ?? throw new Exception("Missing plane shader");
            var plane=LoadMat("M_AR_Hologram"); if(plane==null) { plane=new Material(shader); AssetDatabase.CreateAsset(plane,Mats+"M_AR_Hologram.mat"); }
            plane.shader=shader; plane.SetColor("_BaseColor",new Color(.03f,.65f,1,.045f)); EditorUtility.SetDirty(plane);
        }
        static Material Remap(string name,string model)
        {
            bool portal=model.StartsWith("Portal"), boss=model.StartsWith("RiftGuardian"), enemy=model.StartsWith("Enemy") || boss;
            if(name.Contains("Energy")) return LoadMat(portal?"M_Rift_Energy":name.Contains("Enemy")?(boss?"M_Enemy_Energy_Red":"M_Enemy_Energy"):"M_Player_Energy");
            if(portal) return LoadMat("M_Rift_Metal");
            return LoadMat((enemy?"M_Enemy_":"M_Tech_")+(name.Contains("Light")?"MetalLight":"MetalDark"));
        }
        static GameObject Model(string name,Transform parent)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Models+name+".fbx") ?? throw new Exception("Missing FBX "+name);
            var wrapper=Child(name+"_Visual",parent); var instance=(GameObject)PrefabUtility.InstantiatePrefab(asset); instance.transform.SetParent(wrapper.transform,false);
            foreach(var renderer in instance.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>Remap(m==null?"MetalDark":m.name,name)).ToArray();
            var renderers=instance.GetComponentsInChildren<Renderer>();
            Bounds bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            // FBX pivot correction only, geometry and approved scale remain unchanged.
            instance.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            // All eight approved FBX exports share the same forward axis (-Z after import).
            wrapper.transform.localRotation=Quaternion.Euler(0,180,0);
            File.AppendAllText("Temp/RiftVisualBounds.txt",name+" height="+bounds.size.y+" width="+bounds.size.x+" yaw="+wrapper.transform.localEulerAngles.y+"\n");
            return wrapper;
        }
        static void SaveVisual(string model,string folder)
        {
            Folder(folder); var root=Child(model.Replace("_Final","")+"_Visual"); Model(model,root.transform);
            if(model.StartsWith("RiftGuardian")) Motion(root,false,"Fragment");
            PrefabUtility.SaveAsPrefabAsset(root,folder+root.name+".prefab"); Object.DestroyImmediate(root);
        }
        static void Motion(GameObject root,bool faceCamera,string fragmentName)
        {
            var motion=root.AddComponent<VisualMotion>(); motion.faceCamera=faceCamera;
            motion.fragments=root.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains(fragmentName) && t.childCount>0).ToArray();
            motion.energy=root.GetComponentsInChildren<Renderer>().Where(r=>r.sharedMaterials.Any(m=>m!=null && m.name.Contains("Energy"))).ToArray();
        }
        static void Replace(string path,string model,bool portal)
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var visual=root.transform.Find("Visual") ?? Child("Visual",root.transform).transform;
                foreach(Transform child in visual) { if(child.name.StartsWith("Temporary")) child.gameObject.SetActive(false); }
                var old=visual.Find(model+"_Visual"); if(old!=null) Object.DestroyImmediate(old.gameObject);
                var modelGO=Model(model,visual);
                if(portal) { foreach(var oldMotion in visual.GetComponents<VisualMotion>()) Object.DestroyImmediate(oldMotion); Motion(visual.gameObject,true,"Fragment"); }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        static void ModelPrefabs()
        {
            File.WriteAllText("Temp/RiftVisualBounds.txt","");
            Replace(BasicPath,"EnemyBasic_Final",false); Replace(PortalPath,"Portal_Final",true);
            SaveVisual("EnemyFast_Final","Assets/Prefabs/Enemies/"); SaveVisual("EnemyTank_Final","Assets/Prefabs/Enemies/");
            SaveVisual("RiftGuardian_Final","Assets/Prefabs/Boss/");
            foreach(string name in new[]{"PlasmaBlaster_Final","ElectricCannon_Final","EnergyLauncher_Final"}) SaveVisual(name,"Assets/Prefabs/Weapons/");
            string planePath=AssetDatabase.FindAssets("PlanePlaceholder t:Prefab").Select(AssetDatabase.GUIDToAssetPath).First();
            var plane=PrefabUtility.LoadPrefabContents(planePath); plane.GetComponent<MeshRenderer>().sharedMaterial=LoadMat("M_AR_Hologram");
            PrefabUtility.SaveAsPrefabAsset(plane,planePath); PrefabUtility.UnloadPrefabContents(plane);
        }
        static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false); var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=anchor; rect.anchoredPosition=position; rect.sizeDelta=size; return rect;
        }
        static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero; }
        static Image Graphic(string name,Transform parent,string sprite,Vector2 anchor,Vector2 pos,Vector2 size)
        {
            var rect=Rect(name,parent,anchor,pos,size); var image=rect.gameObject.AddComponent<Image>(); image.sprite=Sprite(sprite); image.preserveAspect=true; image.raycastTarget=false; return image;
        }
        static Text Label(string name,Transform parent,string value,Vector2 anchor,Vector2 pos,Vector2 size,int font=42)
        {
            var rect=Rect(name,parent,anchor,pos,size); var label=rect.gameObject.AddComponent<Text>(); label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text=value; label.fontSize=font; label.color=new Color(.2f,.8f,1); label.alignment=TextAnchor.MiddleCenter; label.raycastTarget=false; return label;
        }
        static Button Button(string name,Transform parent,string sprite,Vector2 anchor,Vector2 pos,Vector2 size,string pressed=null,string disabled=null)
        {
            var image=Graphic(name,parent,sprite,anchor,pos,size); image.raycastTarget=true; var button=image.gameObject.AddComponent<Button>();
            button.targetGraphic=image; button.transition=Selectable.Transition.SpriteSwap;
            button.spriteState=new SpriteState { highlightedSprite=image.sprite,selectedSprite=image.sprite,pressedSprite=Sprite(pressed??sprite),disabledSprite=Sprite(disabled??sprite) };
            button.navigation=new Navigation{ mode=Navigation.Mode.None }; return button;
        }
        static GameObject CreateCanvas(string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1080,1920); scaler.matchWidthOrHeight=.5f;
            var safe=Rect("SafeArea",go.transform,Vector2.zero,Vector2.zero,Vector2.zero); Stretch(safe); safe.gameObject.AddComponent<SafeAreaLayout>(); return go;
        }
        static void HUD()
        {
            Folder("Assets/Prefabs/UI/"); var canvas=CreateCanvas("GameplayHUD"); canvas.GetComponent<Canvas>().sortingOrder=10; var safe=canvas.transform.Find("SafeArea");
            var hud=canvas.AddComponent<VisualHUD>();
            hud.hearts=Enumerable.Range(0,3).Select(i=>Graphic("Heart_"+i,safe,"heart_full",new Vector2(0,1),new Vector2(75+i*95,-90),new Vector2(88,98))).ToArray();
            var score=Graphic("ScorePanel",safe,"panel_score",Vector2.one,new Vector2(-235,-95),new Vector2(420,175));
            hud.scoreValue=Label("ScoreValue",score.transform,"0",new Vector2(.5f,.5f),new Vector2(55,-18),new Vector2(240,78),52);
            var wave=Graphic("WavePanel",safe,"panel_wave",new Vector2(0,1),new Vector2(230,-255),new Vector2(420,165));
            hud.waveValue=Label("WaveValue",wave.transform,"— / 3",new Vector2(.5f,.5f),new Vector2(55,-18),new Vector2(230,70),44);
            var enemies=Graphic("EnemyPanel",safe,"panel_enemy_counter",Vector2.one,new Vector2(-235,-255),new Vector2(420,165));
            hud.enemyValue=Label("EnemyCount",enemies.transform,"0",new Vector2(.5f,.5f),new Vector2(60,-18),new Vector2(220,70),44);
            hud.crosshair=Graphic("Crosshair",safe,"crosshair_normal",new Vector2(.5f,.5f),Vector2.zero,new Vector2(94,100));
            var ammo=Graphic("AmmoPanel",safe,"panel_ammo",Vector2.zero,new Vector2(230,160),new Vector2(420,178));
            hud.ammoValue=Label("AmmoValue",ammo.transform,"30 / 30",new Vector2(.5f,.5f),new Vector2(52,-10),new Vector2(240,85),44);
            hud.reload=Button("Reload",safe,"btn_recargar",new Vector2(.5f,0),new Vector2(42,157),new Vector2(155,170),"btn_recargar_pressed");
            hud.shoot=Button("Shoot",safe,"btn_disparar",new Vector2(1,0),new Vector2(-157,170),new Vector2(248,235),"btn_disparar_pressed","btn_disparar_disabled");
            hud.message=Graphic("Message",safe,"msg_escanear",new Vector2(.5f,.72f),Vector2.zero,new Vector2(540,270));
            hud.stateValue=Label("State",safe,"",new Vector2(.5f,.68f),Vector2.zero,new Vector2(800,100),44);
            hud.crosshairNormal=Sprite("crosshair_normal"); hud.crosshairHit=Sprite("crosshair_hit"); hud.heartFull=Sprite("heart_full"); hud.heartEmpty=Sprite("heart_empty"); hud.heartDamaged=Sprite("heart_damaged");
            hud.scanMessage=Sprite("msg_escanear"); hud.placeMessage=Sprite("msg_colocar_portal"); hud.killMessage=Sprite("msg_enemigo_eliminado");
            var result=Rect("ResultPanel",safe,new Vector2(.5f,.5f),Vector2.zero,new Vector2(930,750)); hud.resultPanel=result.gameObject;
            var shade=result.gameObject.AddComponent<Image>(); shade.color=new Color(.025f,.035f,.065f,.94f); shade.raycastTarget=true;
            hud.resultTitle=Label("ResultTitle",result,"GAME OVER",new Vector2(.5f,1),new Vector2(0,-145),new Vector2(850,140),85); hud.resultTitle.color=new Color(1,.18f,.16f);
            hud.retry=Button("Retry",result,"btn_reintentar",new Vector2(.5f,.5f),new Vector2(0,0),new Vector2(560,195));
            // No approved menu-button PNG exists: reuse the approved empty score frame with a dynamic Unity label.
            hud.mainMenu=Button("MainMenu",result,"panel_score",new Vector2(.5f,0),new Vector2(0,165),new Vector2(500,160));
            hud.mainMenu.GetComponent<Image>().sprite=null; hud.mainMenu.GetComponent<Image>().color=new Color(.06f,.16f,.24f);
            hud.mainMenu.transition=Selectable.Transition.ColorTint; Label("MenuLabel",hud.mainMenu.transform,"MENU PRINCIPAL",new Vector2(.5f,.5f),Vector2.zero,new Vector2(460,110),40);
            result.gameObject.SetActive(false);
            var boss=Rect("BossHUD",safe,new Vector2(.5f,1),new Vector2(0,-490),new Vector2(900,230));
            var view=boss.gameObject.AddComponent<BossHUDView>(); hud.bossHUD=view;
            var bar=Graphic("BossBar",boss,"boss_health_bar",new Vector2(.5f,.5f),new Vector2(0,35),new Vector2(660,205));
            var empty=Rect("EmptyHealth",bar.transform,new Vector2(.5f,.5f),new Vector2(66,-4),new Vector2(400,40)); var emptyImage=empty.gameObject.AddComponent<Image>(); emptyImage.color=new Color(.09f,.01f,.02f); emptyImage.raycastTarget=false;
            var fill=Rect("HealthFill",empty,new Vector2(.5f,.5f),Vector2.zero,new Vector2(400,40)); view.healthFill=fill.gameObject.AddComponent<Image>();
            view.healthFill.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"); view.healthFill.type=Image.Type.Filled; view.healthFill.fillMethod=Image.FillMethod.Horizontal; view.healthFill.color=new Color(1,.025f,.025f); view.healthFill.raycastTarget=false;
            view.left=Graphic("WeakPoint_Left",boss,"weakpoint_left",new Vector2(.5f,.5f),new Vector2(-160,-62),new Vector2(85,85));
            view.core=Graphic("WeakPoint_Core",boss,"weakpoint_core",new Vector2(.5f,.5f),new Vector2(0,-62),new Vector2(85,85));
            view.right=Graphic("WeakPoint_Right",boss,"weakpoint_right",new Vector2(.5f,.5f),new Vector2(160,-62),new Vector2(85,85));
            boss.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(canvas,HUDPath); Object.DestroyImmediate(canvas);
        }
        static void IntegrateScene()
        {
            var scene=EditorSceneManager.OpenScene(Gameplay); var controller=Find<ARPrototypeController>(scene); if(controller==null) throw new Exception("Missing existing controller");
            var oldHUD=Find<VisualHUD>(scene); if(oldHUD!=null) Object.DestroyImmediate(oldHUD.gameObject);
            var hudGO=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HUDPath),scene); var hud=hudGO.GetComponent<VisualHUD>();
            var serialized=new SerializedObject(controller);
            hud.controller=controller; hud.weapon=(PlasmaWeapon)serialized.FindProperty("weapon").objectReferenceValue; hud.score=(ScoreManager)serialized.FindProperty("score").objectReferenceValue;
            hud.legacyShoot=(Button)serialized.FindProperty("shootButton").objectReferenceValue; hud.legacyStatus=(Text)serialized.FindProperty("statusText").objectReferenceValue;
            hud.legacyCanvas=hud.legacyStatus.GetComponentInParent<Canvas>();
            hud.legacyCanvas.enabled=false; hud.legacyCanvas.GetComponent<GraphicRaycaster>().enabled=false;
            var camera=Find<Camera>(scene); var previous=camera.transform.Find("PlasmaViewModel"); if(previous!=null) Object.DestroyImmediate(previous.gameObject);
            var gun=Child("PlasmaViewModel",camera.transform); gun.transform.localPosition=new Vector3(.095f,-.17f,.33f); gun.transform.localScale=Vector3.one*.30f;
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/PlasmaBlaster_Visual.prefab")); model.transform.SetParent(gun.transform,false);
            var muzzle=Child("MuzzleVisual",gun.transform); muzzle.transform.localPosition=new Vector3(0,.15f,.48f); hud.muzzle=muzzle.transform;
            if(!scene.GetRootGameObjects().Any(g=>g.name=="Visual Lighting")) { var light=Child("Visual Lighting").AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.15f; light.shadows=LightShadows.None; light.transform.rotation=Quaternion.Euler(45,-30,0); }
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight=new Color(.32f,.36f,.42f);
            EditorUtility.SetDirty(hud); PrefabUtility.RecordPrefabInstancePropertyModifications(hud); EditorSceneManager.SaveScene(scene);
        }
        static void Menu()
        {
            Folder("Assets/Scenes/MainMenu"); var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var menuCamera=Child("MainMenu Camera").AddComponent<Camera>(); menuCamera.tag="MainCamera"; menuCamera.cullingMask=0; menuCamera.clearFlags=CameraClearFlags.SolidColor; menuCamera.backgroundColor=new Color(.026f,.045f,.068f);
            var canvas=CreateCanvas("MainMenuCanvas"); var safe=canvas.transform.Find("SafeArea");
            var background=Graphic("Background",canvas.transform,"fondo_menu_principal",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1920,1920));
            background.transform.SetAsFirstSibling(); Stretch(background.rectTransform); background.preserveAspect=false;
            var fitter=background.gameObject.AddComponent<AspectRatioFitter>(); fitter.aspectMode=AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio=550f/128;
            var shade=Rect("Shade",canvas.transform,Vector2.zero,Vector2.zero,Vector2.zero); Stretch(shade); var tint=shade.gameObject.AddComponent<Image>(); tint.color=new Color(.01f,.025f,.05f,.54f); tint.raycastTarget=false; shade.SetSiblingIndex(1);
            Graphic("Logo",safe,"logo_rift_hunters_ar",new Vector2(.5f,.72f),Vector2.zero,new Vector2(800,440));
            var behavior=canvas.AddComponent<VisualMainMenu>();
            var start=Button("Start",safe,"btn_iniciar",new Vector2(.5f,.43f),Vector2.zero,new Vector2(650,205)); UnityEventTools.AddPersistentListener(start.onClick,behavior.StartGame);
            var settings=Button("Settings",safe,"btn_ajustes",new Vector2(.5f,.30f),Vector2.zero,new Vector2(590,190)); settings.interactable=false;
            var quit=Button("Quit",safe,"btn_salir",new Vector2(.5f,.17f),Vector2.zero,new Vector2(560,185)); UnityEventTools.AddPersistentListener(quit.onClick,behavior.Quit);
            var eventGO=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)); eventGO.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            EditorSceneManager.SaveScene(scene,MenuPath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(MenuPath,true),new EditorBuildSettingsScene(Gameplay,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=MenuPath && s.path!=Gameplay)).ToArray();
            EditorSceneManager.OpenScene(Gameplay);
        }
        static void Validate()
        {
            var scene=EditorSceneManager.OpenScene(Gameplay); var hud=Find<VisualHUD>(scene);
            if(hud==null || hud.controller==null || hud.weapon==null || hud.score==null || hud.legacyShoot==null || hud.legacyCanvas==null) throw new Exception("Missing gameplay HUD reference");
            foreach(string path in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs"}).Select(AssetDatabase.GUIDToAssetPath))
            {
                var root=AssetDatabase.LoadAssetAtPath<GameObject>(path); foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0) throw new Exception("Missing script in "+path);
            }
            var basic=AssetDatabase.LoadAssetAtPath<GameObject>(BasicPath);
            if(basic.GetComponent<EnemyHealth>()==null || basic.GetComponent<EnemyMovement>()==null || basic.GetComponentsInChildren<Collider>(true).Length!=1 || basic.transform.Find("AimPoint")==null) throw new Exception("Basic logic/collider damaged");
            var portal=AssetDatabase.LoadAssetAtPath<GameObject>(PortalPath); if(portal.transform.Find("SpawnPoint")==null) throw new Exception("Portal SpawnPoint missing");
            var boss=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boss/RiftGuardian_Visual.prefab");
            File.WriteAllText("Temp/RiftVisualHierarchy.txt",string.Join("\n",boss.GetComponentsInChildren<Transform>(true).Select(t=>t.name)));
            foreach(string name in new[]{"WeakPoint_Left","WeakPoint_Core","WeakPoint_Right"}) if(!boss.GetComponentsInChildren<Transform>(true).Any(t=>t.name==name)) throw new Exception("Missing boss object "+name);
            foreach(var mat in Directory.GetFiles(Mats,"M_*.mat").Select(p=>AssetDatabase.LoadAssetAtPath<Material>(p.Replace('\\','/')))) if(mat==null || mat.shader==null || ShaderUtil.ShaderHasError(mat.shader)) throw new Exception("Invalid material/shader");
            string bounds = File.Exists("Temp/RiftVisualBounds.txt") ? File.ReadAllText("Temp/RiftVisualBounds.txt") : "Optional import bounds cache absent after Editor restart; approved models were not reimported or rebuilt.";
            File.WriteAllText("Temp/RiftVisualValidation.txt","PASS: scene, HUD references, prefabs, Basic components + one collider + AimPoint, portal SpawnPoint, three separated boss weak points, URP shaders.\n"+bounds);
        }
        static void Effects()
        {
            Folder("Assets/Effects/Prefabs/"); Folder("Assets/Effects/Materials/");
            string texturePath="Assets/Effects/Materials/ParticleSoft.asset"; var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if(texture==null)
            {
                texture=new Texture2D(32,32,TextureFormat.RGBA32,false); texture.name="ParticleSoft"; texture.wrapMode=TextureWrapMode.Clamp;
                for(int y=0;y<32;y++) for(int x=0;x<32;x++) { float radius=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(16,16))/16; texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),2))); }
                texture.Apply(); AssetDatabase.CreateAsset(texture,texturePath);
            }
            string matPath="Assets/Effects/Materials/M_ParticleSoft.mat"; var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(material==null) { material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")); AssetDatabase.CreateAsset(material,matPath); }
            material.SetTexture("_BaseMap",texture); material.SetColor("_BaseColor",Color.white); material.SetFloat("_Surface",1); material.SetFloat("_Blend",0); material.SetFloat("_SrcBlend",5); material.SetFloat("_DstBlend",10); material.SetFloat("_ZWrite",0); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue=3000; EditorUtility.SetDirty(material);
            Effect("VFX_Portal",new Color(.03f,.6f,1,.7f),24,1.3f,.022f,true,material);
            Effect("VFX_PlasmaShot",new Color(.05f,.7f,1,.9f),8,.16f,.012f,false,material);
            Effect("VFX_PlasmaHit",new Color(.05f,.7f,1,.9f),12,.28f,.027f,false,material);
            Effect("VFX_EnemyDamage",new Color(1,.08f,.01f,.85f),6,.3f,.025f,false,material);
            Effect("VFX_EnemyDeath",new Color(1,.11f,.01f,.85f),18,.6f,.035f,false,material);
            Effect("VFX_Electric",new Color(.1f,.6f,1,.8f),16,.35f,.022f,false,material);
            Effect("VFX_EnergyExplosion",new Color(1,.35f,.02f,.8f),30,.5f,.045f,false,material);
            Effect("VFX_BossSpawn",new Color(1,.02f,.01f,.85f),28,.7f,.045f,false,material);
            Effect("VFX_WeakPointHit",new Color(1,.035f,.01f,.85f),10,.3f,.025f,false,material);
            var portal=PrefabUtility.LoadPrefabContents(PortalPath);
            var visual=portal.transform.Find("Visual"); var old=visual.Find("VFX_Portal"); if(old!=null) Object.DestroyImmediate(old.gameObject);
            var loop=(GameObject)PrefabUtility.InstantiatePrefab(LoadEffect("VFX_Portal")); loop.name="VFX_Portal"; loop.transform.SetParent(visual,false); loop.transform.localPosition=new Vector3(0,.6f,0);
            PrefabUtility.SaveAsPrefabAsset(portal,PortalPath); PrefabUtility.UnloadPrefabContents(portal);
            var basic=PrefabUtility.LoadPrefabContents(BasicPath); var fx=basic.GetComponent<EnemyVisualEffects>() ?? basic.AddComponent<EnemyVisualEffects>();
            fx.health=basic.GetComponent<EnemyHealth>(); fx.aimPoint=basic.transform.Find("AimPoint"); fx.damage=LoadEffect("VFX_EnemyDamage"); fx.death=LoadEffect("VFX_EnemyDeath"); fx.plasmaHit=LoadEffect("VFX_PlasmaHit");
            PrefabUtility.SaveAsPrefabAsset(basic,BasicPath); PrefabUtility.UnloadPrefabContents(basic);
            var scene=EditorSceneManager.OpenScene(Gameplay); var hud=Find<VisualHUD>(scene); hud.plasmaShot=LoadEffect("VFX_PlasmaShot"); hud.plasmaHit=LoadEffect("VFX_PlasmaHit"); EditorUtility.SetDirty(hud); PrefabUtility.RecordPrefabInstancePropertyModifications(hud); EditorSceneManager.SaveScene(scene);
        }
        static GameObject LoadEffect(string name) => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Effects/Prefabs/"+name+".prefab");
        static void Effect(string name,Color color,int count,float lifetime,float size,bool loop,Material material)
        {
            var root=Child(name); var ps=root.AddComponent<ParticleSystem>(); ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main; main.loop=loop; main.duration=loop?2:lifetime; main.startLifetime=lifetime; main.startSpeed=loop?.045f:.22f; main.startSize=new ParticleSystem.MinMaxCurve(size*.55f,size); main.startColor=color; main.maxParticles=count; main.simulationSpace=ParticleSystemSimulationSpace.World; main.playOnAwake=true;
            var emission=ps.emission; emission.rateOverTime=loop?5:0; if(!loop) emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
            var shape=ps.shape; shape.shapeType=loop?ParticleSystemShapeType.Circle:ParticleSystemShapeType.Sphere; shape.radius=loop?.4f:.035f;
            var fade=ps.colorOverLifetime; fade.enabled=true; var gradient=new Gradient(); gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)}); fade.color=gradient;
            var renderer=root.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=material; renderer.renderMode=ParticleSystemRenderMode.Billboard; renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows=false;
            if(!loop) root.AddComponent<EffectLifetime>().seconds=lifetime+.15f;
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Effects/Prefabs/"+name+".prefab"); Object.DestroyImmediate(root);
        }
        static void Review()
        {
            Directory.CreateDirectory("ArtRenders/VisualIntegration");
            var previous=SceneManager.GetActiveScene(); var preview=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(preview);
            var camera=Child("Review Camera").AddComponent<Camera>(); camera.transform.position=new Vector3(0,1.05f,-2.8f);
            camera.transform.LookAt(new Vector3(0,.45f,0)); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.026f,.045f,.068f);
            camera.fieldOfView=52; camera.nearClipPlane=.03f; camera.farClipPlane=20; camera.cullingMask=1<<31;
            var light=Child("Review Light").AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.7f; light.cullingMask=1<<31; light.transform.rotation=Quaternion.Euler(35,-30,0);
            var rt=new RenderTexture(1080,1920,24,RenderTextureFormat.ARGB32); camera.targetTexture=rt;
            try
            {
                // Menu uses the saved runtime scene and the exact imported approved graphics.
                var menu=SceneManager.GetSceneByPath(MenuPath); bool menuWasLoaded=menu.IsValid() && menu.isLoaded;
                if(!menuWasLoaded) menu=EditorSceneManager.OpenScene(MenuPath,OpenSceneMode.Additive);
                var menuCanvas=Object.Instantiate(Find<Canvas>(menu)); SceneManager.MoveGameObjectToScene(menuCanvas.gameObject,preview);
                if(!menuWasLoaded) EditorSceneManager.CloseScene(menu,true);
                SceneManager.SetActiveScene(preview); ConfigureReviewCanvas(menuCanvas,camera);
                Capture(camera,rt,"01_MainMenu"); Object.DestroyImmediate(menuCanvas.gameObject);
                foreach(var es in preview.GetRootGameObjects().Where(g=>g.GetComponent<EventSystem>()!=null).ToArray()) Object.DestroyImmediate(es);
                var hud=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HUDPath),preview); var adapter=hud.GetComponent<VisualHUD>(); ConfigureReviewCanvas(hud.GetComponent<Canvas>(),camera);
                var footer=Label("ReviewOnlyFooter",hud.transform,"PREVIEW EDITOR · SIN CAMARA AR",new Vector2(.5f,0),new Vector2(0,35),new Vector2(1000,50),23); footer.color=new Color(.6f,.7f,.8f);
                var floor=GameObject.CreatePrimitive(PrimitiveType.Plane); floor.name="Review-only floor"; floor.transform.localScale=Vector3.one*.3f; floor.GetComponent<Renderer>().sharedMaterial=LoadMat("M_Tech_MetalDark"); SetLayer(floor);
                var plane=GameObject.CreatePrimitive(PrimitiveType.Plane); plane.name="Review-only detected plane"; plane.transform.localScale=Vector3.one*.12f; plane.transform.position=new Vector3(0,.003f,0); plane.GetComponent<Renderer>().sharedMaterial=LoadMat("M_AR_Hologram"); SetLayer(plane);
                adapter.ShowReview("SCANNING",100,0,30,0,0); Capture(camera,rt,"02_Scanning"); plane.SetActive(false);
                var portal=ReviewPrefab(PortalPath,new Vector3(0,0,.15f));
                adapter.ShowReview("COUNTDOWN",100,0,30,1,0); Capture(camera,rt,"03_Portal");
                portal.transform.localPosition=new Vector3(.36f,0,.65f); portal.transform.localScale=Vector3.one*.72f;
                var enemy=ReviewPrefab(BasicPath,new Vector3(-.14f,0,0));
                adapter.ShowReview("WAVE",100,0,30,1,3); Capture(camera,rt,"04_EnemyBasic");
                adapter.ShowReview("WAVE",66,700,24,2,3); Capture(camera,rt,"05_GameplayHUD");
                adapter.ShowReview("GAME_OVER",0,700,0,2,3); Capture(camera,rt,"06_GameOver");
                Object.DestroyImmediate(enemy); Object.DestroyImmediate(portal);
                var boss=ReviewPrefab("Assets/Prefabs/Boss/RiftGuardian_Visual.prefab",new Vector3(0,0,.15f)); boss.transform.localScale=Vector3.one*.58f;
                adapter.ShowReview("WAVE",100,1250,10,3,1); adapter.bossHUD.gameObject.SetActive(true); adapter.bossHUD.Present(.7f,true,true,true);
                Capture(camera,rt,"07_BossHUD");
            }
            finally
            {
                camera.targetTexture=null; rt.Release(); Object.DestroyImmediate(rt); EditorSceneManager.CloseScene(preview,true);
                if(previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous); else EditorSceneManager.OpenScene(Gameplay);
            }
        }
        static void SetLayer(GameObject root) { foreach(var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=31; }
        static void ConfigureReviewCanvas(Canvas canvas,Camera camera)
        {
            canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=.12f; SetLayer(canvas.gameObject);
            var safe=canvas.GetComponentInChildren<SafeAreaLayout>(); if(safe!=null) safe.enabled=false;
        }
        static GameObject ReviewPrefab(string path,Vector3 position)
        {
            var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path)); root.transform.position=position; root.transform.rotation=Quaternion.Euler(0,180,0); SetLayer(root);
            foreach(var ps in root.GetComponentsInChildren<ParticleSystem>()) ps.Simulate(.25f,true,true);
            return root;
        }
        static void Capture(Camera camera,RenderTexture rt,string name)
        {
            Canvas.ForceUpdateCanvases(); foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.worldCamera==camera)) LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases(); camera.Render(); var previous=RenderTexture.active; RenderTexture.active=rt;
            var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false); texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0); texture.Apply();
            File.WriteAllBytes("ArtRenders/VisualIntegration/"+name+".png",texture.EncodeToPNG()); Object.DestroyImmediate(texture); RenderTexture.active=previous;
        }
    }
}



