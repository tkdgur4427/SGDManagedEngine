using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    enum PipelineStateProperty
    {
        PSP_PipelineState,
        PSP_ConstantBuffers,
        PSP_Resources,
        PSP_Samplers,

        PSP_VertexBuffers,
        PSP_IndexBuffer,
        PSP_PrimitiveTopology,
        PSP_Viewports,
        PSP_RenderTargetViews,
        PSP_DepthStencilView,
        PSP_StencilRef,

        PSP_Last,
    }

    public enum PipelineStatePropertyBits
    {       
        PSPB_PipelineState      = 1 << (PipelineStateProperty.PSP_PipelineState),
        PSPB_ConstantBuffers    = 1 << (PipelineStateProperty.PSP_ConstantBuffers),
        PSPB_Resources          = 1 << (PipelineStateProperty.PSP_Resources),
        PSPB_Samplers           = 1 << (PipelineStateProperty.PSP_Samplers),

        PSPB_VertexBuffers      = 1 << (PipelineStateProperty.PSP_VertexBuffers),
        PSPB_IndexBuffer        = 1 << (PipelineStateProperty.PSP_IndexBuffer),
        PSPB_PrimitiveTopology  = 1 << (PipelineStateProperty.PSP_PrimitiveTopology),
        PSPB_Viewports          = 1 << (PipelineStateProperty.PSP_Viewports),
        PSPB_RenderTargetViews  = 1 << (PipelineStateProperty.PSP_RenderTargetViews),
        PSPB_DepthStencilView   = 1 << (PipelineStateProperty.PSP_DepthStencilView),
        PSPB_StencilRef         = 1 << (PipelineStateProperty.PSP_StencilRef),

        PSPB_OutputResources        = PSPB_RenderTargetViews | PSPB_DepthStencilView,
        PSPB_RenderTargetFormats    = PSPB_RenderTargetViews | PSPB_PipelineState,
        PSPB_DepthStencilFormat     = PSPB_DepthStencilView | PSPB_PipelineState,
        PSPB_InputResources         = PSPB_ConstantBuffers | PSPB_Resources | PSPB_Samplers,
    }

    public class H1ChangeTrackingValue<T> where T : IComparable // should overloaded ICompareable!
    {
        public T Value
        {
            get { return m_Value; }
            set
            {
                if (m_Value.CompareTo(value) != 0)
                {
                    m_Value = value;
                }
            }
        }

        T m_Value;
        PipelineStatePropertyBits P;

       public  H1ChangeTrackingValue(PipelineStatePropertyBits p)
        {
            P = p;
        }        
    }
       
    class H1ChangeTrackingArray<T> where T : IComparable
    {
        public H1ChangeTrackingArray(UInt32 size, PipelineStatePropertyBits p)
        {
            // allocate the value of array
            m_Array = new T[size];
            P = p;
        }

        public T this[Int32 index]
        {
            get { return m_Array[index]; }
            set
            {
                if (m_Array[index].CompareTo(value) != 0)
                {
                    m_Array[index] = value;
                }
            }
        }

        T[] m_Array;
        PipelineStatePropertyBits P;
    }

    public class H1DX12ShaderStageState
    {
        H1ShaderStage Type;
        H1ChangeTrackingValue<H1Shader> Shader = new H1ChangeTrackingValue<H1Shader>(PipelineStatePropertyBits.PSPB_PipelineState);
        H1ChangeTrackingArray<H1DX12Buffer> ConstantBufferViews = new H1ChangeTrackingArray<H1DX12Buffer>(H1GlobalDX12Definitions.COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT, PipelineStatePropertyBits.PSPB_ConstantBuffers);
        H1ChangeTrackingArray<UInt32> ConstantBufferBindRange = new H1ChangeTrackingArray<UInt32>(H1GlobalDX12Definitions.COMMONSHADER_CONSTANT_BUFFER_API_SLOT_COUNT, PipelineStatePropertyBits.PSPB_ConstantBuffers);
        H1ChangeTrackingArray<H1View> ShaderResourceViews = new H1ChangeTrackingArray<H1View>(H1GlobalDX12Definitions.COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT, PipelineStatePropertyBits.PSPB_Resources);
        H1ChangeTrackingArray<H1View> UnorderedAccessViews = new H1ChangeTrackingArray<H1View>(H1GlobalDX12Definitions.COMMONSHADER_INPUT_RESOURCE_SLOT_COUNT, PipelineStatePropertyBits.PSPB_Resources);
        H1ChangeTrackingArray<H1SamplerState> SamplerStates = new H1ChangeTrackingArray<H1SamplerState>(H1GlobalDX12Definitions.COMMONSHADER_SAMPLER_SLOT_COUNT, PipelineStatePropertyBits.PSPB_Samplers);
    }

    public class H1DX12IAState
    {
        H1ChangeTrackingValue<SharpDX.Direct3D.PrimitiveTopology> PrimitiveTopology = new H1ChangeTrackingValue<SharpDX.Direct3D.PrimitiveTopology>(PipelineStatePropertyBits.PSPB_PrimitiveTopology);
        H1ChangeTrackingValue<H1DX12InputLayout> InputLayout = new H1ChangeTrackingValue<H1DX12InputLayout>(PipelineStatePropertyBits.PSPB_PipelineState);
        H1ChangeTrackingArray<H1DX12Buffer> VertexBuffers = new H1ChangeTrackingArray<H1DX12Buffer>(H1GlobalDX12Definitions.IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT, PipelineStatePropertyBits.PSPB_VertexBuffers);
        H1ChangeTrackingArray<UInt32> Strides = new H1ChangeTrackingArray<UInt32>(H1GlobalDX12Definitions.IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT, PipelineStatePropertyBits.PSPB_VertexBuffers);
        H1ChangeTrackingArray<UInt32> Offsets = new H1ChangeTrackingArray<UInt32>(H1GlobalDX12Definitions.IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT, PipelineStatePropertyBits.PSPB_VertexBuffers);
        H1ChangeTrackingValue<UInt32> NumVertexBuffers = new H1ChangeTrackingValue<UInt32>(PipelineStatePropertyBits.PSPB_VertexBuffers);

        H1ChangeTrackingValue<H1DX12Buffer> IndexBuffer = new H1ChangeTrackingValue<H1DX12Buffer>(PipelineStatePropertyBits.PSPB_IndexBuffer);
        H1ChangeTrackingValue<SharpDX.DXGI.Format> IndexBufferFormat = new H1ChangeTrackingValue<SharpDX.DXGI.Format>(PipelineStatePropertyBits.PSPB_IndexBuffer);
        H1ChangeTrackingValue<UInt32> IndexBufferOffset = new H1ChangeTrackingValue<UInt32>(PipelineStatePropertyBits.PSPB_IndexBuffer);
    }

    public class H1DX12RasterizerState
    {
        H1ChangeTrackingValue<H1DepthStencilState> DepthStencilState = new H1ChangeTrackingValue<H1DepthStencilState>(PipelineStatePropertyBits.PSPB_PipelineState);
        H1ChangeTrackingValue<H1RasterizerState> RasterizerState = new H1ChangeTrackingValue<H1RasterizerState>(PipelineStatePropertyBits.PSPB_PipelineState);

        H1ChangeTrackingArray<H1DX12Viewport> Viewports = new H1ChangeTrackingArray<H1DX12Viewport>(H1GlobalDX12Definitions.VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE, PipelineStatePropertyBits.PSPB_Viewports);
        H1ChangeTrackingValue<UInt32> ViewportNum = new H1ChangeTrackingValue<UInt32>(PipelineStatePropertyBits.PSPB_Viewports);

        H1ChangeTrackingArray<H1DX12SissorRect> Sissors = new H1ChangeTrackingArray<H1DX12SissorRect>(H1GlobalDX12Definitions.VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE, PipelineStatePropertyBits.PSPB_Viewports);
        H1ChangeTrackingValue<UInt32> NumSissors = new H1ChangeTrackingValue<UInt32>(PipelineStatePropertyBits.PSPB_Viewports);
        H1ChangeTrackingValue<Boolean> ScissorEnabled = new H1ChangeTrackingValue<Boolean>(PipelineStatePropertyBits.PSPB_Viewports);
    }

    public class H1DX12OutputMergerState
    {
        H1ChangeTrackingValue<H1BlendState> BlendState = new H1ChangeTrackingValue<H1BlendState>(PipelineStatePropertyBits.PSPB_PipelineState);

        H1ChangeTrackingArray<H1View> RenderTargetViews = new H1ChangeTrackingArray<H1View>(H1GlobalDX12Definitions.SIMULTANEOUS_RENDER_TARGET_COUNT, PipelineStatePropertyBits.PSPB_RenderTargetViews);
        H1ChangeTrackingValue<UInt32> NumRenderTargetViews = new H1ChangeTrackingValue<UInt32>(PipelineStatePropertyBits.PSPB_RenderTargetViews);
        H1ChangeTrackingArray<SharpDX.DXGI.Format> RenderTargetFormats = new H1ChangeTrackingArray<SharpDX.DXGI.Format>(H1GlobalDX12Definitions.SIMULTANEOUS_RENDER_TARGET_COUNT, PipelineStatePropertyBits.PSPB_RenderTargetFormats);

        H1ChangeTrackingValue<H1View> DepthStencilView = new H1ChangeTrackingValue<H1View>(PipelineStatePropertyBits.PSPB_DepthStencilView);
        H1ChangeTrackingValue<SharpDX.DXGI.Format> DepthStencilFormat = new H1ChangeTrackingValue<SharpDX.DXGI.Format>(PipelineStatePropertyBits.PSPB_DepthStencilFormat);

        H1ChangeTrackingValue<UInt32> SampleMask = new H1ChangeTrackingValue<UInt32>(PipelineStatePropertyBits.PSPB_PipelineState);
        // DXGI_SAMPLE_DESC - when I need, I need to wrap up!

        H1ChangeTrackingValue<UInt32> StencilRef = new H1ChangeTrackingValue<UInt32>(PipelineStatePropertyBits.PSPB_StencilRef);
    }

    public class H1PipelineState
    {
        private UInt32 m_StateFlags, m_StateFlagsEncountered;
        // general
        private H1DX12ShaderStageState[] m_Stages = new H1DX12ShaderStageState[Convert.ToInt32(H1ShaderStage.Num)];
        // graphics fixed function
        private H1DX12IAState m_InputAssembler;
        private H1DX12RasterizerState m_Rasterizer;
        private H1DX12OutputMergerState m_OutputMerger;
    }
}
