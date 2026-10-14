import sys, math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import Asset
a=Asset('RiftGuardian_Final')
a.energy.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(1,.012,.003,1)
a.energy.node_tree.nodes.get('Principled BSDF').inputs['Emission Color'].default_value=(1,.012,.003,1)
a.ico('CentralMass',(0,0,1.9),(.75,.42,.85),a.dark,'Body',3)
a.ico('CrownSkull',(0,-.12,2.65),(.40,.34,.43),a.dark,'Head',3)
a.wedge('DescendingObsidianSpear',(0,0,1.55),(0,-.08,.06),.38,a.dark,'Body')
for side in (-1,1):
    for row,z in enumerate((1.35,1.7,2.03,2.36)):
        a.plate(f'RibArmor_{side}_{row}',(side*.40,-.22,z),.70,.56,.17,(1.0,side*.30,side*-.24),'Armor',row%2==0)
        a.wedge(f'RibTalon_{side}_{row}',(side*.56,0,z),(side*.86,.06,z-.33),.16,a.dark,'Armor')
    a.ico(f'SuspendedShoulder_{side}',(side*.93,.0,2.25),(.35,.35,.39),a.dark,'Shoulder_'+str(side),3)
    for j in range(3):
        a.plate(f'PauldronShard_{side}_{j}',(side*(.88+j*.10),-.03,2.41-j*.18),.57,.64,.18,(.15,side*.42,side*.12),'Shoulder_'+str(side),True)
    a.wedge(f'GreatCrownHorn_{side}',(side*.29,-.03,2.8),(side*.57,.07,3.45),.18,a.dark,'Head')
    a.wedge(f'ShoulderHorn_{side}',(side*.96,.06,2.44),(side*1.20,.16,3.10),.18,a.dark,'Shoulder_'+str(side))
    a.beam(f'ArmLink_{side}',(side*1.04,0,2.16),(side*1.34,-.03,1.81),.13,.10,a.dark,'Arm_'+str(side),8)
    a.ico(f'FloatingGauntlet_{side}',(side*1.40,-.02,1.73),(.29,.32,.43),a.dark,'Arm_'+str(side),3)
    for j in range(3):
        a.plate(f'GauntletPlate_{side}_{j}',(side*1.43,-.12,1.98-j*.21),.49,.57,.17,(1.1,side*.2,0),'Arm_'+str(side),True)
    name='WeakPoint_Left' if side<0 else 'WeakPoint_Right'
    a.ring(name+'_Housing',(side*1.40,-.32,1.86),.22,.155,.1,a.dark,'Arm_'+str(side),10)
    a.ico(name+'_Crystal',(side*1.40,-.35,1.86),(.15,.07,.19),a.energy,name,3)
    for claw in range(3):
        a.wedge(f'HandTalon_{side}_{claw}',(side*(1.22+claw*.15),-.14,1.45),(side*(1.23+claw*.17),-.25,1.07),.065,a.dark,'Arm_'+str(side))
    a.mesh(f'FaceEye_{side}',[(side*.07,-.43,2.75),(side*.28,-.38,2.69),(side*.17,-.46,2.61)],[(0,1,2)],a.energy,'Head')
    a.beam(f'ChestEnergyVein_{side}',(side*.09,-.48,1.97),(side*.51,-.34,2.27),.022,.012,a.energy,'Body',5)
    for row,z in enumerate((1.35,1.70,2.05,2.36)):
        a.plate(f'RearCarapace_{side}_{row}',(side*.32,.37,z),.64,.59,.15,(-1.08,side*.22,side*.09),'Armor',row%2==0)
    a.plate(f'RearCrownGuard_{side}',(side*.18,.21,2.68),.45,.50,.13,(-1.10,side*.19,0),'Head',True)
    a.plate(f'RearShoulderGuard_{side}',(side*.94,.27,2.28),.65,.69,.16,(-1.0,side*.27,0),'Shoulder_'+str(side),True)
a.ring('CentralCoreArmor',(0,-.38,1.94),.42,.28,.20,a.dark,'Armor',12)
a.ico('CentralRedCrystal',(0,-.46,1.94),(.28,.13,.34),a.energy,'WeakPoint_Core',3)
for j,z in enumerate((1.6,2.0,2.4)):
    a.wedge(f'RearSpinalFin_{j}',(0,.45,z),(0,.78,z+.18),.12,a.dark,'Armor')
# Several disconnected plates and shards remain independently parented for later animation.
for i in range(20):
    theta=2*math.pi*i/20
    x=math.sin(theta)*(1.12 if i%2 else 1.63);z=1.75+math.cos(theta)*1.50
    y=.14+(.14 if i%3 else -.22)
    group=f'FloatingFragment_{i:02d}'
    a.ico(group+'_Rock',(x,y,z),(.14+(i%3)*.025,.12,.23),a.dark,group,1)
    a.wedge(group+'_Spike',(x,y,z+.12),(x*1.06,y+.03,z+.39),.07,a.dark,group)
    if i%4==0:a.ico(group+'_Energy',(x,y-.12,z),(.045,.025,.07),a.energy,group,1)
for i in range(6):
    a.plate(f'LowerFloatingArmor_{i}',((-.35 if i%2 else .35),-.03,1.10-i*.10),.49,.56,.12,(.3,0,i*.35),'Armor',True)
a.finish(1.85)
