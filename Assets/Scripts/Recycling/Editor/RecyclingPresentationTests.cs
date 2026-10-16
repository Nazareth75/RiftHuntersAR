using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RiftHunters.Combate;
using Object=UnityEngine.Object;

namespace RiftHuntersAR.Recycling.Editor
{
    [InitializeOnLoad]
    public static class RecyclingPresentationTests
    {
        const string Flag="RiftRecyclingPlay";
        static double next,started=EditorApplication.timeSinceStartup;
        static int stage,errors;
        static Enemigo[] enemies;
        static Vector3[] roots,pivots;
        static Vector3[] colliderCenters;
        static bool attacked, tankAttacked, guardianAttacked;
        static Enemigo automaticEnemy;
        static readonly List<string> checks=new();
        static RecyclingPresentationTests(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
        static void Log(string text,string stack,LogType type)
        {if(SessionState.GetBool(Flag,false) && (type==LogType.Error || type==LogType.Exception || type==LogType.Assert)){errors++;File.AppendAllText("Temp/RiftRecyclingErrors.txt",text+"\n"+stack+"\n");}}
        public static void Begin()
        {
            File.WriteAllText("Temp/RiftRecyclingErrors.txt","");File.WriteAllText("Temp/RiftRecyclingPlayResult.txt","RUNNING");
            SessionState.SetBool(Flag,true);stage=0;errors=0;checks.Clear();started=EditorApplication.timeSinceStartup;EditorApplication.isPlaying=true;
        }
        static void Require(bool condition,string text){if(!condition)throw new Exception(text);checks.Add("PASS "+text);}
        static void Tick()
        {
            if(!SessionState.GetBool(Flag,false))return;
            try
            {
                double now=Time.timeAsDouble;
                if(EditorApplication.timeSinceStartup-started>120)throw new Exception("Recycling review test timeout");
                if(!EditorApplication.isPlaying || EditorApplication.isPaused)return;
                EditorApplication.QueuePlayerLoopUpdate();
                if(stage==0)
                {
                    var director=Object.FindFirstObjectByType<RecyclingReviewDirector>();if(director==null || director.Subjects==null)return;
                    director.FocusMotion();
                    enemies=director.Subjects;roots=new Vector3[4];pivots=new Vector3[4];colliderCenters=new Vector3[4];
                    for(int i=0;i<4;i++)
                    {
                        var e=enemies[i];e.ConfigurarEstadisticas(e.VidaMaxima,0,1,e.Puntos);
                        roots[i]=e.transform.position;pivots[i]=e.GetComponent<EnemyProceduralVisual>().VisualPivot.localPosition;colliderCenters[i]=e.GetComponent<CapsuleCollider>().center;
                    }
                    stage=1;next=now+.7;return;
                }
                if(now<next)return;
                if(stage==1)
                {
                    for(int i=0;i<4;i++)
                    {
                        var e=enemies[i];var motion=e.GetComponent<EnemyProceduralVisual>();
                        Require(Vector3.Distance(e.transform.position,roots[i])<.00001f,"Functional root position untouched "+i);
                        Require(e.GetComponent<CapsuleCollider>().center==colliderCenters[i],"Functional collider unchanged "+i);
                        Require(Vector3.Distance(motion.VisualPivot.localPosition,pivots[i])>.000001f,"Live procedural movement on child only "+i);
                        Require(motion.Kind==i,"Distinct procedural profile "+i);
                        int hp=e.Vida;e.RecibirDano(1);Require(i==3?e.Vida==hp:e.Vida==hp-1,"Hit/shield functionality preserved "+i);
                        motion.PreviewAttack();bool accepted=e.GetComponent<EnemyPollutionEmitter>().Contaminate();Require(accepted==(i!=3),"Pollution command/Guardian exclusion "+i);
                    }
                    stage=2;next=now+1;return;
                }
                if(stage==2)
                {
                    for(int i=0;i<4;i++)
                    {
                        var emitter=enemies[i].GetComponent<EnemyPollutionEmitter>();int expected=i==3?0:i==2?2:1;
                        Require(emitter.EmittedCount==expected,"Per-enemy waste cap "+i+" = "+expected);
                        Require(!emitter.Contaminate(),"No repeated/spam pollution "+i);
                        Require(Vector3.Distance(enemies[i].transform.position,roots[i])<.00001f,"Pollution never moves functional root "+i);
                    }
                    var items=Object.FindObjectsByType<RecyclableItem>(FindObjectsSortMode.None).Where(x=>x.GetComponentInParent<PollutionZone>()!=null).ToArray();
                    Require(items.Length==4,"Exactly four emitted residues");
                    Require(items.Select(x=>x.Category).Distinct().Count()==3,"Plastic/Paper/Metal generated");
                    foreach(var item in items)
                    {
                        Require(item.GetComponentsInChildren<Transform>().All(t=>t.gameObject.layer==2),"Residue excluded from default shooting raycasts");
                        Require(item.GetComponent<Rigidbody>()==null && item.GetComponentsInChildren<Collider>().All(c=>c.isTrigger),"Residue cannot push or damage enemies");
                        ModeloDeIntegracion.MedirVisible(item.gameObject,item.transform.parent,out var b);Require(Mathf.Abs(b.min.y)<.0015f,"Residue rests on surface, no penetration "+item.Category);
                    }
                    var guardian=enemies[3];var cores=guardian.GetComponent<NucleosJefe>();Require(cores!=null && cores.IzquierdoVivo && cores.DerechoVivo && !cores.CentralExpuesto,"Guardian weak points retained");
                    foreach(var pair in new[]{("Nucleo brazo izquierdo","WeakPoint_Left_Mesh"),("Nucleo brazo derecho","WeakPoint_Right_Mesh"),("Nucleo central","WeakPoint_Core_Mesh")})
                    {
                        var sphere=guardian.transform.Find(pair.Item1);var renderer=guardian.GetComponentsInChildren<Renderer>().First(r=>r.name==pair.Item2);
                        Require(Vector3.Distance(sphere.position,renderer.bounds.center)<.05f,"Floating Guardian collider follows "+pair.Item2);
                    }
                    Require(guardian.transform.Find("Guardian contamination aura")!=null,"Guardian sparse contamination aura");
                    Require(guardian.transform.Find("Guardian contamination aura").GetComponent<ParticleSystem>().particleCount>0,"Guardian VFX actually emits live particles");
                    RecyclingAssetBuilder.Capture(Camera.main,"ArtRenders/Recycling/Unity_Recycling_Contamination.png");
                    enemies[0].RecibirDano(enemies[0].VidaMaxima);
                    Require(Object.FindObjectsByType<EnemyVisualRemnant>(FindObjectsSortMode.None).Length==1,"Death leaves brief visual fall, existing root removal preserved");
                    enemies[1].AtacoJugador+=_=>attacked=true;enemies[1].transform.Find("Review movement target").localPosition=Vector3.zero;
                    enemies[2].ConfigurarEstadisticas(enemies[2].Vida, .05f,1,enemies[2].Puntos);
                    stage=3;next=now+1.2;return;
                }
                if(stage==3)
                {
                    Require(enemies[0]==null,"Dead functional enemy removed");Require(attacked && enemies[1]==null,"Actual enemy windup/attack/damage event and retirement preserved");
                    Require(Object.FindObjectsByType<EnemyVisualRemnant>(FindObjectsSortMode.None).Length==0,"Presentation remnants cleaned up");
                    Require(Vector3.Distance(enemies[2].transform.position,roots[2])>.035f,"Functional walking and heavy visual sway coexist");
                    enemies[2].AtacoJugador+=_=>tankAttacked=true;enemies[2].transform.Find("Review movement target").localPosition=Vector3.zero;
                    enemies[3].AtacoJugador+=_=>guardianAttacked=true;enemies[3].transform.Find("Review movement target").localPosition=Vector3.zero;
                    var director=Object.FindFirstObjectByType<RecyclingReviewDirector>();
                    var extra=Object.Instantiate(director.enemyPrefabs[0],director.transform);extra.transform.localPosition=new Vector3(-1.8f,0,.8f);automaticEnemy=extra.GetComponent<Enemigo>();
                    var goal=new GameObject("Automatic pollution test target");goal.transform.SetParent(extra.transform,false);goal.transform.localPosition=Vector3.forward*20;automaticEnemy.AsignarObjetivo(goal.transform);
                    stage=4;next=now+2.9;return;
                }
                if(stage==4)
                {
                    Require(tankAttacked && enemies[2]==null,"Actual Tank attack preserves functional retirement");
                    Require(guardianAttacked && enemies[3]!=null,"Actual Guardian attack preserves floating boss/root");
                    Require(automaticEnemy.GetComponent<EnemyPollutionEmitter>().EmittedCount==1,"Automatic contamination while moving, no external game manager changes");
                    Require(errors==0,"Zero runtime errors");
                    File.WriteAllText("Temp/RiftRecyclingPlayResult.txt","PASS\n"+string.Join("\n",checks));SessionState.SetBool(Flag,false);EditorApplication.isPlaying=false;
                }
            }
            catch(Exception e)
            {File.WriteAllText("Temp/RiftRecyclingPlayResult.txt","FAIL "+e+"\n"+string.Join("\n",checks));SessionState.SetBool(Flag,false);EditorApplication.isPlaying=false;Debug.LogException(e);}
        }
    }
}
