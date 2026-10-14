"""Angular gun receiver and grip helpers; no firing/weapon logic."""
def receiver(a,name,profile,width,group='Receiver'):
    verts=[(x,y,z) for x in (-width/2,width/2) for y,z in profile];n=len(profile)
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    return a.mesh(name,verts,faces,[a.dark,a.light],group,[0,0,0,1,0,0,1,0,0,0])
def grip(a,y=.2,z=.45,heavy=False):
    profile=[(y-.11,z),(y+.10,z+.01),(y+.21,.16),(y+.15,.07),(y-.02,.07),(y-.03,.18)]
    receiver(a,'AngledGrip',profile,.19 if heavy else .14,'Grip')
    for j in range(4):a.beam(f'GripGroove_{j}',(-.085,y+.075,.15+j*.05),(.085,y+.075,.15+j*.05),.009,.009,a.light,'Grip',4)
    points=[(0,y-.12,z-.03),(0,y-.26,z-.10),(0,y-.21,z-.28),(0,y+.04,z-.28)]
    for j in range(3):a.beam(f'TriggerGuard_{j}',points[j],points[j+1],.025,.025,a.dark,'Grip',6)
    a.beam('Trigger',(0,y-.10,z-.04),(0,y-.09,z-.15),.012,.012,a.light,'Grip',5)
