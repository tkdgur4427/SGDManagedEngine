using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public partial class H1PipelineStateObject
    {
        // common properties
        protected PipelineState m_PipelineStateObject;
    }

    public partial class H1GraphicsPipelineStateObject
    {
        void ConstructPlatformDependentMembers()
        {
            m_GraphicsPipelineStateDesc.NodeMask = 1;
            m_GraphicsPipelineStateDesc.SampleMask = Int32.MaxValue;
            m_GraphicsPipelineStateDesc.SampleDescription.Count = 1; 
        }

        public void SetBlendState(H1BlendStateDescription blendState, Int32 numRenderTargets)
        {
            BlendStateDescription blendStateRef = BlendStateDescription.Default();
            blendStateRef.AlphaToCoverageEnable = blendState.AlphaToCoverageEnable;
            blendStateRef.IndependentBlendEnable = blendState.IndependentBlendEnable;
            
            for (Int32 i = 0; i < numRenderTargets; ++i)
            {
                RenderTargetBlendDescription rtBlendDescDest = blendStateRef.RenderTarget[i];
                H1RenderTargetBlendDescription rtBlendDescSrc = blendState.RenderTargets[i];
                rtBlendDescDest.IsBlendEnabled = rtBlendDescSrc.BlendEnable;
                rtBlendDescDest.LogicOpEnable = rtBlendDescSrc.LogicOpEnable;

                rtBlendDescDest.SourceBlend = H1RHIDefinitionHelper.ConvertToBlend(rtBlendDescSrc.SrcBlend);
                rtBlendDescDest.DestinationBlend = H1RHIDefinitionHelper.ConvertToBlend(rtBlendDescSrc.DestBlend);
                rtBlendDescDest.BlendOperation = H1RHIDefinitionHelper.ConvertToBlendOp(rtBlendDescSrc.BlendOp);

                rtBlendDescDest.SourceAlphaBlend = H1RHIDefinitionHelper.ConvertToBlend(rtBlendDescSrc.SrcBlendAlpha);
                rtBlendDescDest.DestinationAlphaBlend = H1RHIDefinitionHelper.ConvertToBlend(rtBlendDescSrc.DestBlendAlpha);
                rtBlendDescDest.AlphaBlendOperation = H1RHIDefinitionHelper.ConvertToBlendOp(rtBlendDescSrc.BlendOpAlpha);

                rtBlendDescDest.LogicOp = H1RHIDefinitionHelper.ConvertToLogicOp(rtBlendDescSrc.LogicOp);
                rtBlendDescDest.RenderTargetWriteMask = H1RHIDefinitionHelper.ConvertToColorWriteMaskFlags(rtBlendDescSrc.RenderTargetWriteMask);
            }

            // newly assign the created blend state
            m_GraphicsPipelineStateDesc.BlendState = blendStateRef;
        }

        public void SetRasterizerState(H1RasterizerDescription rasterizerDesc)
        {
            RasterizerStateDescription newRasterizerStateDesc = RasterizerStateDescription.Default();
            newRasterizerStateDesc.FillMode = H1RHIDefinitionHelper.ConvertToFillMode(rasterizerDesc.FillMode);
            newRasterizerStateDesc.CullMode = H1RHIDefinitionHelper.ConvertToCullMode(rasterizerDesc.CullMode);
            newRasterizerStateDesc.IsFrontCounterClockwise = rasterizerDesc.FrontCounterClockwise;
            newRasterizerStateDesc.DepthBias = rasterizerDesc.DepthBias;
            newRasterizerStateDesc.DepthBiasClamp = rasterizerDesc.DepthBiasClamp;
            newRasterizerStateDesc.SlopeScaledDepthBias = rasterizerDesc.SlopeScaledDepthBias;
            newRasterizerStateDesc.IsDepthClipEnabled = rasterizerDesc.DepthClipEnable;
            newRasterizerStateDesc.IsMultisampleEnabled = rasterizerDesc.MultiSampleEnable;
            newRasterizerStateDesc.IsAntialiasedLineEnabled = rasterizerDesc.AntialiasedLineEnable;
            newRasterizerStateDesc.ForcedSampleCount = rasterizerDesc.ForcedSampleCount;

            if (rasterizerDesc.ConservativeRasterMode == H1ConservativeRasterizationMode.On)
                newRasterizerStateDesc.ConservativeRaster = ConservativeRasterizationMode.On;
            else
                newRasterizerStateDesc.ConservativeRaster = ConservativeRasterizationMode.Off;

            // assign the newly created rasterizer state desc
            m_GraphicsPipelineStateDesc.RasterizerState = newRasterizerStateDesc;
        }

        public void SetDepthStencilState(H1DepthStencilDescription depthStencilDesc)
        {
            DepthStencilStateDescription newDepthStencilStateDesc = DepthStencilStateDescription.Default();
            newDepthStencilStateDesc.IsDepthEnabled = depthStencilDesc.DepthEnable;
            newDepthStencilStateDesc.DepthWriteMask = H1RHIDefinitionHelper.ConvertToDepthWriteMask(depthStencilDesc.DepthWriteMask);
            newDepthStencilStateDesc.DepthComparison = H1RHIDefinitionHelper.ConvertToComparisonFunc(depthStencilDesc.DepthFunc);
            newDepthStencilStateDesc.IsStencilEnabled = depthStencilDesc.StencilEnable;
            newDepthStencilStateDesc.StencilReadMask = depthStencilDesc.StencilReadMask;
            newDepthStencilStateDesc.StencilWriteMask = depthStencilDesc.StencilWriteMask;

            newDepthStencilStateDesc.FrontFace = new DepthStencilOperationDescription();
            newDepthStencilStateDesc.FrontFace.DepthFailOperation = H1RHIDefinitionHelper.ConvertToStencilOp(depthStencilDesc.FrontFace.StencilDepthFailOp);
            newDepthStencilStateDesc.FrontFace.FailOperation = H1RHIDefinitionHelper.ConvertToStencilOp(depthStencilDesc.FrontFace.StencilFailOp);
            newDepthStencilStateDesc.FrontFace.PassOperation = H1RHIDefinitionHelper.ConvertToStencilOp(depthStencilDesc.FrontFace.StencilPassOp);
            newDepthStencilStateDesc.FrontFace.Comparison = H1RHIDefinitionHelper.ConvertToComparisonFunc(depthStencilDesc.FrontFace.StencilFunc);

            newDepthStencilStateDesc.BackFace = new DepthStencilOperationDescription();
            newDepthStencilStateDesc.BackFace.DepthFailOperation = H1RHIDefinitionHelper.ConvertToStencilOp(depthStencilDesc.BackFace.StencilDepthFailOp);
            newDepthStencilStateDesc.BackFace.FailOperation = H1RHIDefinitionHelper.ConvertToStencilOp(depthStencilDesc.BackFace.StencilFailOp);
            newDepthStencilStateDesc.BackFace.PassOperation = H1RHIDefinitionHelper.ConvertToStencilOp(depthStencilDesc.BackFace.StencilPassOp);
            newDepthStencilStateDesc.BackFace.Comparison = H1RHIDefinitionHelper.ConvertToComparisonFunc(depthStencilDesc.BackFace.StencilFunc);

            m_GraphicsPipelineStateDesc.DepthStencilState = newDepthStencilStateDesc;
        }

        public void SetSampleMask(Int32 sampleMask)
        {
            m_GraphicsPipelineStateDesc.SampleMask = sampleMask;
        }

        public void SetPrimitiveTopologyType(H1PrimitiveTopologyType primitiveTopologyType)
        {
            m_GraphicsPipelineStateDesc.PrimitiveTopologyType = H1RHIDefinitionHelper.ConvertToPrimitiveTopologyType(primitiveTopologyType);
        }

        public void SetRenderTargetFormats(H1PixelFormat[] rtvFormats, H1PixelFormat dsvFormat, Int32 msaaCount, Int32 msaaQuality)
        {
            for (Int32 i = 0; i < rtvFormats.Count(); ++i)
                m_GraphicsPipelineStateDesc.RenderTargetFormats[i] = H1RHIDefinitionHelper.ConvertToFormat(rtvFormats[i]);
            for (Int32 i = rtvFormats.Count(); i < m_GraphicsPipelineStateDesc.RenderTargetCount; ++i)
                m_GraphicsPipelineStateDesc.RenderTargetFormats[i] = SharpDX.DXGI.Format.Unknown;

            m_GraphicsPipelineStateDesc.RenderTargetCount = rtvFormats.Count();
            m_GraphicsPipelineStateDesc.DepthStencilFormat = H1RHIDefinitionHelper.ConvertToFormat(dsvFormat);
            m_GraphicsPipelineStateDesc.SampleDescription.Count = msaaCount;
            m_GraphicsPipelineStateDesc.SampleDescription.Quality = msaaQuality;
        }

        public void SetRenderTargetFormat(H1PixelFormat rtvFormat, H1PixelFormat dsvFormat, Int32 msaaCount, Int32 msaaQuality)
        {
            List<H1PixelFormat> rtvFormats = new List<H1PixelFormat>();
            rtvFormats.Add(rtvFormat);

            SetRenderTargetFormats(rtvFormats.ToArray(), dsvFormat, msaaCount, msaaQuality);
        }

        public Boolean SetInputLayout(H1InputLayout inputLayout)
        {
            List<InputElement> inputElements = new List<InputElement>();
            foreach (H1InputElementDescription element in inputLayout.InputElements)
            {
                InputElement inputElement = new InputElement();
                inputElement.SemanticName = element.SemanticName;
                inputElement.SemanticIndex = element.SemanticIndex;
                inputElement.Format = H1RHIDefinitionHelper.ConvertToFormat(element.Format);
                inputElement.Slot = element.InputSlot;

                inputElements.Add(inputElement);
            }

            m_GraphicsPipelineStateDesc.InputLayout = new InputLayoutDescription(inputElements.ToArray());

            return true;
        }

        public void SetVertexShader(H1Shader shader)
        {
            m_GraphicsPipelineStateDesc.VertexShader = new ShaderBytecode(shader.ShaderByteCode);
        }

        public void SetPixelShader(H1Shader shader)
        {
            m_GraphicsPipelineStateDesc.PixelShader = new ShaderBytecode(shader.ShaderByteCode);
        }

        public void FinalizePipelineState()
        {
            Device deviceRef = H1Global<H1ManagedRenderer>.Instance.Device;
            m_PipelineStateObject = deviceRef.CreateGraphicsPipelineState(m_GraphicsPipelineStateDesc);
        }

        // graphics
        private GraphicsPipelineStateDescription m_GraphicsPipelineStateDesc;
    }

    public partial class H1ComputePipelineStateObject
    {
        // compute
        private ComputePipelineStateDescription m_ComputePipelineStateDesc;
    }
#endif
}
