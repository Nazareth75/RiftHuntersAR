using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiftHunters.Game;
using RiftHunters.Combate;
using RiftHuntersAR.Integration;
using RiftHuntersAR.UI.Visual;
using Object=UnityEngine.Object;

namespace RiftHuntersAR.Recycling.Editor
{
    [InitializeOnLoad]
    public static class RecyclingGameplayTests
    {
        const string Flag="RiftRecyclingGameplayTest",Demo="RiftRecyclingGameplayDemo";
        static ARPrototypeController controller;
        static RecyclingGameplay flow;
        static VisualHUD hud;
        static double started,next;
        static int stage,errors,recoveries,totalCollected,lastEco,waveAtRecovery;
        static bool wasBackground,wrongChecked,finalSeen;
        static GameState stateAtRecovery;
        static readonly Dictionary<int,float> born=new();
        static readonly HashSet<int> pollutedKinds=new();
        static readonly List<string> checks=new();
        static readonly FieldInfo Speed=typeof(Enemigo).GetField("velocidad",BindingFlags.NonPublic|BindingFlags.Instance);
        static RecyclingGameplayTests(){EditorApplication.update+=Poll;Application.logMessageReceived+=Log;}
        static void Log(string text,string stack,LogType type)
        {if(SessionState.GetBool(Flag,false) && (type==LogType.Error || type==LogType.Exception || type==LogType.Assert)){errors++;File.AppendAllText("Temp/RiftRecyclingGameplayErrors.txt",text+"\n"+stack+"\n");}}
        static void Check(bool value,string text){if(!value)throw new Exception(text);checks.Add("PASS "+text);File.WriteAllLines("Temp/RiftRecyclingGameplayChecks.txt",checks);}
        [MenuItem("Rift Hunters/Reciclaje AR/1 Preparar referencias (sin regenerar modelos)")]
        static void Setup()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<RecyclingVisualCatalog>("Assets/Resources/RecyclingVisualCatalog.asset");
            if(catalog==null)throw new Exception("Missing approved recycling catalog");
            catalog.plasticStation=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Recycling/PlasticRecycleStation.prefab");
            catalog.paperStation=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Recycling/PaperRecycleStation.prefab");
            catalog.metalStation=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Recycling/MetalRecycleStation.prefab");
            if(catalog.plasticStation==null || catalog.paperStation==null || catalog.metalStation==null)throw new Exception("Missing approved stations");
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            if(EditorBuildSettings.scenes.Any(s=>s.enabled && s.path.Contains("RecyclingVisualReview")))throw new Exception("Review scene must not be in build");
            GeometryChecks();
        }
        static void GeometryChecks()
        {
            var preview=EditorSceneManager.NewPreviewScene();
            try
            {
                var surfaceRoot=new GameObject("Placement test surface");SceneManager.MoveGameObjectToScene(surfaceRoot,preview);
                var surface=surfaceRoot.AddComponent<RiftHuntersAR.AR.SueloCombateAR>();
                var cameraRoot=new GameObject("Placement test camera");SceneManager.MoveGameObjectToScene(cameraRoot,preview);var camera=cameraRoot.AddComponent<Camera>();
                camera.transform.position=new Vector3(0,1,-3);camera.transform.LookAt(Vector3.zero);camera.aspect=9f/16;
                var boundary=typeof(RiftHuntersAR.AR.SueloCombateAR).GetField("contorno",BindingFlags.NonPublic|BindingFlags.Instance);
                boundary.SetValue(surface,new[]{new Vector2(-1.4f,-1.4f),new Vector2(1.4f,-1.4f),new Vector2(1.4f,1.4f),new Vector2(-1.4f,1.4f)});
                if(!StationPlacement.TryFind(surface,Vector3.zero,Vector3.forward*.1f,camera,out var positions,out var rotation,out var scale))throw new Exception("Station row should fit bounded surface");
                Vector3 right=Vector3.Cross(rotation*Vector3.forward,Vector3.up).normalized;
                for(int i=0;i<3;i++)
                {
                    if(!surface.Contiene(positions[i]) || Mathf.Abs(positions[i].y)>.001f)throw new Exception("Station off scanned plane");
                    if(i>0 && Vector3.Dot(positions[i]-positions[i-1],right)<=0)throw new Exception("Plastic/Paper/Metal row reversed");
                }
                boundary.SetValue(surface,new[]{new Vector2(-.2f,-.2f),new Vector2(.2f,-.2f),new Vector2(.2f,.2f),new Vector2(-.2f,.2f)});
                if(StationPlacement.TryFind(surface,Vector3.zero,Vector3.zero,camera,out _,out _,out _))throw new Exception("Tiny plane must not force floating/overlapping stations");
                File.WriteAllText("Temp/RiftRecyclingPlacementChecks.txt","PASS bounded AR surface: grounded separated row; Plastic/Paper/Metal left-to-right; tiny plane rejected with rescan fallback.\n");
            }
            finally {EditorSceneManager.ClosePreviewScene(preview);}
        }
        [MenuItem("Rift Hunters/Reciclaje AR/2 Probar flujo real completo")]
        static void Play(){Setup();Begin(false);}
        [MenuItem("Rift Hunters/Reciclaje AR/3 Revisar ARGame en PC (F8 termina combate)")]
        static void Review(){Setup();Begin(true);}
        static void Begin(bool demo)
        {
            typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("Clear",BindingFlags.Public|BindingFlags.Static)?.Invoke(null,null);
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay/ARGame.unity");
            SessionState.SetBool(demo?Demo:Flag,true);SessionState.SetInt("RiftRecoveryStage",0);
            File.WriteAllText("Temp/RiftRecyclingGameplayResult.txt",demo?"REVIEW RUNNING":"RUNNING");File.WriteAllText("Temp/RiftRecyclingGameplayErrors.txt","");
            EditorApplication.isPlaying=true;
        }
        static void Poll()
        {
            if(EditorApplication.isPlaying && File.Exists("Temp/RiftRecoveryReviewFinish.txt"))
            {
                File.Delete("Temp/RiftRecoveryReviewFinish.txt");Object.FindFirstObjectByType<RecyclingDesktopReview>()?.FinishCombat();
            }
            if(SessionState.GetBool(Flag,false) || SessionState.GetBool(Demo,false)){Tick();return;}
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists("Temp/RiftRecyclingGameplayCommand.txt"))return;
            string value=File.ReadAllText("Temp/RiftRecyclingGameplayCommand.txt").Trim();File.Delete("Temp/RiftRecyclingGameplayCommand.txt");
            try {if(value=="setup")Setup();else if(value=="play")Play();else if(value=="review")Review();else throw new Exception("Unknown command");File.WriteAllText("Temp/RiftRecyclingGameplaySetup.txt","PASS "+value);}
            catch(Exception e){File.WriteAllText("Temp/RiftRecyclingGameplaySetup.txt","FAIL "+e);Debug.LogException(e);}
        }
        static Rect ScreenRect(RectTransform rt){var corners=new Vector3[4];rt.GetWorldCorners(corners);return Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);}
        static void Tick()
        {
            if(!EditorApplication.isPlaying || EditorApplication.isCompiling)return;
            EditorApplication.QueuePlayerLoopUpdate();
            try
            {
                if(stage==0 || controller==null)
                {
                    controller=Object.FindFirstObjectByType<ARPrototypeController>();if(controller==null || controller.Player==null || Time.time<.5f)return;
                    flow=controller.GetComponent<RecyclingGameplay>();hud=Object.FindFirstObjectByType<VisualHUD>();
                    if(flow==null || hud==null || hud.GetComponent<RecyclingHUD>()==null)return;
                    started=EditorApplication.timeSinceStartup;wasBackground=Application.runInBackground;Application.runInBackground=true;
                    stage=1;errors=0;recoveries=0;totalCollected=0;lastEco=0;born.Clear();pollutedKinds.Clear();checks.Clear();wrongChecked=false;finalSeen=false;
                    controller.HabilitarPruebaEscritorio();
                    var pc=controller.GetComponent<ControlesComputadora>();if(pc!=null)pc.enabled=false;
                    controller.CamaraJuego.transform.SetPositionAndRotation(new Vector3(1000,1,1000),Quaternion.identity);
                    controller.IniciarPruebaEscritorio(new Vector3(1000,0,1003));
                    controller.CamaraJuego.transform.LookAt(new Vector3(1000,.35f,1002.5f));
                    Check(SceneManager.GetActiveScene().name=="ARGame" && controller.PortalActivo!=null && controller.PortalActivo.SpawnPoint!=null,"Actual ARGame, AR camera, portal and original SpawnPoint retained");
                    Check(Object.FindObjectsByType<RecycleStation>(FindObjectsSortMode.None).Length==0,"No stations at placement/countdown");
                    if(SessionState.GetBool(Demo,false))
                    {
                        controller.gameObject.AddComponent<RecyclingDesktopReview>().Configure(controller);
                        SessionState.SetBool(Demo,false);Application.runInBackground=wasBackground;return;
                    }
                }
                if(EditorApplication.timeSinceStartup-started>230)throw new Exception("Full recycling flow timeout, stage="+stage+" state="+controller.CurrentState);
                if(stage==1)
                {
                    if(controller.CurrentState!="WAVE")return;
                    var enemy=Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None).FirstOrDefault(e=>e.GetComponent<EnemyPollutionEmitter>()!=null);if(enemy==null)return;
                    Speed.SetValue(enemy,0f);
                    if(enemy.GetComponent<EnemyPollutionEmitter>().EmittedCount==0)return;
                    var residue=Object.FindFirstObjectByType<RecyclableItem>();if(residue==null || !residue.IsSettled)return;
                    Check(!flow.Select(residue),"Residues cannot be collected during combat");
                    controller.Player.TakeDamage(100);stage=2;next=Time.timeAsDouble+.2;return;
                }
                if(Time.timeAsDouble<next)return;
                if(stage==2)
                {
                    Check(controller.CurrentState=="GAME_OVER" && hud.resultPanel.activeSelf,"Combat death preserves Game Over");
                    hud.retry.onClick.Invoke();stage=3;next=Time.timeAsDouble+.2;return;
                }
                if(stage==3)
                {
                    Check(flow.EcoPoints==0 && flow.Selected==null && !flow.Active && !GameManager.Instance.RecoveryInProgress,"Retry resets ECO, selection and recovery gate");
                    Check(Object.FindObjectsByType<RecyclableItem>(FindObjectsSortMode.None).Length==0 && Object.FindObjectsByType<RecycleStation>(FindObjectsSortMode.None).Length==0,"Retry removes residues, contamination and stations");
                    Check(controller.Player.CurrentHealth==100 && hud.score.Score==0 && hud.weapon.CurrentAmmo==hud.weapon.MagazineSize,"Retry restores lives, score, ammunition and waves");
                    stage=4;Time.timeScale=4;return;
                }
                if(stage==4)
                {
                    if(controller.CurrentState=="VICTORY")
                    {
                        Check(finalSeen && recoveries==5,"Victory deferred through all four recoveries and final boss recovery");
                        Check(pollutedKinds.SetEquals(new[]{0,1,2}),"Basic, Fast and Tank actually contaminated during real waves");
                        Check(flow.EcoPoints==totalCollected*50 && flow.EcoPoints>0,"ECO conserved across all waves, exactly +50 per classified residue");
                        Check(Object.FindObjectsByType<RecycleStation>(FindObjectsSortMode.None).Length==0 && flow.PendingCount==0,"Final stations removed, pending count zero");
                        Capture("Victory");stage=81;next=Time.timeAsDouble+.3;return;
                    }
                    if(flow.Active)
                    {
                        if(!flow.InputAvailable)return;
                        Check(controller.AliveEnemies==0 && GameManager.Instance.RecoveryInProgress,"Last enemy gates next wave during recovery "+recoveries);
                        Check(flow.Stations.Count==3,"Exactly three stations appear after combat");
                        stateAtRecovery=GameManager.Instance.State;waveAtRecovery=controller.CurrentWave;lastEco=flow.EcoPoints;
                        recoveries++;if(flow.FinalRecovery){finalSeen=true;Check(stateAtRecovery==GameState.Boss && controller.CurrentState!="VICTORY","Guardian death waits for final classification");}
                        foreach(var station in flow.Stations)
                        {
                            Check(station.transform.parent==flow.StationsRoot && flow.StationsRoot.parent==controller.PortalActivo.transform.parent,"Station shares actual AR anchor");
                            Check(Mathf.Abs(station.transform.position.y-controller.PortalActivo.transform.position.y)<.001f,"Station on AR surface");
                        }
                        for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)
                        {
                            var a=flow.Stations[i];var b=flow.Stations[j];
                            Check(!Physics.ComputePenetration(a.GetComponent<BoxCollider>(),a.transform.position,a.transform.rotation,b.GetComponent<BoxCollider>(),b.transform.position,b.transform.rotation,out _,out _),"Oriented station colliders do not overlap");
                        }
                        CheckHUD();stage=5;next=Time.timeAsDouble+.4;return;
                    }
                    foreach(var enemy in Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None))
                    {
                        var emitter=enemy.GetComponent<EnemyPollutionEmitter>();if(emitter==null || enemy.Vida<=0)continue;
                        Speed.SetValue(enemy,0f);int id=enemy.GetInstanceID();if(!born.ContainsKey(id))born[id]=Time.time;
                        if(Time.time-born[id]<3.5f)continue;
                        var boss=enemy.GetComponent<NucleosJefe>();
                        if(boss!=null)
                        {
                            foreach(var name in new[]{"Nucleo brazo izquierdo","Nucleo brazo derecho","Nucleo central"})
                            {var core=enemy.transform.Find(name)?.GetComponent<Enemigo>();if(core!=null)core.RecibirDano(core.VidaMaxima);}
                        }
                        else
                        {
                            Check(emitter.EmittedCount==emitter.Limit,"Enemy emits bounded pollution before retirement: "+enemy.name);
                            pollutedKinds.Add(enemy.GetComponent<EnemyProceduralVisual>().Kind);enemy.RecibirDano(enemy.VidaMaxima);
                        }
                    }
                    return;
                }
                if(stage==5)
                {
                    Check(GameManager.Instance.State==stateAtRecovery && controller.CurrentWave==waveAtRecovery,"Wave cannot advance while residues remain");
                    if(flow.PendingCount==0){stage=4;return;}
                    var item=flow.Items[0];int pending=flow.PendingCount;int eco=flow.EcoPoints;int score=hud.score.Score;float health=controller.Player.CurrentHealth;
                    Check(TapItem(item),"Touch ray selects actual residue collider");
                    Check(flow.Selected!=null && flow.Items.Contains(flow.Selected),"Touch selects the frontmost valid residue");
                    item=flow.Selected;
                    if(!wrongChecked)
                    {
                        var wrong=flow.Stations.First(s=>s.Category!=item.Category);TapStation(wrong);
                        Check(!item.Collected && flow.Selected==item && flow.PendingCount==pending && flow.Feedback=="CONTENEDOR INCORRECTO","Incorrect station preserves item and allows retry");
                        Check(flow.EcoPoints==eco && hud.score.Score==score && controller.Player.CurrentHealth==health,"Wrong classification never changes ECO, combat score or life");wrongChecked=true;
                        Capture("Incorrect");
                    }
                    var correct=flow.Stations.First(s=>s.Category==item.Category);Check(TapStation(correct),"Correct station touch accepts category "+item.Category);
                    totalCollected++;
                    Check(flow.EcoPoints==eco+50 && flow.PendingCount==pending-1 && item.Collected,"Correct classification +50 ECO and pending decrements once");
                    Check(!flow.Classify(correct) && flow.EcoPoints==eco+50,"Repeated station tap cannot duplicate ECO");
                    Capture("Recovery");
                    if(flow.PendingCount==0){stage=6;next=Time.timeAsDouble+.1;}else next=Time.timeAsDouble+.4;return;
                }
                if(stage==6)
                {
                    Check(flow.ZoneRecovered && flow.Instruction.StartsWith("ZONA RECUPERADA"),"Zero residues shows recovered zone before next wave");
                    Capture("Recovered");stage=7;next=Time.timeAsDouble+1.8;return;
                }
                if(stage==7)
                {
                    Check(!flow.Active && flow.StationsRoot==null,"Stations hidden before next existing wave/Victory");
                    Check(flow.EcoPoints>=lastEco,"ECO persists after recovery");
                    stage=4;return;
                }
                if(stage==8)
                {
                    Check(flow.EcoPoints==0 && !flow.Active && flow.StationsRoot==null && flow.PendingCount==0 && flow.Selected==null,"Retry after Victory clears every recycling state");
                    Check(controller.CurrentState=="COUNTDOWN" && controller.NucleosJefe==null,"Retry clears boss and restarts existing countdown");
                    Check(errors==0,"Zero runtime errors");Finish();
                }
                if(stage==81){hud.retry.onClick.Invoke();stage=8;next=Time.timeAsDouble+.2;}
            }
            catch(Exception e){File.WriteAllText("Temp/RiftRecyclingGameplayResult.txt","FAIL "+e+"\n"+string.Join("\n",checks));SessionState.SetBool(Flag,false);SessionState.SetBool(Demo,false);Time.timeScale=1;Application.runInBackground=wasBackground;EditorApplication.isPlaying=false;Debug.LogException(e);}
        }
        static bool TapItem(RecyclableItem item)
        {
            var center=item.GetComponent<BoxCollider>().bounds.center;
            controller.CamaraJuego.transform.LookAt(center);Physics.SyncTransforms();
            var screen=controller.CamaraJuego.WorldToScreenPoint(center);
            bool result=flow.HandleTap(screen);
            if(!result)
            {
                var ui=new List<UnityEngine.EventSystems.RaycastResult>();var ev=UnityEngine.EventSystems.EventSystem.current;
                if(ev!=null)ev.RaycastAll(new UnityEngine.EventSystems.PointerEventData(ev){position=screen},ui);
                File.WriteAllText("Temp/RiftRecoveryTapDebug.txt","Input="+flow.InputAvailable+" settled="+item.IsSettled+" items="+flow.Items.Contains(item)+" screen="+screen+" UI="+string.Join(",",ui.Select(x=>x.gameObject.name))+" physics="+string.Join(",",Physics.RaycastAll(controller.CamaraJuego.ScreenPointToRay(screen),20,1<<2,QueryTriggerInteraction.Collide).Select(x=>x.collider.name)));
            }
            return result;
        }
        static bool TapStation(RecycleStation station)
        {
            var center=station.GetComponent<BoxCollider>().bounds.center;
            controller.CamaraJuego.transform.LookAt(center);Physics.SyncTransforms();
            return flow.HandleTap(controller.CamaraJuego.WorldToScreenPoint(center));
        }
        static void CheckHUD()
        {
            Canvas.ForceUpdateCanvases();var ui=hud.GetComponent<RecyclingHUD>();var eco=ScreenRect(ui.EcoLabel.rectTransform);var safe=ScreenRect((RectTransform)hud.resultPanel.transform.parent);
            Check(safe.Contains(eco.min) && safe.Contains(eco.max),"ECO inside portrait SafeArea");
            foreach(var other in new[]{hud.scoreValue.rectTransform,hud.waveValue.rectTransform,hud.enemyValue.rectTransform,hud.ammoValue.rectTransform,hud.shoot.GetComponent<RectTransform>(),hud.reload.GetComponent<RectTransform>(),hud.bossHUD.GetComponent<RectTransform>()})Check(!eco.Overlaps(ScreenRect(other)),"ECO does not overlap HUD "+other.name);
        }
        static void Capture(string name){Directory.CreateDirectory("Builds/RecyclingGameplay");ScreenCapture.CaptureScreenshot("Builds/RecyclingGameplay/"+name+".png");}
        static void Finish()
        {
            Time.timeScale=1;Application.runInBackground=wasBackground;SessionState.SetBool(Flag,false);
            File.WriteAllText("Temp/RiftRecyclingGameplayResult.txt","PASS full actual ARGame: 5 recoveries; collected="+totalCollected+"; errors="+errors+"\n"+string.Join("\n",checks));EditorApplication.isPlaying=false;
        }
    }
}
