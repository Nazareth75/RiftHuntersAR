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
        public int EcoTotal {get;private set;}
        public bool IntentarGastarECO(int cantidad)
        {
            if (cantidad <= 0 || EcoPoints < cantidad) return false;
            EcoPoints -= cantidad; return true;
        }
        public int PendingCount => Items.Count;
        public bool Active {get;private set;}
        public bool ZoneRecovered {get;private set;}
        public bool FinalRecovery {get;private set;}
        public RecyclableItem Selected {get;private set;}
        public string Feedback {get;private set;}="";
        public Transform StationsRoot => colocacion != null ? colocacion.Raiz : null;
        ColocacionContenedores colocacion;
        public bool EsperandoColocacion => Active && initialized && !ZoneRecovered && colocacion != null && colocacion.Buscando;
        public bool PuedeConfirmarContenedores => EsperandoColocacion && controller.SeguimientoDisponible && colocacion.PuedeColocar;
        public List<RecyclableItem> Items {get;}=new();
        public IReadOnlyList<RecycleStation> Stations => stations;
        readonly List<RecycleStation> stations=new();
        readonly List<RaycastResult> uiHits=new();
        ARPrototypeController controller;
        GameManager manager;
        RecyclingVisualCatalog catalog;
        PollutionZone zone;
        float settledWait,completeWait,feedbackTime;
        bool initialized;
        int collectedThisWave;
        ParticleSystem finalAura;
        public bool InputAvailable => Active && initialized && !ZoneRecovered && !EsperandoColocacion && controller!=null && controller.SeguimientoDisponible && manager!=null && manager.RecoveryInProgress;
        public bool ClasificacionCompacta => Active && initialized && !ZoneRecovered && colocacion != null && colocacion.Compacto;
        public bool ReadyToAdvance => ZoneRecovered && Items.Count==0;
        public string Instruction => !Active?"":!controller.SeguimientoDisponible?"RECUPERANDO TRACKING: MIRA EL PORTAL":
            ZoneRecovered?$"ZONA RECUPERADA\n+{collectedThisWave*50} ECO":!initialized?"ZONA CONTAMINADA\nPREPARANDO RECUPERACIÓN":
            EsperandoColocacion?colocacion.Mensaje:
            ClasificacionCompacta?(Selected!=null?"ELIGE PLÁSTICO, PAPEL O METAL":"TOCA UN RESIDUO Y USA LOS BOTONES"):
            Selected!=null?"TOCA EL CONTENEDOR CORRECTO":"ZONA CONTAMINADA\nCLASIFICA LOS RESIDUOS";
        void Awake()
        {
            controller=GetComponent<ARPrototypeController>();manager=GetComponent<GameManager>();
            catalog=Resources.Load<RecyclingVisualCatalog>("RecyclingVisualCatalog");
            colocacion=gameObject.AddComponent<ColocacionContenedores>();
            var tienda = GetComponent<TiendaECO>();
            if (tienda == null) tienda = gameObject.AddComponent<TiendaECO>();
            tienda.Configurar(controller, manager, this);
        }
        void Start()
        {
            var hud=FindFirstObjectByType<RiftHuntersAR.UI.Visual.VisualHUD>();
            if(hud!=null)
            {
                hud.gameObject.AddComponent<RecyclingHUD>().Configure(this,hud);
                hud.gameObject.AddComponent<RiftHuntersAR.UI.Visual.InterfazTienda>().Configurar(GetComponent<TiendaECO>(), hud);
            }
        }
        public void BeginRecovery(bool final)
        {
            ClearStations();Deselect();Items.Clear();Active=true;FinalRecovery=final;ZoneRecovered=false;
            settledWait=.65f;initialized=false;collectedThisWave=0;Feedback="";
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
            if(resetEco) { EcoPoints=0; EcoTotal=0; }
        }
        void Update()
        {
            if(finalAura!=null)
            {
                bool tracked=controller!=null && controller.SeguimientoDisponible;
                if(tracked && finalAura.isPaused)finalAura.Play();else if(!tracked && finalAura.isPlaying)finalAura.Pause();
            }
            if(!Active || controller==null)return;
            colocacion.Actualizar(controller.SeguimientoDisponible);
            SincronizarEstaciones();
            if(!controller.SeguimientoDisponible)return;
            if(feedbackTime>0){feedbackTime-=Time.deltaTime;if(feedbackTime<=0)Feedback="";}
            if(!initialized)
            {
                settledWait-=Time.deltaTime;if(settledWait>0)return;
                if(zone!=null)
                    foreach(var item in zone.GetComponentsInChildren<RecyclableItem>())if(item.Classifiable)Items.Add(item);
                initialized=true;if(Items.Count>0)TryPlaceStations();
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
            // También se pueden seleccionar residuos en el modo de botones.
            if(TryTap(out Vector2 point))HandleTap(point);
        }
        void TryPlaceStations()
        {
            colocacion.Preparar(controller.PortalActivo, controller.CamaraJuego, catalog);
            SincronizarEstaciones();
        }
        void SincronizarEstaciones()
        {
            stations.Clear();
            for(int i=0;i<colocacion.Estaciones.Count;i++) stations.Add(colocacion.Estaciones[i]);
        }
        void ClearStations() { if(colocacion!=null)colocacion.Ocultar(); }
        public bool ConfirmarContenedores()
        {
            if(!PuedeConfirmarContenedores || !colocacion.Confirmar())return false;
            SincronizarEstaciones(); return true;
        }
        public void UsarBotonesClasificacion()
        {
            if(!Active || !initialized || ZoneRecovered || !controller.SeguimientoDisponible)return;
            Deselect();colocacion.UsarBotones();
        }
        public void ReubicarContenedores()
        {
            if(!Active || !initialized || ZoneRecovered || !controller.SeguimientoDisponible)return;
            Deselect();colocacion.Reubicar();
        }
        void Deselect(){if(Selected!=null)Selected.SetSelected(false);Selected=null;}
        public bool Select(RecyclableItem item)
        {
            if(!InputAvailable || item==null || !item.IsSettled || !Items.Contains(item) || !item.Classifiable)return false;
            Deselect();Selected=item;Selected.SetSelected(true);Feedback="RESIDUO SELECCIONADO";feedbackTime=1;return true;
        }
        public bool Classify(RecycleStation station)
        {
            if(!InputAvailable || ClasificacionCompacta || Selected==null || station==null || !station.gameObject.activeInHierarchy || !stations.Contains(station))return false;
            return Clasificar(station.Category, station.transform.TransformPoint(Vector3.up * .65f));
        }
        public bool ClasificarCategoria(RecyclingCategory categoria)
        {
            if (!ClasificacionCompacta || !InputAvailable || Selected == null || categoria == RecyclingCategory.Other) return false;
            return Clasificar(categoria, Selected.transform.position + Vector3.up * .12f);
        }
        private bool Clasificar(RecyclingCategory categoria, Vector3 destino)
        {
            if (Selected == null || !Selected.Classifiable) return false;
            if(Selected.Category!=categoria)
            {Feedback="CONTENEDOR INCORRECTO";feedbackTime=1.4f;return false;}
            var item=Selected;item.MarkCollected();Items.Remove(item);Selected=null;EcoPoints+=50;EcoTotal+=50;collectedThisWave++;
            Feedback="RESIDUO CLASIFICADO\n+50 ECO";feedbackTime=1.25f;
            item.gameObject.AddComponent<RecyclingCollection>().Begin(destino,catalog != null ? catalog.contaminationVFX : null);
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
            if(!InputAvailable && !EsperandoColocacion)return false;
            if(EventSystem.current!=null)
            {
                uiHits.Clear();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},uiHits);
                if(uiHits.Count>0)return false;
            }
            if(EsperandoColocacion)return ConfirmarContenedores();
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
