using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDUtil
{
    public static class H1PrimitiveIntersection
    {
        public static bool Intersect(H1Ray InRay, H1Bound InBound, out float OutT0, out float OutT1)
        {
            float T0 = 0.0f;
            float T1 = float.MaxValue;

            H1Vector3 RayOrigin = InRay.Origin;

            for (Int32 SlabIndex = 0; SlabIndex < InBound.SafeSlabs.Count(); ++SlabIndex)
            {
                // curr slab
                H1Slab Slab = InBound.SafeSlabs[SlabIndex];

                // slab normal
                H1Vector3 SlabNormal = Slab.Normal;

                float NominatorPlane0 = Slab[0].Distance - H1Vector3.Dot(Slab[0].Normal, InRay.Origin);
                float NominatorPlane1 = Slab[1].Distance - H1Vector3.Dot(Slab[1].Normal, InRay.Origin);

                float Denominator = H1Vector3.Dot(InRay.Direction, SlabNormal);

                float CurrT0 = NominatorPlane0 / Denominator;
                float CurrT1 = NominatorPlane1 / Denominator;

                // check if we need to swap
                if (CurrT0 > CurrT1)
                {
                    float Temp = CurrT0;
                    CurrT0 = CurrT1;
                    CurrT1 = Temp;
                }

                // if we need to update T0 or T1, update it
                if (T0 < CurrT0)
                {
                    T0 = CurrT0;
                }

                if (T1 > CurrT1)
                {
                    T1 = CurrT1;
                }


                if (T0 > T1)
                {
                    OutT0 = float.MaxValue;
                    OutT1 = float.MinValue;
                    return false;
                }
            }

            OutT0 = T0;
            OutT1 = T1;

            return true;
        }
    }
}
