using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1Texture1D
    {
        public struct Description
        {
            // texture width
            public UInt32 Width;
            // texture mip levels
            public UInt32 MipLevels;
            // texture array size
            public UInt32 ArraySize;
            public H1PixelFormat Format;            
            public H1Usage Usage;
            public UInt32 BindFlags;
            public UInt32 CPUAccessFlags;
            public UInt32 MiscFlags;
        }
    }
}
