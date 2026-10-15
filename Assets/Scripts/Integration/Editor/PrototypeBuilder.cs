using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.ARCore;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;
using RiftHuntersAR.AR;
using RiftHuntersAR.Combat;
using RiftHuntersAR.Enemies;
using RiftHuntersAR.Game;
using Object = UnityEngine.Object;

namespace RiftHuntersAR.Integration.Editor
{
    [InitializeOnLoad]
    public static class PrototypeBuilder
    {
        private const string ScenePath = "Assets/Scenes/Gameplay/ARGame.unity";
        private const string CommandPath = "Temp/RiftPrototypeCommand.txt";
        static PrototypeBuilder() { EditorApplication.update += PollCommand; }
        private static void PollCommand()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(CommandPath)) return;
            string command;
            try { command = File.ReadAllText(CommandPath).Trim(); File.Delete(CommandPath); }
            catch (IOException) { return; }
            try
            {
                if (command == "create") Create();
                else if (command == "build") Build();
                else if (command == "test") CombatTest();
                else if (command == "playtest-tests") PlaytestTests();
                else if (command == "configure-playtest") ConfigurePlaytest();
                else if (command == "anchor") ConfigureAnchor();
                else if (command == "android") EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                else throw new Exception("Unknown prototype command");
                File.WriteAllText("Temp/RiftPrototypeResult.txt", "SUCCESS: " + command);
            }
            catch (Exception ex)
            {
                File.WriteAllText("Temp/RiftPrototypeResult.txt", "FAILED: " + command + "\n" + ex);
                Debug.LogException(ex);
            }
        }
        private static void Set(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            var field = serialized.FindProperty(property);
            if (field == null) throw new Exception("Missing serialized field: " + property);
            field.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        private static GameObject Child(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }
        private static Material Material(string name, Color color)
        {
            string path = "Assets/Art/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
                material.SetColor("_BaseColor", color);
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }
        [MenuItem("Rift Hunters/Create AR Prototype")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) throw new Exception("ARGame already exists; refusing to overwrite it.");
            if (Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Any(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty))
                throw new Exception("Save existing scene changes before creating the prototype.");
            ConfigureAndroid();
            var portal = new GameObject("PortalPlaceholder");
            var point = portal.AddComponent<PortalSpawnPoint>();
            var visual = Child("Visual", portal.transform);
            // Primitive geometry only: PENDIENTE DE DISEÑO.
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "TemporaryPortalDisc";
            disc.transform.SetParent(visual.transform, false);
            disc.transform.localPosition = new Vector3(0, .015f, 0);
            disc.transform.localScale = new Vector3(.5f, .015f, .5f);
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            disc.GetComponent<Renderer>().sharedMaterial = Material("PortalPlaceholder", new Color(.2f, .6f, 1));
            var spawn = Child("SpawnPoint", portal.transform);
            spawn.transform.localPosition = Vector3.zero;
            Set(point, "spawnPoint", spawn.transform);
            var portalAsset = PrefabUtility.SaveAsPrefabAsset(portal, "Assets/Prefabs/Portal/PortalPlaceholder.prefab");
            Object.DestroyImmediate(portal);

            var enemy = new GameObject("EnemyBasicPlaceholder");
            var health = enemy.AddComponent<EnemyHealth>();
            var enemyVisual = Child("Visual", enemy.transform);
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "TemporaryCapsule";
            capsule.transform.SetParent(enemyVisual.transform, false);
            capsule.transform.localPosition = new Vector3(0, .3f, 0);
            capsule.transform.localScale = new Vector3(.25f, .3f, .25f);
            Object.DestroyImmediate(capsule.GetComponent<Collider>());
            capsule.GetComponent<Renderer>().sharedMaterial = Material("EnemyPlaceholder", new Color(1, .3f, .2f));
            var hit = Child("HitCollider", enemy.transform).AddComponent<CapsuleCollider>();
            hit.center = new Vector3(0, .3f, 0); hit.height = .6f; hit.radius = .125f;
            Child("AimPoint", enemy.transform).transform.localPosition = new Vector3(0, .3f, 0);
            var enemyAsset = PrefabUtility.SaveAsPrefabAsset(enemy, "Assets/Prefabs/Enemies/EnemyBasicPlaceholder.prefab");
            Object.DestroyImmediate(enemy);

            var plane = new GameObject("PlanePlaceholder", typeof(ARPlane), typeof(MeshFilter), typeof(MeshRenderer), typeof(ARPlaneMeshVisualizer));
            plane.GetComponent<MeshRenderer>().sharedMaterial = Material("PlanePlaceholder", new Color(.15f, .4f, .3f));
            var planeAsset = PrefabUtility.SaveAsPrefabAsset(plane, "Assets/Prefabs/Portal/PlanePlaceholder.prefab");
            Object.DestroyImmediate(plane);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("AR Session", typeof(ARSession), typeof(ARInputManager));
            var originGo = new GameObject("XR Origin", typeof(XROrigin), typeof(ARPlaneManager), typeof(ARRaycastManager), typeof(ARAnchorManager));
            var offset = Child("Camera Offset", originGo.transform);
            var cameraGo = Child("AR Camera", offset.transform);
            var camera = cameraGo.AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            camera.nearClipPlane = .05f; camera.farClipPlane = 30;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.AddComponent<ARCameraManager>();
            cameraGo.AddComponent<ARCameraBackground>();
            var pose = cameraGo.AddComponent<TrackedPoseDriver>();
            pose.positionInput = new InputActionProperty(new InputAction("Position", InputActionType.Value, "<HandheldARInputDevice>/devicePosition", expectedControlType: "Vector3"));
            pose.rotationInput = new InputActionProperty(new InputAction("Rotation", InputActionType.Value, "<HandheldARInputDevice>/deviceRotation", expectedControlType: "Quaternion"));
            var origin = originGo.GetComponent<XROrigin>();
            origin.Camera = camera; origin.CameraFloorOffsetObject = offset; origin.CameraYOffset = 0;
            var planes = originGo.GetComponent<ARPlaneManager>();
            planes.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            planes.planePrefab = planeAsset;
            var systems = new GameObject("Prototype Systems");
            var score = systems.AddComponent<ScoreManager>();
            var weapon = systems.AddComponent<PlasmaWeapon>();
            Set(weapon, "aimCamera", camera);
            var controller = systems.AddComponent<ARPrototypeController>();
            var canvasGo = new GameObject("Temporary HUD - PENDIENTE DE DISEÑO", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = .5f;
            var crosshair = Label("Crosshair", "+", canvasGo.transform, new Vector2(.5f,.5f), new Vector2(0,0), new Vector2(80,80), 60);
            var scoreLabel = Label("Score", "SCORE: 0", canvasGo.transform, new Vector2(.5f,1), new Vector2(0,-100), new Vector2(800,80), 44);
            var stateLabel = Label("State", "SCAN ENVIRONMENT", canvasGo.transform, new Vector2(.5f,1), new Vector2(0,-200), new Vector2(950,100), 40);
            var feedback = Label("Feedback", "PENDIENTE DE DISEÑO", canvasGo.transform, new Vector2(.5f,0), new Vector2(0,350), new Vector2(950,80), 28);
            var buttonGo = new GameObject("SHOOT", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(canvasGo.transform, false);
            Rect(buttonGo.GetComponent<RectTransform>(), new Vector2(.5f,0), new Vector2(0,180), new Vector2(600,180));
            buttonGo.GetComponent<Image>().color = new Color(.15f,.15f,.15f,.9f);
            var button = buttonGo.GetComponent<Button>();
            button.interactable = false;
            Label("Label", "SHOOT", buttonGo.transform, new Vector2(.5f,.5f), Vector2.zero, new Vector2(600,180), 50);
            var eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventGo.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            Set(controller,"planes", planes); Set(controller,"raycasts",originGo.GetComponent<ARRaycastManager>());
            Set(controller,"portalPrefab",portalAsset.GetComponent<PortalSpawnPoint>()); Set(controller,"enemyPrefab",enemyAsset.GetComponent<EnemyHealth>());
            Set(controller,"anchors",originGo.GetComponent<ARAnchorManager>());
            Set(controller,"weapon",weapon); Set(controller,"score",score); Set(controller,"statusText",stateLabel); Set(controller,"scoreText",scoreLabel); Set(controller,"feedbackText",feedback); Set(controller,"shootButton",button);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene(ScenePath,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[Rift] ARGame and placeholder prefabs created.");
        }
        private static void Rect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        { rect.anchorMin=rect.anchorMax=anchor; rect.pivot=new Vector2(.5f,.5f); rect.anchoredPosition=position; rect.sizeDelta=size; }
        private static Text Label(string name,string value,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,int fontSize)
        {
            var go = new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            Rect(go.GetComponent<RectTransform>(),anchor,position,size);
            var text=go.GetComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text=value; text.fontSize=fontSize;
            text.alignment=TextAnchor.MiddleCenter; text.color=Color.white; text.raycastTarget=false;
            return text;
        }
        private static void ConfigureAndroid()
        {
            const string xrPath="Assets/Settings/RiftXRSettings.asset";
            EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var container);
            if (container == null) { var ids = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget"); if (ids.Length > 0) container = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(AssetDatabase.GUIDToAssetPath(ids[0])); }
            if (container == null) { container=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>(); AssetDatabase.CreateAsset(container,xrPath); }
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,container,true);
            if (!container.HasSettingsForBuildTarget(BuildTargetGroup.Android)) container.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            if (!container.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android)) container.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            var general=container.SettingsForBuildTarget(BuildTargetGroup.Android); general.InitManagerOnStart=true;
            if (!XRPackageMetadataStore.AssignLoader(general.Manager,"UnityEngine.XR.ARCore.ARCoreLoader",BuildTargetGroup.Android)) throw new Exception("ARCore loader assignment failed");
            var arcore=ARCoreSettings.GetOrCreateSettings();
            if (!AssetDatabase.Contains(arcore)) AssetDatabase.CreateAsset(arcore,"Assets/Settings/RiftARCoreSettings.asset");
            arcore.requirement=ARCoreSettings.Requirement.Required; arcore.depth=ARCoreSettings.Requirement.Optional; ARCoreSettings.currentSettings=arcore;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.rifthunters.ar");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.productName="RIFT HUNTERS AR";
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/Mobile_Renderer.asset");
            if (renderer == null) throw new Exception("Missing mobile URP renderer");
            if (!renderer.rendererFeatures.Any(f=>f is ARBackgroundRendererFeature))
            {
                var feature=ScriptableObject.CreateInstance<ARBackgroundRendererFeature>(); feature.name="AR Camera Background";
                AssetDatabase.AddObjectToAsset(feature,renderer); renderer.rendererFeatures.Add(feature); feature.Create(); EditorUtility.SetDirty(renderer);
            }
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
            if (pipeline==null) throw new Exception("Missing mobile URP pipeline");
            GraphicsSettings.defaultRenderPipeline=pipeline;
            int quality=QualitySettings.GetQualityLevel();
            for(int i=0;i<QualitySettings.names.Length;i++) { QualitySettings.SetQualityLevel(i,false); QualitySettings.renderPipeline=pipeline; }
            QualitySettings.SetQualityLevel(quality,false);
            EditorUtility.SetDirty(container); EditorUtility.SetDirty(general); EditorUtility.SetDirty(general.Manager); EditorUtility.SetDirty(arcore);
            AssetDatabase.SaveAssets();
        }
        private static void ConfigureAnchor()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != ScenePath || scene.isDirty) throw new Exception("Open saved ARGame before configuring anchors");
            var planes = Object.FindFirstObjectByType<ARPlaneManager>();
            var anchors = planes.GetComponent<ARAnchorManager>();
            if (anchors == null) anchors = planes.gameObject.AddComponent<ARAnchorManager>();
            Set(Object.FindFirstObjectByType<ARPrototypeController>(), "anchors", anchors);
            EditorSceneManager.SaveScene(scene);
        }
        [MenuItem("Rift Hunters/Test Four Hits And Score")]
        public static void CombatTest()
        {
            var go=new GameObject("Health test"); var health=go.AddComponent<EnemyHealth>();
            var scoreObject=new GameObject("Score test"); var score=scoreObject.AddComponent<ScoreManager>();
            int deaths=0;
            health.Died += _=>{deaths++; score.AddEnemyScore();};
            // Awake runs on AddComponent in play mode; in edit mode invoke explicitly for this isolated smoke test.
            typeof(EnemyHealth).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(health, null);
            for(int i=0;i<3;i++) health.TakeDamage(25);
            if(health.CurrentHealth!=25 || deaths!=0 || score.Score!=0) throw new Exception("Premature enemy death");
            health.TakeDamage(25); health.TakeDamage(25); health.Die();
            if(deaths!=1 || score.Score!=100 || go.activeSelf) throw new Exception("Death/score must happen exactly once");
            Object.DestroyImmediate(go); Object.DestroyImmediate(scoreObject);
            Debug.Log("[Rift] PASS: four hits, one death, score 100.");
        }
        private static void ConfigurePlaytest()
        {
            string path="Assets/Prefabs/Enemies/EnemyBasicPlaceholder.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            try { if(root.GetComponent<EnemyMovement>()==null) root.AddComponent<EnemyMovement>(); PrefabUtility.SaveAsPrefabAsset(root,path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Rift Hunters/Test Playtest")]
        public static void PlaytestTests()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Deten Play antes de ejecutar las pruebas del prototipo.");
                return;
            }
            var anterior = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var escenaPrueba = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath);
            bool estabaAbierta = escenaPrueba.IsValid() && escenaPrueba.isLoaded;
            if (!estabaAbierta) escenaPrueba = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
            CombatTest();
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EnemyBasicPlaceholder.prefab");
            if (prefab == null || prefab.GetComponent<EnemyHealth>() == null)
                throw new Exception("Falta el prefab original o su componente EnemyHealth.");
            if(new SerializedObject(prefab.GetComponent<EnemyHealth>()).FindProperty("maxHealth").floatValue!=100) throw new Exception("Prefab HP is not 100");
            if(prefab.GetComponentsInChildren<Collider>().Length!=1) throw new Exception("Unexpected enemy colliders");

            var controller = escenaPrueba.GetRootGameObjects()
                .SelectMany(raiz => raiz.GetComponentsInChildren<ARPrototypeController>(true)).FirstOrDefault();
            if (controller == null) throw new Exception("ARGame no contiene ARPrototypeController.");
            var refs=new SerializedObject(controller);
            var button=(Button)refs.FindProperty("shootButton").objectReferenceValue;
            if (button == null) throw new Exception("Falta la referencia al boton SHOOT en ARGame.");
            if(button.onClick.GetPersistentEventCount()!=0) throw new Exception("Unexpected persistent SHOOT listener");
            var source=(PlasmaWeapon)refs.FindProperty("weapon").objectReferenceValue;
            if (source == null) throw new Exception("Falta la referencia a PlasmaWeapon en ARGame.");
            if(new SerializedObject(source).FindProperty("damage").floatValue!=25) throw new Exception("Scene damage not 25");
            var enemy=Object.Instantiate(prefab).GetComponent<EnemyHealth>();
            var cameraGo=new GameObject("Test camera");
            var systemGo=new GameObject("Test weapon");
            var playerGo=new GameObject("Test player");
            try
            {
                enemy.transform.position=new Vector3(1000,0,1002);
                typeof(EnemyHealth).GetMethod("Awake",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(enemy,null);
                var camera=cameraGo.AddComponent<Camera>(); camera.transform.position=new Vector3(1000,.3f,1000);
                var weapon=systemGo.AddComponent<PlasmaWeapon>(); Set(weapon,"aimCamera",camera); weapon.Ready=true;
                var fire=typeof(PlasmaWeapon).GetMethod("Fire",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                int deaths=0; enemy.Died+=_=>deaths++;
                Physics.SyncTransforms();
                fire.Invoke(weapon,new object[]{false,0f,0});
                if(weapon.CurrentAmmo!=30) throw new Exception("Shot consumed while tracking unavailable");
                for(int i=0;i<4;i++)
                {
                    fire.Invoke(weapon,new object[]{true,(float)i,i+1});
                    fire.Invoke(weapon,new object[]{true,(float)i,i+1});
                    fire.Invoke(weapon,new object[]{true,i+.05f,i+10});
                    if(enemy.CurrentHealth!=100-25*(i+1) || weapon.CurrentAmmo!=29-i) throw new Exception("Raycast HP/ammo/duplicate test failed hit "+(i+1));
                }
                if(deaths!=1) throw new Exception("Expected one death");
                for(int i=4;i<35;i++) fire.Invoke(weapon,new object[]{true,(float)i,i+100});
                if(weapon.CurrentAmmo!=0) throw new Exception("Ammo must stop at zero");
                weapon.Reload(); if(weapon.CurrentAmmo!=30) throw new Exception("Reload failed");
                var player=playerGo.AddComponent<PlayerHealth>(); int lost=0; player.Died+=()=>lost++;
                player.TakeDamage(34); player.TakeDamage(34); if(player.IsDead) throw new Exception("Early player death");
                player.TakeDamage(34); player.TakeDamage(34); player.Die();
                if(lost!=1 || player.CurrentHealth!=0) throw new Exception("Player death duplicated");
                player.Retry(); if(player.CurrentHealth!=100 || player.IsDead) throw new Exception("Retry failed");
                player.TakeDamage(100); if(lost!=2) throw new Exception("Retry death not reset");
                player.Retry();
                var movement=enemy.GetComponent<EnemyMovement>(); movement.Initialize(camera.transform,player);
                enemy.transform.position=camera.transform.position+new Vector3(0,-.3f,2);
                var advance=typeof(EnemyMovement).GetMethod("Advance",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                var initial=enemy.transform.position;
                advance.Invoke(movement,new object[]{false,1000f,100f});
                if(enemy.transform.position!=initial) throw new Exception("Enemy moved without tracking");
                advance.Invoke(movement,new object[]{true,1000f,100f});
                var horizontal=enemy.transform.position-camera.transform.position; horizontal.y=0;
                if(Mathf.Abs(horizontal.magnitude-.65f)>.001f || enemy.transform.position.y!=initial.y) throw new Exception("Enemy crossed camera or changed floor height");
                advance.Invoke(movement,new object[]{true,1000f,.01f});
                advance.Invoke(movement,new object[]{true,1000.1f,.01f});
                if(player.CurrentHealth!=66) throw new Exception("Attack cooldown failed");
                advance.Invoke(movement,new object[]{true,1002.1f,.01f});
                if(player.CurrentHealth!=32) throw new Exception("Second attack failed");
                File.WriteAllText("Temp/RiftPlaytestTests.txt","PASS: actual prefab 100 HP, scene 25 damage, one collider, zero persistent listeners; physics raycast four hits 75/50/25/0; duplicate frame/cooldown rejected; ammo zero/reload; player death and retry; horizontal movement clamp, tracking pause and attack cooldown.");
            }
            finally { Object.DestroyImmediate(enemy.gameObject); Object.DestroyImmediate(cameraGo); Object.DestroyImmediate(systemGo); Object.DestroyImmediate(playerGo); }
                        Debug.Log("[Rift] PASS: pruebas originales completadas; la escena del usuario se conserva.");
            }
            finally
            {
                if (!estabaAbierta)
                {
                    EditorSceneManager.CloseScene(escenaPrueba, true);
                    if (anterior.IsValid()) UnityEngine.SceneManagement.SceneManager.SetActiveScene(anterior);
                }
            }
        }

        [MenuItem("Rift Hunters/Probar partida integrada")]
        public static void ProbarPartidaIntegrada()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene("Assets/Scenes/Pruebas/PartidaIntegrada.unity");
            EditorApplication.isPlaying = true;
        }
        [MenuItem("Rift Hunters/Build Android Prototype")]
        public static void Build()
        {
#if UNITY_ANDROID
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android) throw new Exception("Switch to Android before build");
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds/Android");
            // Unity 6000.3 no compila AndroidExternalToolsSettings. No se toca el SDK desde aquí.
            BuildReport report;
            bool outputCopyFailed = false;
            Directory.CreateDirectory("Logs");
            Application.LogCallback log = (message, stack, type) =>
            {
                if (message.Contains("DirectoryNotFoundException") && message.Contains("launcher-debug.apk")) outputCopyFailed = true;
                File.AppendAllText("Logs/RiftBuild.log", type + ": " + message + Environment.NewLine + stack + Environment.NewLine);
            };
            Application.logMessageReceived += log;
            try
            {
                report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/Scenes/MainMenu/MainMenu.unity",ScenePath},locationPathName="Builds/Android/RiftHuntersAR_IntegracionEquipo_Final.apk",target=BuildTarget.Android,options=BuildOptions.Development});
            }
            finally
            {
                Application.logMessageReceived -= log;
            }
            string generatedGradle = "Library/Bee/Android/Prj/IL2CPP/Gradle";
            string shortApk = "Builds/g/launcher/outputs/apk/debug/launcher-debug.apk";
            bool shortPaths = File.Exists(generatedGradle + "/build.gradle") && File.ReadAllText(generatedGradle + "/build.gradle").Contains("// Rift short build paths");
            bool repaired = false;
            if (shortPaths && File.Exists(shortApk) && (report.summary.result == BuildResult.Succeeded || (outputCopyFailed && report.summary.totalErrors == 1)))
            {
                using (var archive = System.IO.Compression.ZipFile.OpenRead(shortApk))
                {
                    if (archive.GetEntry("AndroidManifest.xml") == null || archive.GetEntry("lib/arm64-v8a/libil2cpp.so") == null)
                        throw new Exception("Short-path output is not the expected ARM64 APK");
                }
                string expected = generatedGradle + "/launcher/build/outputs/apk/debug/launcher-debug.apk";
                Directory.CreateDirectory(Path.GetDirectoryName(expected));
                File.Copy(shortApk, expected, true);
                File.Copy(shortApk, "Builds/Android/RiftHuntersAR_IntegracionEquipo_Final.apk", true);
                repaired = outputCopyFailed;
            }
            if (repaired)
            {
                File.WriteAllText("Temp/RiftBuildReport.txt", "Succeeded: Gradle packaging complete; Unity output copy repaired. Original Unity report: " + report.summary.result);
                Debug.Log("[Rift] Repaired final APK copy from short Gradle output.");
                return;
            }
            File.WriteAllText("Temp/RiftBuildReport.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings);
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Android build failed; inspect Editor.log and build report");
#else
            throw new Exception("Instala Android Build Support en Unity Hub y cambia el proyecto a Android antes de generar el APK.");
#endif
        }
    }
}
