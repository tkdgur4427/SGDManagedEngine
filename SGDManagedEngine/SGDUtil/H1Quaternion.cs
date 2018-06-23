using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDUtil
{
    public class H1Quaternion
    {
        public H1Quaternion(float X, float Y, float Z, float W)
        {
            Data.X = X;
            Data.Y = Y;
            Data.Z = Z;
            Data.W = W;
        }

        public float X
        {
            get { return Data.X; }
        }

        public float Y
        {
            get { return Data.Y; }
        }

        public float Z
        {
            get { return Data.Z; }
        }

        public float W
        {
            get { return Data.W; }
        }

        public Quaternion Data = new Quaternion();
    }
}
