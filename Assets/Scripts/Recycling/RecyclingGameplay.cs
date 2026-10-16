using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using RiftHunters.Game;
using RiftHuntersAR.AR;
using RiftHuntersAR.Integration;

namespace RiftHuntersAR.Recycling
{
    // Owns classification only. The existing GameManager alone sequences waves.
    public sealed class RecyclingGameplay : MonoBehaviour,IWaveRecovery
    {
        public int EcoPoints {get;private set;}
        public int PendingCount => Items.Count;
        public bool Active {get;private set;}
        public bool ZoneRecovered {get;private set;}
        public bool FinalRecovery {get;private set;}
        public RecyclableItem Selected {get;private set;}
        public string Feedback {get;private set;}="";
        public Transform StationsRoot {get;private set;}
        public List<RecyclableItem> Items {get;}=new();
        public IReadOnlyList<RecycleStation> Stations => stations;
        readonly List<RecycleStation> stations=new();
        readonly List<RaycastResult> uiHits=new();
        ARPrototypeController controller;
        GameManager manager;
        RecyclingVisualCatalog catalog;
        PollutionZone zone;
        float settledWait,completeWait,feedbackTime,placementRetry;
        bool initialized;
        int collectedThisWave;
        ParticleSystem finalAura;
        public bool InputAvailable => Active && initialized && !ZoneRecovered && controller!=null && controller.SeguimientoDisponible && manager!=null && manager.RecoveryInProgress;
        public bool ReadyToAdvance => ZoneRecovered && Items.Count==0;
        public string Instruction => !Active?"":!controller.SeguimientoDisponible?"RECUPERANDO TRACKING: MIRA EL PORTAL":
            ZoneRecovered?$"ZONA RECUPERADA\n+{collectedThisWave*50} ECO":!initialized?"ZONA CONTAMINADA\nPREPARANDO RECUPERACIÓN":
            stations.Count==0?"NECESITAS MÁS ESPACIO EN LA SUPERFICIE\nREINICIA Y ESCANEA UNA ZONA MÁS AMPLIA":
            Selected!=null?"TOCA EL CONTENEDOR CORRECTO":"ZONA CONTAMINADA\nCLASIFICA LOS RESIDUOS";
        void Awake()
        {
            controller=GetComponent<ARPrototypeController>();manager=GetComponent<GameManager>();
            catalog=Resources.Load<RecyclingVisualCatalog>("RecyclingVisualCatalog");
        }
        void Start()
        {
            var hud=FindFirstObjectByType<RiftHuntersAR.UI.Visual.VisualHUD>();
            if(hud!=null)hud.gameObject.AddComponent<RecyclingHUD>().Configure(this,hud);
        }
        public void BeginRecovery(bool final)
        {
            ClearStations();Deselect();Items.Clear();Active=true;FinalRecovery=final;ZoneRecovered=false;
            settledWait=.65f;initialized=false;collectedThisWave=0;Feedback="";placementRetry=0;
            var portal=controller.PortalActivo;
            if(portal!=null)zone=PollutionZone.For(portal.transform.parent);
            if(final && zone!=null && catalog!=null && catalog.contaminationVFX!=null)
            {
                var fx=Instantiate(catalog.contaminationVFX,portal.transform.position+portal.transform.up*.04f,Quaternion.identity,zone.transform);
                fx.name="Final zone contamination";fx.transform.localScale=Vector3.one*1.8f;
                finalAura=fx.GetComponent<ParticleSystem>();finalAura.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=finalAura.main;main.loop=true;main.stopAction=ParticleSystemStopAction.None;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;finalAura.Play();
            }
        }
        public void CancelRecovery(bool resetEco)
        {
            Active=false;ZoneRecovered=false;FinalRecovery=false;initialized=false;completeWait=0;Feedback="";
            Deselect();Items.Clear();ClearStations();
            if(zone==null && controller!=null && controller.PortalActivo!=null)
                zone=PollutionZone.For(controller.PortalActivo.transform.parent);
            if(zone!=null)zone.Clear();
            if(resetEco)EcoPoints=0;
        }
        void Update()
        {
            if(finalAura!=null)
            {
                bool tracked=controller!=null && controller.SeguimientoDisponible;
                if(tracked && finalAura.isPaused)finalAura.Play();else if(!tracked && finalAura.isPlaying)finalAura.Pause();
            }
            if(!Active || controller==null || !controller.SeguimientoDisponible)return;
            if(feedbackTime>0){feedbackTime-=Time.deltaTime;if(feedbackTime<=0)Feedback="";}
            if(!initialized)
            {
                settledWait-=Time.deltaTime;if(settledWait>0)return;
                if(zone!=null)
                    foreach(var item in zone.GetComponentsInChildren<RecyclableItem>())if(item.Classifiable)Items.Add(item);
                initialized=true;TryPlaceStations();
            }
            Items.RemoveAll(x=>x==null || !x.Classifiable);
            if(Items.Count==0 && !ZoneRecovered)
            {
                ZoneRecovered=true;completeWait=1.6f;Deselect();
                // Existing next-level sound remains owned by SonidosPartida on transition.
            }
            if(ZoneRecovered)
            {
                completeWait-=Time.deltaTime;
                if(completeWait<=0)
                {
                    ClearStations();if(zone!=null)zone.Clear();Active=false;Items.Clear();
                    manager.CompleteRecovery();
                }
                return;
            }
            if(stations.Count==0)
            {placementRetry-=Time.deltaTime;if(placementRetry<=0){placementRetry=1;TryPlaceStations();}return;}
            if(TryTap(out Vector2 point))HandleTap(point);
        }
        void TryPlaceStations()
        {
            if(stations.Count>0 || controller.PortalActivo==null || catalog==null)return;
            var portal=controller.PortalActivo;var floor=portal.GetComponentInParent<SueloCombateAR>();
            if(!StationPlacement.TryFind(floor,portal.transform.position,portal.SpawnPoint.position,controller.CamaraJuego,out var positions,out var rotation,out float scale))return;
            var root=new GameObject("Recovery stations - anchored");root.transform.SetParent(portal.transform.parent,false);StationsRoot=root.transform;
            for(int i=0;i<3;i++)
            {
                var prefab=catalog.Station(i);
                if(prefab==null){Debug.LogError("Recycling catalog: missing station "+i);ClearStations();return;}
                var instance=Instantiate(prefab,positions[i],rotation,StationsRoot);instance.transform.localScale*=scale;
                stations.Add(instance.GetComponent<RecycleStation>());
            }
        }
        void ClearStations()
        {
            stations.Clear();
            if(StationsRoot!=null){StationsRoot.gameObject.SetActive(false);Destroy(StationsRoot.gameObject);StationsRoot=null;}
        }
        void Deselect(){if(Selected!=null)Selected.SetSelected(false);Selected=null;}
        public bool Select(RecyclableItem item)
        {
            if(!InputAvailable || item==null || !item.IsSettled || !Items.Contains(item) || !item.Classifiable)return false;
            Deselect();Selected=item;Selected.SetSelected(true);Feedback="RESIDUO SELECCIONADO";feedbackTime=1;return true;
        }
        public bool Classify(RecycleStation station)
        {
            if(!InputAvailable || Selected==null || station==null || !stations.Contains(station))return false;
            if(Selected.Category!=station.Category)
            {Feedback="CONTENEDOR INCORRECTO";feedbackTime=1.4f;return false;}
            var item=Selected;item.MarkCollected();Items.Remove(item);Selected=null;EcoPoints+=50;collectedThisWave++;
            Feedback="RESIDUO CLASIFICADO\n+50 ECO";feedbackTime=1.25f;
            item.gameObject.AddComponent<RecyclingCollection>().Begin(station.transform.position+station.transform.up*.65f,catalog.contaminationVFX);
            return true;
        }
        bool TryTap(out Vector2 point)
        {
            point=default;
            if(Touchscreen.current!=null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {point=Touchscreen.current.primaryTouch.position.ReadValue();return true;}
#if UNITY_EDITOR || UNITY_STANDALONE
            if(Mouse.current!=null && Mouse.current.leftButton.wasPressedThisFrame){point=Mouse.current.position.ReadValue();return true;}
#endif
            return false;
        }
        public bool HandleTap(Vector2 point)
        {
            if(!InputAvailable)return false;
            if(EventSystem.current!=null)
            {
                uiHits.Clear();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},uiHits);
                if(uiHits.Count>0)return false;
            }
            // Physics.DefaultRaycastLayers excludes recycling. This explicit query
            // opts into layer 2 and triggers only during the recovery phase.
            var ray=controller.CamaraJuego.ScreenPointToRay(point);
            var hits=Physics.RaycastAll(ray,20,1<<2,QueryTriggerInteraction.Collide);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                var item=hit.collider.GetComponentInParent<RecyclableItem>();
                if(item!=null && item.Classifiable)return Select(item);
                var station=hit.collider.GetComponentInParent<RecycleStation>();
                if(station!=null && stations.Contains(station))return Classify(station);
            }
            // Small cans/bottles need a finger-sized margin on a phone. Only
            // considered after a clean physics miss, never through a station/UI.
            RecyclableItem nearest=null;float best=Mathf.Max(10,Screen.width*.022f);
            foreach(var item in Items)
            {
                if(item==null || !item.IsSettled || !item.Classifiable)continue;
                var bounds=item.GetComponent<BoxCollider>().bounds;
                Vector3 center=controller.CamaraJuego.WorldToScreenPoint(bounds.center);
                if(center.z<=0 || center.z>20)continue;
                Vector2 min=Vector2.positiveInfinity,max=Vector2.negativeInfinity;
                for(int i=0;i<8;i++)
                {
                    var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    Vector2 pixel=controller.CamaraJuego.WorldToScreenPoint(corner);min=Vector2.Min(min,pixel);max=Vector2.Max(max,pixel);
                }
                Vector2 closest=new(Mathf.Clamp(point.x,min.x,max.x),Mathf.Clamp(point.y,min.y,max.y));float distance=Vector2.Distance(point,closest);
                if(distance<best){best=distance;nearest=item;}
            }
            if(nearest!=null)return Select(nearest);
            return false;
        }
    }
}
