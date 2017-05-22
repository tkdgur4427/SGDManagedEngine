using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD
{
    public class H1Texture2D : H1Resource
    {
        public struct Description
        {
            // texture width/height
            public UInt32 Width;
            public UInt32 Height;
            // texture miplevels
            public UInt32 MipLevels;
            // texture arraysize
            public UInt32 ArraySize;
            public H1PixelFormat Format;
            // texture sample description
            public H1SampleDescription SampleDesc;
            public H1Usage Usage;
            public UInt32 BindFlags;
            public UInt32 CPUAccessFlags;
            public UInt32 MiscFlags;
        }

        public UInt32 Width
        {
            get { return m_Description.Width; }
        }

        public UInt32 Height
        {
            get { return m_Description.Height; }
        }

        public UInt32 Stride
        {
            get { return H1RHIDefinitionHelper.ElementTypeToSize(PixelFormat) * Width; }
        }

        public H1PixelFormat PixelFormat
        {
            get { return m_Description.Format; }
        }

        public uint Index
        {
            get { return m_Index; }
        }

        public Resource Resource
        {
            get
            {
                return H1Global<H1ManagedRenderer>.Instance.ResourceManager.GetTexture2D(Convert.ToInt32(Index));
            }
        }

        public H1Texture2D(H1GPUResourceManager manager, uint index, H1PixelFormat elementType, int width, int height)
            : base(manager, Convert.ToUInt32(H1RHIDefinitionHelper.ElementTypeToSize(elementType) * width * height))
        {
            m_Index = index;
            m_Description.Format = elementType;
            m_Description.Width = Convert.ToUInt32(width);
            m_Description.Height = Convert.ToUInt32(height);
            // @TODO - I need to add initial variables like NumMips, NumSamples and TextureName
        }

        // texture resource index in GPUResourceManager
        private uint m_Index;
        // texture RHI properties
        private String m_TextureName;
        private Description m_Description;  
    }    
}
