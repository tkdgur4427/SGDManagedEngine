using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

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

        void Initialize()
        {

        }

        void Destroy()
        {

        }

        public bool Render()
        {
            H1Entity CameraObject = new H1Entity();
            H1RayTracerCameraComponent RayTracerCameraComponent = new H1RayTracerCameraComponent(CameraObject);

            RayTracerCameraComponent.UpdateCamera(new H1Vector3(0, 0, 0), new H1Vector3(0, 1, 0));
            RayTracerCameraComponent.SetViewFrustum(100, 1000, (float)(30.0 / Math.PI), 800, 600);

            H1Ray[] Rays = RayTracerCameraComponent.GenerateRays();

            return true;
        }
    }
}
