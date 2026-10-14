"""Render real Blender geometry at shared scale with labels. Never imports into Unity."""
import sys, math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
import bpy
from mathutils import Vector
from arte_comun import ROOT, studio, mat, options
args=options()
NAMES=['EnemyFast_Final','EnemyBasic_Final','EnemyTank_Final','RiftGuardian_Final','Portal_Final','PlasmaBlaster_Final','ElectricCannon_Final','EnergyLauncher_Final']
def append(name):
    directory='EnemyBasic' if name=='EnemyBasic_Final' else name
    path=ROOT/'ArtSource'/directory/(name+'.blend')
    with bpy.data.libraries.load(str(path),link=False) as (src,dst):dst.objects=src.objects
    objects=[o for o in dst.objects if o]
    root=next(o for o in objects if o.name.startswith(name) and o.parent is None)
    def belongs(o):
        p=o
        while p:
            if p==root:return True
            p=p.parent
        return False
    for o in objects:
        if belongs(o):bpy.context.scene.collection.objects.link(o)
    return root
def label(scene,camera,text,x,y,size=.14):
    data=bpy.data.curves.new('Label_'+text,'FONT');data.body=text;data.align_x='CENTER';data.size=size;data.extrude=0
    ob=bpy.data.objects.new('Label_'+text,data);scene.collection.objects.link(ob);ob.location=(x,y,.035)
    ob.rotation_euler=camera.rotation_euler;data.materials.append(mat('Label_'+text,(.65,.78,.9),0,.8,.25))
def run(all_assets):
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    scene=bpy.context.scene
    positions=[(-2.65,.8),(-1.55,.8),(-.05,.8),(1.85,.8)]
    selected=NAMES if all_assets else NAMES[:4]
    for i,name in enumerate(selected):
        model=append(name)
        if i<4:x,y=positions[i]
        else:x,y=[(-2.5,-2.1),(-.8,-2.1),(.7,-2.1),(2.2,-2.1)][i-4]
        model.location=(x,y,0)
        if i>=5:model.rotation_euler.z=math.radians(60)
    camera=studio(scene,5.5,1.85,samples=args.samples,resolution=args.resolution)
    scene.render.resolution_x=args.resolution;scene.render.resolution_y=round(args.resolution*(.75 if all_assets else .445))
    camera.location=(.25,-8,5.6 if all_assets else 3.7)
    target=Vector((.1,-.3,.62)) if all_assets else Vector((.1,.65,.63))
    camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
    camera.data.ortho_scale=7.0
    for i,name in enumerate(selected):
        if i<4:x,y=positions[i];ly=y-.95
        else:x,y=[(-2.5,-2.1),(-.8,-2.1),(.7,-2.1),(2.2,-2.1)][i-4];ly=y-.8
        label(scene,camera,name.removesuffix('_Final'),x,ly,.13)
    label(scene,camera,'ESCALA COMPARTIDA EN METROS | BASIC 0.60 m',.1,2.75,.17)
    out=ROOT/'ArtRenders'/'Lineups';out.mkdir(parents=True,exist_ok=True)
    scene.render.filepath=str(out/('RiftHunters_3D_Lineup.png' if all_assets else 'RiftHunters_Enemies_Lineup.png'))
    bpy.ops.render.render(write_still=True)
run(False)
run(True)
print('LINEUPS_SUCCESS',flush=True)
