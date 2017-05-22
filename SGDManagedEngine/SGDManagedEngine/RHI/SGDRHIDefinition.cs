using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public enum H1VertexElementType
    {
        // floating types
        Float1,
        Float2,
        Float3,
        Float4,
        // integer types
        Int1,
        Int2,
        Int3,
        Int4,
    }

    public enum H1PixelFormat
    {
        INVALID,
        Unknown,
        R8G8B8A8,
        A8B8G8R8,        
        R16F,
        R16_UINT,
        R16_SINT,
        R32_UINT,
        R32_FLOAT,
        R32G32_FLOAT,
        R32G32B32_FLOAT,
        R32G32B32A32_FLOAT,
        R32G32B32A32_SINT,
        R32_TYPELESS,
        // ... add more pixel format type
    }

    public enum H1Usage
    {
        Default,
        Immutable,
        Dynamic,
        Staging,
    }

    public enum H1BindFlag
    {
        VertexBuffer,
        IndexBuffer,
        ContantBuffer,
        ShaderResource,
        StreamOutput,
        RenderTarget,
        DepthStencil,
        UnorderedAccess,        
    }

    public enum H1CPUAccessFlag
    {
        Write = 0x10000,
        Read = 0x20000,
    }

    public struct H1SampleDescription
    {
        public UInt32 Count;
        public UInt32 Quality;
    }

    public struct H1MemcpyDest
    {
        IntPtr pData;
        UInt32 RowPitch;
        UInt32 SlicePitch;
    }

    public struct H1SubresourceData
    {
        public IntPtr pData;
        public UInt64 RowPitch;
        public UInt64 SlicePitch;
    }

    public enum H1SamplerFilter
    {
        MinMagMipPoint,
        MinMagMipLinear,
        Anisotropic,
        TotalNum,
    }

    public enum H1TextureAddressMode
    {
        Wrap,
        Mirror,
        Clamp,
        TotalNum,
    }

    public enum H1ComparisonFunc
    {
        Never,
        Less,
        Equal,
        LessEqual,
        Greater,
        NotEqual,
        GreaterEqual,
        Always,
        TotalNum,
    }

    public partial class H1SamplerDescription
    {
        public H1SamplerFilter Filter;
        public H1TextureAddressMode AddressU;
        public H1TextureAddressMode AddressV;
        public H1TextureAddressMode AddressW;
        public float MipLODBias;
        public Int32 MaxAnisotropy;
        public H1ComparisonFunc ComparisonFunc;
        public float[] BorderColor = new float[4];
        public float MinLOD;
        public float MaxLOD;
    }

    public enum H1Blend
    {
        Zero = 1,
        One,
        SrcColor,
        InvSrcColor,
        SrcAlpha,
        InvSrcAlpha,
        DestAlpha,
        InvDestAlpha,
        DestColor,
        InvDestColor,
        SrcAlphaSat,
        BlendFactor,
        InvBlendFactor,
        Src1Color,
        InvSrc1Color,
        Src1Alpha,
        InvSrc1Alpha,
    }

    public enum H1BlendOp
    {
        Add,
        Subtract,
        RevSubtract,
        Min,
        Max,
    }

    public enum H1ColorWriteEnable
    {
        Red     = 1,
        Green   = 2,
        Blue    = 4,
        Alpha   = 8,
        All     = (Red | Green | Blue | Alpha),
    }

    public enum H1LogicOp
    {
        Clear,
        Set,
        Copy,
        CopyInverted,
        Noop,
        Invert,
        And,
        Nand,
        Or,
        Nor,
        Xor,
        Equiv,
        AndReverse,
        AndInverted,
        OrReverse,
        OrInverted,
    }

    public class H1RenderTargetBlendDescription
    {
        public Boolean BlendEnable;
        public Boolean LogicOpEnable;
        public H1Blend SrcBlend;
        public H1Blend DestBlend;
        public H1BlendOp BlendOp;
        public H1Blend SrcBlendAlpha;
        public H1Blend DestBlendAlpha;
        public H1BlendOp BlendOpAlpha;
        public H1LogicOp LogicOp;
        public H1ColorWriteEnable RenderTargetWriteMask;
    }

    public class H1BlendStateDescription
    {
        public Boolean AlphaToCoverageEnable;
        public Boolean IndependentBlendEnable;
        public H1RenderTargetBlendDescription[] RenderTargets = new H1RenderTargetBlendDescription[8];
    }

    public enum H1FillMode
    {
        Wireframe,
        Solid,
    }

    public enum H1CullMode
    {
        None,
        Front,
        Back,
    }

    public enum H1ConservativeRasterizationMode
    {
        Off,
        On,
    }

    public class H1RasterizerDescription
    {
        public H1FillMode FillMode;
        public H1CullMode CullMode;
        public Boolean FrontCounterClockwise;
        public Int32 DepthBias;
        public float DepthBiasClamp;
        public float SlopeScaledDepthBias;
        public Boolean DepthClipEnable;
        public Boolean MultiSampleEnable;
        public Boolean AntialiasedLineEnable;
        public Int32 ForcedSampleCount;
        public H1ConservativeRasterizationMode ConservativeRasterMode;
    }

    public enum H1DepthWriteMask
    {
        Zero,
        All,
    }

    public enum H1StencilOp
    {
        Keep,
        Zero,
        Replace,
        IncrSat,
        DecrSat,
        Invert,
        Incr,
        Decr,
    }

    public class H1DepthStencilOpDescription
    {
        public H1StencilOp StencilFailOp;
        public H1StencilOp StencilDepthFailOp;
        public H1StencilOp StencilPassOp;
        public H1ComparisonFunc StencilFunc;
    }

    public class H1DepthStencilDescription
    {
        public Boolean DepthEnable;
        public H1DepthWriteMask DepthWriteMask;
        public H1ComparisonFunc DepthFunc;
        public Boolean StencilEnable;
        public Byte StencilReadMask;
        public Byte StencilWriteMask;
        public H1DepthStencilOpDescription FrontFace;
        public H1DepthStencilOpDescription BackFace;
    }

    public enum H1PrimitiveTopologyType
    {
        Undefined,
        Point,
        Line,
        Triangle,
        Patch,
    }

    public enum H1InputClassification
    {
        PerVertexData,
        PerInstanceData,
    }

    public class H1InputElementDescription
    {
        public String SemanticName;
        public Int32 SemanticIndex;
        public H1PixelFormat Format;
        public Int32 InputSlot;
        public Int32 AlignedByteOffset;
        public H1InputClassification InputSlotClass;
        public Int32 InstanceDataStepRate;
    }

    // @TODO - currently based on DirectX 12 ; need to consider 
    public enum H1ResourceStates
    {
        Invalid = -1,
        // - the recommended initial state for resources that do not have a more specific known or planned initial state (through note that command lists must be aware of initial states)
        // - textures must be in this COMMON state for CPU access to be legal; but buffers do not
        // - CPU access to a resource is typically done through map/unmap
        // - to use a resource initially on a Copy queue, the resource should start in this COMMON state
        // - the COMMON state can be used for all usages on a Copy queue using the implicit state transitions
        Common = 0,
        // - synonymous with common
        Present,
        VertexAndConstantBuffer,
        IndexBuffer,
        RenderTarget,
        UnorderedAccess,
        DepthWrite,
        DepthRead,
        NonPixelShaderResource,
        PixelShaderResource,
        StreamOut,
        IndirectArgument,
        Predication,
        CopyDestination,
        CopySource,
        GenericRead,
        ResolveDestination,
        ResolveSource,
        TotalNum,
    }

    public enum H1HeapType
    {
        Unknown = -1,
        Default,
        Upload,
        Readback,
        TotalNum,
    }

    public enum H1Dimension
    {
        Buffer,
        Texture1D,
        Texture2D,
        Texture3D,
        TotalNum,
    }

    public enum H1TextureLayout
    {
        Unknown,
        RowMajor,
        UndefinedSwizzle64KB,
        StandardSwizzle64KB,
        TotalNum,
    }

    // make H1ResourceFlags to enable Bit operations
    [Flags] 
    public enum H1ResourceFlags
    {
        Unknown = 0x0,
        AllowRenderTarget = 0x1,
        AllowDepthStencil = 0x2,
        AllowUnorderedAccess = 0x4,
        DenyShaderResource = 0x8,
        AllowCrossAdapter = 0x10,
        AllowSimultaneousAccess = 0x20,
        TotalNum,
    }

    public class H1GpuResourceDesc
    {
        public H1Dimension Dimension = new H1Dimension();
        public UInt32 Alignment = 0;
        public UInt32 Width = 1;
        public UInt32 Height = 1;
        public UInt16 DepthOrArraySize = 1;
        public UInt16 MipLevels;
        public H1PixelFormat Format = H1PixelFormat.Unknown;
        public H1SampleDescription SampleDesc = new H1SampleDescription();
        public H1TextureLayout Layout = H1TextureLayout.Unknown;
        // the combination of H1ResourceFlags enum
        public H1ResourceFlags Flags;
    }

    public enum H1DescriptorHeapType
    {
        ConstantBufferView_ShaderResourceView_UnorderedAccessView = 0,
        Sampler,
        RenderTargetView,
        DepthStencilView,
        TotalNum,
    }

    public class H1RHIDefinitionHelper
    {
        // @TODO - need to separate layout for platform-dependent

        // converted to directx 12 friendly format from H1VertexElementType
        public static SharpDX.DXGI.Format ConvertToFormat(H1VertexElementType vertexElementType)
        {
            SharpDX.DXGI.Format format = new SharpDX.DXGI.Format();
            switch (vertexElementType)
            {
                case H1VertexElementType.Float2:
                    format = SharpDX.DXGI.Format.R32G32_Float;
                    break;

                case H1VertexElementType.Float3:
                    format = SharpDX.DXGI.Format.R32G32B32_Float;
                    break;

                case H1VertexElementType.Float4:
                    format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                    break;
                case H1VertexElementType.Int4:
                    format = SharpDX.DXGI.Format.R32G32B32A32_SInt;
                    break;
            }

            return format;
        }

        public static H1PixelFormat ConvertToPixelFormat(H1VertexElementType vertexElementType)
        {
            H1PixelFormat pixelFormat = new H1PixelFormat();
            switch (vertexElementType)
            {
                case H1VertexElementType.Float2:
                    pixelFormat = H1PixelFormat.R32G32_FLOAT;
                    break;

                case H1VertexElementType.Float3:
                    pixelFormat = H1PixelFormat.R32G32B32_FLOAT;
                    break;

                case H1VertexElementType.Float4:
                    pixelFormat = H1PixelFormat.R32G32B32A32_FLOAT;
                    break;

                case H1VertexElementType.Int4:
                    pixelFormat = H1PixelFormat.R32G32B32A32_SINT;
                    break;
            }

            return pixelFormat;
        }

        public static SharpDX.DXGI.Format ConvertToFormat(H1PixelFormat textureElementType)
        {
            SharpDX.DXGI.Format format = new SharpDX.DXGI.Format();
            switch(textureElementType)
            {
                case H1PixelFormat.Unknown:
                    format = SharpDX.DXGI.Format.Unknown;
                    break;

                case H1PixelFormat.R8G8B8A8:
                    format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                    break;

                case H1PixelFormat.R32_UINT:
                    format = SharpDX.DXGI.Format.R32_UInt;
                    break;

                case H1PixelFormat.R32G32_FLOAT:
                    format = SharpDX.DXGI.Format.R32G32_Float;
                    break;

                case H1PixelFormat.R32G32B32_FLOAT:
                    format = SharpDX.DXGI.Format.R32G32B32_Float;
                    break;

                case H1PixelFormat.R32G32B32A32_FLOAT:
                    format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                    break;

                case H1PixelFormat.R32G32B32A32_SINT:
                    format = SharpDX.DXGI.Format.R32G32B32A32_SInt;
                    break;

                case H1PixelFormat.R32_TYPELESS:
                    format = SharpDX.DXGI.Format.R32_Typeless;
                    break;
            }

            return format;
        }

        public static uint ElementTypeToSize(H1VertexElementType vertexElementType)
        {
            uint size = 0;
            switch (vertexElementType)
            {
                case H1VertexElementType.Float2:
                    size = sizeof(float) * 2;
                    break;

                case H1VertexElementType.Float3:
                    size = sizeof(float) * 3;
                    break;

                case H1VertexElementType.Float4:
                    size = sizeof(float) * 4;
                    break;
            }

            return size;
        }

        public static uint ElementTypeToSize(H1PixelFormat textureElementType)
        {
            uint size = 0;
            switch (textureElementType)
            {
                case H1PixelFormat.R8G8B8A8:
                    size = 4;
                    break;
            }

            return size;
        }

        public static SharpDX.Direct3D12.Filter ConvertToFilter(H1SamplerFilter filter)
        {
            SharpDX.Direct3D12.Filter result = new SharpDX.Direct3D12.Filter();
            switch (filter)
            {
                case H1SamplerFilter.MinMagMipPoint:
                    result = SharpDX.Direct3D12.Filter.MinMagMipPoint;
                    break;
                case H1SamplerFilter.MinMagMipLinear:
                    result = SharpDX.Direct3D12.Filter.MinMagMipLinear;
                    break;
                case H1SamplerFilter.Anisotropic:
                    result = SharpDX.Direct3D12.Filter.Anisotropic;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.TextureAddressMode ConvertToTextureAddressMode(H1TextureAddressMode addressMode)
        {
            SharpDX.Direct3D12.TextureAddressMode result = new SharpDX.Direct3D12.TextureAddressMode();
            switch (addressMode)
            {
                case H1TextureAddressMode.Wrap:
                    result = SharpDX.Direct3D12.TextureAddressMode.Wrap;
                    break;
                case H1TextureAddressMode.Mirror:
                    result = SharpDX.Direct3D12.TextureAddressMode.Mirror;
                    break;
                case H1TextureAddressMode.Clamp:
                    result = SharpDX.Direct3D12.TextureAddressMode.Clamp;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.Comparison ConvertToComparisonFunc(H1ComparisonFunc comparisonFunc)
        {
            SharpDX.Direct3D12.Comparison result = new SharpDX.Direct3D12.Comparison();
            switch (comparisonFunc)
            {
                case H1ComparisonFunc.Less:
                    result = SharpDX.Direct3D12.Comparison.Less;
                    break;
                case H1ComparisonFunc.LessEqual:
                    result = SharpDX.Direct3D12.Comparison.LessEqual;
                    break;
                case H1ComparisonFunc.Never:
                    result = SharpDX.Direct3D12.Comparison.Never;
                    break;
                case H1ComparisonFunc.Equal:
                    result = SharpDX.Direct3D12.Comparison.Equal;
                    break;
                case H1ComparisonFunc.NotEqual:
                    result = SharpDX.Direct3D12.Comparison.NotEqual;
                    break;
                case H1ComparisonFunc.Greater:
                    result = SharpDX.Direct3D12.Comparison.Greater;
                    break;
                case H1ComparisonFunc.GreaterEqual:
                    result = SharpDX.Direct3D12.Comparison.GreaterEqual;
                    break;
                case H1ComparisonFunc.Always:
                    result = SharpDX.Direct3D12.Comparison.Always;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.BlendOperation ConvertToBlendOp(H1BlendOp blendOp)
        {
            SharpDX.Direct3D12.BlendOperation result = new SharpDX.Direct3D12.BlendOperation();
            switch (blendOp)
            {
                case H1BlendOp.Add:
                    result = SharpDX.Direct3D12.BlendOperation.Add;
                    break;
                case H1BlendOp.Subtract:
                    result = SharpDX.Direct3D12.BlendOperation.Subtract;
                    break;
                case H1BlendOp.RevSubtract:
                    result = SharpDX.Direct3D12.BlendOperation.ReverseSubtract;
                    break;
                case H1BlendOp.Max:
                    result = SharpDX.Direct3D12.BlendOperation.Maximum;
                    break;
                case H1BlendOp.Min:
                    result = SharpDX.Direct3D12.BlendOperation.Minimum;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.BlendOption ConvertToBlend(H1Blend blend)
        {
            SharpDX.Direct3D12.BlendOption result = new SharpDX.Direct3D12.BlendOption();
            switch (blend)
            {
                case H1Blend.One:
                    result = SharpDX.Direct3D12.BlendOption.One;
                    break;
                case H1Blend.Zero:
                    result = SharpDX.Direct3D12.BlendOption.Zero;
                    break;
                case H1Blend.SrcColor:
                    result = SharpDX.Direct3D12.BlendOption.SourceColor;
                    break;
                case H1Blend.DestColor:
                    result = SharpDX.Direct3D12.BlendOption.DestinationColor;
                    break;
                case H1Blend.SrcAlpha:
                    result = SharpDX.Direct3D12.BlendOption.SourceAlpha;
                    break;
                case H1Blend.DestAlpha:
                    result = SharpDX.Direct3D12.BlendOption.DestinationAlpha;
                    break;
                case H1Blend.Src1Color:
                    result = SharpDX.Direct3D12.BlendOption.SecondarySourceColor;
                    break;
                case H1Blend.Src1Alpha:
                    result = SharpDX.Direct3D12.BlendOption.SecondarySourceAlpha;
                    break;
                case H1Blend.InvSrcColor:
                    result = SharpDX.Direct3D12.BlendOption.InverseSourceColor;
                    break;
                case H1Blend.InvSrcAlpha:
                    result = SharpDX.Direct3D12.BlendOption.InverseSourceAlpha;
                    break;
                case H1Blend.InvSrc1Color:
                    result = SharpDX.Direct3D12.BlendOption.InverseSecondarySourceColor;
                    break;
                case H1Blend.InvSrc1Alpha:
                    result = SharpDX.Direct3D12.BlendOption.InverseSecondarySourceAlpha;
                    break;
                case H1Blend.InvDestColor:
                    result = SharpDX.Direct3D12.BlendOption.InverseDestinationColor;
                    break;
                case H1Blend.InvDestAlpha:
                    result = SharpDX.Direct3D12.BlendOption.InverseDestinationAlpha;
                    break;
                case H1Blend.BlendFactor:
                    result = SharpDX.Direct3D12.BlendOption.BlendFactor;
                    break;
                case H1Blend.InvBlendFactor:
                    result = SharpDX.Direct3D12.BlendOption.InverseBlendFactor;
                    break;
                case H1Blend.SrcAlphaSat:
                    result = SharpDX.Direct3D12.BlendOption.SourceAlphaSaturate;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.LogicOperation ConvertToLogicOp(H1LogicOp logicOp)
        {
            SharpDX.Direct3D12.LogicOperation result = new SharpDX.Direct3D12.LogicOperation();
            switch (logicOp)
            {
                case H1LogicOp.And:
                    result = SharpDX.Direct3D12.LogicOperation.And;
                    break;

                case H1LogicOp.Or:
                    result = SharpDX.Direct3D12.LogicOperation.Or;
                    break;

                case H1LogicOp.Noop:
                    result = SharpDX.Direct3D12.LogicOperation.Noop;
                    break;

                case H1LogicOp.Clear:
                    result = SharpDX.Direct3D12.LogicOperation.Clear;
                    break;

                case H1LogicOp.AndInverted:
                    result = SharpDX.Direct3D12.LogicOperation.AndInverted;
                    break;

                case H1LogicOp.AndReverse:
                    result = SharpDX.Direct3D12.LogicOperation.AndReverse;
                    break;

                case H1LogicOp.Copy:
                    result = SharpDX.Direct3D12.LogicOperation.Copy;
                    break;

                case H1LogicOp.CopyInverted:
                    result = SharpDX.Direct3D12.LogicOperation.CopyInverted;
                    break;

                case H1LogicOp.Equiv:
                    result = SharpDX.Direct3D12.LogicOperation.Equiv;
                    break;

                case H1LogicOp.Invert:
                    result = SharpDX.Direct3D12.LogicOperation.Invert;
                    break;

                case H1LogicOp.Nand:
                    result = SharpDX.Direct3D12.LogicOperation.Nand;
                    break;

                case H1LogicOp.Nor:
                    result = SharpDX.Direct3D12.LogicOperation.Nor;
                    break;

                case H1LogicOp.OrInverted:
                    result = SharpDX.Direct3D12.LogicOperation.OrInverted;
                    break;

                case H1LogicOp.OrReverse:
                    result = SharpDX.Direct3D12.LogicOperation.OrReverse;
                    break;

                case H1LogicOp.Set:
                    result = SharpDX.Direct3D12.LogicOperation.Set;
                    break;

                case H1LogicOp.Xor:
                    result = SharpDX.Direct3D12.LogicOperation.Xor;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.ColorWriteMaskFlags ConvertToColorWriteMaskFlags(H1ColorWriteEnable colorWriteEnable)
        {
            SharpDX.Direct3D12.ColorWriteMaskFlags result = new SharpDX.Direct3D12.ColorWriteMaskFlags();
            switch (colorWriteEnable)
            {
                case H1ColorWriteEnable.Red:
                    result = SharpDX.Direct3D12.ColorWriteMaskFlags.Red;
                    break;

                case H1ColorWriteEnable.Blue:
                    result = SharpDX.Direct3D12.ColorWriteMaskFlags.Blue;
                    break;

                case H1ColorWriteEnable.Green:
                    result = SharpDX.Direct3D12.ColorWriteMaskFlags.Green;
                    break;

                case H1ColorWriteEnable.Alpha:
                    result = SharpDX.Direct3D12.ColorWriteMaskFlags.Alpha;
                    break;

                case H1ColorWriteEnable.All:
                    result = SharpDX.Direct3D12.ColorWriteMaskFlags.All;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.FillMode ConvertToFillMode(H1FillMode fillMode)
        {
            SharpDX.Direct3D12.FillMode result = new SharpDX.Direct3D12.FillMode();
            switch (fillMode)
            {
                case H1FillMode.Solid:
                    result = SharpDX.Direct3D12.FillMode.Solid;
                    break;

                case H1FillMode.Wireframe:
                    result = SharpDX.Direct3D12.FillMode.Wireframe;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.CullMode ConvertToCullMode(H1CullMode cullMode)
        {
            SharpDX.Direct3D12.CullMode result = new SharpDX.Direct3D12.CullMode();
            switch (cullMode)
            {
                case H1CullMode.None:
                    result = SharpDX.Direct3D12.CullMode.None;
                    break;

                case H1CullMode.Front:
                    result = SharpDX.Direct3D12.CullMode.Front;
                    break;

                case H1CullMode.Back:
                    result = SharpDX.Direct3D12.CullMode.Back;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.DepthWriteMask ConvertToDepthWriteMask(H1DepthWriteMask depthWriteMask)
        {
            SharpDX.Direct3D12.DepthWriteMask result = new SharpDX.Direct3D12.DepthWriteMask();
            switch (depthWriteMask)
            {
                case H1DepthWriteMask.All:
                    result = SharpDX.Direct3D12.DepthWriteMask.All;
                    break;

                case H1DepthWriteMask.Zero:
                    result = SharpDX.Direct3D12.DepthWriteMask.Zero;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.StencilOperation ConvertToStencilOp(H1StencilOp stencilOp)
        {
            SharpDX.Direct3D12.StencilOperation result = new SharpDX.Direct3D12.StencilOperation();
            switch (stencilOp)
            {
                case H1StencilOp.Zero:
                    result = SharpDX.Direct3D12.StencilOperation.Zero;
                    break;

                case H1StencilOp.Replace:
                    result = SharpDX.Direct3D12.StencilOperation.Replace;
                    break;

                case H1StencilOp.Keep:
                    result = SharpDX.Direct3D12.StencilOperation.Keep;
                    break;

                case H1StencilOp.Invert:
                    result = SharpDX.Direct3D12.StencilOperation.Invert;
                    break;

                case H1StencilOp.Decr:
                    result = SharpDX.Direct3D12.StencilOperation.Decrement;
                    break;

                case H1StencilOp.DecrSat:
                    result = SharpDX.Direct3D12.StencilOperation.DecrementAndClamp;
                    break;

                case H1StencilOp.Incr:
                    result = SharpDX.Direct3D12.StencilOperation.Increment;
                    break;

                case H1StencilOp.IncrSat:
                    result = SharpDX.Direct3D12.StencilOperation.IncrementAndClamp;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.PrimitiveTopologyType ConvertToPrimitiveTopologyType(H1PrimitiveTopologyType primitiveTopologyType)
        {
            SharpDX.Direct3D12.PrimitiveTopologyType result = new SharpDX.Direct3D12.PrimitiveTopologyType();
            switch (primitiveTopologyType)
            {
                case H1PrimitiveTopologyType.Line:
                    result = SharpDX.Direct3D12.PrimitiveTopologyType.Line;
                    break;

                case H1PrimitiveTopologyType.Patch:
                    result = SharpDX.Direct3D12.PrimitiveTopologyType.Patch;
                    break;

                case H1PrimitiveTopologyType.Point:
                    result = SharpDX.Direct3D12.PrimitiveTopologyType.Point;
                    break;

                case H1PrimitiveTopologyType.Triangle:
                    result = SharpDX.Direct3D12.PrimitiveTopologyType.Triangle;
                    break;

                case H1PrimitiveTopologyType.Undefined:
                    result = SharpDX.Direct3D12.PrimitiveTopologyType.Undefined;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.DescriptorHeapType ConvertToDescriptorHeapType(H1DescriptorHeapType descriptorHeapType)
        {
            SharpDX.Direct3D12.DescriptorHeapType result = new SharpDX.Direct3D12.DescriptorHeapType();
            switch (descriptorHeapType)
            {
                case H1DescriptorHeapType.ConstantBufferView_ShaderResourceView_UnorderedAccessView:
                    result = SharpDX.Direct3D12.DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView;
                    break;

                case H1DescriptorHeapType.Sampler:
                    result = SharpDX.Direct3D12.DescriptorHeapType.Sampler;
                    break;

                case H1DescriptorHeapType.RenderTargetView:
                    result = SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView;
                    break;

                case H1DescriptorHeapType.DepthStencilView:
                    result = SharpDX.Direct3D12.DescriptorHeapType.DepthStencilView;
                    break;
            }

            return result;
        }

        public static SharpDX.Direct3D12.ClearValue FormatToClearValue(H1PixelFormat pixelFormat, Boolean bDepth)
        {
            SharpDX.Direct3D12.ClearValue clearValue;

            clearValue.Color.X = 0.0f;
            clearValue.Color.Y = 0.0f;
            clearValue.Color.Z = 0.0f;
            clearValue.Color.W = 0.0f;

            clearValue.Format = H1RHIDefinitionHelper.ConvertToFormat(pixelFormat);
            if (clearValue.Format == SharpDX.DXGI.Format.R32_Typeless)
            {
                clearValue.Format = bDepth ? SharpDX.DXGI.Format.D32_Float : SharpDX.DXGI.Format.R32_Float;
            }

            clearValue.DepthStencil.Depth = 0.0f;
            clearValue.DepthStencil.Stencil = 0;

            return clearValue;
        }

        public static H1GpuResourceDesc Texture2DDescToGpuResourceDesc(H1Texture2D.Description desc)
        {
            H1GpuResourceDesc result = new H1GpuResourceDesc();
            result.Format = desc.Format;
            result.Width = desc.Width;
            result.Height = desc.Height;
            result.DepthOrArraySize = Convert.ToUInt16(desc.ArraySize);
            result.MipLevels = Convert.ToUInt16(desc.MipLevels);
            result.Flags = H1ResourceFlags.Unknown;
            result.Layout = H1TextureLayout.Unknown;
            result.Dimension = H1Dimension.Texture2D;

            // setting sample description
            result.SampleDesc.Count = desc.SampleDesc.Count;
            result.SampleDesc.Quality = desc.SampleDesc.Quality;

            // setting resource flag
            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.UnorderedAccess)) != 0)
                result.Flags |= H1ResourceFlags.AllowUnorderedAccess;

            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.DepthStencil)) != 0)
                result.Flags |= H1ResourceFlags.AllowDepthStencil;

            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.RenderTarget)) != 0)
                result.Flags |= H1ResourceFlags.AllowRenderTarget;

            return result;
        }

        public static H1ResourceStates GetResourceStatesFromTexture2DDesc(H1Texture2D.Description desc)
        {
            H1ResourceStates result = H1ResourceStates.CopyDestination;

            if (desc.Usage == H1Usage.Dynamic)
                result = H1ResourceStates.GenericRead;

            if (desc.CPUAccessFlags != 0)
            {
                if (desc.CPUAccessFlags == Convert.ToUInt32(H1CPUAccessFlag.Write))
                    result = H1ResourceStates.GenericRead;

                if (desc.CPUAccessFlags == Convert.ToUInt32(H1CPUAccessFlag.Write))
                    result = H1ResourceStates.CopyDestination;

                else
                    return H1ResourceStates.Invalid;
            }

            // setting resource flag
            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.UnorderedAccess)) != 0)
                result = H1ResourceStates.UnorderedAccess;

            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.DepthStencil)) != 0)
                result = H1ResourceStates.DepthWrite;

            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.RenderTarget)) != 0)
                result = H1ResourceStates.RenderTarget;

            return result;
        }

        public static H1HeapType GetHeapTypeFromTexture2DDesc(H1Texture2D.Description desc)
        {
            H1HeapType result = H1HeapType.Default;

            if (desc.Usage == H1Usage.Staging)
                result = H1HeapType.Readback;

            else if (desc.Usage == H1Usage.Dynamic)
                result = H1HeapType.Upload;

            if (desc.CPUAccessFlags != 0)
            {
                if (desc.CPUAccessFlags == Convert.ToUInt32(H1CPUAccessFlag.Write))
                    result = H1HeapType.Upload;

                if (desc.CPUAccessFlags == Convert.ToUInt32(H1CPUAccessFlag.Write))
                    result = H1HeapType.Readback;
            }

            return result;
        }        

        public static SharpDX.Direct3D12.ResourceFlags FormatToResourceFlags(H1ResourceFlags resourceFlags)
        {
            SharpDX.Direct3D12.ResourceFlags result = SharpDX.Direct3D12.ResourceFlags.None;

            Boolean bHaveFlag = resourceFlags.HasFlag(H1ResourceFlags.AllowCrossAdapter);
            result |= bHaveFlag ? SharpDX.Direct3D12.ResourceFlags.AllowCrossAdapter : SharpDX.Direct3D12.ResourceFlags.None;

            bHaveFlag = resourceFlags.HasFlag(H1ResourceFlags.AllowDepthStencil);
            result |= bHaveFlag ? SharpDX.Direct3D12.ResourceFlags.AllowDepthStencil : SharpDX.Direct3D12.ResourceFlags.None;

            bHaveFlag = resourceFlags.HasFlag(H1ResourceFlags.AllowRenderTarget);
            result |= bHaveFlag ? SharpDX.Direct3D12.ResourceFlags.AllowRenderTarget : SharpDX.Direct3D12.ResourceFlags.None;

            bHaveFlag = resourceFlags.HasFlag(H1ResourceFlags.AllowSimultaneousAccess);
            result |= bHaveFlag ? SharpDX.Direct3D12.ResourceFlags.AllowSimultaneousAccess : SharpDX.Direct3D12.ResourceFlags.None;

            bHaveFlag = resourceFlags.HasFlag(H1ResourceFlags.AllowUnorderedAccess);
            result |= bHaveFlag ? SharpDX.Direct3D12.ResourceFlags.AllowUnorderedAccess : SharpDX.Direct3D12.ResourceFlags.None;

            bHaveFlag = resourceFlags.HasFlag(H1ResourceFlags.DenyShaderResource);
            result |= bHaveFlag ? SharpDX.Direct3D12.ResourceFlags.DenyShaderResource : SharpDX.Direct3D12.ResourceFlags.None;

            return result;
        }
    }
}
