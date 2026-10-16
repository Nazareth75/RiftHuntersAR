using UnityEngine;
using RiftHunters.Combate;
using RiftHuntersAR.AR;

namespace RiftHuntersAR.Recycling
{
    public sealed class EnemyPollutionEmitter : MonoBehaviour
    {
        EnemyProceduralVisual motion;
        RecyclingVisualCatalog catalog;
        Enemigo enemy;
        float untilDrop=-1, untilAutomatic;
        bool guardian;
        GameObject aura;
        public int EmittedCount {get;private set;}
        public int Limit {get;private set;}
        public void Configure(EnemyProceduralVisual presentation, Enemigo owner, int kind)
        {
            motion=presentation;enemy=owner;guardian=kind==3;Limit=guardian?0:kind==2?2:1;
            catalog=Resources.Load<RecyclingVisualCatalog>("RecyclingVisualCatalog");
            if(guardian) enemy.Murio+=FinalResidues;
            untilAutomatic=kind==1?1.3f:kind==2?2.8f:2.1f;
            if(guardian && catalog!=null && catalog.contaminationVFX!=null)
            {
                aura=Instantiate(catalog.contaminationVFX,transform);
                aura.name="Guardian contamination aura";aura.transform.localPosition=Vector3.up*.06f;
                aura.transform.localScale=Vector3.one*1.8f;
                var system=aura.GetComponent<ParticleSystem>();var main=system.main;main.loop=true;
                // A single, capped aura keeps its timeline when outside the narrow AR viewport.
                // Rendering is still culled; tracking/application pauses explicitly pause simulation.
                main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
                main.stopAction=ParticleSystemStopAction.None;system.Play();
            }
        }
        // Safe reusable visual command; does not stop navigation or change combat state.
        public bool Contaminate()
        {
            if(guardian || catalog==null || enemy==null || enemy.Vida<=0 || EmittedCount>=Limit || untilDrop>=0 || !motion.CanAnimate) return false;
            motion.Pollute();untilDrop=motion.Kind==1?.12f:.3f;return true;
        }
        void Update()
        {
            if(motion==null || enemy==null || enemy.Vida<=0) return;
            bool active=motion.CanAnimate;
            if(aura!=null)
            {
                var ps=aura.GetComponent<ParticleSystem>();
                if(active && ps.isPaused) ps.Play();else if(!active && ps.isPlaying) ps.Pause();
            }
            if(!active) return;
            if(untilDrop>=0)
            {
                untilDrop-=Time.deltaTime;
                if(untilDrop<=0) { untilDrop=-1;Drop(); }
            }
            else if(EmittedCount<Limit && !enemy.PreparandoAtaque)
            {
                untilAutomatic-=Time.deltaTime;
                if(untilAutomatic<=0) Contaminate();
            }
        }
        void Drop()
        {
            SpawnResidues(Limit-EmittedCount,motion.Kind);
            untilAutomatic=float.PositiveInfinity;
        }
        void FinalResidues(Enemigo owner)
        {
            // One final release from the defeated Guardian's polluted zone, never a timer/spam.
            if(GetComponentInParent<NucleosJefe>()!=null && catalog!=null) SpawnResidues(3,0);
        }
        void OnDestroy() { if(enemy!=null) enemy.Murio-=FinalResidues; }
        void SpawnResidues(int amount,int firstType)
        {
            var surface=GetComponentInParent<SueloCombateAR>();
            Transform parent=surface!=null?surface.transform:transform.parent;
            var zone=PollutionZone.For(parent);
            for(int i=0;i<amount && zone.HasSpace;i++)
            {
                GameObject prefab=catalog.Waste((firstType+i)%3);
                if(prefab==null) continue;
                float side=i==0?1:-1;
                Vector3 landing=transform.position+transform.right*(.12f*side)-transform.forward*.035f;
                landing=surface!=null?surface.PosicionAparicion(landing):new Vector3(landing.x,transform.position.y,landing.z);
                var residue=Instantiate(prefab,zone.transform);
                Quaternion settled=Quaternion.Euler(0,(EmittedCount*137+GetInstanceID())%360,0);
                // Lay bottle/can on their sides, then measure rotated mesh bottom precisely.
                if(residue.GetComponent<RecyclableItem>().Category!=RecyclingCategory.Paper) settled*=Quaternion.Euler(0,0,90);
                residue.transform.localRotation=settled;
                ModeloDeIntegracion.MedirVisible(residue,zone.transform,out Bounds bounds);
                Vector3 end=zone.transform.InverseTransformPoint(landing)-Vector3.up*bounds.min.y;
                Vector3 from=zone.transform.InverseTransformPoint(transform.position+Vector3.up*(motion.Height*.48f));
                var fall=residue.AddComponent<ResidueLanding>();fall.Begin(from,end,settled);fall.SetImpact(catalog.contaminationVFX);
                EmittedCount++;
            }
        }
    }
}
