using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using RiftHuntersAR.UI.Visual;
namespace RiftHuntersAR.Integration.Editor
{
    [InitializeOnLoad]
    public static class EndScreenBuilder
    {
        static EndScreenBuilder(){EditorApplication.update+=Poll;}
        static void Poll()
        {
            const string command="Temp/RiftEndScreensCommand.txt";
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(command))return;
            string value=File.ReadAllText(command).Trim();File.Delete(command);
            try {if(value=="fonts") {var package=Directory.GetFiles("Library/PackageCache","TMP Essential Resources.unitypackage",SearchOption.AllDirectories).Single();AssetDatabase.ImportPackage(package,false);}else if(value=="connect")Connect();else if(value=="audit")Audit();else throw new Exception(value);File.WriteAllText("Temp/RiftEndScreensResult.txt","PASS "+value);}
            catch(Exception ex){File.WriteAllText("Temp/RiftEndScreensResult.txt","FAIL "+ex);Debug.LogException(ex);}
        }
        static Sprite Sprite(string path){var s=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/EndScreens/"+path+".png");if(s==null)throw new Exception("Missing sprite "+path);return s;}
        static void Audit()
        {
            var roots=AssetDatabase.FindAssets("",new[]{"Assets/Resources"}).Select(AssetDatabase.GUIDToAssetPath)
                .Concat(new[]{"Assets/Scenes/MainMenu/MainMenu.unity","Assets/Scenes/Gameplay/ARGame.unity","Assets/Settings/Mobile_RPAsset.asset","Assets/Settings/UniversalRenderPipelineGlobalSettings.asset"}).Where(p=>!AssetDatabase.IsValidFolder(p)).ToArray();
            var dependencies=AssetDatabase.GetDependencies(roots,true).Where(p=>p.StartsWith("Assets/")).Distinct().OrderBy(p=>p).ToArray();
            foreach(var p in dependencies)if(!File.Exists(p) || !File.Exists(p+".meta"))throw new Exception("Missing delivery dependency "+p);
            File.WriteAllLines("Temp/RiftDeliveryDependencies.txt",dependencies);
            File.WriteAllText("Temp/RiftDeliveryAudit.txt","PASS: "+dependencies.Length+" local asset dependencies with metadata; no external dependency paths.\n");
        }
        static void Connect()
        {
            foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Art/UI/EndScreens"}).Select(AssetDatabase.GUIDToAssetPath))
            {var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.SaveAndReimport();}
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");if(font==null)throw new Exception("Import TMP Essential Resources first");
            foreach(string path in new[]{"Assets/Prefabs/UI/GameplayHUD.prefab","Assets/Scenes/Gameplay/ARGame.unity"})
            {
                bool prefab=path.EndsWith(".prefab");GameObject root=null;
                if(prefab)root=PrefabUtility.LoadPrefabContents(path);else EditorSceneManager.OpenScene(path);
                var hud=prefab?root.GetComponentInChildren<VisualHUD>(true):UnityEngine.Object.FindFirstObjectByType<VisualHUD>();if(hud==null)throw new Exception("Missing HUD "+path);
                var view=hud.GetComponent<EndScreenView>()??hud.gameObject.AddComponent<EndScreenView>();view.hud=hud;view.font=font;
                view.gameOverSprites=new[]{"GameOver/panel_title_gameover","GameOver/panel_stats_gameover","GameOver/decor_portal_red","GameOver/decor_crystals_red","GameOver/btn_retry_gameover","Shared/btn_main_menu_gameover"}.Select(Sprite).ToArray();
                view.victorySprites=new[]{"Victory/panel_title_victory","Victory/panel_stats_victory","Victory/decor_portal_blue","Victory/decor_crystals_green_blue","Victory/btn_retry_victory","Shared/btn_main_menu_victory"}.Select(Sprite).ToArray();
                EditorUtility.SetDirty(view);if(prefab){PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);}else{EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);}
            }
            AssetDatabase.SaveAssets();
        }
    }
}
