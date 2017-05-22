using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public class H1RootParameterDX12
    {
        public RootParameter Parameter
        {
            get { return m_RootParameter.Value; }
        }

        public void Clear()
        {
            // nullify root parameter
            m_RootParameter = null;
        }

        Boolean InitAsConstants(Int32 register, Int32 numDWORDs, ShaderVisibility visibility = ShaderVisibility.All)
        {
            if (m_RootParameter != null)
                return false;

            m_RootParameter = new RootParameter(visibility, new RootConstants(register, 0, numDWORDs));
            return true;
        }

        Boolean InitAsConstantBuffer(Int32 register, ShaderVisibility visibility = ShaderVisibility.All)
        {
            if (m_RootParameter != null)
                return false;

            m_RootParameter = new RootParameter(visibility, new DescriptorRange(DescriptorRangeType.ConstantBufferView, 1, register));
            return true;
        }

        Boolean InitAsBufferSRV(Int32 register, ShaderVisibility visibility = ShaderVisibility.All)
        {
            if (m_RootParameter != null)
                return false;

            m_RootParameter = new RootParameter(visibility, new DescriptorRange(DescriptorRangeType.ShaderResourceView, 1, register));
            return true;
        }

        Boolean InitAsBufferUAV(Int32 register, ShaderVisibility visibility = ShaderVisibility.All)
        {
            if (m_RootParameter != null)
                return false;

            m_RootParameter = new RootParameter(visibility, new DescriptorRange(DescriptorRangeType.UnorderedAccessView, 1, register));
            return true;
        }

        Boolean InitAsDescriptorRange(DescriptorRangeType type, Int32 register, Int32 counts, ShaderVisibility visibility = ShaderVisibility.All)
        {
            if (m_RootParameter != null)
                return false;

            m_RootParameter = new RootParameter(visibility, new DescriptorRange(type, counts, register));
            return true;
        }

        private RootParameter? m_RootParameter;
    }

    /*
     * maximum 64DWORDS divided up against all root parameters
     * root constants = 1 DWORD * NumConstants
     * root descriptors (CBV, SRV or UAV) = 2 DWORDs each
     * descriptor table pointer = 1 DWORD
     * static samplers = 0 DWROD (compiled into shader)
     */
    public partial class H1RootSignature
    {
        // root parameter indexer
        public H1RootParameterDX12 this[Int32 index]
        {
            get { return m_ParamArray[index]; }
        }

        void ConstructPlatformDependentMembers()
        {
            // platform-specific initialization
            Reset();
        }

        public void Reset()
        {
            if (m_NumParameters > 0)
                m_ParamArray = new H1RootParameterDX12[m_NumParameters];
            else
                m_ParamArray = null;

            if (m_NumSamplers > 0)
                m_SamplerArray = new StaticSamplerDescription[m_NumSamplers];
            else
                m_SamplerArray = null;
        }

        public Boolean InitStaticSampler(Int32 register, H1SamplerDescription samplerDesc, ShaderVisibility visibility)
        {
            if (m_NumInitializedStaticSamplers > m_NumSamplers)
                return false; // there are no available to initialize static samplers

            StaticSamplerDescription staticSamplerDesc = m_SamplerArray[m_NumInitializedStaticSamplers];
            staticSamplerDesc = new StaticSamplerDescription(visibility, register, 0); // initialize static sampler description
            staticSamplerDesc.Filter = H1RHIDefinitionHelper.ConvertToFilter(samplerDesc.Filter);
            staticSamplerDesc.AddressU = H1RHIDefinitionHelper.ConvertToTextureAddressMode(samplerDesc.AddressU);
            staticSamplerDesc.AddressV = H1RHIDefinitionHelper.ConvertToTextureAddressMode(samplerDesc.AddressV);
            staticSamplerDesc.AddressW = H1RHIDefinitionHelper.ConvertToTextureAddressMode(samplerDesc.AddressW);
            staticSamplerDesc.MipLODBias = samplerDesc.MipLODBias;
            staticSamplerDesc.MaxAnisotropy = samplerDesc.MaxAnisotropy;
            staticSamplerDesc.ComparisonFunc = H1RHIDefinitionHelper.ConvertToComparisonFunc(samplerDesc.ComparisonFunc);
            staticSamplerDesc.BorderColor = StaticBorderColor.OpaqueWhite;
            staticSamplerDesc.MinLOD = samplerDesc.MinLOD;
            staticSamplerDesc.MaxLOD = samplerDesc.MaxLOD;
            
            if (staticSamplerDesc.AddressU == TextureAddressMode.Border
                || staticSamplerDesc.AddressV == TextureAddressMode.Border
                || staticSamplerDesc.AddressW == TextureAddressMode.Border)
            {
                //@TODO - warning for the case, different border color with 'samplerDesc'
            }

            return true;
        }

        public Boolean Finalize(RootSignatureFlags flags)
        {
            if (m_Finalized)
                return false;

            if (m_NumInitializedStaticSamplers != m_NumSamplers)
                return false;

            // make root parameter array
            List<RootParameter> rootParams = new List<RootParameter>();
            foreach (H1RootParameterDX12 param in m_ParamArray)
                rootParams.Add(param.Parameter);

            // create root signature description
            RootSignatureDescription rootSigDesc = new RootSignatureDescription(flags, rootParams.ToArray(), m_SamplerArray.ToArray());
                        
            // clear descriptor table bit map
            for (Int32 i = 0; i < m_DescriptorTableBitMap.Count; ++i)
                m_DescriptorTableBitMap[i] = false;

            m_MaxDescriptorCacheHandleCount = 0;

            for (Int32 param = 0; param < m_NumParameters; ++param)
            {
                RootParameter rootParam = rootSigDesc.Parameters[param];
                m_DescriptorTableSize[param] = 0;

                if (rootParam.ParameterType == RootParameterType.DescriptorTable)
                {
                    // if there is no descriptor range for descriptor table
                    if (rootParam.DescriptorTable.Count() == 0)
                        return false;

                    // we don't care about sampler descriptor tables
                    // we don't manage them in descriptor cache
                    if (rootParam.DescriptorTable[0].RangeType == DescriptorRangeType.Sampler)
                        continue;

                    // set the descriptor table bit map as true
                    m_DescriptorTableBitMap[param] = true;
                    // looping descriptor table and calculate descriptor table size for current root parameter
                    for (Int32 tableRange = 0; tableRange < rootParam.DescriptorTable.Count(); ++tableRange)
                        m_DescriptorTableSize[param] += rootParam.DescriptorTable[tableRange].DescriptorCount;

                    m_MaxDescriptorCacheHandleCount += m_DescriptorTableSize[param];
                }
            }

            // create root signature instance
            Device deviceRef = H1Global<H1ManagedRenderer>.Instance.Device;
            m_RootSignature = deviceRef.CreateRootSignature(rootSigDesc.Serialize());

            // mark as finalized
            m_Finalized = true;

            return true;
        }

        private H1RootParameterDX12[] m_ParamArray;
        private StaticSamplerDescription[] m_SamplerArray;
        private RootSignature m_RootSignature;
    }
#endif
}
