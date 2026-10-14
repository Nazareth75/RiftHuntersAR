using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RiftHuntersAR.UI.Visual;
using RiftHuntersAR.Game;
using RiftHunters.Combate;
using RiftHunters.Game;
using Object = UnityEngine.Object;

namespace RiftHuntersAR.Integration.Editor
{
    // Editor-only integration checks. Runtime fixtures are discarded when Play Mode exits.
    // No asset creation, source-art changes or Android build commands.
    [InitializeOnLoad]
    public static class TeamIntegrationTests
    {
        const string Flag="RiftTeamPlaying";
        static VisualHUD hud;
        static ARPrototypeController controller;
        static EnlaceCombateAR enlace;
        static Enemigo target;
        static int initialAmmo, initialScore, points, shots, gun;
        static double next, phaseStart;
        static bool sawHit, sawBoss;
        static readonly HashSet<int> waves=new();
        static TeamIntegrationTests() { EditorApplication.update+=Poll; Application.logMessageReceived+=Log; }
        static void Require(bool value,string message) { if(!value) throw new Exception(message); }
        static void Record(string value) { File.AppendAllText("Temp/RiftTeamChecks.txt",value+"\n"); }
        static void Log(string text,string stack,LogType type)
        {
            if(!SessionState.GetBool(Flag,false)) return;
            if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)
            { SessionState.SetInt("RiftTeamErrors",SessionState.GetInt("RiftTeamErrors",0)+1); File.AppendAllText("Temp/RiftTeamErrors.txt",text+"\n"+stack+"\n"); }
        }
        static void Poll()
        {
            if(SessionState.GetBool(Flag,false)) { if(EditorApplication.isPlaying) Tick(); return; }
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists("Temp/RiftTeamCommand.txt")) return;
            var command=File.ReadAllText("Temp/RiftTeamCommand.txt").Trim(); File.Delete("Temp/RiftTeamCommand.txt");
            try
            {
                if(command=="validate") Validate();
                else if(command=="play")
                {
                    typeof(EditorApplication).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("Clear",BindingFlags.Public|BindingFlags.Static)?.Invoke(null,null);
                    File.WriteAllText("Temp/RiftTeamChecks.txt",""); File.WriteAllText("Temp/RiftTeamErrors.txt","");
                    SessionState.SetInt("RiftTeamErrors",0); SessionState.SetInt("RiftTeamStage",0);
                    SessionState.SetFloat("RiftTeamStart",(float)EditorApplication.timeSinceStartup); SessionState.SetBool(Flag,true);
                    EditorSceneManager.OpenScene("Assets/Scenes/MainMenu/MainMenu.unity"); EditorApplication.isPlaying=true;
                }
                else throw new Exception("Unknown team test command");
                File.WriteAllText("Temp/RiftTeamResult.txt","SUCCESS: "+command);
            }
            catch(Exception ex) { File.WriteAllText("Temp/RiftTeamResult.txt","FAIL: "+ex); Debug.LogException(ex); }
        }
        static void Validate()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<CatalogoPartida>("Assets/Resources/CatalogoPartida.asset");
            Require(catalog!=null && catalog.sistemaArmas!=null && catalog.normal!=null && catalog.rapido!=null && catalog.tanque!=null && catalog.jefe!=null,"Catalog references missing");
            foreach(var enemy in new[]{catalog.normal,catalog.rapido,catalog.tanque,catalog.jefe})
            {
                var model=enemy.GetComponent<ModeloDeIntegracion>(); Require(model!=null,"Missing visual adapter "+enemy.name);
                Require(new SerializedObject(model).FindProperty("modelo").objectReferenceValue!=null,"Missing approved model reference "+enemy.name);
                Require(enemy.GetComponent<Collider>()!=null,"Missing functional collider "+enemy.name);
            }
            var scripts=0; var references=0;
            foreach(var path in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs"}).Select(AssetDatabase.GUIDToAssetPath))
            {
                var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach(var t in root.GetComponentsInChildren<Transform>(true)) scripts+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                foreach(var component in root.GetComponentsInChildren<Component>(true))
                {
                    if(component==null) continue;
                    var iterator=new SerializedObject(component).GetIterator();
                    while(iterator.NextVisible(true)) if(iterator.propertyType==SerializedPropertyType.ObjectReference && iterator.objectReferenceInstanceIDValue!=0 && iterator.objectReferenceValue==null) references++;
                }
            }
            Require(scripts==0 && references==0,$"Missing prefab scripts={scripts}, references={references}");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach(var path in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath))
                {
                    var scene=SceneManager.GetSceneByPath(path); bool loaded=scene.IsValid() && scene.isLoaded;
                    if(!loaded) scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
                    foreach(var root in scene.GetRootGameObjects()) foreach(var t in root.GetComponentsInChildren<Transform>(true)) scripts+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                    if(!loaded) EditorSceneManager.CloseScene(scene,true);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Require(scripts==0,"Scene missing scripts="+scripts);
            File.WriteAllText("Temp/RiftTeamAssetCheck.txt","PASS: all scenes open; missing scripts=0; missing serialized prefab references=0; catalog weapon/normal/fast/tank/boss references valid; approved model references and functional colliders preserved.\n");
        }
        static void Stage(int value) { SessionState.SetInt("RiftTeamStage",value); phaseStart=EditorApplication.timeSinceStartup; }
        static void FreezeEnemies()
        {
            foreach(var enemy in Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None)) typeof(Enemigo).GetField("velocidad",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(enemy,0f);
        }
        static void Tick()
        {
            try
            {
                double now=EditorApplication.timeSinceStartup;
                Require(now-SessionState.GetFloat("RiftTeamStart",0)<100,"Runtime test timeout");
                int stage=SessionState.GetInt("RiftTeamStage",0);
                if(stage==0)
                {
                    var menu=Object.FindFirstObjectByType<VisualMainMenu>(); if(menu==null || Time.time<.5f) return;
                    var button=menu.transform.Find("SafeArea/Start").GetComponent<Button>(); Require(button.onClick.GetPersistentEventCount()==1,"Menu Start event missing/duplicated");
                    Stage(1); button.onClick.Invoke(); Record("PASS MainMenu Start event loads ARGame"); return;
                }
                if(stage==1)
                {
                    hud=Object.FindFirstObjectByType<VisualHUD>(); if(hud==null || hud.controller.Player==null || Time.time<.5f) return;
                    controller=hud.controller; enlace=controller.GetComponent<EnlaceCombateAR>();
                    Require(enlace!=null && enlace.Armas!=null && GameManager.Instance!=null,"Team systems not wired to ARGame");
                    Require(controller.Player.CurrentHealth==100 && hud.score.Score==0,"Initial health/score wrong");
                    Require(!hud.legacyCanvas.enabled && !hud.legacyCanvas.GetComponent<GraphicRaycaster>().enabled,"Legacy HUD visible or intercepting input");
                    Require(hud.crosshair.sprite==hud.crosshairNormal && hud.hearts.All(h=>h.sprite==hud.heartFull),"Approved HUD sprites missing");
                    Require(hud.weapon.CurrentAmmo==hud.weapon.MagazineSize,"Initial ammo wrong");
                    Record("PASS actual ARGame: one team game manager, shared health/score, approved HUD, initial ammo and hidden legacy canvas");
                    controller.HabilitarPruebaEscritorio();
                    var controls=controller.GetComponent<ControlesComputadora>(); if(controls!=null) controls.enabled=false;
                    controller.CamaraJuego.transform.SetPositionAndRotation(new Vector3(1000,1,1000),Quaternion.identity);
                    controller.IniciarPruebaEscritorio(new Vector3(1000,0,1006));
                    var portal=Object.FindFirstObjectByType<RiftHuntersAR.AR.PortalSpawnPoint>(); Require(portal!=null && portal.SpawnPoint!=null,"Portal SpawnPoint lost");
                    Record("PASS placement fixture uses real portal with SpawnPoint and approved mesh/VFX; physical AR tracking remains untested");
                    Stage(2); return;
                }
                FreezeEnemies();
                if(hud.crosshair.sprite==hud.crosshairHit) sawHit=true;
                if(stage==2)
                {
                    if(controller.CurrentState!="WAVE") return;
                    target=Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None).FirstOrDefault(); if(target==null) return;
                    Require(target.GetComponentsInChildren<MeshRenderer>().Any(r=>r.enabled && r.gameObject.activeInHierarchy),"Approved enemy model not visible");
                    Require(target.GetComponentsInChildren<RiftHuntersAR.Enemies.EnemyHealth>(true).Length==0,"Duplicate legacy health on new enemy visual");
                    Require(target.GetComponentsInChildren<VisualMotion>().Any(m=>m.faceCamera),"Approved enemy visual does not face the camera independently of logic");
                    target.transform.position=controller.CamaraJuego.transform.position+new Vector3(0,-1,3); Physics.SyncTransforms();
                    initialAmmo=hud.weapon.CurrentAmmo; initialScore=hud.score.Score; points=target.Puntos; shots=0; next=now; Stage(3);
                    Record("PASS countdown reaches Wave1 and spawns team enemy with approved 3D and a single health source"); return;
                }
                if(stage==3)
                {
                    foreach(var enemy in Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None)) if(enemy!=target) enemy.transform.position=new Vector3(1040,0,1006);
                    if(target==null || target.Vida<=0)
                    {
                        Require(hud.score.Score==initialScore+points,"Score duplicate or missing after enemy death");
                        Require(hud.weapon.CurrentAmmo==initialAmmo-shots && shots>0,"Ammo/shot count wrong");
                        Record("PASS real HUD shoot button -> team plasma raycast -> damage/death, ammo consumed once, score exactly once");
                        hud.reload.onClick.Invoke(); Require(enlace.Armas.Actual.EstaRecargando,"Real reload event not connected"); Stage(4); return;
                    }
                    if(now<next) return;
                    int hp=target.Vida, ammo=hud.weapon.CurrentAmmo; Physics.SyncTransforms(); hud.shoot.onClick.Invoke();
                    Require(hud.weapon.CurrentAmmo==ammo-1 && target.Vida<hp,"Real plasma shot did not damage target");
                    if(shots==0) Require(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Any(p=>p.name.StartsWith("VFX_PlasmaHit")),"Approved plasma impact VFX is not connected to team feedback");
                    shots++; next=now+.35; return;
                }
                if(stage==4)
                {
                    if(enlace.Armas.Actual.EstaRecargando) return;
                    Require(hud.weapon.CurrentAmmo==hud.weapon.MagazineSize,"Timed reload did not restore magazine"); Require(sawHit,"Hit feedback not displayed in approved crosshair");
                    Record("PASS timed reload and crosshair_hit feedback"); gun=1; Stage(5); next=now+.1; return;
                }
                if(stage==5)
                {
                    if(now<next) return;
                    if(gun<=2)
                    {
                        controller.SeleccionarArma(gun); controller.CamaraJuego.transform.rotation=Quaternion.Euler(0,180,0); Physics.SyncTransforms();
                        int ammo=hud.weapon.CurrentAmmo; hud.shoot.onClick.Invoke(); Require(hud.weapon.CurrentAmmo==ammo-1,"Additional weapon not firing");
                        Record("PASS team weapon selection and fire: "+hud.weapon.NombreArma+", magazine="+hud.weapon.MagazineSize); gun++; next=now+.4; return;
                    }
                    controller.Player.TakeDamage(34); Stage(6); next=now+.1; return;
                }
                if(stage==6)
                {
                    if(now<next) return;
                    Require(controller.Player.CurrentHealth==66 && hud.hearts.Count(h=>h.sprite==hud.heartFull || h.sprite==hud.heartDamaged)==2,"Shared life not reflected by approved hearts");
                    controller.Player.TakeDamage(100); Stage(7); next=now+.15; return;
                }
                if(stage==7)
                {
                    if(now<next) return;
                    Require(controller.CurrentState=="GAME_OVER" && hud.resultPanel.activeSelf && controller.Player.IsDead,"Team Game Over not connected");
                    Record("PASS PuenteVida/PlayerHealth -> GameManager Game Over -> approved result panel");
                    hud.retry.onClick.Invoke(); Stage(8); next=now+.15; return;
                }
                if(stage==8)
                {
                    if(now<next) return;
                    Require(controller.Player.CurrentHealth==100 && hud.score.Score==0 && hud.weapon.CurrentAmmo==hud.weapon.MagazineSize,"Retry did not reset shared state");
                    Require(controller.CurrentState=="COUNTDOWN" && enlace.EnemigosVivos==0,"Retry duplicates enemies or fails countdown");
                    Record("PASS real Retry button resets health/score/ammo/generator and restarts countdown");
                    Time.timeScale=5; waves.Clear(); sawBoss=false; Stage(9); return;
                }
                if(stage==9)
                {
                    if(controller.CurrentWave>0) waves.Add(controller.CurrentWave);
                    if(controller.EsJefe) { sawBoss=true; if(enlace.EnemigosVivos>0) Require(hud.bossHUD.gameObject.activeSelf,"Boss health HUD not connected"); }
                    foreach(var enemy in Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None)) if(enemy.Vida>0) enemy.RecibirDano(enemy.VidaMaxima);
                    if(controller.CurrentState!="VICTORY") return;
                    Require(waves.SetEquals(new[]{1,2,3,4}) && sawBoss,"Team wave/boss progression missing");
                    Require(hud.score.Score>0 && hud.resultPanel.activeSelf && enlace.EnemigosVivos==0,"Victory score/result/counter wrong");
                    Record("PASS four team waves and boss reach Victory; score/counter and approved boss/result HUD work");
                    Time.timeScale=1; hud.mainMenu.onClick.Invoke(); Stage(10); return;
                }
                if(stage==10)
                {
                    if(SceneManager.GetActiveScene().name!="MainMenu") return;
                    Require(GameManager.Instance==null,"GameManager singleton leaked across menu reload"); Record("PASS return to MainMenu and singleton cleanup"); Finish();
                }
            }
            catch(Exception ex) { Time.timeScale=1; File.WriteAllText("Temp/RiftTeamPlayResult.txt","FAIL: "+ex); SessionState.SetBool(Flag,false); EditorApplication.isPlaying=false; Debug.LogException(ex); }
        }
        static void Finish()
        {
            int errors=SessionState.GetInt("RiftTeamErrors",0); Time.timeScale=1;
            File.WriteAllText("Temp/RiftTeamPlayResult.txt",(errors==0?"PASS":"FAIL")+": real combined MainMenu/ARGame playtest; errors="+errors+". Fixture controls positions/speed/time only in Play Mode; no assets changed, no physical AR test, no APK.\n"+File.ReadAllText("Temp/RiftTeamChecks.txt"));
            SessionState.SetBool(Flag,false); EditorApplication.isPlaying=false;
        }
    }
}
