using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDUtil
{
    public class H1Plane
    {
        public H1Plane(H1Vector3 InNormal, float InDistance)
        {
            Data = new Plane(InNormal.Data, InDistance);
        }

        public H1Vector3 Normal
        {
            get { return new H1Vector3(Data.Normal); }
        }

        public float Distance
        {
            get { return Data.D; }
        }

        public Plane Data;
    }
}
