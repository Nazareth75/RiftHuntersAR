"""Read-only Blender source + FBX round-trip verification, isolated CLI process."""
import bpy, json
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
source=root/'ArtSource/EnemyBasic/EnemyBasic_Final.blend'
fbx=root/'ArtExports/EnemyBasic/EnemyBasic_Final.fbx'
stats=json.loads((source.parent/'EnemyBasic_Stats.json').read_text())
bpy.ops.wm.open_mainfile(filepath=str(source))
model=bpy.data.objects['EnemyBasic_Final']
assert model.location.length < 1e-6
parts=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.parent and o.parent.parent==model]
assert len(parts)==8
assert sum(len(o.data.polygons) for o in parts)==stats['triangles']
assert {m.name for o in parts for m in o.data.materials}=={'MetalDark','MetalLight','EnemyEnergy'}
for name in ('Perspectiva','Frente','Lateral','Trasera'):
    assert (root/f'ArtRenders/EnemyBasic/EnemyBasic_{name}.png').stat().st_size>10000
# Clear only this CLI work scene. Neither source nor Unity scene is saved/modified.
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(fbx))
parts=[o for o in bpy.context.scene.objects if o.type=='MESH']
assert len(parts)==8, 'Preview objects leaked into FBX'
bpy.context.view_layer.update()
bounds=[o.matrix_world@Vector(v) for o in parts for v in o.bound_box]
dims=[max(p[i] for p in bounds)-min(p[i] for p in bounds) for i in range(3)]
assert abs(dims[2]-.6)<.001, f'FBX height incorrect: {dims}'
assert abs(min(p.z for p in bounds))<.001, 'Feet do not sit at model origin'
assert sum(len(o.data.polygons) for o in parts)==stats['triangles']
assert not any(o.type in {'LIGHT','CAMERA'} and o.name.startswith('PREVIEW') for o in bpy.context.scene.objects)
print('ART_VALIDATION_PASS',json.dumps({'meshes':8,'triangles':stats['triangles'],'dimensions_m':dims,'materials':3,'rigged':False}))
