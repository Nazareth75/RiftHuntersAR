using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using RiftHuntersAR.Enemies;
namespace RiftHuntersAR.Combat
{
    public sealed class PlasmaWeapon : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float damage = 25;
        [SerializeField] private float range = 20;
        [SerializeField] private int magazineSize = 30;
        public Camera AimCamera => aimCamera;
        private int municionOriginal = 30;
        private RiftHunters.Combate.GestorArmas sistemaCombate;
        public int CurrentAmmo => sistemaCombate != null ? sistemaCombate.Actual.Municion : municionOriginal;
        public string NombreArma => sistemaCombate != null ? sistemaCombate.Actual.NombreVisible : "Plasma";
        public int MagazineSize => sistemaCombate != null ? sistemaCombate.Actual.CapacidadCargador : magazineSize;
        public bool Ready { get; set; }
        public event Action<string> Feedback;
        private float nextShot;
        private int lastShotFrame = -1;
        private bool pruebaEscritorio;
        public void HabilitarPruebaEscritorio()
        {
            if (Application.isEditor || !Application.isMobilePlatform) pruebaEscritorio = true;
        }
        // Mantiene las interfaces del prototipo para su escena y sus pruebas.
        public void ConectarSistema(RiftHunters.Combate.GestorArmas sistema)
        {
            sistemaCombate = sistema;
            sistemaCombate.AsignarCamara(aimCamera);
        }

        public void RestaurarMunicion()
        {
            if (sistemaCombate != null) sistemaCombate.ReiniciarArmas();
            else municionOriginal = magazineSize;
        }

        public void Reload()
        {
            if (sistemaCombate != null)
            {
                sistemaCombate.Recargar();
                Feedback?.Invoke(sistemaCombate.Actual.UltimoResultado);
            }
            else
            {
                municionOriginal = magazineSize;
                Feedback?.Invoke("RELOADED");
            }
        }
        public void Shoot()
        {
            Fire(pruebaEscritorio || ARSession.state == ARSessionState.SessionTracking, Time.unscaledTime, Time.frameCount);
        }
        internal void Fire(bool tracking, float now, int frame)
        {
            if (!Ready || aimCamera == null || !tracking) return;
            if (sistemaCombate != null)
            {
                if (lastShotFrame == frame) return;
                int municionAnterior = CurrentAmmo;
                sistemaCombate.Disparar();
                if (CurrentAmmo != municionAnterior) lastShotFrame = frame;
                Feedback?.Invoke(sistemaCombate.Actual.UltimoResultado);
                return;
            }
            if (lastShotFrame == frame || now < nextShot) return;
            if (CurrentAmmo <= 0) { Feedback?.Invoke("EMPTY - RELOAD"); return; }
            lastShotFrame = frame;
            nextShot = now + .2f;
            municionOriginal--;
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Feedback?.Invoke(enemy.CurrentHealth <= 0 ? "ENEMY DESTROYED +100" : $"HIT -{enemy.CurrentHealth:0} HP");
                    Debug.Log($"[Rift Shot] frame={Time.frameCount} target={enemy.GetInstanceID()} damage={damage} hp={enemy.CurrentHealth} ammo={CurrentAmmo}");
                    return;
                }
            }
            Feedback?.Invoke("MISS");
        }
    }
}
