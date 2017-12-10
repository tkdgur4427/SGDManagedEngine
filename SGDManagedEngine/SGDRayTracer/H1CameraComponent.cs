using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SGDUtil;

namespace SGDRayTracer
{
    public class H1CameraViewport
    {
        public float ResolutionX;
        public float ResolutionY;
    }

    public class H1ViewFrustum
    {
        public float NearDistance;
        public float FarDistance;

        // field of view - radian
        public float Fov;

        /// <summary>
        /// screen ratio is supposed that height(y-axis) is 1.0
        /// </summary>
        public float ScreenRatio;

        // camera viewport
        public H1CameraViewport Viewport
        {
            get { return CameraViewport; }
        }

        protected H1CameraViewport CameraViewport = new H1CameraViewport();

        // view frustum vertices
        public enum EFrustumVerticeIndex
        {
            /// <summary>
            /// 0 --- 3
            /// |     |
            /// 1 --- 2
            /// </summary>
            NEAR_0 = 0,
            NEAR_1,
            NEAR_2,
            NEAR_3,

            /// <summary>
            /// 0 --- 3
            /// |     |
            /// 1 --- 2
            /// </summary>
            FAR_0,
            FAR_1,
            FAR_2,
            FAR_3,

            TOTAL_NUM,
        }

        // these vertices resides in local space (RHS)
        /// <summary>
        /// z
        /// |
        /// |
        /// --------- y
        /// \
        ///  \
        ///   \ x
        /// </summary>
        public H1Vector3[] LocalFrustumVertices = new H1Vector3[Convert.ToInt32(EFrustumVerticeIndex.TOTAL_NUM)];
    }

    public class H1CameraComponent : H1Component
    {
        public H1CameraComponent(H1Entity InEntity)
            : base(InEntity)
        {

        }

        public override void Activate()
        {

        }

        public override void Deactivate()
        {

        }

        public override void Destroy()
        {

        }

        public override void Initialize()
        {

        }

        public void UpdateCamera(H1Vector3 InPosition, H1Vector3 InLookAt)
        {
            Position = InPosition;
            LookAt = InLookAt;

            // create camera matrix (view matrix)
            H1Vector3 UpVector = new H1Vector3(0, 0, 1);
            H1Vector3 RightVector = H1Vector3.Cross(LookAt, UpVector);
            UpVector = H1Vector3.Cross(RightVector, LookAt);

            ViewMatrix = new H1Matrix();
            ViewMatrix.Look = LookAt;
            ViewMatrix.Right = RightVector;
            ViewMatrix.Up = UpVector;
        }

