using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using RiftHuntersAR.AR;
using RiftHuntersAR.Combat;
using RiftHuntersAR.Enemies;
using RiftHuntersAR.Game;

namespace RiftHuntersAR.Integration
{
    public sealed class ARPrototypeController : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager planes;
        [SerializeField] private ARRaycastManager raycasts;
        [SerializeField] private ARAnchorManager anchors;
        [SerializeField] private PortalSpawnPoint portalPrefab;
        [SerializeField] private EnemyHealth enemyPrefab;
        [SerializeField] private PlasmaWeapon weapon;
        [SerializeField] private ScoreManager score;
        [SerializeField] private Text statusText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button shootButton;
        private readonly List<ARRaycastHit> hits = new();
        private readonly List<RaycastResult> uiHits = new();
        private bool placed;
        private bool spawning;
        private void OnEnable()
        {
            score.Changed += UpdateScore;
            weapon.Feedback += UpdateFeedback;
        }
        private void OnDisable()
        {
            score.Changed -= UpdateScore;
            weapon.Feedback -= UpdateFeedback;
        }
        private void Start()
        {
            Application.targetFrameRate = 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            UpdateScore(score.Score);
            shootButton.onClick.AddListener(weapon.Shoot);
        }
        private void UpdateScore(int value) { scoreText.text = $"SCORE: {value}"; }
        private void UpdateFeedback(string value) { feedbackText.text = value; }
        private bool TouchPosition(out Vector2 position)
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            { position = Touchscreen.current.primaryTouch.position.ReadValue(); return true; }
#if UNITY_EDITOR
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            { position = Mouse.current.position.ReadValue(); return true; }
#endif
            position = default; return false;
        }
        private bool OverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = position };
            uiHits.Clear();
            EventSystem.current.RaycastAll(data, uiHits);
            return uiHits.Count > 0;
        }
        private void Update()
        {
            bool tracking = ARSession.state == ARSessionState.SessionTracking;
            shootButton.interactable = tracking && weapon.Ready;
            if (!tracking)
            {
                statusText.text = ARSession.state == ARSessionState.Unsupported ? "AR NOT SUPPORTED" : "SCAN ENVIRONMENT";
                return;
            }
            if (placed)
            {
                foreach (ARPlane plane in planes.trackables) { var r = plane.GetComponent<MeshRenderer>(); if (r != null) r.enabled = false; }
                statusText.text = spawning ? "SPAWNING..." : weapon.Ready ? "READY" : "TARGET ELIMINATED";
                return;
            }
            bool hasPlane = false;
            foreach (ARPlane plane in planes.trackables)
                if (plane.trackingState == TrackingState.Tracking) { hasPlane = true; break; }
            statusText.text = hasPlane ? "TAP TO PLACE PORTAL" : "SCAN ENVIRONMENT";
            if (!TouchPosition(out Vector2 position) || OverUI(position)) return;
            if (!raycasts.Raycast(position, hits, TrackableType.PlaneWithinPolygon)) return;
            ARPlane selected = planes.GetPlane(hits[0].trackableId);
            if (selected == null || selected.trackingState != TrackingState.Tracking) return;
            Pose pose = hits[0].pose;
            ARAnchor anchor = anchors.AttachAnchor(selected, pose);
            if (anchor == null) { UpdateFeedback("PLACEMENT FAILED: TAP AGAIN"); return; }
            PortalSpawnPoint portal = Instantiate(portalPrefab, pose.position, pose.rotation, anchor.transform);
            placed = true;
            foreach (ARPlane plane in planes.trackables)
            {
                var renderer = plane.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
            // Keep tracking active; the anchor owns the placed content during plane refinements.

            StartCoroutine(Spawn(portal));
        }
        private IEnumerator Spawn(PortalSpawnPoint portal)
        {
            spawning = true;
            yield return new WaitForSeconds(1);
            while (ARSession.state != ARSessionState.SessionTracking) yield return null;
            if (portal == null) yield break;
            EnemyHealth enemy = Instantiate(enemyPrefab, portal.SpawnPoint.position, portal.SpawnPoint.rotation, portal.transform);
            enemy.Died += OnEnemyDied;
            weapon.Ready = true;
            spawning = false;
        }
        private void OnEnemyDied(EnemyHealth enemy)
        {
            enemy.Died -= OnEnemyDied;
            score.AddEnemyScore();
            weapon.Ready = false;
        }
    }
}
