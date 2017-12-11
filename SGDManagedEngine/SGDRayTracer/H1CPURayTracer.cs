using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;
using System.Drawing;
using System.Drawing.Imaging;

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

            Int32 ResolutionX = BackBuffer.Width;
            Int32 ResolutionY = BackBuffer.Height;

            H1Vector3 Direction = H1Vector3.Normalize(new H1Vector3(0, 1, -1));
            RayTracerCameraComponent.UpdateCamera(new H1Vector3(0, 0, 500), Direction);
            RayTracerCameraComponent.SetViewFrustum(100, 1000, (float)(30.0 / Math.PI), ResolutionX, ResolutionY);

            // test ray tracer component entity
            H1RayTracerComponent OneComponent = new H1RayTracerComponent(CameraObject);
            OneComponent.Activate();

            float Extent = 25.0f;
            OneComponent.CreateBoxBound(new H1Vector3(0, 500, 0), new H1Vector3(Extent, Extent, Extent));

            H1Ray Ray = new H1Ray(new H1Vector3(0, 0, 500), Direction);
            float T0, T1;

            bool bCollide = OneComponent.IsCollide(Ray, out T0, out T1);

            H1Ray[] Rays = RayTracerCameraComponent.GenerateRays();
            H1RayTracerSystem.Singleton.RayCast(Rays, ref BackBuffer);

            Bitmap NewBitmap = new Bitmap(ResolutionX, ResolutionY);

            for (Int32 PixelY = 0; PixelY < ResolutionY; ++PixelY)
            {
                for (Int32 PixelX = 0; PixelX < ResolutionX; ++PixelX)
                {
                    H1Color PixelColor = BackBuffer[PixelX, PixelY];
                    Color OutColor = Color.FromArgb(PixelColor.A, PixelColor.R, PixelColor.G, PixelColor.B);

                    NewBitmap.SetPixel(PixelX, PixelY, OutColor);
                }
            }

            NewBitmap.Save("RayTracerOutput.png", ImageFormat.Png);

            return true;
        }
    }
}
