using UnityEngine;
using RiftHunters.Combate;
using RiftHuntersAR.Integration;

namespace RiftHuntersAR.Recycling
{
    [DefaultExecutionOrder(50)]
    public sealed class EnemyProceduralVisual : MonoBehaviour
    {
        public int Kind {get;private set;} // Basic, Fast, Tank, Guardian.
        public float Height {get;private set;}
        public Transform VisualPivot {get;private set;}
        public bool CanAnimate => enemy!=null && enemy.Vida>0 && (controller==null || (controller.SeguimientoDisponible && controller.CurrentState=="WAVE"));
        Enemigo enemy;
        NucleosJefe cores;
        ARPrototypeController controller;
        Transform instance;
        Vector3 lastRoot;
        float clock, hit, pollution, attack, preparation;
        float units;
        bool detached;
        public static void Attach(Enemigo owner,GameObject visual,string model,float height)
        {
            if(owner==null || visual==null) return;
            var motion=owner.gameObject.AddComponent<EnemyProceduralVisual>();
            motion.enemy=owner;motion.instance=visual.transform;motion.Height=height;
            string name=model.ToLowerInvariant();motion.Kind=name.Contains("guardian")?3:name.Contains("fast")?1:name.Contains("tank")?2:0;
            var pivot=new GameObject("ProceduralVisualPivot").transform;pivot.SetParent(visual.transform,false);
            // Preserve all original mesh/piece references; only introduce a presentation pivot.
            for(int i=visual.transform.childCount-1;i>=0;i--)
            { var child=visual.transform.GetChild(i);if(child!=pivot) child.SetParent(pivot,false); }
            // Some approved visual prefabs have a renderer directly on their root.
            var rootMesh=visual.GetComponent<MeshFilter>();var rootRenderer=visual.GetComponent<MeshRenderer>();
            if(rootMesh!=null && rootRenderer!=null)
            {
                var meshChild=new GameObject("Original root mesh");meshChild.transform.SetParent(pivot,false);
                meshChild.AddComponent<MeshFilter>().sharedMesh=rootMesh.sharedMesh;
                var renderer=meshChild.AddComponent<MeshRenderer>();renderer.sharedMaterials=rootRenderer.sharedMaterials;
                rootRenderer.enabled=false;
            }
            motion.VisualPivot=pivot;motion.units=1/Mathf.Max(.00001f,pivot.lossyScale.y);
            motion.lastRoot=owner.transform.position;motion.clock=(Mathf.Abs(owner.GetInstanceID())%100)*.07f;
            motion.controller=FindFirstObjectByType<ARPrototypeController>();
            owner.VidaCambiada+=motion.OnHit;owner.ImpactoBloqueado+=motion.OnHit;
            owner.Murio+=motion.OnDeath;owner.AtacoJugador+=motion.OnAttack;
            owner.gameObject.AddComponent<EnemyPollutionEmitter>().Configure(motion,owner,motion.Kind);
        }
        public void Pollute() { pollution=Kind==1?.25f:.6f; }
        void Start()
        {
            cores=GetComponent<NucleosJefe>();
            if(cores!=null) cores.Cambio+=OnHit;
        }
#if UNITY_EDITOR
        public void PreviewAttack() { attack=Kind==1?.12f:.22f; }
#endif
        void OnHit() { if(enemy.Vida>0) hit=Kind==1?.10f:.18f; }
        void OnDeath(Enemigo _) { Detach(true); }
        void OnAttack(int _) { attack=Kind==1?.12f:.22f;if(Kind!=3) Detach(false); }
        void Detach(bool death)
        {
            if(detached || instance==null || Kind==3) return;
            detached=true;instance.SetParent(transform.parent,true);
            foreach(var c in instance.GetComponentsInChildren<Collider>(true)) c.enabled=false;
            foreach(var t in instance.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=2;
            foreach(var behaviour in instance.GetComponentsInChildren<MonoBehaviour>()) behaviour.enabled=false;
            instance.gameObject.AddComponent<EnemyVisualRemnant>().Begin(death);
        }
        void LateUpdate()
        {
            if(VisualPivot==null || detached) return;
            float dt=Time.deltaTime;
            if(!CanAnimate) { lastRoot=transform.position;return; }
            clock+=dt;hit=Mathf.Max(0,hit-dt);pollution=Mathf.Max(0,pollution-dt);attack=Mathf.Max(0,attack-dt);
            float speed=(transform.position-lastRoot).magnitude/Mathf.Max(.0001f,dt);lastRoot=transform.position;
            float walking=Mathf.Clamp01(speed/.12f),freq=Kind==1?16:Kind==2?6:8;
            float beat=Mathf.Sin(clock*freq),sway=Mathf.Sin(clock*freq*.5f);
            Vector3 pos=Vector3.zero;Vector3 rot=Vector3.zero;float pulse=1;
            if(Kind==3)
            { pos.y=(1+Mathf.Sin(clock*1.7f))*.019f;rot=new Vector3(Mathf.Sin(clock*.8f)*2,Mathf.Sin(clock*.6f)*3,Mathf.Sin(clock)*1.8f);pulse=1+Mathf.Sin(clock*1.7f)*.013f; }
            else
            {
                float breathing=Mathf.Sin(clock*(Kind==1?4.3f:Kind==2?1.5f:2));
                pos.y=(1+breathing)*.0012f+walking*Mathf.Abs(beat)*(Kind==2?.013f:.007f);
                pos.x=walking*sway*(Kind==1?.013f:.0035f);
                rot=new Vector3(walking*(Kind==2?5.5f:Kind==1?9:3),0,sway*walking*(Kind==1?5.5f:Kind==2?3.5f:3));
                pulse=1+breathing*.004f;
            }
            if(enemy.PreparandoAtaque) preparation+=dt;else preparation=0;
            float windup=Kind==1?.68f:Kind==2?.45f:.55f;
            float winding=Mathf.Clamp01(preparation/windup),lunge=Mathf.Clamp01((preparation-windup)/(.78f-windup));
            rot.x-=(Kind==2?14:Kind==1?5:9)*winding;rot.x+=(Kind==2?32:Kind==1?18:23)*lunge;
            pos.z+=Height*(-.025f*winding+(Kind==2?.10f:.08f)*lunge);
            if(attack>0) { rot.x+=Mathf.Sin(attack/.22f*Mathf.PI)*12;pos.z+=Height*.025f; }
            if(hit>0) { pos.z-=Height*(Kind==2?.009f:.035f);rot.z+=Mathf.Sin(hit*120)*(Kind==2?1.5f:4); }
            if(pollution>0) {float phase=pollution/(Kind==1?.25f:.6f);rot.z+=Mathf.Sin(phase*Mathf.PI*2)*(Kind==2?13:9);rot.x+=Mathf.Sin(phase*Mathf.PI)*-6;pos.y+=Mathf.Sin(phase*Mathf.PI)*.009f;}
            VisualPivot.localPosition=pos*units;VisualPivot.localRotation=Quaternion.Euler(rot);VisualPivot.localScale=Vector3.one*pulse;
        }
        void OnDestroy()
        {
            if(cores!=null) cores.Cambio-=OnHit;
            if(enemy==null) return;
            enemy.VidaCambiada-=OnHit;enemy.ImpactoBloqueado-=OnHit;enemy.Murio-=OnDeath;enemy.AtacoJugador-=OnAttack;
        }
    }
}
