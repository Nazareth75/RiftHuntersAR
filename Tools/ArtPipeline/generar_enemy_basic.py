"""EnemyBasic procedural visual pipeline. Run with Blender --background --factory-startup --python this_file.
No Unity imports or gameplay edits. Coordinates: X lateral, -Y front, Z up. Static approval model.
"""
import bpy, math, json, argparse, sys
from pathlib import Path
from mathutils import Vector
ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser()
parser.add_argument("--samples", type=int, default=32)
parser.add_argument("--resolution", type=int, default=1000)
parser.add_argument("--revision", type=int, default=2)
args = parser.parse_args(sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else [])
SOURCE=ROOT/"ArtSource/EnemyBasic"
EXPORT=ROOT/"ArtExports/EnemyBasic"
RENDERS=ROOT/"ArtRenders/EnemyBasic"
for p in (SOURCE,EXPORT,RENDERS): p.mkdir(parents=True,exist_ok=True)
# Work scene is this isolated factory-startup Blender process, never another user's scene.
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.unit_settings.system="METRIC"
scene.unit_settings.scale_length=1.0
bpy.context.preferences.filepaths.save_version=0
bpy.context.preferences.filepaths.file_preview_type="NONE"

def material(name,color,metallic=0,rough=.4,emission=0):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*color,1)
    p.inputs["Metallic"].default_value=metallic
    p.inputs["Roughness"].default_value=rough
    if emission:
        p.inputs["Emission Color"].default_value=(*color,1)
        p.inputs["Emission Strength"].default_value=emission
    return m
dark=material("MetalDark",(.009,.013,.019),.28,.43)
light=material("MetalLight",(.17,.20,.24),.60,.37)
energy=material("EnemyEnergy",(1,.060,.003),.05,.35,2.5)
materials=[dark,light,energy]
model=bpy.data.objects.new("EnemyBasic_Final",None)
scene.collection.objects.link(model)
groups={}
for name in ("Body","Armor","Head","Core","Leg_FL","Leg_FR","Leg_BL","Leg_BR"):
    ob=bpy.data.objects.new(name,None);scene.collection.objects.link(ob);ob.parent=model;groups[name]=ob

def mesh(name,verts,faces,mat,group,face_mats=None):
    data=bpy.data.meshes.new(name+"_Mesh");data.from_pydata(verts,[],faces);data.update()
    ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);ob.parent=groups[group]
    mats=mat if isinstance(mat,list) else [mat]
    for m in mats: data.materials.append(m)
    if face_mats:
        for i,p in enumerate(data.polygons): p.material_index=face_mats[i%len(face_mats)]
    return ob

def ico(name,loc,scale,mat,group,sub=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=loc)
    ob=bpy.context.object;ob.name=name;ob.scale=scale;ob.data.materials.append(mat);ob.parent=groups[group]
    return ob

def beam(name,start,end,r1,r2,mat,group,n=6):
    a,b=Vector(start),Vector(end);d=(b-a).normalized()
    tangent=d.cross(Vector((0,1,0)))
    if tangent.length<.01:tangent=d.cross(Vector((1,0,0)))
    tangent.normalize();other=d.cross(tangent).normalized()
    verts=[]
    for pt,r in ((a,r1),(b,r2)):
        for i in range(n):
            theta=2*math.pi*i/n
            verts.append(tuple(pt+r*(math.cos(theta)*tangent+math.sin(theta)*other)))
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return mesh(name,verts,faces,mat,group)

