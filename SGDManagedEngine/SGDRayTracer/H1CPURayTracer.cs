using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDRayTracer
{
    public struct H1CPURayTracerSettings
    {
        public Int32 Width;
        public Int32 Height;
    }

    public class H1CPURayTracer : H1RayTracer
    {
        public H1CPURayTracer(H1CPURayTracerSettings InSettings)
            : base(InSettings.Width, InSettings.Height)
        {

        }

        bool Render()
        {
            

            return true;
        }
    }
}
