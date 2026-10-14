import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import Asset,mat
from armas_comun import receiver,grip
a=Asset('EnergyLauncher_Final','tech')
orange=mat('EnemyEnergy',(1,.06,.003),.05,.35,2.5);a.mats.append(orange)
receiver(a,'HeavyReceiver',[(-.45,.42),(-.55,.73),(-.28,.98),(.55,.93),(.77,.65),(.46,.40)],.57)
grip(a,.32,.49,True)
a.ring('LargeChamber',(0,-.50,.68),.38,.285,.35,a.dark,'Barrel',16)
a.ring('FrontBlastArmor',(0,-.78,.68),.395,.29,.17,a.dark,'Barrel',16)
a.ring('FrontMetalLip',(0,-.89,.68),.367,.307,.046,a.light,'Barrel',16)
a.ring('OrangeEmissionRing',(0,-.92,.68),.302,.255,.023,orange,'EnergyChamber',16)
a.ico('RecessedCharge',(0,-.58,.68),(.22,.065,.22),orange,'EnergyChamber',2)
for side in (-1,1):
    a.plate(f'HeavySidePlate_{side}',(side*.28,-.24,.78),.47,.92,.16,(0,side*.94,0),'Receiver',True)
    a.beam(f'BarrelStabilizer_{side}',(side*.31,.24,.82),(side*.31,-.78,.86),.062,.052,a.light,'Barrel',6)
    a.plate(f'MuzzleFin_{side}',(side*.34,-.66,.87),.28,.46,.10,(0,side*.35,0),'Barrel',True)
    a.beam(f'CyanSafetyStrip_{side}',(side*.294,.33,.64),(side*.294,.56,.64),.019,.019,a.energy,'Indicator',4)
    for j in range(5):
        a.beam(f'HeavyCoolingVent_{side}_{j}',(side*.315,.21+j*.08,.54),(side*.315,.21+j*.08,.75),.022,.022,a.light,'Receiver',4)
    a.beam(f'CarryHandleSide_{side}',(side*.07,-.05,.97),(side*.07,.02,1.14),.045,.045,a.dark,'Handle',6)
    a.beam(f'CarryHandleRear_{side}',(side*.07,.48,.97),(side*.07,.43,1.14),.045,.045,a.dark,'Handle',6)
    a.beam(f'CarryHandleTop_{side}',(side*.07,.02,1.14),(side*.07,.43,1.14),.043,.043,a.light,'Handle',6)
receiver(a,'ReinforcedRearStock',[(.46,.48),(.90,.46),(.98,.62),(.87,.83),(.49,.79)],.48,'Stock')
a.finish(.42,weapon=True)
