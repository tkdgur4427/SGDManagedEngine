using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    class H1ResourceMappings
    {
        // map of all <stage, slot> pairs to descriptor-tables offsets
        struct H1ShaderInputMapping
        {
            DescriptorRangeType ViewType;
            H1ShaderStage ShaderStage;
            Byte ShaderSlot;
        }

        // order matters
        private readonly H1ShaderInputMapping[] m_RESs = new H1ShaderInputMapping[128];
        private UInt32 m_NumRESs;
        private readonly H1ShaderInputMapping[] m_SMPs = new H1ShaderInputMapping[32];
        private UInt32 m_NumSMPs;

        // memory holding all descriptor-ranges which will be added to the root-signature (basically heap)
        private readonly DescriptorRange[] m_DescRanges = new DescriptorRange[128];
        private UInt32 m_DescRangeCursor;

        // consecutive list of all root-parameter descriptors of all types (points to m_DescRanges)
        private RootParameter[] m_RootParameters = new RootParameter[64];
        private UInt32 m_NumRootParameters;

        // static sampler, directly embedded in the shaders once PSO is created with corresponding root signature
        private StaticSamplerDescription[] m_StaticSampler = new StaticSamplerDescription[16];
        private UInt32 m_NumStaticSamplers;

        // consecutive list of all descriptors which are in the root-signature (without holes)
        struct H1DescriptorTableInfo
        {
            DescriptorHeapType Type;
            UInt32 Offset;
        }

        H1DescriptorTableInfo[] m_DescriptorTables = new H1DescriptorTableInfo[64];
        UInt32 m_NumDescriptorTables;

        // total number of resources bound to all shader stages (CBV+SRV+UAV)
        UInt32 m_NumResources;

        // total number of samplers bound to all shader stages
        UInt32 m_NumDynamicSamplers;
    }

    public class H1RootSignature
    {
        private Int32 m_Hash;
        private RootSignature m_RootSignature;
        private H1ResourceMappings m_ResourceMappings;
    }

    class H1RootSignatureCache
    {
        private H1DX12Device m_DeviceRef;
        private Dictionary<Int32/*Hash*/, H1RootSignature> m_RootSignatureMap = new Dictionary<int, H1RootSignature>();
    }        
}
