using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1CommandListPool
    {
        public Direct3D12.H1DX12Device Device
        {
            get { return m_DeviceRef; }
        }

        public CommandQueue CommandQueue
        {
            get { return m_CmdQueue; }
        }

        public H1CommandListPool(H1DX12Device device)
        {
            m_DeviceRef = device;            
        }

        public void Initialize(UInt32 CmdQueueType)
        {
            // 1. create command queue
            CommandQueueDescription? descCmdQueue = null;
            switch (CmdQueueType)
            {
                case H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS:
                    descCmdQueue = new CommandQueueDescription(CommandListType.Direct);
                    break;
                case H1GlobalDX12Definitions.CMDQUEUE_COMPUTE:
                    descCmdQueue = new CommandQueueDescription(CommandListType.Compute);
                    break;
                case H1GlobalDX12Definitions.CMDQUEUE_COPY:
                    descCmdQueue = new CommandQueueDescription(CommandListType.Copy);
                    break;
            }

           if (descCmdQueue != null)
            m_CmdQueue = m_DeviceRef.Device.CreateCommandQueue(descCmdQueue.Value);
        }

        H1DX12Device m_DeviceRef;
        H1CommandListFenceSet m_CmdFencesRef;
        Int32 m_PoolFenceId;
        CommandListType m_PoolType;

        Queue<H1CommandList> m_LiveCommandLists = new Queue<H1CommandList>();
        Queue<H1CommandList> m_BusyCommandLists = new Queue<H1CommandList>();
        Queue<H1CommandList> m_FreeCommandLists = new Queue<H1CommandList>();

        CommandQueue m_CmdQueue;
        H1AsyncCommandQueue m_AsyncCommandQueue;
    }

    public class H1CommandList
    {
        public CommandAllocator CommandAllocator
        {
            get { return m_CmdAllocator; }
        }

        public GraphicsCommandList CommandList
        {
            get { return m_CmdList; }
        }

        public H1CommandList(H1DX12Device refDevice, H1CommandListPool refCommandListPool)
        {
            m_DeviceRef = refDevice;
            m_PoolRef = refCommandListPool;
            m_CmdQueueRef = refCommandListPool.CommandQueue;            
        }

        public void Initialize()
        {
            m_CmdAllocator = m_DeviceRef.Device.CreateCommandAllocator(CommandListType.Direct);

            // create command list
            m_CmdList = m_DeviceRef.Device.CreateCommandList(CommandListType.Direct, m_CmdAllocator, null);
            // in this point, there is nothing to record, so close it
            m_CmdList.Close();
        }

        H1CommandListPool m_PoolRef;
        CommandQueue m_CmdQueueRef;
        H1DX12Device m_DeviceRef;

        GraphicsCommandList m_CmdList;
        CommandAllocator m_CmdAllocator;
        
        UInt64[,] m_UsedFenceValues = new UInt64[H1GlobalDX12Definitions.CMDTYPE_NUM, H1GlobalDX12Definitions.CMDQUEUE_NUM];

        // only used by IsUsedByOutputViews()
        H1View m_DSV;
        H1View[] m_RTVs = new H1View[H1GlobalDX12Definitions.SIMULTANEOUS_RENDER_TARGET_COUNT];
        Int32 m_CurrentNumRTVs;

        UInt64 m_CurrentFenceValue;
        CommandListType m_ListType;

        List<H1DescriptorBlock> m_DescriptorHeaps = new List<H1DescriptorBlock>();

        Int32 m_CurrentNumVertexBuffers;
        VertexBufferView[] m_VertexBufferHeap = new VertexBufferView[H1GlobalDX12Definitions.VERTEX_INPUT_RESOURCE_SLOT_COUNT];

        enum State
        {
            Free,
            Started,
            Utilized,
            Completed,
            Scheduled,
            Submitted,
            Finished,
            Cleaning,
        }

        State m_State;
        UInt32 m_Commands;
    }
}
