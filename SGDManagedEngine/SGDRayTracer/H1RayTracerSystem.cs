using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDRayTracer
{
    /// <summary>
    /// system for ray tracing
    /// </summary>
    public class H1RayTracerSystem
    {
        /// <summary>
        /// singlton for H1RayTracerSystem
        /// </summary>
        protected static H1RayTracerSystem Instance;

        public static H1RayTracerSystem Singleton
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new H1RayTracerSystem();
                }
                return Instance;
            }
        }

        // disable creation of ray tracer system
        protected H1RayTracerSystem()
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

        public void RayCast(H1Ray[] InRays, ref H1RenderTarget OutRenderTarget)
        {
            Int32 Index = 0;

            foreach (H1Ray Ray in InRays)
            {
                float T0 = float.MaxValue;
                foreach (H1RayTracerComponent Component in RayTracerComponents)
                {
                    float CurrT0, CurrT1;
                    if (Component.IsCollide(Ray, out CurrT0, out CurrT1))
                    {
                        if (T0 > CurrT0)
                        {
                            OutRenderTarget[Index] = new H1Color(1, 0, 0, 1);
                            T0 = CurrT0;
                        }                        
                    }
                }

                Index++;
            }
        }

        // components which need to be handled by ray tracer render world
        protected List<H1RayTracerComponent> RayTracerComponents = new List<H1RayTracerComponent>();
    }
}
