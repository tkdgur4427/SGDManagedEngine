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
            // clear the render target
            Clear();
        }

        public H1Color this[Int32 X, Int32 Y]
        {
            get { return Pixels[Y * Width + X]; }
            set { Pixels[Y * Width + X] = value; }
        }

        public H1Color this[Int32 Index]
        {
            get { return Pixels[Index]; }
            set { Pixels[Index] = value; }
        }

        public void Clear()
        {
            for (Int32 PixelY = 0; PixelY < Height; ++PixelY)
            {
                for (Int32 PixelX = 0; PixelX < Width; ++PixelX)
                {
                    Pixels[PixelY * Width + PixelX] = new H1Color(0, 0, 0, 1);
                }
            }
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
