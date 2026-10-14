import sys, math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import Asset
a=Asset('EnemyFast_Final')
a.ico('LeanThorax',(0,.02,1.10),(.34,.78,.25),a.dark,'Body',2)
a.ico('Pelvis',(0,.61,1.05),(.27,.32,.21),a.dark,'Body',1)
a.ico('ElongatedSkull',(0,-.83,1.16),(.28,.43,.22),a.dark,'Head',2)
a.beam('JawSpine',(0,-.8,1.03),(0,-1.35,.92),.17,.035,a.dark,'Head')
for side in (-1,1):
    for row,y in enumerate((-.4,-.02,.37,.65)):
        a.plate(f'SweptArmor_{side}_{row}',(side*.23,y,1.29),.40,.59,.085,(0,side*.33,side*-.27),'Armor',row==0)
        a.wedge(f'SweptSpur_{side}_{row}',(side*.32,y,1.28),(side*.49,y+.43,1.42),.08,a.dark,'Armor')
    a.wedge(f'CrownBlade_{side}',(side*.18,-.7,1.29),(side*.35,-.13,1.72),.13,a.dark,'Head')
    a.plate(f'CheekBlade_{side}',(side*.23,-.96,1.19),.27,.57,.08,(.1,side*.7,side*-.2),'Head',True)
    a.mesh(f'Eye_{side}',[(side*.07,-1.22,1.22),(side*.23,-1.18,1.20),(side*.15,-1.235,1.14)],[(0,1,2)],a.energy,'Core')
    a.beam(f'Fang_{side}',(side*.17,-1.2,1.0),(side*.12,-1.38,.79),.035,.004,a.light,'Head',5)
    a.beam(f'HeatVein_{side}',(side*.13,-.65,1.33),(side*.10,-.32,1.34),.012,.008,a.energy,'Core',4)
    for front in (True,False):
        g=('Leg_F' if front else 'Leg_B')+('L' if side<0 else 'R')
        hip=(side*.27,-.4 if front else .57,1.08)
        knee=(side*.56,-.25 if front else .93,.72)
        ankle=(side*.49,-.89 if front else 1.04,.13)
        a.ico(g+'_Joint',hip,(.12,.14,.12),a.dark,g,1)
        a.beam(g+'_Upper',hip,knee,.09,.072,a.dark,g)
        a.beam(g+'_Shin',knee,ankle,.08,.038,a.dark,g)
        a.ico(g+'_KneeJoint',knee,(.095,.095,.085),a.light,g,1)
        a.beam(g+'_Piston',(side*.48,-.31 if front else .83,.7),ankle,.023,.02,a.light,g)
        a.plate(g+'_BladeArmor',(side*.49,-.44 if front else .92,.52),.22,.57,.065,(.7 if front else -.6,side*.3,0),g,True)
        a.plate(g+'_HipArmor',hip,.27,.39,.07,(0,side*.3,0),g)
        for i in range(3):
            x=ankle[0]+(i-1)*.055
            a.beam(g+f'_Claw{i}',(x,ankle[1],.12),(x,ankle[1]-.25,.015),.031,.003,a.light,g,5)
a.ring('CoreCage',(0,-1.245,1.055),.125,.09,.10,a.dark,'Head',8)
a.ico('CoreCrystal',(0,-1.27,1.055),(.09,.04,.09),a.energy,'Core',1)
for j in range(3):
    a.wedge(f'TailFin_{j}',(0,.62+j*.10,1.17),(0,.94+j*.10,1.40-j*.05),.06,a.dark,'Armor')
a.finish(.52)
