using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDRayTracer
{
    /// <summary>
    /// ray tracer render target
    /// </summary>
    public class H1RenderTarget
    {
        public H1RenderTarget(Int32 InWidth, Int32 InHeight)
        {
            Width = InWidth;
            Height = InHeight;

            // allocate pixels in screen
            Pixels = new H1Color[Width * Height];
        }

        public Int32 Width { get; private set; }
        public Int32 Height { get; private set; }

        protected H1Color[] Pixels;
    }

    /// <summary>
    /// abstract class for ray tracer
    /// </summary>
    public abstract class H1RayTracer
    {
        public enum ERayTracerType
        {
            CPU_RayTracer,
            GPU_RayTracer
        }

        // properties
        protected ERayTracerType RayTracerType
        {
            get;
            private set;
        }

        protected H1RayTracer(Int32 InWidth, Int32 InHeight)
        {
            // create back buffer
            BackBuffer = new H1RenderTarget(InWidth, InHeight);
        }

        // back buffer
        protected H1RenderTarget BackBuffer;
    }
}
