import sys,math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import Asset
from armas_comun import receiver,grip
a=Asset('ElectricCannon_Final','tech')
receiver(a,'GeneratorReceiver',[(-.30,.39),(-.36,.66),(-.10,.83),(.47,.76),(.62,.57),(.39,.39)],.43)
grip(a,.29,.43)
a.beam('AxialEnergyChamber',(0,-.26,.60),(0,-1.07,.60),.105,.085,a.energy,'EnergyChamber',12)
for j in range(5):
    y=-.28-j*.17;r=.22-.014*j
    a.ring(f'ConductorCoil_{j}',(0,y,.60),r,r-.035,.045,a.light,'Coils',12)
    a.ring(f'BlueCoilInsulator_{j}',(0,y-.025,.60),r-.038,r-.052,.02,a.energy,'EnergyChamber',12)
for j in range(4):
    theta=math.pi/4+j*math.pi/2;x=.18*math.cos(theta);z=.60+.18*math.sin(theta)
    a.beam(f'LongConductor_{j}',(x,-.22,z),(x,-1.06,z),.028,.018,a.dark,'BarrelCage',6)
    a.beam(f'ConductorTip_{j}',(x,-1.02,z),(x*.72,-1.21,.6+(z-.6)*.72),.023,.01,a.light,'BarrelCage',6)
a.ring('OpenEmitter',(0,-1.12,.6),.163,.12,.06,a.dark,'BarrelCage',12)
for side in (-1,1):
    a.ico(f'SideCapacitor_{side}',(side*.24,.12,.6),(.09,.18,.10),a.dark,'Generator',2)
    a.ico(f'CapacitorIndicator_{side}',(side*.323,.10,.6),(.015,.07,.035),a.energy,'Generator',1)
    a.plate(f'GeneratorPlate_{side}',(side*.20,.34,.69),.28,.43,.09,(0,side*.9,0),'Receiver',True)
    for j in range(4):
        a.beam(f'GeneratorCoolingRib_{side}_{j}',(side*.24,.22+j*.07,.46),(side*.24,.22+j*.07,.64),.016,.016,a.light,'Generator',4)
    a.beam(f'VisibleEnergyBus_{side}',(side*.23,-.14,.56),(side*.30,.05,.52),.017,.017,a.energy,'Generator',5)
receiver(a,'TopConductiveBridge',[(-.17,.78),(-.12,.94),(.19,.94),(.25,.80)],.21,'Generator')
a.ring('BridgeCoolingVent',(0,-.15,.87),.067,.036,.09,a.light,'Generator',8)
a.finish(.36,weapon=True)
