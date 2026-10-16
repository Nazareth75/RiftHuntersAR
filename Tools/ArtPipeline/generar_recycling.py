"""Deterministic, editable Recycling sources; never reads/writes approved enemies.
Run with Blender --background --factory-startup --python-exit-code 1 --python this_file.
"""
import bpy, bmesh, math, json, random, sys
from pathlib import Path
from mathutils import Vector
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import mat, studio
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSource'/'Recycling'
FBX=ROOT/'Assets'/'Art'/'Models'/'Recycling'
RENDER=ROOT/'ArtRenders'/'Recycling'
for p in (SOURCE,FBX,RENDER):p.mkdir(parents=True,exist_ok=True)
stats=[]
parts=[]

def mesh(name,v,f,m):
    d=bpy.data.meshes.new(name);d.from_pydata(v,[],f);d.update()
    o=bpy.data.objects.new(name,d);bpy.context.scene.collection.objects.link(o);o.data.materials.append(m);parts.append(o);return o

def box(name,p,s,m,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.scale=s
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m);parts.append(o)
    if bevel:
        mod=o.modifiers.new('Angular edges','BEVEL');mod.width=bevel;mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return o

def rings(name,levels,m,n=10,dent=0,seed=0):
    rng=random.Random(seed);v=[]
    for j,(z,r,ox,oy) in enumerate(levels):
        for i in range(n):
            a=i*math.tau/n;rr=r*(1+rng.uniform(-dent,dent))
            v.append((ox+rr*math.cos(a),oy+rr*math.sin(a),z))
    f=[tuple(range(n-1,-1,-1)),tuple(range((len(levels)-1)*n,len(v)))]
    for j in range(len(levels)-1):
        for i in range(n):
            a=j*n+i;b=j*n+(i+1)%n;c=b+n;d=a+n
            f.extend(((a,b,c),(a,c,d)))
    return mesh(name,v,f,m)

def tube(name,x,y,z,r,inner,h,m,n=12):
    v=[]
    for rr,zz in ((r,z),(r,z+h),(inner,z+h),(inner,z)):
        v.extend((x+rr*math.cos(i*math.tau/n),y+rr*math.sin(i*math.tau/n),zz) for i in range(n))
    f=[]
    for i in range(n):
        j=(i+1)%n
        f.extend(((i,j,j+n,i+n),(i+n,j+n,j+2*n,i+2*n),(i+2*n,j+2*n,j+3*n,i+3*n),(i+3*n,j+3*n,j,i)))
    return mesh(name,v,f,m)

def ico(name,p,s,m,seed=0):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=p);o=bpy.context.object;o.name=name
    rng=random.Random(seed)
    for v in o.data.vertices:v.co*=rng.uniform(.83,1.13)
    o.scale=s;o.data.materials.append(m);parts.append(o);return o

def line(name,a,b,width,m):
    mid=(Vector(a)+Vector(b))/2;d=Vector(b)-Vector(a)
    o=box(name,mid,(width,width,d.length),m);o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return o

def symbol(kind,m):
    # Model fronts face Blender -Y; no fonts, textures or copied artwork.
    y=-.196
    if kind=='Plastic':
        for k in range(3):
            a=k*math.tau/3+math.pi/6;r=a+math.pi/2
            points=[(-.052,.012),(.012,.012),(.012,.029),(.052,0),(.012,-.029),(.012,-.012),(-.052,-.012)]
            v=[]
            for x,z in points:
                x,z=x*math.cos(r)-z*math.sin(r),x*math.sin(r)+z*math.cos(r)
                v.append((x+.070*math.cos(a),y,.425+z+.070*math.sin(a)))
            arrow=mesh('Recycle arrow',v,[(0,1,5,6),(2,3,4)],m)
            bpy.context.view_layer.objects.active=arrow
            mod=arrow.modifiers.new('Raised metal symbol','SOLIDIFY');mod.thickness=.0015
            bpy.ops.object.modifier_apply(modifier=mod.name)
    elif kind=='Paper':
        pts=[(-.056,.35),(.056,.35),(.056,.47),(.02,.51),(-.056,.51),(-.056,.35)]
        for a,b in zip(pts,pts[1:]):line('Document outline',(a[0],y,a[1]),(b[0],y,b[1]),.008,m)
        for z in (.383,.41,.437):line('Paper line',(-.033,y,z),(.032,y,z),.006,m)
        line('Fold',(.02,y,.51),(.02,y,.47),.006,m);line('Fold',(.02,y,.47),(.056,y,.47),.006,m)
    else:
        for x in (-.046,.046):line('Can side',(x,y,.365),(x,y,.485),.009,m)
        for z in (.365,.485):
            for i in range(10):
                a=i*math.tau/10;b=(i+1)*math.tau/10
                line('Can rim',(.046*math.cos(a),y,z+.013*math.sin(a)),(.046*math.cos(b),y,z+.013*math.sin(b)),.006,m)
        line('Metal slash',(-.015,y,.40),(.015,y,.45),.009,m)

