"""Shared low-poly mesh, material, export, studio and quality helpers. Blender-only."""
import bpy, bmesh, math, json, sys, argparse
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
def options():
    p=argparse.ArgumentParser();p.add_argument('--samples',type=int,default=16);p.add_argument('--resolution',type=int,default=800)
    return p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
def mat(name,color,metal=.28,rough=.43,emission=0):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*color,1)
    n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=rough
    n.inputs['Emission Color'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=emission
    return m
class Asset:
    def __init__(self,name,family='enemy'):
        bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
        self.name=name;self.groups={};self.family=family;self.args=options()
        self.root=bpy.data.objects.new(name,None);bpy.context.scene.collection.objects.link(self.root)
        if family=='enemy':
            self.dark=mat('MetalDark',(.009,.013,.019));self.light=mat('MetalLight',(.17,.20,.24),.6,.37)
            self.energy=mat('EnemyEnergy',(1,.06,.003),.05,.35,2.5)
        else:
            self.dark=mat('TechMetalDark',(.009,.013,.019));self.light=mat('TechMetalLight',(.17,.20,.24),.6,.37)
            self.energy=mat('PlayerEnergy',(.005,.35,1),.05,.35,2.5)
        self.mats=[self.dark,self.light,self.energy]
    def group(self,name):
        if name not in self.groups:
            ob=bpy.data.objects.new(name,None);bpy.context.scene.collection.objects.link(ob);ob.parent=self.root;self.groups[name]=ob
        return self.groups[name]
    def mesh(self,name,verts,faces,material,group,indices=None):
        data=bpy.data.meshes.new(name+'_Mesh');data.from_pydata(verts,[],faces);data.update()
        ob=bpy.data.objects.new(name,data);bpy.context.scene.collection.objects.link(ob);ob.parent=self.group(group)
        mats=material if isinstance(material,list) else [material]
        for m in mats:data.materials.append(m)
        if indices:
            for i,p in enumerate(data.polygons):p.material_index=indices[i%len(indices)]
        return ob
    def ico(self,name,loc,scale,material,group,sub=1):
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=loc)
        o=bpy.context.object;o.name=name;o.scale=scale;o.data.materials.append(material);o.parent=self.group(group);return o
    def beam(self,name,start,end,r1,r2,material,group,n=6):
        a,b=Vector(start),Vector(end);d=(b-a).normalized();t=d.cross(Vector((0,1,0)))
        if t.length<.01:t=d.cross(Vector((1,0,0)))
        t.normalize();u=d.cross(t).normalized();v=[]
        for p,r in ((a,r1),(b,r2)):
            for i in range(n):v.append(tuple(p+r*(math.cos(2*math.pi*i/n)*t+math.sin(2*math.pi*i/n)*u)))
        f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
        return self.mesh(name,v,f,material,group)
    def plate(self,name,loc,w,l,h,rot=(0,0,0),group='Armor',steel=False):
        outline=[(-.48,-.32),(-.23,-.56),(.27,-.5),(.55,-.12),(.39,.4),(-.05,.55),(-.48,.28)]
        v=[(x*w,y*l,0) for x,y in outline]+[(x*w*.78,y*l*.8,h) for x,y in outline]+[(0,-l*.05,h*1.16)]
        n=7;f=[tuple(range(n-1,-1,-1))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]+[(n+i,n+(i+1)%n,2*n) for i in range(n)]
        idx=[0]+[1 if steel and i in (0,3) else 0 for i in range(n)]+[0]*n
        o=self.mesh(name,v,f,[self.dark,self.light],group,idx);o.location=loc;o.rotation_euler=rot;return o
    def ring(self,name,center,outer,inner,depth,material,group,n=12,axis='Y'):
        v=[]
        for axial,r in ((-depth/2,outer),(depth/2,outer),(-depth/2,inner),(depth/2,inner)):
            for i in range(n):
                t=2*math.pi*i/n;x,z=r*math.cos(t),r*math.sin(t)
                q=(x,axial,z) if axis=='Y' else (x,z,axial)
                v.append(tuple(Vector(center)+Vector(q)))
        f=[]
        for i in range(n):
            j=(i+1)%n;f.extend(((i,j,j+n,i+n),(i+2*n,i+3*n,j+3*n,j+2*n),(i,i+2*n,j+2*n,j),(i+n,j+n,j+3*n,i+3*n)))
        return self.mesh(name,v,f,material,group)
    def wedge(self,name,start,end,width,material,group):
        a=Vector(start);b=Vector(end)
        v=[tuple(a+Vector((x*width,y*width,z*width))) for x,y,z in ((-1,-.5,0),(1,-.5,0),(1,.5,0),(-1,.5,0),(0,0,.5))]+[tuple(b)]
        return self.mesh(name,v,[(0,1,4),(1,2,4),(2,3,4),(3,0,4),(0,5,1),(1,5,2),(2,5,3),(3,5,0)],material,group)
    def finish(self,height,weapon=False):
        scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
        bpy.context.preferences.filepaths.save_version=0;bpy.context.preferences.filepaths.file_preview_type='NONE'
        objects=[o for o in scene.objects if o.type=='MESH' and o.parent in self.groups.values()]
        bpy.context.view_layer.update();bounds=[o.matrix_world@v.co for o in objects for v in o.data.vertices]
        lo=min(p.z for p in bounds);hi=max(p.z for p in bounds);factor=height/(hi-lo)
        for o in objects:
            world=o.matrix_world.copy()
            for v in o.data.vertices:
                p=world@v.co;v.co=Vector((p.x*factor,p.y*factor,(p.z-lo)*factor))
            o.location=(0,0,0);o.rotation_euler=(0,0,0);o.scale=(1,1,1)
        parts=[]
        for name,parent in self.groups.items():
            obs=[o for o in scene.objects if o.type=='MESH' and o.parent==parent]
            bpy.ops.object.select_all(action='DESELECT')
            for o in obs:o.select_set(True)
            bpy.context.view_layer.objects.active=obs[0]
            if len(obs)>1:bpy.ops.object.join()
            ob=bpy.context.object;ob.name=name+'_Mesh'
            modifier=ob.modifiers.new('Triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=modifier.name)
            bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(ob.data);bm.free();ob.data.update()
            # Independent groups have local pivots for future animation; no rig now.
            center=sum((v.co for v in ob.data.vertices),Vector())/len(ob.data.vertices)
            for v in ob.data.vertices:v.co-=center
            parent.location=center;parts.append(ob)
            assert all(math.isfinite(c) for v in ob.data.vertices for c in v.co)
            assert all(p.area>1e-12 for p in ob.data.polygons), 'Degenerate triangle'
            assert all(abs(p.normal.length-1)<.001 for p in ob.data.polygons)
            assert ob.scale==Vector((1,1,1))
        if self.name=='Portal_Final':
            container=self.group('Portal_Fragments')
            for name,parent in self.groups.items():
                if name.startswith('Portal_Fragments_'):parent.parent=container
        bpy.context.view_layer.update();bounds=[o.matrix_world@Vector(v) for o in parts for v in o.bound_box]
        dims=[max(p[i] for p in bounds)-min(p[i] for p in bounds) for i in range(3)]
        source=ROOT/'ArtSource'/self.name;export=ROOT/'ArtExports'/self.name;renders=ROOT/'ArtRenders'/self.name
        for p in (source,export,renders):p.mkdir(parents=True,exist_ok=True)
        stats={'asset':self.name,'meshes':len(parts),'triangles':sum(len(o.data.polygons) for o in parts),'vertices':sum(len(o.data.vertices) for o in parts),
            'materials':sorted({m.name for o in parts for m in o.data.materials}),'parts':list(self.groups),'dimensions_xyz_m':dims,
            'height_m':height,'pivot':'ground origin; separate part pivots','front_blender':'-Y','fbx_axes':'-Z forward/Y up',
            'rigged':False,'animated':False,'unity_integration':False,'geometry_checks':'finite vertices, positive area triangles, normalized normals, unit mesh scales'}
        (source/(self.name+'_Stats.json')).write_text(json.dumps(stats,indent=2),encoding='utf8')
        camera=studio(scene,max(dims),height,self.args.samples,self.args.resolution)
        bpy.ops.wm.save_as_mainfile(filepath=str(source/(self.name+'.blend')),compress=True)
        bpy.ops.object.select_all(action='DESELECT');self.root.select_set(True)
        for o in list(self.groups.values())+parts:o.select_set(True)
        bpy.context.view_layer.objects.active=self.root
        target=export/(self.name+'.fbx')
        bpy.ops.export_scene.fbx(filepath=str(target),use_selection=True,object_types={'EMPTY','MESH'},axis_forward='-Z',axis_up='Y',
            global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,bake_anim=False)
        assert target.stat().st_size>1000
        render_views(scene,camera,max(dims),height,renders,self.name)
        print('ASSET_SUCCESS',json.dumps(stats),flush=True)
def studio(scene,size,height,samples=16,resolution=800):
    floor=mat('PREVIEW_ONLY_Floor',(.024,.031,.044),0,.85)
    bpy.ops.mesh.primitive_plane_add(size=max(200,size*30),location=(0,0,-.008))
    bpy.context.object.name='PREVIEW_ONLY_Ground';bpy.context.object.data.materials.append(floor)
    scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.075,.095,.14,1)
    scene.world.node_tree.nodes['Background'].inputs[1].default_value=.35
    scale=max(size/.95,.45)
    for name,xyz,power,color,diam in [('Key',(-1.1,-1.5,1.8),130,(.82,.9,1),1.2),('Fill',(1.5,-.7,.8),65,(1,.66,.43),1.1),('Rim',(.6,1.5,1.4),180,(.32,.6,1),.8)]:
        data=bpy.data.lights.new('PREVIEW_'+name,'AREA');data.energy=power*scale*scale;data.color=color;data.shape='DISK';data.size=diam*scale
        o=bpy.data.objects.new('PREVIEW_'+name,data);scene.collection.objects.link(o);o.location=Vector(xyz)*scale
        o.rotation_euler=(Vector((0,0,height*.5))-o.location).to_track_quat('-Z','Y').to_euler()
    scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=samples;scene.cycles.use_denoising=True
    scene.render.threads_mode='FIXED';scene.render.threads=4;scene.render.resolution_x=scene.render.resolution_y=resolution;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX';scene.view_settings.exposure=-.35
    data=bpy.data.cameras.new('PREVIEW_Camera');o=bpy.data.objects.new('PREVIEW_Camera',data);scene.collection.objects.link(o);scene.camera=o
    data.type='ORTHO';data.ortho_scale=size*1.45;o.location=Vector((1.15,-1.6,1.03))*scale
    o.rotation_euler=(Vector((0,0,height*.5))-o.location).to_track_quat('-Z','Y').to_euler();return o
def render_views(scene,camera,size,height,path,name):
    views={'Perspectiva':(1.3,-1.8,1.12),'Frente':(0,-2.4,.52),'Lateral':(2.4,0,.52),'Trasera':(0,2.4,.65)}
    for label,xyz in views.items():
        camera.location=Vector(xyz)*size;camera.rotation_euler=(Vector((0,0,height*.5))-camera.location).to_track_quat('-Z','Y').to_euler()
        scene.render.filepath=str(path/(name+'_'+label+'.png'));bpy.ops.render.render(write_still=True)