def plate(name,loc,width,length,depth,rot,group="Armor",accent=False):
    outline=[(-.48,-.32),(-.23,-.56),(.27,-.5),(.55,-.12),(.39,.4),(-.05,.55),(-.48,.28)]
    verts=[(x*width,y*length,0) for x,y in outline]
    verts += [(x*width*.78,y*length*.8,depth) for x,y in outline]
    verts += [(0,-length*.05,depth*1.16)]
    n=len(outline)
    faces=[tuple(range(n-1,-1,-1))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    faces += [(n+i,n+(i+1)%n,2*n) for i in range(n)]
    # Secondary steel only on selected beveled edges, not every facet.
    idx=[0]+[1 if accent and i in (0,3) else 0 for i in range(n)]+[0]*n
    ob=mesh(name,verts,faces,[dark,light],group,idx)
    ob.location=loc;ob.rotation_euler=rot
    return ob

# Broad low torso: faceted internal shell rather than stacked boxes.
ico("Body_InternalChassis",(0,.1,1.02),(.84,1.05,.39),dark,"Body",2)
ico("Rear_PowerHousing",(0,.77,.95),(.59,.5,.32),dark,"Body",1)
beam("Underslung_Spine",(0,-.75,.75),(0,.95,.77),.22,.15,dark,"Body",8)
# Layered roof scales, paired shoulder armor, jagged outer skirt.
for row,y in enumerate((-.62,-.13,.38,.82)):
    for sign in (-1,1):
        plate(f"DorsalScale_{row}_{sign}",(sign*.34,y,1.29-row*.035),
              .73,.68,.13,(0,sign*.23,sign*.10),accent=(row%2==0))
        plate(f"FlankPlate_{row}_{sign}",(sign*.75,y,1.01),
              .61,.66,.12,(0,sign*.98,sign*.06),accent=(row==0))
for sign in (-1,1):
    plate(f"ShoulderShield_{sign}",(sign*.71,-.73,1.16),.85,.85,.19,
          (0,sign*.48,sign*-.28),accent=True)
    # Large diagonal shoulder horns + smaller armored protrusions.
    beam(f"ShoulderSpike_{sign}",(sign*.78,-.51,1.33),(sign*1.15,-.30,1.63),.17,.012,dark,"Armor",5)
    beam(f"ShoulderSteelTip_{sign}",(sign*1.05,-.36,1.55),(sign*1.15,-.30,1.63),.055,.005,light,"Armor",5)
    beam(f"RibSpike_{sign}",(sign*.81,.24,1.16),(sign*1.06,.49,1.41),.105,.004,dark,"Armor",5)
# Asymmetric-height spinal armor silhouette, repeated directional fins.
for i,(y,h) in enumerate(((-.4,1.67),(.03,1.59),(.49,1.47),(.86,1.34))):
    mesh(f"SpineFin_{i}",[(-.11,y-.18,1.27),(.11,y-.18,1.27),(-.09,y+.22,1.22),
        (.09,y+.22,1.22),(0,y+.12,h)],[(0,1,4),(1,3,4),(3,2,4),(2,0,4),(0,2,3,1)],
        dark,"Armor")
# Integrated wide predator skull. Furnace core in armored face, not dog snout.
ico("Head_ArmoredSkull",(0,-.98,1.03),(.55,.54,.38),dark,"Head",1)
for sign in (-1,1):
    plate(f"CheekGuard_{sign}",(sign*.40,-1.02,1.13),.47,.68,.13,
          (.13,sign*.75,sign*.2),"Head",True)
    plate(f"BrowWedge_{sign}",(sign*.21,-1.19,1.28),.49,.54,.115,
          (.08,sign*.18,sign*-.20),"Head",True)
    beam(f"TempleHorn_{sign}",(sign*.43,-.91,1.26),(sign*.60,-.75,1.63),.145,.005,dark,"Head",5)
    beam(f"Mandible_{sign}",(sign*.40,-1.16,.94),(sign*.28,-1.52,.69),.13,.035,dark,"Head",5)
    beam(f"Fang_{sign}",(sign*.27,-1.41,.79),(sign*.23,-1.52,.55),.055,.002,light,"Head",5)
    # Narrow angular energy eyes: restrained accent distinct from central furnace.
    mesh(f"EyeSlit_{sign}",[(sign*.13,-1.415,1.22),(sign*.37,-1.34,1.22),
         (sign*.29,-1.385,1.13)],[(0,1,2)],energy,"Core")
# Octagonal beveled aperture around exposed crystal, aligned in front X/Z plane.
verts=[]
for y,r in ((-1.35,.31),(-1.46,.285),(-1.455,.216),(-1.29,.206)):
    for i in range(8):
        t=2*math.pi*i/8
        verts.append((r*math.cos(t),y,.94+r*math.sin(t)))
faces=[];indices=[]
for ring in range(3):
    for i in range(8):
        faces.append((ring*8+i,ring*8+(i+1)%8,(ring+1)*8+(i+1)%8,(ring+1)*8+i))
        indices.append(1 if ring==1 and i in (1,2,5) else 0)
mesh("Face_CoreAperture",verts,faces,[dark,light],"Head",indices)
ico("Core_FurnaceCrystal",(0,-1.405,.94),(.211,.106,.211),energy,"Core",1)
# Broken obsidian facets cut across the furnace, echoing the reference's cracked face.
mesh("Core_FractureA",[(-.155,-1.511,.99),(-.025,-1.521,1.09),(.02,-1.522,.955)],
     [(0,1,2)],dark,"Head")
mesh("Core_FractureB",[(.06,-1.518,.81),(.14,-1.515,.88),(-.01,-1.522,.94)],
     [(0,1,2)],dark,"Head")
for sign in (-1,1):
    beam(f"BrowHeatFissure_{sign}",(sign*.12,-1.415,1.28),(sign*.045,-1.19,1.39),
         .014,.008,energy,"Core",4)
# Energy fissures across shoulders: very small, geometric, no texture dependency.
for sign in (-1,1):
    beam(f"ShoulderEnergySeam_{sign}",(sign*.50,-.6,1.37),(sign*.66,-.40,1.35),
         .018,.012,energy,"Core",4)
    ico(f"HipEnergyPin_{sign}",(sign*.78,.75,.91),(.035,.04,.04),energy,"Core",1)
# Four mechanical splayed limbs, haunches, reverse-jointed shins, three ground claws.
for front in (True,False):
    for sign in (-1,1):
        label=("Leg_F" if front else "Leg_B")+("L" if sign<0 else "R")
        hip=(sign*.64,-.59 if front else .66,.99)
        knee=(sign*1.05,-.68 if front else .94,.65)
        ankle=(sign*.95,-1.08 if front else 1.23,.22)
        foot=(sign*1.02,-1.17 if front else 1.27,.09)
        ico(label+"_Hip",hip,(.23,.25,.23),dark,label,1)
        beam(label+"_UpperStrut",hip,knee,.17,.145,dark,label,6)
        beam(label+"_LowerStrut",knee,ankle,.14,.095,dark,label,6)
        beam(label+"_HydraulicPiston",
             (sign*.86,-.58 if front else .83,.78),ankle,.055,.044,light,label,6)
        ico(label+"_KneeJoint",knee,(.17,.18,.16),light,label,1)
        plate(label+"_UpperArmor",(sign*.89,-.59 if front else .88,.88),
              .44,.56,.11,(.25 if front else -.3,sign*.60,sign*.1),label,True)
        plate(label+"_ShinArmor",(sign*.99,-.89 if front else 1.13,.48),
              .35,.53,.10,(.62 if front else -.56,sign*.1,0),label,False)
        plate(label+"_Foot",foot,.42,.43,.085,(0,0,sign*.12),label,True)
        for c in range(3):
            x=foot[0]+(c-1)*.105
            y=foot[1]-.07
            beam(label+f"_Claw_{c}",(x,y,.095),(x,y-.25,.025),.05,.003,light,label,5)

# Make model 0.60 m tall, including spikes. Feet centered at ground, root pivot at origin.
mesh_objects=[o for o in scene.objects if o.type=="MESH" and o.parent in groups.values()]
bpy.context.view_layer.update()
maxz=max((o.matrix_world@Vector(v)).z for o in mesh_objects for v in o.bound_box)
minz=min((o.matrix_world@Vector(v)).z for o in mesh_objects for v in o.bound_box)
factor=.60/(maxz-minz)
for ob in mesh_objects:
    # Bake object transforms into mesh and normalize in world/model coordinates.
    world=ob.matrix_world.copy()
    for vertex in ob.data.vertices:
        p=world@vertex.co
        vertex.co=Vector((p.x*factor,p.y*factor,(p.z-minz)*factor))
    ob.location=(0,0,0);ob.rotation_euler=(0,0,0);ob.scale=(1,1,1)
# Join by semantic group: 8 named parts, no camera/lights exported.
parts=[]
for name,parent in groups.items():
    objects=[o for o in scene.objects if o.type=="MESH" and o.parent==parent]
    bpy.ops.object.select_all(action="DESELECT")
    for ob in objects: ob.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.object.join()
    part=bpy.context.object;part.name=name+"_Mesh";part.parent=parent
    # Triangulate deterministically to make face budget reproducible.
    modifier=part.modifiers.new("ExportTriangles","TRIANGULATE")
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    parts.append(part)
bpy.context.view_layer.update()
vertices=sum(len(o.data.vertices) for o in parts)
triangles=sum(len(o.data.polygons) for o in parts)
bounds=[o.matrix_world@Vector(v) for o in parts for v in o.bound_box]
dims=[max(p[i] for p in bounds)-min(p[i] for p in bounds) for i in range(3)]
stats={"revision":args.revision,"blender":bpy.app.version_string,"triangles":triangles,
       "vertices":vertices,"materials":[m.name for m in materials],"parts":list(groups),
       "dimensions_blender_xyz_m":dims,"height_m":dims[2],"pivot":"feet, model origin (0,0,0)",
       "front_blender":"-Y","export_axes":"-Z forward / Y up","rigged":False,
       "unity_integration":"pending visual approval","texture_count":0}
(SOURCE/"EnemyBasic_Stats.json").write_text(json.dumps(stats,indent=2),encoding="utf8")
# Isolated studio setup, not exported.
floor=material("PreviewOnly_Floor",(.024,.031,.044),.0,.85)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.008))
ground=bpy.context.object;ground.name="PREVIEW_ONLY_Ground";ground.data.materials.append(floor)
scene.world.color=(.05,.05,.05)
scene.world.use_nodes=True
scene.world.node_tree.nodes["Background"].inputs[0].default_value=(.075,.095,.14,1)
scene.world.node_tree.nodes["Background"].inputs[1].default_value=.35
def area(name,position,power,color,size):
    data=bpy.data.lights.new(name,"AREA");data.energy=power;data.color=color;data.shape="DISK";data.size=size
    ob=bpy.data.objects.new(name,data);scene.collection.objects.link(ob);ob.location=position
    ob.rotation_euler=(Vector((0,0,.28))-ob.location).to_track_quat("-Z","Y").to_euler()
