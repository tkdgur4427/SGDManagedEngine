using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1DX12Device
    {
        public Device Device
        {
            get { return m_Device; }
        }            
        
        public H1DescriptorHeap RenderTargetDescriptorCache
        {
            get { return m_RenderTargetDescriptorCache; }
        }

        public H1DescriptorHeap DepthStencilDescriptorCache
        {
            get { return m_DepthStencilDescriptorCache; }
        }

        public H1DX12Device()
        {
            // create new device
            m_Device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);

            m_RenderTargetDescriptorCache = new H1DescriptorHeap();
            m_DepthStencilDescriptorCache = new H1DescriptorHeap();
        }

        Device m_Device;        

        H1PSOCache m_PSOCache;
        H1RootSignatureCache m_RootSignatureCache;
        H1DataStreamer m_DataStreamer;

        H1DescriptorHeap m_SamplerCache;
        H1DescriptorHeap m_ShaderResourceDescriptorCache;
        H1DescriptorHeap m_UnorderedAccessDescriptorCache;
        H1DescriptorHeap m_DepthStencilDescriptorCache;
        H1DescriptorHeap m_RenderTargetDescriptorCache;

        List<H1DescriptorHeap> m_GlobalDescriptorHeaps = new List<H1DescriptorHeap>();

        Dictionary<UInt32, CpuDescriptorHandle> m_SamplerCacheLookupTable = new Dictionary<uint, CpuDescriptorHandle>();
        Dictionary<UInt32, CpuDescriptorHandle> m_ShaderResourceCacheLookupTable = new Dictionary<uint, CpuDescriptorHandle>();
        Dictionary<UInt32, CpuDescriptorHandle> m_UnorderedAccessCacheLookupTable = new Dictionary<uint, CpuDescriptorHandle>();
        Dictionary<UInt32, CpuDescriptorHandle> m_DepthStencilCacheLookupTable = new Dictionary<uint, CpuDescriptorHandle>();
        Dictionary<UInt32, CpuDescriptorHandle> m_RenderTargetCacheLookupTable = new Dictionary<uint, CpuDescriptorHandle>();

        List<CpuDescriptorHandle> m_SamplerCacheFreeTable = new List<CpuDescriptorHandle>();
        List<CpuDescriptorHandle> m_ShaderResourceCacheFreeTable = new List<CpuDescriptorHandle>();
        List<CpuDescriptorHandle> m_UnorderedAccessCacheFreeTable = new List<CpuDescriptorHandle>();
        List<CpuDescriptorHandle> m_DepthStencilCacheFreeTable = new List<CpuDescriptorHandle>();
        List<CpuDescriptorHandle> m_RenderTargetCacheFreeTable = new List<CpuDescriptorHandle>();

        static object m_SamplerThreadSafeLock = new object();
        static object m_ShaderResourceThreadSafeLock = new object();
        static object m_UnorderedAccessThreadSafeLock = new object();
        static object m_DepthStencilThreadSafeLock = new object();
        static object m_RenderTargetThreadSafeLock = new object();
        static object m_DescriptorAllocatorThreadSafeLock = new object();

        // objects that should be released when they are not in use anymore
        class ReleaseInfo
        {
            UInt32 Hash;
            UInt32 bFlags;
            UInt64[] FenceValues = new UInt64[H1GlobalDX12Definitions.CMDQUEUE_NUM];
        }

        class ReuseInfo
        {
            Resource Object;
            UInt64[] FenceValues = new UInt64[H1GlobalDX12Definitions.CMDQUEUE_NUM];
        }

        Dictionary<Resource, ReleaseInfo> m_ReleaseHeap = new Dictionary<Resource, ReleaseInfo>();
        Dictionary<Resource, ReuseInfo> m_ReuseHeap = new Dictionary<Resource, ReuseInfo>();
    }
}
