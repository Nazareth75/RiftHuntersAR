import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import Asset
from armas_comun import receiver,grip
a=Asset('PlasmaBlaster_Final','tech')
receiver(a,'AngularReceiver',[(-.13,.40),(-.21,.64),(-.10,.78),(.34,.77),(.52,.58),(.37,.39)],.40)
grip(a,.22,.43)
a.beam('CyanPowerCell',(0,-.12,.59),(0,-.65,.59),.135,.135,a.energy,'PowerCell',12)
a.ring('BreechCollar',(0,-.10,.59),.21,.13,.10,a.dark,'Barrel',12)
a.ring('MuzzleArmor',(0,-.72,.59),.235,.16,.17,a.dark,'Barrel',12)
a.ring('MuzzleSteel',(0,-.82,.59),.217,.175,.055,a.light,'Barrel',12)
a.ring('CyanAperture',(0,-.86,.59),.175,.143,.024,a.energy,'PowerCell',12)
for side in (-1,1):
    a.plate(f'SideShroud_{side}',(side*.18,.15,.67),.24,.50,.08,(0,side*1.1,0),'Receiver',True)
    a.beam(f'UpperRail_{side}',(side*.16,.21,.79),(side*.16,-.66,.78),.039,.03,a.light,'Receiver',6)
    for j in range(3):a.beam(f'CellBand_{side}_{j}',(side*.132,-.26-j*.13,.52),(side*.132,-.26-j*.13,.68),.018,.018,a.dark,'Barrel',5)
    for j in range(4):a.beam(f'SideCoolingFin_{side}_{j}',(side*.227,.02+j*.085,.52),(side*.227,.02+j*.085,.64),.015,.015,a.light,'Receiver',4)
receiver(a,'RearBattery',[ (.36,.45),(.59,.48),(.63,.64),(.49,.70),(.35,.64)],.30,'Receiver')
a.plate('TopSight',(0,.15,.82),.16,.31,.065,(0,0,0),'Sight',True)
a.finish(.30,weapon=True)
