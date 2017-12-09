using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDRayTracer
{
    /// <summary>
    /// system for ray tracing
    /// </summary>
    public class H1RayTracerSystem
    {
        public H1RayTracerSystem()
        {

        }

        public void AddComponent(H1RayTracerComponent InComponent)
        {
            H1RayTracerComponent Component = RayTracerComponents.Find((e) => { return InComponent.Descriptor.Id == e.Descriptor.Id; });
            if (Component == null)
            {
                // making sure that there is no overlapped adding on the list
                RayTracerComponents.Add(InComponent);
            }            
        }

        public void RemoveComponent(H1RayTracerComponent InComponent)
        {
            // making sure that there is no dangling
            H1RayTracerComponent Component = RayTracerComponents.Find((e) => { return InComponent.Descriptor.Id == e.Descriptor.Id; });
            if (Component != null)
            {
                RayTracerComponents.Remove(InComponent);
            }            
        }

        // components which need to be handled by ray tracer render world
        protected List<H1RayTracerComponent> RayTracerComponents = new List<H1RayTracerComponent>();
    }
}
