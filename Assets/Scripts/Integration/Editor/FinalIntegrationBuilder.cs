using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RiftHuntersAR.UI.Visual;

namespace RiftHuntersAR.Integration.Editor
{
    [InitializeOnLoad]
    public static class FinalIntegrationBuilder
    {
        static FinalIntegrationBuilder() { EditorApplication.update+=Poll; }
        static void Poll()
        {
            const string path="Temp/RiftFinalCommand.txt";
            if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(path)) return;
            string command=File.ReadAllText(path).Trim(); File.Delete(path);
            try
            {
                if(command=="connect") Connect();
                else throw new Exception("Unknown final command "+command);
                File.WriteAllText("Temp/RiftFinalResult.txt","PASS "+command);
            }
            catch(Exception ex) { File.WriteAllText("Temp/RiftFinalResult.txt","FAIL "+ex); Debug.LogException(ex); }
        }
        static void Connect()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Gameplay/ARGame.unity");
            var hud=UnityEngine.Object.FindFirstObjectByType<VisualHUD>();
            var component=hud.GetComponent<WeaponVisuals>() ?? hud.gameObject.AddComponent<WeaponVisuals>();
            component.hud=hud;
            component.viewRoot=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(t=>t.name=="PlasmaViewModel");
            component.viewRoot.localPosition=new Vector3(.035f,-.17f,.45f);
            component.plasmaView=component.viewRoot.Find("PlasmaBlaster_Visual").gameObject;
            component.prefabs=new[]{"PlasmaBlaster","ElectricCannon","EnergyLauncher"}.Select(n=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/"+n+"_Visual.prefab")).ToArray();
            component.icons=new[]{"icon_plasma","icon_electrica","icon_energia_pesada"}.Select(n=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Approved/"+n+".png")).ToArray();
            if(component.prefabs.Any(p=>p==null) || component.icons.Any(p=>p==null)) throw new Exception("Missing approved weapon asset");
            EditorUtility.SetDirty(component); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu/MainMenu.unity");
        }
    }
}
