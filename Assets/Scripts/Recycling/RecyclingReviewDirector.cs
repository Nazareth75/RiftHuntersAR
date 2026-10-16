using UnityEngine;
using RiftHunters.Combate;

namespace RiftHuntersAR.Recycling
{
    // Isolated review scene only; never referenced from MainMenu, ARGame or build settings.
    public sealed class RecyclingReviewDirector : MonoBehaviour
    {
        public GameObject[] enemyPrefabs;
        public GameObject staticMonsters;
        public GameObject staticAssets;
#if UNITY_EDITOR
        Enemigo[] enemies;
        bool previousRunInBackground;
        Vector3[] origins;
        public Enemigo[] Subjects => enemies;
        public void FocusMotion()
        {
            staticAssets.SetActive(false);
            var camera=Camera.main;camera.orthographicSize=1.1f;
            camera.transform.position=new Vector3(1.5f,1.65f,-2.8f);camera.transform.LookAt(new Vector3(0,.26f,1.1f));
        }
        void Start() { previousRunInBackground=Application.runInBackground;Application.runInBackground=true;staticMonsters.SetActive(false);ResetReview(); }
        void OnDestroy() { Application.runInBackground=previousRunInBackground; }
        public void ResetReview()
        {
            if(enemies!=null) foreach(var enemy in enemies) if(enemy!=null) Destroy(enemy.gameObject);
            var zone=transform.Find("Pollution - visual residues");if(zone!=null) zone.GetComponent<PollutionZone>().Clear();
            enemies=new Enemigo[4];origins=new Vector3[4];
            for(int i=0;i<4;i++)
            {
                var root=Instantiate(enemyPrefabs[i],transform);origins[i]=new Vector3(-1.4f+i*.92f,0,1.1f);root.transform.localPosition=origins[i];
                var enemy=root.GetComponent<Enemigo>();enemies[i]=enemy;
                enemy.ConfigurarEstadisticas(enemy.VidaMaxima,.04f,1,enemy.Puntos);
                var goal=new GameObject("Review movement target");goal.transform.SetParent(root.transform,false);goal.transform.localPosition=Vector3.forward*20;
                enemy.AsignarObjetivo(goal.transform);
                if(i==3) {var cores=root.GetComponent<NucleosJefe>();if(cores==null)cores=root.AddComponent<NucleosJefe>();cores.Inicializar(Camera.main.transform);}
            }
        }
        void Update()
        {
            if(enemies==null) return;
            for(int i=0;i<4;i++) if(enemies[i]!=null && Vector3.Distance(enemies[i].transform.localPosition,origins[i])>.25f) enemies[i].transform.localPosition=origins[i];
        }
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12,12,Mathf.Min(620,Screen.width-24),150),GUI.skin.box);
            GUILayout.Label("REVISIÓN LOCAL · Assets + movimiento · no clasificación ni nuevas waves");
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Contaminar")) foreach(var e in enemies) if(e!=null)e.GetComponent<EnemyPollutionEmitter>().Contaminate();
            if(GUILayout.Button("Hit")) foreach(var e in enemies) if(e!=null)e.RecibirDano(1);
            if(GUILayout.Button("Ataque visual")) foreach(var e in enemies) if(e!=null)e.GetComponent<EnemyProceduralVisual>().PreviewAttack();
            if(GUILayout.Button("Reiniciar")) ResetReview();
            GUILayout.EndHorizontal();GUILayout.Label("Basic: 1 botella · Fast: 1 papel · Tank: 2 (metal/plástico) · Guardian: aura");
            if(GUILayout.Button("Ver movimiento sin contenedores delante")) FocusMotion();
            GUILayout.EndArea();
        }
#endif
    }
}
