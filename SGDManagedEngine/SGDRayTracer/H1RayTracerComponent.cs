using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // @TODO - temporary we put all data related ray-tracer in here, but we need to seperate out
    }
}
