import sys, math
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent))
from arte_comun import Asset, mat
a=Asset('Portal_Final','tech')
rift=mat('RiftEnergy',(.006,.19,1),.05,.35,3);a.mats.append(rift)
# Irregular extruded annulus sections; individual pieces remain separately pivoted.
for i in range(22):
    t=2*math.pi*i/22;half=math.pi/22*.79
    outer=1.12+.07*math.sin(i*2.37);inner=.84+.025*math.cos(i*1.9)
    outline=[]
    for theta,r in ((t-half,inner),(t-half,outer),(t,outer+.07),(t+half,outer),(t+half,inner),(t,inner-.025)):
        outline.append((r*math.cos(theta),r*math.sin(theta)+1.47))
    y=.045*math.sin(i*1.3);depth=.15+.035*(i%3)
    verts=[(x,y+dy,z) for dy in (-depth/2,depth/2) for x,z in outline]
    faces=[tuple(range(5,-1,-1)),tuple(range(6,12))]+[(j,(j+1)%6,(j+1)%6+6,j+6) for j in range(6)]
    a.mesh(f'ObsidianRingSegment_{i:02d}',verts,faces,[a.dark,a.light],'Portal_Frame',[0,0,0,1,0,0,0,0])
    for j in range(2):
        u=t-half+j*half
        v=u+half*.80
        p=(inner*math.cos(u),y-.11,inner*math.sin(u)+1.47)
        q=(inner*math.cos(v),y-.11,inner*math.sin(v)+1.47)
        a.beam(f'InnerBlueArc_{i}_{j}',p,q,.022,.018,rift,'Portal_Frame',5)
    if i%2==0:
        p=(outer*math.cos(t),y,outer*math.sin(t)+1.47)
        q=((outer+.29)*math.cos(t+.07),y+.08,(outer+.29)*math.sin(t+.07)+1.47)
        a.wedge(f'FrameSpear_{i}',p,q,.095,a.dark,'Portal_Frame')
for i in range(14):
    t=2*math.pi*i/14+.11;r=1.4+.06*math.sin(i*2.0)
    x,z=r*math.cos(t),1.47+r*math.sin(t)
    group=f'Portal_Fragments_{i:02d}'
    o=a.ico(group+'_Shard',(x,.12*math.sin(i),z),(.10,.085,.22),a.dark,group,1)
    o.rotation_euler=(0,-t+.3,.3)
    if i%3==0:a.ico(group+'_EnergyEdge',(x,-.08,z),(.025,.02,.095),rift,group,1)
# Static ribbon spiral is reference geometry; animation/particles deliberately absent.
for arm in range(3):
    verts=[]
    for j in range(49):
        u=j/48;theta=arm*2*math.pi/3+u*math.pi*3.4;r=.83*(1-u)+.02
        w=.042*(1-u)+.012
        for delta in (-w,w):verts.append(((r+delta)*math.cos(theta),-.04-.06*math.sin(u*math.pi),1.47+(r+delta)*math.sin(theta)))
    a.mesh(f'SpiralRibbon_{arm}',verts,[(2*j,2*j+1,2*j+3,2*j+2) for j in range(48)],rift,'Portal_CoreReference')
a.ico('VortexSeed',(0,-.08,1.47),(.12,.07,.12),a.energy,'Portal_CoreReference',2)
a.finish(1.25)
