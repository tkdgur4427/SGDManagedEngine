using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    class H1DX12DeviceContext
    {
        public H1DX12Device Dx12Device
        {
            get { return m_Device; }
        }

        public H1CommandListPool MainCommandListPool
        {
            get { return m_CmdListPools[0]; }
        }

        public H1CommandListFenceSet CommandListFenceSet
        {
            get { return m_CmdFenceSet; }
        }

        public H1DX12DeviceContext()
        {
            
        }        

        public void Intialize()
        {
            // create device
            m_Device = new H1DX12Device();
            // @TODO - organize command list pools
            H1CommandListPool mainCommandListPool = new H1CommandListPool(m_Device);
            mainCommandListPool.Initialize(H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS);
            m_CmdListPools.Add(mainCommandListPool);

            // create command list fence set
            m_CmdFenceSet = new H1CommandListFenceSet(m_Device);
            m_CmdFenceSet.Initialize();
        }

        // dx12 device
        H1DX12Device m_Device;

        UInt64[,] m_FrameFenceValuesSubmitted = new UInt64[H1GlobalDX12Definitions.FRAME_FENCES, H1GlobalDX12Definitions.CMDQUEUE_NUM];
        UInt64[,] m_FrameFenceValuesCompleted = new UInt64[H1GlobalDX12Definitions.FRAME_FENCES, H1GlobalDX12Definitions.CMDQUEUE_NUM];
        UInt64 m_FrameFenceCursor;

        H1CommandListFenceSet m_CmdFenceSet;
        List<H1CommandListPool> m_CmdListPools = new List<H1CommandListPool>();

        H1CommandList[] m_CmdLists = new H1CommandList[H1GlobalDX12Definitions.CMDQUEUE_NUM];
        Boolean[] m_bCmdListBegins = new Boolean[H1GlobalDX12Definitions.CMDQUEUE_NUM];

        H1PSO m_CurrentPSO;
        H1RootSignature m_CurrentRootSignature;

        H1PipelineState m_GraphicsState;
        H1PipelineState m_ComputeState;

        H1DX12Resource m_TimestampDownloadBuffer;
        H1DX12Resource m_OcclusionDownloadBuffer;
        UInt32 m_TimestampIndex;
        UInt32 m_OcclusionIndex;
        // void* m_TimestampMemory, m_OcclusionMemory
        H1DescriptorHeap m_ResourceHeap;
        H1DescriptorHeap m_SamplerHeap;

        H1QueryHeap m_TimestampHeap;
        H1QueryHeap m_OcclusionHeap;
        H1QueryHeap m_PipelineHeap;
    }
}
