using UnityEngine;
using RiftHuntersAR.AR;

namespace RiftHuntersAR.Recycling
{
    public static class StationPlacement
    {
        // Search the same surface for a complete, non-overlapping row. Never
        // independently clamp its three centres: that collapses rows at AR edges.
        public static bool TryFind(SueloCombateAR floor,Vector3 portal,Vector3 spawn,Camera camera,out Vector3[] positions,out Quaternion rotation,out float scale)
        {
            positions=null;rotation=Quaternion.identity;scale=1;
            Vector3 up=floor!=null?floor.transform.up:Vector3.up;
            Vector3 toward=Vector3.ProjectOnPlane(camera.transform.position-portal,up).normalized;
            if(toward.sqrMagnitude<.1f)toward=-Vector3.ProjectOnPlane(camera.transform.forward,up).normalized;
            Vector3 right=Vector3.Cross(up,toward).normalized;
            var wastes=Object.FindObjectsByType<RecyclableItem>(FindObjectsSortMode.None);
            // First try side-by-side near the portal, then around it; slight scale
            // adaptation only (82 cm -> at least 73.8 cm).
            foreach(float size in new[]{1f,.95f,.90f})
            foreach(float spacing in new[]{.55f,.50f})
            foreach(int step in new[]{3,9,6,2,10,4,8,0,1,11,5,7})
            foreach(float distance in new[]{.9f,1.2f,1.5f,1.8f,2.1f})
            {
                float angle=step*30*Mathf.Deg2Rad;
                Vector3 center=portal+(toward*Mathf.Cos(angle)+right*Mathf.Sin(angle))*distance;
                if(floor!=null)center=floor.Apoyar(center);else center.y=portal.y;
                Vector3 forward=Vector3.ProjectOnPlane(camera.transform.position-center,up).normalized;
                if(forward.sqrMagnitude<.1f)continue;
                // Screen-left to screen-right as viewed by the player, not the
                // station's own local-right (which faces the player).
                Vector3 side=Vector3.Cross(forward,up).normalized;
                var candidate=new Vector3[3];bool valid=true;
                for(int i=0;i<3 && valid;i++)
                {
                    Vector3 p=center+side*((i-1)*spacing);candidate[i]=p;
                    Vector3 view=camera.WorldToViewportPoint(p+up*.35f);
                    if(view.z<=.1f || Vector3.ProjectOnPlane(camera.transform.position-p,up).magnitude<.65f
                        || Vector3.ProjectOnPlane(portal-p,up).magnitude<.62f
                        || Vector3.ProjectOnPlane(spawn-p,up).magnitude<.48f) {valid=false;break;}
                    for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                        if(floor!=null && !floor.Contiene(p+side*(x*.238f*size)+forward*(z*.22f*size)))valid=false;
                    // Do not hide selectable waste behind a tall station. Test the
                    // camera-to-residue segment against a conservative prop bounds.
                    var footprint=new Bounds(p+up*(.41f*size),new Vector3(.59f*size,.82f*size,.59f*size));
                    foreach(var item in wastes)
                    {
                        if(!item.Classifiable)continue;
                        Vector3 target=item.GetComponent<BoxCollider>().bounds.center;
                        var ray=new Ray(camera.transform.position,target-camera.transform.position);
                        if(footprint.IntersectRay(ray,out float hit) && hit<Vector3.Distance(camera.transform.position,target)) {valid=false;break;}
                    }
                }
                if(valid){positions=candidate;rotation=Quaternion.LookRotation(forward,up);scale=size;return true;}
            }
            return false; // Ask for more scanned space instead of floating/overlapping props.
        }
    }
}