        public bool SetViewFrustum(float InNearDist, float InFarDist, float InFov, float InScreenResolutionX, float InScreenResolutionY)
        {
            float ScreenRatio = InScreenResolutionX / InScreenResolutionY;

            if (ViewFrustum == null)
            {
                ViewFrustum = new H1ViewFrustum();
            }

            ViewFrustum.Fov = InFov;
            ViewFrustum.ScreenRatio = ScreenRatio;
            ViewFrustum.NearDistance = InNearDist;
            ViewFrustum.FarDistance = InFarDist;

            ViewFrustum.Viewport.ResolutionX = InScreenResolutionX;
            ViewFrustum.Viewport.ResolutionY = InScreenResolutionY;

            // generate view frustum vertices
            H1Vector3 LookAt = new H1Vector3(0, 1, 0);
            H1Vector3 LocalX = new H1Vector3(1, 0, 0);
            H1Vector3 LocalZ = new H1Vector3(0, 0, 1);

            H1Vector3 NearPlanePosition = Position + ViewFrustum.NearDistance * LookAt;

            float HalfHeightForNearPlane = ViewFrustum.NearDistance * (float)Math.Tan(ViewFrustum.Fov);
            float HalfWidthForNearPlane = ViewFrustum.ScreenRatio * HalfHeightForNearPlane;

            Int32 NearPlaneStartIndex = Convert.ToInt32(H1ViewFrustum.EFrustumVerticeIndex.NEAR_0);
            ViewFrustum.LocalFrustumVertices[NearPlaneStartIndex + 0] = NearPlanePosition
                + HalfWidthForNearPlane * (-LocalX) + HalfHeightForNearPlane * (LocalZ);
            ViewFrustum.LocalFrustumVertices[NearPlaneStartIndex + 1] = NearPlanePosition
                + HalfWidthForNearPlane * (-LocalX) + HalfHeightForNearPlane * (-LocalZ);
            ViewFrustum.LocalFrustumVertices[NearPlaneStartIndex + 2] = NearPlanePosition
                + HalfWidthForNearPlane * (LocalX) + HalfHeightForNearPlane * (LocalZ);
            ViewFrustum.LocalFrustumVertices[NearPlaneStartIndex + 3] = NearPlanePosition
                + HalfWidthForNearPlane * (LocalX) + HalfHeightForNearPlane * (LocalZ);

            H1Vector3 FarPlanePosition = Position + ViewFrustum.FarDistance * LookAt;

            float HalfHeightForFarPlane = ViewFrustum.FarDistance * (float)Math.Tan(ViewFrustum.Fov);
            float HalfWidthForFarPlane = ViewFrustum.ScreenRatio * HalfHeightForFarPlane;

            Int32 FarPlaneStartIndex = Convert.ToInt32(H1ViewFrustum.EFrustumVerticeIndex.FAR_0);
            ViewFrustum.LocalFrustumVertices[FarPlaneStartIndex + 0] = FarPlanePosition
                + HalfWidthForFarPlane * (-LocalX) + HalfHeightForFarPlane * (LocalZ);
            ViewFrustum.LocalFrustumVertices[FarPlaneStartIndex + 1] = FarPlanePosition
                + HalfWidthForFarPlane * (-LocalX) + HalfHeightForFarPlane * (-LocalZ);
            ViewFrustum.LocalFrustumVertices[FarPlaneStartIndex + 2] = FarPlanePosition
                + HalfWidthForFarPlane * (LocalX) + HalfHeightForFarPlane * (LocalZ);
            ViewFrustum.LocalFrustumVertices[FarPlaneStartIndex + 3] = FarPlanePosition
                + HalfWidthForFarPlane * (LocalX) + HalfHeightForFarPlane * (LocalZ);

            return true;
        }

        // camera position
        public H1Vector3 Position { get; set; }

        // look at
        public H1Vector3 LookAt { get; set; }

        // view matrix (camera matrix)
        public H1Matrix ViewMatrix { get; set; }

        // view frsutum
        protected H1ViewFrustum ViewFrustum;
    }

    public class H1RayTracerCameraComponent : H1CameraComponent
    {
        public H1RayTracerCameraComponent(H1Entity InEntity)
            : base(InEntity)
        {

        }

        // generate rays
        public H1Ray[] GenerateRays()
        {
            List<H1Ray> Rays = new List<H1Ray>();

            H1Vector3 CameraLocation = Position;
            
            Int32 ResolutionX = Convert.ToInt32(ViewFrustum.Viewport.ResolutionX);
            Int32 ResolutionY = Convert.ToInt32(ViewFrustum.Viewport.ResolutionY);

            Int32 IndexLT = Convert.ToInt32(H1ViewFrustum.EFrustumVerticeIndex.NEAR_0);

            H1Vector3 WorldScreenLT = H1Vector3.Transform(ViewFrustum.LocalFrustumVertices[IndexLT + 0], ViewMatrix);
            H1Vector3 WorldScreenLB = H1Vector3.Transform(ViewFrustum.LocalFrustumVertices[IndexLT + 1], ViewMatrix);
            H1Vector3 WorldScreenRB = H1Vector3.Transform(ViewFrustum.LocalFrustumVertices[IndexLT + 2], ViewMatrix);
            H1Vector3 WorldScreenRT = H1Vector3.Transform(ViewFrustum.LocalFrustumVertices[IndexLT + 3], ViewMatrix);

            H1Vector3 OffsetX = (WorldScreenRT - WorldScreenLT) / ResolutionX;
            H1Vector3 OffsetY = (WorldScreenLB - WorldScreenLT) / ResolutionY;

            for (Int32 PixelY = 0; PixelY < ResolutionY; ++PixelY)
            {
                for (Int32 PixelX = 0; PixelX < ResolutionX; ++PixelX)
                {
                    H1Vector3 Offset = WorldScreenLT + OffsetX * PixelX + OffsetY * PixelY;

                    H1Ray NewRay = new H1Ray();
                    NewRay.Origin = CameraLocation;
                    NewRay.Direction = H1Vector3.Normalize(Offset - NewRay.Origin);

                    Rays.Add(NewRay);
                }
            }

            return Rays.ToArray();
        }
    }
}
