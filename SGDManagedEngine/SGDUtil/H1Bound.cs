using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDUtil
{
    public class H1Slab
    {
        public H1Slab(H1Vector3 InNormal, H1Vector3 InPosition, float InExtent)
        {
            CreatePlanes(InNormal, InPosition, InExtent);
        }

        protected void CreatePlanes(H1Vector3 InNormal, H1Vector3 InPosition, float InExtent)
        {
            H1Vector3 Origin = new H1Vector3(0, 0, 0);

            // generate first plane
            H1Vector3 PointOnPlane0 = InPosition + InNormal * InExtent;

            // calculate distance from origin
            float DistanceOnPlane0 = H1Vector3.Dot(PointOnPlane0 - Origin, InNormal);
            Planes[0] = new H1Plane(InNormal, DistanceOnPlane0);

            // generate second plane
            H1Vector3 PointOnPlane1 = InPosition - InNormal * InExtent;

            float DistanceOnPlane1 = H1Vector3.Dot(PointOnPlane1 - Origin, InNormal);
            Planes[1] = new H1Plane(InNormal, DistanceOnPlane1);
        }

        // indexers for planes
        public H1Plane this[Int32 Index]
        {
            get { return Planes[Index]; }
        }

        public H1Vector3 Normal
        {
            get { return Planes[0].Normal; }
        }

        protected H1Plane[] Planes = new H1Plane[2];
    }

    public class H1Bound
    {
        public enum EBoundType
        {
            Box,
        }

        public H1Bound(EBoundType InType, H1Vector3 InPosition, H1Vector3 InExtent)
        {
            CreateSlabs(InType, InPosition, InExtent);
        }

        protected void CreateSlabs(EBoundType InBoundType, H1Vector3 InLocation, H1Vector3 InExtent)
        {
            // depending on bound type, define the number of slabs
            Int32 SlabNum = 0;

            // setting transform properties
            Position = InLocation;
            Extent = InExtent;

            switch (InBoundType)
            {
                case EBoundType.Box:
                    {
                        // total 3 slabs needed (x, y, z)
                        SlabNum = 3;

                        Slabs = new H1Slab[SlabNum];

                        // x-axis
                        H1Vector3 DirectionX = new H1Vector3(1, 0, 0);
                        float ExtentX = InExtent.X;

                        Slabs[0] = new H1Slab(DirectionX, Position, ExtentX);

                        // y-axis
                        H1Vector3 DirectionY = new H1Vector3(0, 1, 0);
                        float ExtentY = InExtent.Y;

                        Slabs[1] = new H1Slab(DirectionY, Position, ExtentY);

                        // z-axis
                        H1Vector3 DirectionZ = new H1Vector3(0, 0, 1);
                        float ExtentZ = InExtent.Z;

                        Slabs[2] = new H1Slab(DirectionZ, Position, ExtentZ);

                        break;
                    }
                default:
                    break;
            }  
        }

        public H1Slab[] SafeSlabs { get { if (Slabs != null) return Slabs; return null; } }

        // implemented as slabs
        public H1Slab[] Slabs;

        // origin
        protected H1Vector3 Position;
        // extent
        //  - extent is half size
        protected H1Vector3 Extent;
    }
}