def station(kind,dark,energy):
    # Identical graphite structure in all three exports; shared material in Unity.
    box('Cabinet', (0,0,.43),(.34,.32,.66),dark,.035)
    for z in (.105,.745):box('Octagonal bumper',(0,0,z),(.43,.40,.12),dark,.045)
    for x in (-.18,.18):
        for y in (-.155,.155):
            o=box('Corner brace',(x,y,.43),(.065,.065,.59),dark,.012)
            box('Foot',(x,y,.035),(.095,.105,.07),dark,.012)
            if y<0:box('Side light',(x,-.195,.43),(.013,.012,.43),energy,.003)
    box('Recessed door',(0,-.169,.425),(.267,.025,.48),dark,.022)
    box('Top hopper',(0,0,.805),(.30,.25,.03),dark,.025)
    box('Hopper lip',(0,-.11,.815),(.22,.025,.01),energy,.003)
    box('Status bar',(0,-.204,.70),(.13,.012,.017),energy,.004)
    for x in (-.115,.115):box('Lower status',(x,-.204,.14),(.035,.012,.018),energy,.003)
    symbol(kind,energy)

names=['PlasticBottle','MetalCan','PaperCardboard','TrashBag','PlasticRecycleStation','PaperRecycleStation','MetalRecycleStation']
for name in names:
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);parts=[]
    scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
    if name=='PlasticBottle':
        base=mat('Recycling_Plastic',(.29,.36,.41),.05,.43);detail=mat('Recycling_Cyan',(.004,.24,.55),.25,.3,.35)
        levels=[(0,.027,0,0),(.012,.035,0,0),(.045,.037,0,0),(.075,.024,.006,0),(.105,.036,0,0),(.135,.027,-.004,.004),(.16,.034,.002,0),(.19,.033,0,0),(.216,.021,.004,0),(.24,.013,.006,0),(.25,.013,.006,0)]
        rings('Dented bottle',levels,base,10,.16,3)
        rings('Blue cap',[(.25,.016,.006,0),(.272,.016,.006,0),(.275,.014,.006,0)],detail,10)
        for z in (.065,.145):rings('Subtle cyan seam',[(z,.029,0,0),(z+.004,.03,0,0)],detail,10)
    elif name=='MetalCan':
        base=mat('Recycling_RedMetal',(.23,.018,.028),.72,.37);detail=mat('Recycling_CanRim',(.27,.31,.35),.8,.32)
        rings('Dented can',[(0,.029,0,0),(.008,.033,0,0),(.035,.034,.001,0),(.075,.027,-.003,.002),(.12,.032,0,0),(.135,.03,0,0)],base,10,.12,7)
        tube('Top rim',0,0,.133,.032,.027,.006,detail)
        tube('Open lid',0,0,.133,.027,.012,.002,detail)
        tube('Pull tab',0,-.004,.137,.009,.005,.003,detail,8)
        tube('Bottom rim',0,0,0,.032,.027,.004,detail)
        box('Metallic seam',(0,-.032,.069),(.012,.002,.056),detail)
    elif name=='PaperCardboard':
        base=mat('Recycling_Cardboard',(.31,.16,.066),0,.9);detail=mat('Recycling_Paper',(.68,.57,.40),0,.95)
        ico('Crumpled paper',(-.052,-.028,.064),(.065,.068,.066),detail,9)
        # Open, torn cardboard corner with two folded flaps, rather than a solid new box.
        mesh('Cardboard corner',[(.005,.01,0),(.13,.01,0),(.13,.11,0),(.005,.11,0),(.13,.11,.12),(.005,.11,.14),(.13,.01,.10),(.13,-.045,.11),(.005,-.04,.135),(.17,.11,.165),(.17,.01,.14)],[(0,3,2,1),(3,5,4,2),(1,2,4,6),(0,1,7,8),(6,4,9,10)],base)
        mod=parts[-1].modifiers.new('Paper thickness','SOLIDIFY');mod.thickness=.0015;bpy.context.view_layer.objects.active=parts[-1];bpy.ops.object.modifier_apply(modifier=mod.name)
    elif name=='TrashBag':
        base=mat('Recycling_Bag',(.018,.009,.032),.12,.48);detail=mat('Recycling_Violet',(.16,.025,.26),.2,.36,.12)
        rings('Bag folds',[(0,.07,0,0),(.035,.11,0,0),(.11,.12,.009,0),(.21,.086,-.008,0),(.285,.025,0,0),(.305,.018,0,0)],base,10,.24,11)
        rings('Tied neck',[(.287,.024,0,0),(.302,.022,0,0)],detail,8)
        rings('Gathered top',[(.303,.018,0,0),(.35,.044,-.008,.006),(.36,.024,.002,0)],base,7,.25,15)
        for x in (-1,1):mesh('Purple facet',[(x*.02,-.018,.29),(x*.065,-.07,.20),(x*.04,-.09,.12)],[(0,1,2)],detail)
    else:
        base=mat('Recycling_Graphite',(.038,.053,.07),.67,.36)
        kind=name.replace('RecycleStation','');color={'Plastic':(.015,.75,.08),'Paper':(.005,.20,1),'Metal':(1,.22,.008)}[kind]
        detail=mat('Recycling_'+kind+'Light',color,.12,.28,1.6);station(kind,base,detail)
    # Join to one renderer, preserve two material slots, bake transforms and triangulate.
    bpy.ops.object.select_all(action='DESELECT')
    for o in parts:o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name+'_Final'
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    lo=min(v.co.z for v in o.data.vertices)
    for v in o.data.vertices:v.co.z-=lo
    mod=o.modifiers.new('Final triangulation','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=mod.name)
    bm=bmesh.new();bm.from_mesh(o.data)
    # Clamped one-segment bevels can leave collapsed corner triangles on thin LEDs.
    bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.calc_area()<1e-12],context='FACES')
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free();o.data.update()
    assert all(p.area>1e-12 for p in o.data.polygons),name+' degenerate face'
    tris=len(o.data.polygons);assert tris< (2500 if 'Station' in name else 800),(name,tris)
    h=max(v.co.z for v in o.data.vertices);dims=list(o.dimensions)
    entry={'asset':o.name,'triangles':tris,'materials':sorted({m.name for m in o.data.materials}),'dimensions_blender_m':dims,'height_m':h,'pivot':'ground origin','mesh_renderers':1,'transforms_applied':True}
    stats.append(entry)
    out=FBX/(o.name+'.fbx')
    bpy.ops.export_scene.fbx(filepath=str(out),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,bake_anim=False)
    camera=studio(scene,max(dims),h,12,640)
    bpy.context.preferences.filepaths.save_version=0
    bpy.context.preferences.filepaths.file_preview_type='NONE'
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(o.name+'.blend')),compress=True)
    scene.render.filepath=str(RENDER/(o.name+'.png'))
    if not Path(scene.render.filepath).exists() or (name=='PlasticRecycleStation' and '--refresh-plastic' in sys.argv):bpy.ops.render.render(write_still=True)
    # Actual FBX round trip: metric scale, grounding and applied transforms.
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(out));imported=[q for q in scene.objects if q.type=='MESH'];assert len(imported)==1
    q=imported[0];verts=[q.matrix_world@v.co for v in q.data.vertices]
    bounds=[max(v[i] for v in verts)-min(v[i] for v in verts) for i in range(3)]
    assert abs(bounds[2]-h)<.0001,(name,bounds,h)
    assert abs(min(v.z for v in verts))<.0001
    entry['fbx_roundtrip']='PASS';entry['fbx_bytes']=out.stat().st_size
    print('RECYCLING_ASSET',json.dumps(entry),flush=True)
(SOURCE/'Recycling_Stats.json').write_text(json.dumps(stats,indent=2),encoding='utf8')
print('RECYCLING_SUCCESS',flush=True)
