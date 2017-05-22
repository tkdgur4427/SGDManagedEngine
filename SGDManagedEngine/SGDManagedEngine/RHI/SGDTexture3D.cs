using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1Texture3D
    {
        public struct Description
        {
            // texture width/height
            public UInt32 Width;
            public UInt32 Height;
            public UInt32 Depth;
            // texture miplevels
            public UInt32 MipLevels;            
            public H1PixelFormat Format;            
            public H1Usage Usage;
            public UInt32 BindFlags;
            public UInt32 CPUAccessFlags;
            public UInt32 MiscFlags;
        }
    }
}
