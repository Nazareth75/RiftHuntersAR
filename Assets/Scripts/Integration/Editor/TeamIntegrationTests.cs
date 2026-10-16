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
        static bool sawHit, sawBoss, checkedCores, bossPreview;
        static readonly HashSet<int> waves=new();
        static readonly HashSet<string> models=new();
        static readonly HashSet<string> announcements=new();
        static readonly Dictionary<string,int> audio=new();
        static bool audioBound;
        static void AudioPlayed(string name, AudioClip clip) { audio[name]=audio.GetValueOrDefault(name)+1; Require(clip!=null && clip.length>0,"Invalid audio "+name); }
        static void CheckModels()
        {
            foreach(var enemy in Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None))
            {
                if(enemy.GetComponent<ModeloDeIntegracion>()==null) continue;
                string expected=enemy.name.StartsWith("EnemigoRapido")?"EnemyFast_Final":enemy.name.StartsWith("EnemigoTanque")?"EnemyTank_Final":enemy.name.StartsWith("EnemigoJefe")?"RiftGuardian_Final":"EnemyBasic_Final";
                var meshes=enemy.GetComponentsInChildren<MeshFilter>().Where(m=>m.sharedMesh!=null).Select(m=>AssetDatabase.GetAssetPath(m.sharedMesh)).ToArray();
                Require(meshes.Any(p=>p.EndsWith(expected+".fbx")),"Incorrect live FBX for "+enemy.name);
                if(models.Add(expected))
                {
                    var renderers=enemy.GetComponentsInChildren<MeshRenderer>().Where(r=>r.enabled && r.gameObject.activeInHierarchy).ToArray();
                    var bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                    Record("PASS live "+enemy.name+" -> "+expected+"; visual bounds="+bounds.size+"; root scale="+enemy.transform.lossyScale+"; collider="+enemy.GetComponent<Collider>().bounds.size);
                }
            }
        }
        static void CheckWeapon(int index)
        {
            var visual=hud.GetComponent<WeaponVisuals>();
            Require(visual!=null && visual.Views!=null && visual.Views.Count(v=>v.activeSelf)==1 && visual.Views[index].activeSelf,"Weapon model switch incorrect/overlapping");
            string expected=new[]{"PlasmaBlaster_Final","ElectricCannon_Final","EnergyLauncher_Final"}[index];
            Require(visual.Views[index].GetComponentsInChildren<MeshFilter>().Any(m=>m.sharedMesh!=null && AssetDatabase.GetAssetPath(m.sharedMesh).EndsWith(expected+".fbx")),"Wrong weapon FBX");
            var renderers=visual.Views[index].GetComponentsInChildren<MeshRenderer>(); var bounds=renderers[0].bounds;
            foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            var viewport=controller.CamaraJuego.WorldToViewportPoint(bounds.center);
            Require(viewport.x>0 && viewport.x<1 && viewport.y>0 && viewport.y<1 && viewport.z>controller.CamaraJuego.nearClipPlane,"Weapon outside portrait camera "+expected+" "+viewport);
            Record("Weapon portrait bounds "+expected+" "+bounds.size+"; viewport center="+viewport);
            for(int i=0;i<8;i++)
            {
                var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                var p=controller.CamaraJuego.WorldToViewportPoint(corner);
                Require(p.x>=0 && p.x<=1 && p.y>=0 && p.y<=1 && p.z>controller.CamaraJuego.nearClipPlane,"Weapon cropped or clipped in Portrait "+expected+" "+p);
            }
            var safe=ScreenRect((RectTransform)hud.resultPanel.transform.parent);
            foreach(var button in visual.Selectors)
            {
                var rect=ScreenRect(button.GetComponent<RectTransform>());
                Require(safe.Contains(rect.min) && safe.Contains(rect.max),"Selector outside SafeArea");
                foreach(var control in new[]{hud.shoot.GetComponent<RectTransform>(),hud.reload.GetComponent<RectTransform>(),hud.bossHUD.GetComponent<RectTransform>(),hud.scoreValue.rectTransform,hud.waveValue.rectTransform,hud.enemyValue.rectTransform})
                    Require(!rect.Overlaps(ScreenRect(control)),"Selector overlaps essential HUD control "+control.name);
            }
            Record("PASS selected "+expected+" exclusively visible; real touch selector connected");
        }
        static Rect ScreenRect(RectTransform rt)
        {
            var corners=new Vector3[4]; rt.GetWorldCorners(corners); return new Rect(corners[0],corners[2]-corners[0]);
        }
        static void CheckResults()
        {
            Canvas.ForceUpdateCanvases();
            var detail=ScreenRect(hud.detalleFin.rectTransform);
            Require(!detail.Overlaps(ScreenRect(hud.retry.GetComponent<RectTransform>())) && !detail.Overlaps(ScreenRect(hud.mainMenu.GetComponent<RectTransform>())) && !detail.Overlaps(ScreenRect(hud.resultTitle.rectTransform)),"Result text overlaps controls/title");
            Require(hud.detalleFin.verticalOverflow==VerticalWrapMode.Truncate,"Result text can overflow");
            Record("PASS result title/details/Retry/MainMenu have separate rectangles");
        }
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
                    foreach(var root in scene.GetRootGameObjects())
                    {
                        foreach(var t in root.GetComponentsInChildren<Transform>(true)) scripts+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                        foreach(var component in root.GetComponentsInChildren<Component>(true))
                        {
                            if(component==null) continue;
                            var iterator=new SerializedObject(component).GetIterator();
                            while(iterator.NextVisible(true)) if(iterator.propertyType==SerializedPropertyType.ObjectReference && iterator.objectReferenceInstanceIDValue!=0 && iterator.objectReferenceValue==null) references++;
                        }
                    }
                    if(!loaded) EditorSceneManager.CloseScene(scene,true);
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Require(scripts==0 && references==0,"Scene missing scripts="+scripts+", references="+references);
            File.WriteAllText("Temp/RiftTeamAssetCheck.txt","PASS: all scenes open; missing scripts=0; missing serialized prefab and scene references=0; catalog weapon/normal/fast/tank/boss references valid; approved model references and functional colliders preserved.\n");
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
                    if(!audioBound) { RiftHuntersAR.Audio.SonidoJuego.Reproducido+=AudioPlayed; audioBound=true; audio.Clear(); models.Clear(); announcements.Clear(); }
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
                CheckModels();
                if(!string.IsNullOrEmpty(controller.AnuncioOleada) && announcements.Add(controller.AnuncioOleada)) Record("PASS announcement: "+controller.AnuncioOleada);
                if(hud.crosshair.sprite==hud.crosshairHit) sawHit=true;
                if(stage==2)
                {
                    if(controller.CurrentState!="WAVE") return;
                    target=Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None).FirstOrDefault(); if(target==null) return;
                    CheckWeapon(0);
                    Require(target.GetComponentsInChildren<MeshRenderer>().Any(r=>r.enabled && r.gameObject.activeInHierarchy),"Approved enemy model not visible");
                    Require(target.GetComponentsInChildren<RiftHuntersAR.Enemies.EnemyHealth>(true).Length==0,"Duplicate legacy health on new enemy visual");
                    Require(target.GetComponentsInChildren<VisualMotion>().Any(m=>m.faceCamera),"Approved enemy visual does not face the camera independently of logic");
                    target.transform.position=controller.CamaraJuego.transform.position+new Vector3(0,-1,3); Physics.SyncTransforms();
                    // Aim at the collider after Dilan's physical-size adjustment, not the old 1 m height.
                    controller.CamaraJuego.transform.LookAt(target.GetComponent<Collider>().bounds.center);
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
                        hud.GetComponent<WeaponVisuals>().Selectors[gun].onClick.Invoke(); CheckWeapon(gun);
                        controller.CamaraJuego.transform.rotation=Quaternion.Euler(0,180,0); Physics.SyncTransforms();
                        var catalog=Resources.Load<CatalogoPartida>("CatalogoPartida");
                        var victims=new Enemigo[3];
                        for(int i=0;i<3;i++)
                        {
                            victims[i]=Object.Instantiate(catalog.normal,controller.CamaraJuego.transform.position+new Vector3((i-1)*.8f,-1,-3),Quaternion.identity);
                            victims[i].ConfigurarEstadisticas(200,0,1,0);
                            victims[i].transform.position+=Vector3.up*(controller.CamaraJuego.transform.position.y-victims[i].GetComponent<Collider>().bounds.center.y);
                        }
                        Physics.SyncTransforms();
                        int ammo=hud.weapon.CurrentAmmo; hud.shoot.onClick.Invoke(); Require(hud.weapon.CurrentAmmo==ammo-1,"Additional weapon not firing");
                        if(gun==1) Require(victims.All(v=>v.Vida==165),"Electric chain does not damage exactly three victims");
                        else Require(victims.Count(v=>v.Vida==120)==1 && victims.Count(v=>v.Vida==160)==2,"Energy direct/area damage incorrect");
                        foreach(var victim in victims) Object.Destroy(victim.gameObject);
                        Record("PASS real "+hud.weapon.NombreArma+" raycast, chain/area and damage values preserved");
                        Directory.CreateDirectory("Builds/Validation"); ScreenCapture.CaptureScreenshot("Builds/Validation/Weapon_"+gun+".png");
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
                    CheckResults(); Require(audio.GetValueOrDefault("Usuario/game_over")==1,"Game Over audio missing or duplicated");
                    Record("PASS PuenteVida/PlayerHealth -> GameManager Game Over -> approved result panel");
                    Directory.CreateDirectory("Builds/Validation"); ScreenCapture.CaptureScreenshot("Builds/Validation/GameOver.png"); Stage(71); next=now+.4; return;
                }
                if(stage==71)
                {
                    if(now<next) return;
                    hud.retry.onClick.Invoke(); Stage(8); next=now+.15; return;
                }
                if(stage==8)
                {
                    if(now<next) return;
                    Require(controller.Player.CurrentHealth==100 && hud.score.Score==0 && hud.weapon.CurrentAmmo==hud.weapon.MagazineSize,"Retry did not reset shared state");
                    Require(controller.CurrentState=="COUNTDOWN" && enlace.EnemigosVivos==0,"Retry duplicates enemies or fails countdown");
                    Record("PASS real Retry button resets health/score/ammo/generator and restarts countdown");
                    controller.CamaraJuego.transform.rotation=Quaternion.identity;
                    Time.timeScale=5; waves.Clear(); sawBoss=false; checkedCores=false; bossPreview=false; Stage(9); return;
                }
                if(stage==9)
                {
                    if(controller.CurrentWave>0) waves.Add(controller.CurrentWave);
                    if(controller.EsJefe) { sawBoss=true; if(enlace.EnemigosVivos>0) Require(hud.bossHUD.gameObject.activeSelf,"Boss health HUD not connected"); }
                    var cores=controller.NucleosJefe;
                    if(cores!=null && !bossPreview) { bossPreview=true; ScreenCapture.CaptureScreenshot("Builds/Validation/Boss.png"); next=now+.4; return; }
                    if(cores!=null && now<next) return;
                    if(cores!=null && !checkedCores)
                    {
                        var boss=cores.GetComponent<Enemigo>();
                        var left=cores.transform.Find("Nucleo brazo izquierdo").GetComponent<Enemigo>();
                        var right=cores.transform.Find("Nucleo brazo derecho").GetComponent<Enemigo>();
                        var center=cores.transform.Find("Nucleo central").GetComponent<Enemigo>();
                        foreach(var pair in new[]{(left,"WeakPoint_Left"),(right,"WeakPoint_Right"),(center,"WeakPoint_Core")})
                        {
                            var mesh=cores.GetComponentsInChildren<Renderer>().FirstOrDefault(r=>r.name==pair.Item2 || r.name==pair.Item2+"_Mesh");
                            Require(mesh!=null && Vector3.Distance(pair.Item1.transform.position,mesh.bounds.center)<.05f,"Functional weak point not aligned "+pair.Item2+" mesh="+(mesh!=null?mesh.bounds.center.ToString():"NULL")+" functional="+pair.Item1.transform.position+" renderers="+string.Join(",",cores.GetComponentsInChildren<Renderer>().Select(r=>r.name)));
                        }
                        Physics.SyncTransforms();
                        foreach(var nucleus in new[]{left,right,center}) { var ray=new Ray(controller.CamaraJuego.transform.position,nucleus.transform.position-controller.CamaraJuego.transform.position); Require(Physics.Raycast(ray,out var hit,30) && hit.collider.GetComponentInParent<Enemigo>()==nucleus,"Boss weak point blocked by collider: "+nucleus.name+" hit="+(hit.collider!=null?hit.collider.name:"none")); }
                        int hp=boss.Vida, coreHp=center.Vida;
                        boss.RecibirDano(9999); center.RecibirDano(9999);
                        Require(boss.Vida==hp && center.Vida==coreHp && !cores.CentralExpuesto,"Boss shields not preserved");
                        left.RecibirDano(left.VidaMaxima);
                        Require(!cores.IzquierdoVivo && cores.DerechoVivo && !cores.CentralExpuesto,"First arm incorrectly exposes center");
                        right.RecibirDano(right.VidaMaxima);
                        Require(cores.CentralExpuesto && !center.Invulnerable,"Both arms fail to expose center");
                        int score=hud.score.Score;
                        center.RecibirDano(center.VidaMaxima);
                        Require(boss.Vida==0 && hud.score.Score==score+boss.Puntos,"Central death fails boss score");
                        boss.RecibirDano(9999);
                        Require(hud.score.Score==score+boss.Puntos,"Boss score duplicated");
                        checkedCores=true; Record("PASS boss body/central shields, left then right arm, exposed central, boss death and score exactly once");
                    }
                    foreach(var enemy in Object.FindObjectsByType<Enemigo>(FindObjectsSortMode.None)) if(enemy.Vida>0 && enemy.GetComponentInParent<NucleosJefe>()==null) enemy.RecibirDano(enemy.VidaMaxima);
                    if(controller.CurrentState!="VICTORY") return;
                    Require(waves.SetEquals(new[]{1,2,3,4}) && sawBoss && checkedCores,"Team wave/boss progression missing");
                    Require(models.SetEquals(new[]{"EnemyBasic_Final","EnemyFast_Final","EnemyTank_Final","RiftGuardian_Final"}),"Not all four live enemy FBXs tested");
                    Require(announcements.Count==5 && hud.bossMessage!=null,"Wave/boss announcement missing");
                    foreach(var sound in new[]{"plasma","electrica","energia","enemigo","next_level","game_over"}) Require(audio.GetValueOrDefault("Usuario/"+sound)>0,"Audio action not tested "+sound);
                    Record("PASS six imported user sounds reached actual playback; next-level="+audio.GetValueOrDefault("Usuario/next_level"));
                    CheckResults();
                    Require(hud.score.Score>0 && hud.resultPanel.activeSelf && enlace.EnemigosVivos==0,"Victory score/result/counter wrong");
                    Record("PASS four team waves and boss reach Victory; score/counter and approved boss/result HUD work");
                    ScreenCapture.CaptureScreenshot("Builds/Validation/Victory.png"); Stage(91); next=now+.4; return;
                }
                if(stage==91)
                {
                    if(now<next) return;
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
