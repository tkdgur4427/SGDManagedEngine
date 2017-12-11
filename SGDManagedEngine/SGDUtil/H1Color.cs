using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDUtil
{
    public class H1Color
    {
        public H1Color(float R, float G, float B, float A)
        {
            Data = new Color(R, G, B, A);
        }

        public byte R
        {
            get { return Data.R; }
        }

        public byte G
        {
            get { return Data.G; }
        }

        public byte B
        {
            get { return Data.B; }
        }

        public byte A
        {
            get { return Data.A; }
        }

        Color Data;
    }

    public class H1Color3
    {
        Color3 Data;
    }

    public class H1Color4
    {
        Color4 Data;
    }
}
