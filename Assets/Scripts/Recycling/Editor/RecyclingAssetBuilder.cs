using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using RiftHunters.Combate;
using Object=UnityEngine.Object;

namespace RiftHuntersAR.Recycling.Editor
{
    [InitializeOnLoad]
    public static class RecyclingAssetBuilder
    {
        const string Models="Assets/Art/Models/Recycling/", Prefabs="Assets/Prefabs/Recycling/", Mats="Assets/Art/Materials/Recycling/";
        public const string ReviewScene="Assets/Scenes/Review/RecyclingVisualReview.unity";
        static readonly string[] Names={"PlasticBottle","MetalCan","PaperCardboard","TrashBag","PlasticRecycleStation","PaperRecycleStation","MetalRecycleStation"};
        static readonly string[] Enemies={"EnemigoNormal","EnemigoRapido","EnemigoTanque","EnemigoJefe"};
        static readonly string[] Visuals={"Assets/Prefabs/Enemies/EnemyBasicPlaceholder.prefab","Assets/Prefabs/Enemies/EnemyFast_Visual.prefab","Assets/Prefabs/Enemies/EnemyTank_Visual.prefab","Assets/Prefabs/Boss/RiftGuardian_Visual.prefab"};
        static Dictionary<string,Material> materials=new();
        static RecyclingAssetBuilder(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            string command="Temp/RiftRecyclingCommand.txt";if(!File.Exists(command))return;
            string value=File.ReadAllText(command).Trim();File.Delete(command);
            try
            {
                if(value=="create") Create();else if(value=="validate") Validate();else if(value=="review") OpenReview();
                else if(value=="play") { OpenReview();RecyclingPresentationTests.Begin();return; }
                else throw new Exception("Unknown recycling editor command: "+value);
                File.WriteAllText("Temp/RiftRecyclingResult.txt","PASS "+value+" "+DateTime.Now.ToString("s"));
            }
            catch(Exception e){File.WriteAllText("Temp/RiftRecyclingResult.txt","FAIL "+e);Debug.LogException(e);}
        }
        [MenuItem("Rift Hunters/Recycling/1 Crear assets de reciclaje")]
        static void Create()
        {
            foreach(string folder in new[]{Prefabs,Mats,"Assets/Scenes/Review","Assets/Prefabs/VFX"}) Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();materials.Clear();
            Material Make(string name,Color color,float metal=0,float emission=0)
            {
                string path=Mats+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
                m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",.35f);
                if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;}
                EditorUtility.SetDirty(m);materials[name]=m;return m;
            }
            Make("Recycling_Plastic",new Color(.40f,.49f,.55f));Make("Recycling_Cyan",new Color(.005f,.32f,.72f),.25f,.25f);
            Make("Recycling_RedMetal",new Color(.40f,.025f,.045f),.72f);Make("Recycling_CanRim",new Color(.43f,.49f,.55f),.8f);
            Make("Recycling_Cardboard",new Color(.48f,.26f,.11f));Make("Recycling_Paper",new Color(.79f,.68f,.49f));
            Make("Recycling_Bag",new Color(.034f,.016f,.06f));Make("Recycling_Violet",new Color(.25f,.035f,.42f),.15f,.1f);
            Make("Recycling_Graphite",new Color(.095f,.125f,.16f),.65f);
            Make("Recycling_PlasticLight",new Color(.01f,.9f,.085f),.1f,1.3f);Make("Recycling_PaperLight",new Color(.01f,.25f,1),.1f,1.3f);Make("Recycling_MetalLight",new Color(1,.27f,.005f),.1f,1.3f);
            for(int i=0;i<Names.Length;i++)
            {
                string name=Names[i],path=Models+name+"_Final.fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);if(importer==null)throw new Exception("FBX missing: "+path);
                importer.globalScale=1;importer.isReadable=false;importer.importAnimation=false;importer.addCollider=false;
                importer.bakeAxisConversion=true;importer.importCameras=false;importer.importLights=false;
                importer.importNormals=ModelImporterNormals.Import;
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach(var renderer in source.GetComponentsInChildren<MeshRenderer>())foreach(var material in renderer.sharedMaterials)
                {
                    string key=System.Text.RegularExpressions.Regex.Replace(material.name,@"\.\d+$","");
                    if(!materials.TryGetValue(key,out var mapped))throw new Exception("Unknown source material: "+material.name);
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material),material.name),mapped);
                }
                importer.SaveAndReimport();source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var root=new GameObject(name);root.layer=2;
                var visual=(GameObject)PrefabUtility.InstantiatePrefab(source);visual.transform.SetParent(root.transform,false);
                // Export front -Y becomes -Z in Unity: show its front toward local +Z.
                visual.transform.localRotation=Quaternion.Euler(0,180,0)*visual.transform.localRotation;
                foreach(var child in root.GetComponentsInChildren<Transform>(true))child.gameObject.layer=2;
                ModeloDeIntegracion.MedirVisible(root,root.transform,out Bounds bounds);
                var collider=root.AddComponent<BoxCollider>();collider.center=bounds.center;collider.size=bounds.size;collider.isTrigger=true;
                if(i<4)root.AddComponent<RecyclableItem>().Configure(i==0?RecyclingCategory.Plastic:i==1?RecyclingCategory.Metal:i==2?RecyclingCategory.Paper:RecyclingCategory.Other);
                else root.AddComponent<RecycleStation>().Configure(i==4?RecyclingCategory.Plastic:i==5?RecyclingCategory.Paper:RecyclingCategory.Metal);
                PrefabUtility.SaveAsPrefabAsset(root,Prefabs+name+".prefab");Object.DestroyImmediate(root);
            }
            var fx=new GameObject("ContaminationVFX");fx.layer=2;
            var ps=fx.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.duration=.8f;main.loop=false;main.startLifetime=new ParticleSystem.MinMaxCurve(.5f,1.15f);main.startSpeed=new ParticleSystem.MinMaxCurve(.015f,.045f);
            main.startSize=new ParticleSystem.MinMaxCurve(.008f,.024f);main.maxParticles=20;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.stopAction=ParticleSystemStopAction.Destroy;main.useUnscaledTime=false;
            var emission=ps.emission;emission.rateOverTime=9;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Hemisphere;shape.radius=.075f;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;velocity.y=.025f;
            var col=ps.colorOverLifetime;col.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(.23f,.025f,.36f),0),new GradientColorKey(new Color(.48f,.045f,.72f),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.45f,.2f),new GradientAlphaKey(0,1)});col.color=gradient;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.5f),new Keyframe(.4f,1),new Keyframe(1,0)));
            string particlePath=Mats+"ContaminationParticles.mat";var particle=AssetDatabase.LoadAssetAtPath<Material>(particlePath);
            if(particle==null){particle=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(particle,particlePath);}
            particle.SetFloat("_Surface",1);particle.SetFloat("_Blend",0);particle.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);particle.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);particle.SetFloat("_ZWrite",0);particle.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");particle.renderQueue=3000;
            var pr=fx.GetComponent<ParticleSystemRenderer>();pr.sharedMaterial=particle;pr.renderMode=ParticleSystemRenderMode.Billboard;pr.shadowCastingMode=ShadowCastingMode.Off;pr.receiveShadows=false;
            PrefabUtility.SaveAsPrefabAsset(fx,"Assets/Prefabs/VFX/ContaminationVFX.prefab");Object.DestroyImmediate(fx);
            var catalog=AssetDatabase.LoadAssetAtPath<RecyclingVisualCatalog>("Assets/Resources/RecyclingVisualCatalog.asset");
            if(catalog==null){catalog=ScriptableObject.CreateInstance<RecyclingVisualCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/Resources/RecyclingVisualCatalog.asset");}
            catalog.plasticBottle=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"PlasticBottle.prefab");catalog.metalCan=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"MetalCan.prefab");catalog.paperCardboard=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"PaperCardboard.prefab");catalog.contaminationVFX=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/ContaminationVFX.prefab");EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();BuildReview();Validate();
        }
        static void BuildReview()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.55f,.59f,.67f);
            var camera=new GameObject("Review Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.018f,.024f,.04f);camera.orthographic=true;camera.orthographicSize=1.65f;camera.nearClipPlane=.03f;camera.farClipPlane=30;
            camera.transform.position=new Vector3(2.7f,2.8f,-4.6f);camera.transform.LookAt(new Vector3(0,.27f,.38f));
            var light=new GameObject("Review key light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=2.1f;light.transform.rotation=Quaternion.Euler(45,-30,0);
            var fill=new GameObject("Review fill").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.65f;fill.color=new Color(.55f,.72f,1);fill.transform.rotation=Quaternion.Euler(30,150,0);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Review surface (metres)";floor.transform.position=new Vector3(0,-.025f,.3f);floor.transform.localScale=new Vector3(4.5f,.05f,3);
            Object.DestroyImmediate(floor.GetComponent<Collider>());floor.GetComponent<Renderer>().sharedMaterial=materials["Recycling_Graphite"];
            var gallery=new GameObject("Static recycling gallery");
            for(int i=0;i<Names.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+Names[i]+".prefab");var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
                obj.transform.SetParent(gallery.transform,false);
                obj.transform.position=i<4?new Vector3(-1.25f+i*.37f,0,-.65f):new Vector3(.35f+(i-4)*.58f,0,-.47f);
                obj.transform.rotation=Quaternion.Euler(0,180,0);
            }
            var staticMonsters=new GameObject("Static approved monsters (hidden during Play)");
            var rig=new GameObject("Recycling Review - editor only").AddComponent<RecyclingReviewDirector>();rig.staticMonsters=staticMonsters;rig.staticAssets=gallery;rig.enemyPrefabs=new GameObject[4];
            float[] heights={.28f,.24f,.42f,.8f};
            for(int i=0;i<4;i++)
            {
                rig.enemyPrefabs[i]=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemigos/"+Enemies[i]+".prefab");
                var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Visuals[i]),staticMonsters.transform);model.transform.position=new Vector3(-1.4f+i*.92f,0,1.1f);
                model.transform.rotation=Quaternion.Euler(0,180,0);
                ModeloDeIntegracion.MedirVisible(model,model.transform,out Bounds bounds);model.transform.localScale*=heights[i]/bounds.size.y;
                ModeloDeIntegracion.MedirVisible(model,staticMonsters.transform,out bounds);model.transform.position-=Vector3.up*bounds.min.y;
            }
            EditorSceneManager.SaveScene(scene,ReviewScene);Capture(camera,"ArtRenders/Recycling/Unity_Recycling_Overview.png");
            SceneView.lastActiveSceneView?.Frame(new Bounds(new Vector3(0,.4f,.4f),new Vector3(4,2,3)),false);
        }
        public static void Capture(Camera camera,string path)
        {
            var rt=new RenderTexture(1600,1000,24);var old=camera.targetTexture;var previous=RenderTexture.active;
            try {camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1600,1000,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,1000),0,0);texture.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);}
            finally{camera.targetTexture=old;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);}
        }
        [MenuItem("Rift Hunters/Recycling/2 Abrir revisión visual")]
        public static void OpenReview(){EditorSceneManager.OpenScene(ReviewScene);}
        [MenuItem("Rift Hunters/Recycling/3 Validar assets")]
        static void Validate()
        {
            var lines=new List<string>();
            for(int i=0;i<Names.Length;i++)
            {
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+Names[i]+".prefab");if(asset==null)throw new Exception("Missing prefab "+Names[i]);
                var root=Object.Instantiate(asset);
                try
                {
                if(root.GetComponentsInChildren<Transform>(true).Any(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0))throw new Exception("Missing script "+Names[i]);
                var renderers=root.GetComponentsInChildren<MeshRenderer>();
                if(renderers.Any(r=>r.sharedMaterials.Any(m=>m==null || m.shader==null || m.shader.name.Contains("Error"))))throw new Exception("Missing material/shader "+Names[i]);
                int tris=root.GetComponentsInChildren<MeshFilter>().Sum(f=>Enumerable.Range(0,f.sharedMesh.subMeshCount).Sum(s=>(int)f.sharedMesh.GetIndexCount(s)/3));
                if(tris>=(i<4?800:2500))throw new Exception("Triangle budget "+Names[i]);
                ModeloDeIntegracion.MedirVisible(root,root.transform,out var b);
                if(Mathf.Abs(b.min.y)>.001f || b.size.y<(i<4?.10f:.75f) || b.size.y>(i<4?.4f:.90f))throw new Exception("Ground/scale "+Names[i]+" "+b);
                var box=root.GetComponent<BoxCollider>();if(box==null || !box.isTrigger || root.layer!=2 || root.GetComponent<Rigidbody>()!=null)throw new Exception("Unsafe collider "+Names[i]);
                if(i<4 && root.GetComponent<RecyclableItem>()==null)throw new Exception("Missing category "+Names[i]);
                lines.Add("PASS "+Names[i]+": "+tris+" tris; size="+b.size.ToString("F3")+"; bottom="+b.min.y+"; trigger BoxCollider; Ignore Raycast; materials="+string.Join(",",renderers.SelectMany(r=>r.sharedMaterials).Distinct().Select(m=>m.name)));
                }
                finally { Object.DestroyImmediate(root); }
            }
            var fx=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/ContaminationVFX.prefab");if(fx==null || fx.GetComponent<ParticleSystem>().main.maxParticles>20)throw new Exception("Invalid contamination VFX");
            var cat=Resources.Load<RecyclingVisualCatalog>("RecyclingVisualCatalog");if(cat==null || cat.plasticBottle==null || cat.metalCan==null || cat.paperCardboard==null || cat.contaminationVFX==null)throw new Exception("Missing catalog references");
            lines.Add("PASS contamination VFX: max 20 particles; no physics; sparse purple emission. Catalog references valid.");
            File.WriteAllLines("Temp/RiftRecyclingAssetChecks.txt",lines);Debug.Log("Recycling assets validated: 7 prefabs, no missing scripts/references.");
        }
    }
}
