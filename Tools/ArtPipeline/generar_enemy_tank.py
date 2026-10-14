import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import Asset
a=Asset('EnemyTank_Final')
a.ico('MassiveThorax',(0,.10,1.4),(.86,.74,.68),a.dark,'Body',3)
a.ico('RearEngine',(0,.57,1.14),(.69,.57,.48),a.dark,'Body',2)
a.ico('BellyArmorChassis',(0,.05,.96),(.70,.65,.35),a.dark,'Body',2)
a.ico('IntegratedHead',(0,-.62,1.86),(.43,.36,.39),a.dark,'Head',2)
for side in (-1,1):
    for row,y in enumerate((-.48,-.1,.3,.65)):
        a.plate(f'HeavyBackPlate_{side}_{row}',(side*.4,y,1.9-row*.09),.84,.67,.20,(0,side*.34,0),'Armor',row in (0,2))
        a.plate(f'RibBulwark_{side}_{row}',(side*.79,y,1.4),.70,.59,.22,(0,side*1.08,0),'Armor',True)
    a.ico(f'ShoulderMass_{side}',(side*.86,-.47,1.6),(.38,.47,.44),a.dark,'Armor',2)
    a.plate(f'HipSkirt_{side}',(side*.62,.64,1.13),.68,.62,.19,(0,side*.66,0),'Armor',True)
    a.plate(f'ShoulderFortress_{side}',(side*.85,-.6,1.92),.88,.91,.27,(0,side*.48,side*.1),'Armor',True)
    a.wedge(f'ShoulderHorn_{side}',(side*.97,-.2,1.9),(side*1.18,.05,2.22),.19,a.dark,'Armor')
    a.plate(f'FaceGuard_{side}',(side*.29,-.85,1.87),.44,.56,.17,(.3,side*.64,side*.2),'Head',True)
    a.beam(f'FaceTusk_{side}',(side*.32,-.92,1.57),(side*.19,-1.09,1.39),.085,.018,a.light,'Head',5)
    a.mesh(f'Eye_{side}',[(side*.10,-.96,2.0),(side*.29,-.91,2.01),(side*.2,-.99,1.92)],[(0,1,2)],a.energy,'Core')
    # Column legs at the rear; massive braced forearms at front create a heavy gorilla-like silhouette.
    for front in (True,False):
        g=('Leg_F' if front else 'Leg_B')+('L' if side<0 else 'R')
        hip=(side*.82,-.42,1.45) if front else (side*.50,.56,.95)
        knee=(side*1.10,-.66,.76) if front else (side*.58,.7,.5)
        ankle=(side*1.09,-.9,.14) if front else (side*.58,.77,.13)
        a.beam(g+'_UpperMass',hip,knee,.28 if front else .25,.25,a.dark,g,8)
        a.beam(g+'_Forearm',knee,ankle,.30 if front else .22,.20,a.dark,g,8)
        a.ring(g+'_JointCollar',knee,.29,.20,.13,a.light,g,12,axis='Z')
        a.plate(g+'_Shield',(side*(1.10 if front else .58),-.70 if front else .70,.57),.67,.88,.22,(.45 if front else -.15,side*.08,0),g,True)
        a.ico(g+'_Foot',(ankle[0],ankle[1]-.09,.1),(.30,.30,.10),a.dark,g,1)
        for i in range(3):
            x=ankle[0]+(i-1)*.13
            a.beam(g+f'_Toe{i}',(x,ankle[1]-.19,.10),(x,ankle[1]-.39,.025),.07,.015,a.light,g,6)
        a.ico(g+'_EnergyJoint',(knee[0],knee[1]-.25,knee[2]),(.055,.03,.065),a.energy,'Core',1)
a.ring('ProtectedFurnace',(0,-.94,1.73),.30,.19,.18,a.dark,'Head',10)
a.ico('RecessedCore',(0,-.98,1.73),(.18,.065,.18),a.energy,'Core',1)
a.beam('CoreProtectionBar',(-.23,-1.04,1.71),(.23,-1.04,1.71),.034,.034,a.light,'Head',6)
a.plate('ForeheadArmor',(0,-.53,2.12),.59,.64,.16,(.1,0,0),'Head',True)
a.finish(.95)
