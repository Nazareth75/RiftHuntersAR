#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using RiftHunters.Combate;
using RiftHuntersAR.Integration;
using System.Collections.Generic;
using System.Reflection;

namespace RiftHuntersAR.Recycling
{
    // Instantiated only by the explicit Editor menu in the actual ARGame scene.
    public sealed class RecyclingDesktopReview : MonoBehaviour
    {
        ARPrototypeController controller;
        bool finishCombat;
        bool framed;
        Vector3 combatCamera;
        Quaternion combatRotation;
        Material floorMaterial;
        readonly Dictionary<int,float> born=new();
        readonly FieldInfo speed=typeof(Enemigo).GetField("velocidad",BindingFlags.Instance|BindingFlags.NonPublic);
        public void FinishCombat() => finishCombat=true;
        public void Configure(ARPrototypeController value)
        {
            controller=value;
            combatCamera=controller.CamaraJuego.transform.position;combatRotation=controller.CamaraJuego.transform.rotation;
            var portal=controller.PortalActivo;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Editor only - simulated AR surface";
            floor.transform.SetParent(portal.transform.parent,false);floor.transform.position=portal.transform.position-Vector3.up*.006f;floor.transform.localScale=Vector3.one*.7f;
            Destroy(floor.GetComponent<Collider>());floorMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));floorMaterial.SetColor("_BaseColor",new Color(.13f,.16f,.20f));
            floor.GetComponent<Renderer>().sharedMaterial=floorMaterial;
        }
        void OnDestroy(){if(floorMaterial!=null)Destroy(floorMaterial);}
        void Update()
        {
            if(controller==null)return;
            if(Keyboard.current!=null && Keyboard.current.f8Key.wasPressedThisFrame)finishCombat=true;
            var flow=controller.GetComponent<RecyclingGameplay>();
            if(flow.Active)
            {
                finishCombat=false;
                if(!framed && flow.Stations.Count==3)
                {
                    var bounds=new Bounds(flow.Stations[1].transform.position,Vector3.zero);
                    foreach(var station in flow.Stations)bounds.Encapsulate(station.GetComponent<BoxCollider>().bounds);
                    foreach(var item in flow.Items)if(item!=null)bounds.Encapsulate(item.GetComponent<BoxCollider>().bounds);
                    var camera=controller.CamaraJuego;Vector3 direction=combatCamera-bounds.center;direction.y=0;direction.Normalize();
                    float distance=Mathf.Max(3.5f,bounds.size.magnitude/(2*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f)*camera.aspect)*1.15f);
                    camera.transform.position=bounds.center+direction*distance+Vector3.up*1.35f;
                    camera.transform.LookAt(bounds.center);framed=true;
                }
                if(Keyboard.current!=null)
                {
                    var key=Keyboard.current;float x=(key.rightArrowKey.isPressed?1:0)-(key.leftArrowKey.isPressed?1:0);
                    float y=(key.downArrowKey.isPressed?1:0)-(key.upArrowKey.isPressed?1:0);
                    var camera=controller.CamaraJuego.transform;camera.Rotate(Vector3.up,x*40*Time.deltaTime,Space.World);camera.Rotate(Vector3.right,y*40*Time.deltaTime,Space.Self);
                }
                return;
            }
            if(framed){controller.CamaraJuego.transform.SetPositionAndRotation(combatCamera,combatRotation);framed=false;}
            foreach(var enemy in FindObjectsByType<Enemigo>(FindObjectsSortMode.None))
            {
                var emitter=enemy.GetComponent<EnemyPollutionEmitter>();if(emitter==null)continue;
                // Slow fixture only: gives time to inspect contamination without dying.
                speed.SetValue(enemy,.025f);int id=enemy.GetInstanceID();if(!born.ContainsKey(id))born[id]=Time.time;
                if(!finishCombat || Time.time-born[id]<3.6f)continue;
                if(enemy.GetComponent<NucleosJefe>()!=null)
                {
                    foreach(string name in new[]{"Nucleo brazo izquierdo","Nucleo brazo derecho","Nucleo central"})
                    {var core=enemy.transform.Find(name)?.GetComponent<Enemigo>();if(core!=null)core.RecibirDano(core.VidaMaxima);}
                }
                else enemy.RecibirDano(enemy.VidaMaxima);
            }
        }
        void OnGUI()
        {
            if(controller!=null && controller.CurrentState=="WAVE")
            {
                var buttonStyle=new GUIStyle(GUI.skin.button){fontSize=30};
                if(GUI.Button(new Rect(20,Screen.height-165,Screen.width-40,85),"TERMINAR COMBATE DE PRUEBA",buttonStyle))FinishCombat();
            }
            var style=new GUIStyle(GUI.skin.box){fontSize=12,wordWrap=true};
            GUI.Box(new Rect(8,Screen.height-62,Mathf.Min(Screen.width-16,700),54),"PRUEBA EDITOR EN ARGame · F8 completa el combate tras ver contaminación.\nReciclaje: clic en residuo + clic en estación. Flechas: mirar. Sin cambios en Android.",style);
        }
    }
}
#endif
