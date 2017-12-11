using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDRayTracer
{
    /// <summary>
    /// ray tracer feature component
    /// </summary>
    public class H1RayTracerComponent : H1Component
    {
        public H1RayTracerComponent(H1Entity InEntity)
            : base(InEntity)
        {

        }

        public override void Activate()
        {
            // when it activate the component, add its component to the ray tracer system
            H1RayTracerSystem.Singleton.AddComponent(this);
        }

        public override void Deactivate()
        {
            // when it Deactivate the component, remove it from the ray tracer system
            H1RayTracerSystem.Singleton.RemoveComponent(this);
        }

        public override void Initialize()
        {

        }

        public override void Destroy()
        {

        }

        // @TODO - it just temporary, need to think where to move
        public void CreateBoxBound(H1Vector3 InPosition, H1Vector3 InExtent)
        {
            Bound = new H1Bound(H1Bound.EBoundType.Box, InPosition, InExtent);
        }

        public bool IsCollide(H1Ray InRay, out float T0, out float T1)
        {
            return H1PrimitiveIntersection.Intersect(InRay, Bound, out T0, out T1);
        }

        // @TODO - temporary we put all data related ray-tracer in here, but we need to seperate out
        H1Bound Bound;
    }
}