area("PREVIEW_Key",(-1.1,-1.5,1.8),130,(.82,.9,1),1.2)
area("PREVIEW_Fill",(1.5,-.7,.8),65,(1,.66,.43),1.1)
area("PREVIEW_Rim",(.6,1.5,1.4),180,(.32,.6,1),.8)
scene.render.engine="CYCLES";scene.cycles.device="CPU";scene.cycles.samples=args.samples
scene.cycles.use_denoising=True
scene.render.threads_mode="FIXED";scene.render.threads=4
scene.render.resolution_x=args.resolution;scene.render.resolution_y=args.resolution;scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
scene.view_settings.view_transform="AgX"
scene.view_settings.exposure=-.35
camera_data=bpy.data.cameras.new("PREVIEW_Camera")
camera=bpy.data.objects.new("PREVIEW_Camera",camera_data);scene.collection.objects.link(camera);scene.camera=camera
camera_data.type="ORTHO";camera_data.ortho_scale=1.50
views={
 "Perspectiva":((1.15,-1.6,1.03),(0,-.08,.29)),
 "Frente":((0,-2.3,.54),(0,0,.28)),
 "Lateral":((2.3,0,.53),(0,0,.29)),
 "Trasera":((0,2.3,.64),(0,0,.28)),
}
# Save source with useful first camera, then export only model hierarchy.
camera.location=views["Perspectiva"][0]
camera.rotation_euler=(Vector(views["Perspectiva"][1])-camera.location).to_track_quat("-Z","Y").to_euler()
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/"EnemyBasic_Final.blend"),compress=True)
bpy.ops.object.select_all(action="DESELECT");model.select_set(True)
for ob in list(groups.values())+parts:ob.select_set(True)
bpy.context.view_layer.objects.active=model
bpy.ops.export_scene.fbx(filepath=str(EXPORT/"EnemyBasic_Final.fbx"),use_selection=True,
    object_types={"EMPTY","MESH"},axis_forward="-Z",axis_up="Y",global_scale=1,
    apply_unit_scale=True,apply_scale_options="FBX_SCALE_UNITS",use_mesh_modifiers=True,
    add_leaf_bones=False,bake_anim=False,path_mode="AUTO")
# Verify export exists before expensive renders.
assert (EXPORT/"EnemyBasic_Final.fbx").stat().st_size>1000
for name,(position,target) in views.items():
    camera.location=position
    camera.rotation_euler=(Vector(target)-camera.location).to_track_quat("-Z","Y").to_euler()
    scene.render.filepath=str(RENDERS/f"EnemyBasic_{name}.png")
    bpy.ops.render.render(write_still=True)
print("ART_PIPELINE_SUCCESS",json.dumps(stats))
