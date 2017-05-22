using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public class H1CommandAllocatorPoolDX12
    {
        public H1CommandAllocatorPoolDX12(H1CommandListType type, Device deviceRef)
        {
            m_Type = type;

            // assign device reference
            m_DeviceRef = deviceRef;
        }

        public CommandAllocator RequestCommandAllocator()
        {
            CommandAllocator newCommandAllocator = null;
            
            if (m_AvailableAllocators.Count > 0)
            {
                // when there are available allocators
                newCommandAllocator = m_AvailableAllocators.Dequeue();
            }

            // if there is no available allocator, create new allocator
            if (newCommandAllocator == null)
            {
                switch (m_Type)
                {
                    case H1CommandListType.Direct:
                        newCommandAllocator = m_DeviceRef.CreateCommandAllocator(CommandListType.Direct);
                        break;
                    case H1CommandListType.Compute:
                        newCommandAllocator = m_DeviceRef.CreateCommandAllocator(CommandListType.Compute);
                        break;
                    case H1CommandListType.Copy:
                        newCommandAllocator = m_DeviceRef.CreateCommandAllocator(CommandListType.Copy);
                        break;
                    case H1CommandListType.Bundle:
                        newCommandAllocator = m_DeviceRef.CreateCommandAllocator(CommandListType.Bundle);
                        break;
                }

                // add new allocator to the pool
                m_AllocatorPool.Add(newCommandAllocator);             
            }

            return newCommandAllocator;
        }

        public Boolean ReleaseCommandAllocator(CommandAllocator commandAllocator)
        {
            // check if the current command allocator is exists in the pool
            Boolean bExist = false;
            foreach (CommandAllocator allocator in m_AllocatorPool)
            {
                if (allocator == commandAllocator)
                {
                    bExist = true;
                    break;
                }                    
            }

            if (bExist == false) // if the allocator doesn't exists in the pool, return false
                return false;

            // the current command allocator push to the available queue
            m_AvailableAllocators.Enqueue(commandAllocator);

            return true;
        }

        public void Destroy()
        {
            // empty the available allocator pools
            while (m_AvailableAllocators.Count > 0)
                m_AvailableAllocators.Dequeue();

            // looping the pool, release command allocators
            foreach (CommandAllocator commandAllocator in m_AllocatorPool)
                commandAllocator.Dispose();

            // empty the pool
            m_AllocatorPool.Clear();
        }

        private H1CommandListType m_Type;
        private Device m_DeviceRef = null;
        private List<CommandAllocator> m_AllocatorPool = new List<CommandAllocator>();
        private Queue<CommandAllocator> m_AvailableAllocators = new Queue<CommandAllocator>();
    }

    public partial class H1CommandQueue
    {
        public CommandQueue Queue
        {
            get { return m_CommandQueue; }
        }

        public Fence FenceValue
        {
            get
            {
                H1GpuFence fenceDX12 = m_Fence;
                return fenceDX12.FenceValue;
            }
        }

        void InitializePlatformDependent()
        {
            // get the device reference
            m_DeviceRef = H1Global<H1ManagedRenderer>.Instance.Device;

            // platform-dependent assignments
            m_Fence = new H1GpuFence();
            m_CommandAllocatorPool = new H1CommandAllocatorPoolDX12(m_Type, m_DeviceRef);

            // create command queue (depending on command queue type)
            CommandQueueDescription? commandQueueDesc = null;
            switch (m_Type)
            {
                case H1CommandListType.Bundle:
                    commandQueueDesc = new CommandQueueDescription(CommandListType.Bundle);
                    break;
                case H1CommandListType.Compute:
                    commandQueueDesc = new CommandQueueDescription(CommandListType.Compute);
                    break;
                case H1CommandListType.Direct:
                    commandQueueDesc = new CommandQueueDescription(CommandListType.Direct);
                    break;
                case H1CommandListType.Copy:
                    commandQueueDesc = new CommandQueueDescription(CommandListType.Copy);
                    break;
            }

            // try to create command queue
            if (commandQueueDesc != null) 
                m_CommandQueue = m_DeviceRef.CreateCommandQueue(commandQueueDesc.Value);
        }

        public Boolean HasCommandQueue()
        {
            return m_CommandQueue != null;
        }

        void DestroyPlatformDependent()
        {
            // release the command queue
            m_CommandQueue.Dispose();

            // release command allocator pools
            m_CommandAllocatorPool.Destroy();
        }

        // platform-specific implementations (methods)
        public Boolean RequestAllocator(H1CommandContext commandContext)
        {
            commandContext.Allocator = m_CommandAllocatorPool.RequestCommandAllocator();
            if (commandContext.Allocator == null)
                return false;

            return true;
        }

        public Int64 IncrementFence()
        {
            H1GpuFence fenceDX12 = m_Fence;
            m_CommandQueue.Signal(fenceDX12.FenceValue, fenceDX12.NextFrameFenceValue);
            return fenceDX12.IncreNextFenceValue();
        }

        public Boolean IsFenceComplete(Int64 fenceValue)
        {
            H1GpuFence fenceDX12 = m_Fence;
            if (fenceValue > fenceDX12.LastCompletedFenceValue)
                fenceDX12.UpdateLastCompletedFenceValue();

            return fenceValue <= fenceDX12.LastCompletedFenceValue;
        }

        private Device m_DeviceRef = null;
        private CommandQueue m_CommandQueue = null;
        // platform specific pool for command allocator
        private H1CommandAllocatorPoolDX12 m_CommandAllocatorPool;
    }
#endif
}
