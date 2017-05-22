using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    [Flags]
    public enum H1GpuViewTypes
    {
        Unknown = 0,
        VertexBufferView = 0,
        IndexBufferView,
        ConstantBufferView,
        ShaderResourceView,
        UnorderedAccessView,
        DepthStencilView,
        RenderTargetView,
        TotalNum,
    }

    public abstract class H1GpuViewData
    {

    }

    public partial class H1GpuView
    {
        public H1GpuView(H1GpuResource resourceRef)
        {
            m_ResourceRef = resourceRef;
        }

        protected H1GpuViewTypes m_Type;
        protected H1GpuResource m_ResourceRef;
    }

    public partial class H1VertexBufferView : H1GpuView
    {
        public H1VertexBufferView(H1GpuResource resourceRef, Int64 bufferLocation, Int32 sizeInBytes, Int32 strideInBytes)
            : base(resourceRef)
        {
            ConstructPlatformIndependentMembers(bufferLocation, sizeInBytes, strideInBytes);
            ConstructPlatformDependentMembers(bufferLocation, sizeInBytes, strideInBytes);
        }

        void ConstructPlatformIndependentMembers(Int64 bufferLocation, Int32 sizeInBytes, Int32 strideInBytes)
        {
            m_Type = H1GpuViewTypes.VertexBufferView;
            m_BufferLocation = bufferLocation;
            m_SizeInBytes = sizeInBytes;
            m_StrideInBytes = strideInBytes;
        }

        protected Int64 m_BufferLocation;
        protected Int32 m_SizeInBytes;
        protected Int32 m_StrideInBytes;
    }

    public partial class H1IndexBufferView : H1GpuView
    {
        public H1IndexBufferView(H1GpuResource resourceRef, Int64 bufferLocation, H1PixelFormat format, Int32 sizeInBytes)
            : base(resourceRef)
        {
            ConstructPlatformIndependentMembers(bufferLocation, format, sizeInBytes);
            ConstructPlatformDependentMembers(bufferLocation, format, sizeInBytes);
        }

        void ConstructPlatformIndependentMembers(Int64 bufferLocation, H1PixelFormat format, Int32 sizeInBytes)
        {
            m_Type = H1GpuViewTypes.IndexBufferView;
            m_BufferLocation = bufferLocation;
            m_Format = format;
            m_SizeInBytes = sizeInBytes;
        }

        protected Int64 m_BufferLocation;
        protected H1PixelFormat m_Format;
        protected Int32 m_SizeInBytes;
    }

    public partial class H1ConstantBufferView : H1GpuView
    {
        public H1ConstantBufferView(H1GpuResource resourceRef, Int64 bufferLocation, Int32 sizeInBytes)
            : base(resourceRef)
        {
            ConstructPlatformIndependentMembers(bufferLocation, sizeInBytes);
            ConstructPlatformDependentMembers(bufferLocation, sizeInBytes);
        }

        void ConstructPlatformIndependentMembers(Int64 bufferLocation, Int32 sizeInBytes)
        {
            m_Type = H1GpuViewTypes.ConstantBufferView;
            m_BufferLocation = bufferLocation;
            m_SizeInBytes = sizeInBytes;
        }

        protected Int64 m_BufferLocation;
        protected Int32 m_SizeInBytes;
    }

    public enum H1ShaderResourceViewDimension
    {
        Unknown,
        Buffer,
        Texture1D,
        Texture1DArray,
        Texture2D,
        Texture2DArray,
        Texture2DMultiSampled,
        Texture2DArrayMultiSampled,
        Texture3D,
        TextureCube,
        TextureCubeArray,
    }

    public enum H1BufferShaderResourceViewFlags
    {
        None,
        Raw,
    }

    public class H1BufferSharderResourceViewDesc
    {
        public Int64 FirstElement;
        public Int32 NumElement;
        public Int32 StructuredByteStride;
        public H1BufferShaderResourceViewFlags Flags;
    }

    public class H1Texture1DShaderResourceViewDesc
    {
        public Int32 MostDetailedMip;
        public Int32 MipLevels;
        // a value to clamp sample LOD values to
        public float ResourceMinLODClamp;
    }

    public class H1Texture1DArrayShaderResourceViewDesc
    {
        public Int32 MostDetailedMip;
        public Int32 MipLevels;
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
        public float ResourceMinLODClamp;
    }

    public class H1Texture2DShaderResourceViewDesc
    {
        public Int32 MostDetailedMip;
        public Int32 MipLevels;
        public Int32 PlaneSlice;
        public float ResourceMinLODClamp;
    }

    public class H1Texture2DArrayShaderResourceViewDesc
    {
        public Int32 MostDetailedMip;
        public Int32 MipLevels;
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
        public Int32 PlaneSlice;
        public float ResourceMinLODClamp;
    }

    public class H1Texture3DShaderResourceViewDesc
    {
        public Int32 MostDetailedMip;
        public Int32 MipLevels;
        public float ResourceMinLODClamp;
    }

    public class H1TextureCubeShaderResourceViewDesc
    {
        public Int32 MostDetailedMip;
        public Int32 MipLevels;
        public Int32 First2DArrayFace;
        public Int32 NumCubes;
        public float ResourceMinLODClamp;
    }

    public class H1Texture2DMultiSampledShaderResourceViewDesc
    {
        public Int32 UnusedField_NothingToDefine;
    }

    public class H1Texture2DArrayMultiSampledShaderResourceViewDesc
    {
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
    }

    public class H1ShaderResourceViewDescription
    {
        public H1PixelFormat Format = H1PixelFormat.INVALID;
        public H1ShaderResourceViewDimension ViewDimension = H1ShaderResourceViewDimension.Unknown;

        // below properties determined based on H1ShaderResourceViewDescription
        public H1BufferSharderResourceViewDesc Buffer = null;
        public H1Texture1DShaderResourceViewDesc Texture1D = null;
        public H1Texture1DArrayShaderResourceViewDesc Texture1DArray = null;
        public H1Texture2DShaderResourceViewDesc Texture2D = null;
        public H1Texture2DArrayShaderResourceViewDesc Texture2DArray = null;
        public H1Texture2DMultiSampledShaderResourceViewDesc Texture2DMultiSampled = null;
        public H1Texture2DArrayMultiSampledShaderResourceViewDesc Texture2DArrayMultiSampled = null;
        public H1Texture3DShaderResourceViewDesc Texture3D = null;
        public H1TextureCubeShaderResourceViewDesc TextureCube = null;
    }

    public partial class H1ShaderResourceView : H1GpuView
    {
        public H1ShaderResourceViewDescription Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;

                // update description flag (bHasDesc)
                if (m_Description != null)
                    m_bHasDesc = true;
                else
                    m_bHasDesc = false;
            }
        }

        public H1ShaderResourceView(H1GpuResource resourceRef)
            : base(resourceRef)
        {
            m_Type = H1GpuViewTypes.ShaderResourceView;
        }

        protected H1ShaderResourceViewDescription m_Description = null;
    }

    public enum H1UnorderedAccesViewDimension
    {
        Unknown,
        Buffer,
        Texture1D,
        Texture1DArray,
        Texture2D,
        Texture2DArray,
        Texture3D,
    }

    public enum H1UnorderedAccessViewFlags
    {
        None,
        Raw,
    }

    public class H1BufferUnorderedAccessViewDesc
    {
        public Int64 FirstElement;
        public Int32 NumElements;
        public Int32 StructureByteStride;
        // the counter offset in bytes
        public Int64 CounterOffsetInBytes;
        public H1UnorderedAccessViewFlags Flags;
    }

    public class H1Texture1DUnorderedAccessViewDesc
    {
        public Int32 MipSlice;
    }

    public class H1Texture1DArrayUnorderedAccessViewDesc
    {
        public Int32 MipSlice;
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
    }

    public class H1Texture2DUnorderedAccessViewDesc
    {
        public Int32 MipSlice;
        public Int32 PlaneSlice;
    }

    public class H1Texture2DArrayUnorderedAccessViewDesc
    {
        public Int32 MipSlice;
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
        public Int32 PlaneSlice;
    }

    public class H1Texture3DUnorderedAccessViewDesc
    {
        public Int32 MipSlice;
        public Int32 FirstWSlice;
        public Int32 WSize;
    }

    public class H1UnorderedAccessViewDescription
    {
        public H1PixelFormat Format = H1PixelFormat.INVALID;
        public H1UnorderedAccesViewDimension ViewDimension = H1UnorderedAccesViewDimension.Unknown;

        // below properties determined based on H1UnorderedAccessViewDesc
        public H1BufferUnorderedAccessViewDesc Buffer = null;
        public H1Texture1DUnorderedAccessViewDesc Texture1D = null;
        public H1Texture1DArrayUnorderedAccessViewDesc Texture1DArray = null;
        public H1Texture2DUnorderedAccessViewDesc Texture2D = null;
        public H1Texture2DArrayUnorderedAccessViewDesc Texture2DArray = null;
        public H1Texture3DUnorderedAccessViewDesc Texture3D = null;
    }

    public partial class H1UnorderedAccessView : H1GpuView
    {
        public H1UnorderedAccessViewDescription Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;
                if (m_Description != null)
                    m_bHasDesc = true;
                else
                    m_bHasDesc = false;
            }
        }

        public H1UnorderedAccessView(H1GpuResource resourceRef, H1GpuResource counterResourceRef = null)
            : base(resourceRef)
        {
            m_Type = H1GpuViewTypes.UnorderedAccessView;
            m_CounterResourceRef = counterResourceRef;
        }

        private H1GpuResource m_CounterResourceRef;
        protected H1UnorderedAccessViewDescription m_Description = null;
    }

    public enum H1DepthStencilViewDimension
    {
        Unknown,
        Texture1D,
        Texture1DArray,
        Texture2D,
        Texture2DArray,
        Texture2DMutliSampled,
        Texture2DArrayMultiSampled,
    }

    public enum H1DepthStencilViewFlags
    {
        None,
        ReadOnlyDepth,
        ReadOnlyDepthStencil,
    }

    public class H1Texture1DDepthStencilViewDesc
    {
        public Int32 MipSlice;
    }

    public class H1Texture1DArrayDepthStencilViewDesc
    {
        public Int32 MipSlice;
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
    }

    public class H1Texture2DDepthStencilViewDesc
    {
        public Int32 MipSlice;
    }

    public class H1Texture2DArrayDepthStencilViewDesc
    {
        public Int32 MipSlice;
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
    }

    public class H1Texture2DMutliSampledDepthStencilViewDesc
    {
        public Int32 UnusedField_NothingToDefine;
    }

    public class H1Texture2DArrayMultiSampledDepthStencilViewDesc
    {
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
    }

    public class H1DepthStencilViewDescription
    {
        public H1PixelFormat Format = H1PixelFormat.INVALID;
        public H1DepthStencilViewDimension ViewDimension = H1DepthStencilViewDimension.Unknown;
        public H1DepthStencilViewFlags Flags = H1DepthStencilViewFlags.None;

        // below properties determined based on H1DepthStencilViewDesc
        public H1Texture1DDepthStencilViewDesc Texture1D = null;
        public H1Texture1DArrayDepthStencilViewDesc Texture1DArray = null;
        public H1Texture2DDepthStencilViewDesc Texture2D = null;
        public H1Texture2DArrayDepthStencilViewDesc Texture2DArray = null;
        public H1Texture2DMutliSampledDepthStencilViewDesc Texture2DMutliSampled = null;
        public H1Texture2DArrayMultiSampledDepthStencilViewDesc Texture2DArrayMultiSampled = null;
    }

    public partial class H1DepthStencilView : H1GpuView
    {
        public H1DepthStencilView(H1GpuResource resourceRef)
            : base(resourceRef)
        {
            m_Type = H1GpuViewTypes.DepthStencilView;
        }        

        protected H1DepthStencilViewDescription m_Description =  null;
    }

    public enum H1RenderTargetViewDimension
    {
        Unknown,
        Buffer,
        Texture1D,
        Texture1DArray,
        Texture2D,
        Texture2DArray,
        Texture2DMultiSampled,
        Texture2DArrayMultiSampled,
        Texture3D,
    }

    public class H1BufferRenderTargetViewDesc
    {
        public Int64 FirstElement;
        public Int32 NumElements;
    }

    public class H1Texture1DRenderTargetViewDesc
    {
        public Int32 MipSlice;
    }

    public class H1Texture1DArrayRenderTargetVewDesc
    {
        public Int32 MipSlice;
        // The index of the first texture to use in an array of textures
        public Int32 FirstArraySlice; 
        public Int32 ArraySize;
    }

    public class H1Texture2DRenderTargetViewDesc
    {
        public Int32 MipSlice;
        public Int32 PlaneSlice;
    }

    public class H1Texture2DArrayRenderTargetViewDesc
    {
        public Int32 MipSlice;
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
        public Int32 PlaneSlice;
    }

    public class H1Texture2DMultiSampledRenderTargetViewDesc
    {
        public Int32 UnusedField_NothingToDefine;
    }

    public class H1Texture2DArrayMutliSampledRenderTargetViewDesc
    {
        public Int32 FirstArraySlice;
        public Int32 ArraySize;
    }

    public class H1Texture3DRenderTargetViewDesc
    {
        public Int32 MipSlice;
        public Int32 FirstWSlice;
        public Int32 WSize;
    }

    public class H1RenderTargetViewDescription
    {
        public H1PixelFormat Format = H1PixelFormat.INVALID;
        public H1RenderTargetViewDimension ViewDimension = H1RenderTargetViewDimension.Unknown;

        // below properties determined based on H1RenderTargetViewDimension
        public H1BufferRenderTargetViewDesc Buffer = null;
        public H1Texture1DRenderTargetViewDesc Texture1D = null;
        public H1Texture1DArrayRenderTargetVewDesc Texture1DArray = null;
        public H1Texture2DRenderTargetViewDesc Texture2D = null;
        public H1Texture2DArrayRenderTargetViewDesc Texture2DArray = null;
        public H1Texture2DMultiSampledRenderTargetViewDesc Texture2DMultiSampled = null;
        public H1Texture2DArrayMutliSampledRenderTargetViewDesc Texture2DArrayMultiSampled = null;
        public H1Texture3DRenderTargetViewDesc Texture3D = null;
    }

    public partial class H1RenderTargetView : H1GpuView
    {
        public H1RenderTargetView(H1GpuResource resourceRef)
            : base(resourceRef)
        {
            m_Type = H1GpuViewTypes.RenderTargetView;
        }

        protected H1RenderTargetViewDescription m_Description = null;
    }
}
