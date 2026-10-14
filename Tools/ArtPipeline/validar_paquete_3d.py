"""Source geometry + independent FBX reimport QA for the eight assets."""
import bpy,math,json,hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
names=['EnemyBasic_Final','EnemyFast_Final','EnemyTank_Final','RiftGuardian_Final','Portal_Final','PlasmaBlaster_Final','ElectricCannon_Final','EnergyLauncher_Final']
expected_basic={'ArtSource/EnemyBasic/EnemyBasic_Final.blend':'8EF622EF58CA2379C0D0811FE5953B11D392AA381734A53ADAA978A0F59D7E88',
                'ArtExports/EnemyBasic/EnemyBasic_Final.fbx':'9D5A41DB9A4DF256F44482D83B8233ECFC96CBF0CB8B593F0C2735753E068E12'}
for path,digest in expected_basic.items():assert hashlib.sha256((ROOT/path).read_bytes()).hexdigest().upper()==digest,'Approved Basic changed'
report=[]
for name in names:
    directory='EnemyBasic' if name=='EnemyBasic_Final' else name
    source=ROOT/'ArtSource'/directory/(name+'.blend');fbx=ROOT/'ArtExports'/directory/(name+'.fbx')
    stats_path=ROOT/'ArtSource'/directory/('EnemyBasic_Stats.json' if name=='EnemyBasic_Final' else name+'_Stats.json')
    stats=json.loads(stats_path.read_text())
    bpy.ops.wm.open_mainfile(filepath=str(source));scene=bpy.context.scene
    model=bpy.data.objects[name];assert model.location.length<1e-6
    def belongs(o):
        p=o
        while p:
            if p==model:return True
            p=p.parent
        return False
    parts=[o for o in scene.objects if o.type=='MESH' and belongs(o)]
    assert len({o.name for o in parts})==len(parts)
    assert sum(len(o.data.polygons) for o in parts)==stats['triangles']
    for o in parts:
        assert o.scale==Vector((1,1,1))
        assert all(len(p.vertices)==3 and p.area>1e-12 and p.normal.length>.99 for p in o.data.polygons)
        assert all(math.isfinite(c) for v in o.data.vertices for c in v.co)
    if name=='RiftGuardian_Final':
        for key in ('WeakPoint_Left','WeakPoint_Right','WeakPoint_Core'):assert key in bpy.data.objects
        assert sum(o.name.startswith('FloatingFragment_') and o.type=='EMPTY' for o in scene.objects)>=20
    if name=='Portal_Final':
        for key in ('Portal_Frame','Portal_Fragments','Portal_CoreReference'):assert key in bpy.data.objects
    renderdir=ROOT/'ArtRenders'/directory
    for view in ('Perspectiva','Frente','Lateral','Trasera'):
        filename=('EnemyBasic' if name=='EnemyBasic_Final' else name)+'_'+view+'.png'
        assert (renderdir/filename).stat().st_size>10000
    # New process scene only; no source files saved or changed.
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(fbx));bpy.context.view_layer.update()
    imported=[o for o in bpy.context.scene.objects if o.type=='MESH']
    assert len(imported)==len(parts)
    assert sum(len(o.data.polygons) for o in imported)==stats['triangles']
    assert not any(o.type in ('CAMERA','LIGHT') for o in bpy.context.scene.objects)
    bounds=[o.matrix_world@Vector(v) for o in imported for v in o.bound_box]
    dims=[max(p[i] for p in bounds)-min(p[i] for p in bounds) for i in range(3)]
    assert abs(dims[2]-stats['height_m'])<.001
    assert abs(min(p.z for p in bounds))<.001
    materials=sorted({m.name.split('.')[0] for o in imported for m in o.data.materials})
    report.append({'asset':name,'triangles':stats['triangles'],'meshes':len(imported),'materials':materials,'dimensions_xyz_m':dims,'checks':'PASS'})
    print('PASS',name,stats['triangles'],flush=True)
out=ROOT/'ArtSource'/'PAQUETE_3D_QA.json';out.write_text(json.dumps(report,indent=2),encoding='utf8')
print('PACKAGE_QA_SUCCESS',flush=True)
