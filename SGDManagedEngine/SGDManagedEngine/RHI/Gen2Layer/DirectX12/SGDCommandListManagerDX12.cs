using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public partial class H1CommandListManager
    {
        void InitializePlatformDependent()
        {
            m_DeviceRef = H1Global<H1ManagedRenderer>.Instance.Device;            
        }

        public Boolean CreateCommandList(H1CommandListType type, H1CommandContext commandContext)
        {
            H1CommandContext commandContextDX12 = commandContext;
            if (commandContextDX12 == null)
                return false; // inappropriate type for command context DX12

            // request command allocator from command queue and create command list
            switch (type)
            {
                case H1CommandListType.Direct:
                    {
                        H1CommandQueue graphicsQueueDX12 = m_GraphicsQueue;
                        graphicsQueueDX12.RequestAllocator(commandContextDX12);
                        commandContextDX12.CommandList = m_DeviceRef.CreateCommandList(CommandListType.Direct, commandContextDX12.Allocator, null);
                        break;
                    }
                    
                case H1CommandListType.Bundle:
                    {
                        commandContextDX12.CommandList = m_DeviceRef.CreateCommandList(CommandListType.Bundle, null, null);
                        break;
                    }
                case H1CommandListType.Compute:
                    {
                        H1CommandQueue computeQueueDX12 = m_ComputeQueue;
                        computeQueueDX12.RequestAllocator(commandContextDX12);
                        commandContextDX12.CommandList = m_DeviceRef.CreateCommandList(CommandListType.Compute, commandContextDX12.Allocator, null);
                        break;
                    }
                case H1CommandListType.Copy:
                    {
                        H1CommandQueue copyQueueDX12 = m_ComputeQueue;
                        copyQueueDX12.RequestAllocator(commandContextDX12);
                        commandContextDX12.CommandList = m_DeviceRef.CreateCommandList(CommandListType.Copy, commandContextDX12.Allocator, null);
                        break;
                    }
            }

            return true;
        }

        public void StallForProducer(H1CommandListType ProducerType, H1CommandListType ConsumerType)
        {
            H1CommandQueue producerQueue = GetQueue(ProducerType);
            H1CommandQueue consumerQueue = GetQueue(ConsumerType);

            // consumer queue wait for producer queue to reach to the 'NextFrameFenceValue'
            consumerQueue.Queue.Wait(producerQueue.FenceValue, producerQueue.Fence.NextFrameFenceValue);
        }

        public void StallForProducer(H1CommandListType ProducerType, H1CommandListType ConsumerType, long fenceValue)
        {
            H1CommandQueue producerQueue = GetQueue(ProducerType);
            H1CommandQueue consumerQueue = GetQueue(ConsumerType);

            // consumer queue wait for producer queue to reach to the certain fence value
            consumerQueue.Queue.Wait(producerQueue.FenceValue, fenceValue);
        }

        public void WaitForFence(H1CommandListType type, Int64 fenceValue)
        {
            H1CommandQueue cmdQueue = GetQueue(type);
            if (cmdQueue.IsFenceComplete(fenceValue))
                return; // the cmd queue is already completed with fenceValue

            // @TODO by mini-engine
            // think about how this might affect a multi-threaded situation
            // suppose thread A wants to wait for fence 100, then thread B comes along and wants to wait for 99
            // if the fence can only have one event set on completion, then thread B has to wait for 100 before it knows 99 is ready
            // maybe insert sequential events?
            {
                // @TODO - synchronization needed
                cmdQueue.FenceValue.SetEventOnCompletion(fenceValue, cmdQueue.Fence.FenceEvent.Handle);
                cmdQueue.Fence.FenceEvent.WaitOne();
                cmdQueue.Fence.UpdateLastCompletedFenceValue(fenceValue);
            }
        }

        public void WaitForFence(Int64 fenceValue)
        {        
            // @TODO by mini-engine
            // think about how this might affect a multi-threaded situation
            // suppose thread A wants to wait for fence 100, then thread B comes along and wants to wait for 99
            // if the fence can only have one event set on completion, then thread B has to wait for 100 before it knows 99 is ready
            // maybe insert sequential events?
            {
                SetEventOnCompletion(fenceValue);

                // get all wait handles from all command queues and wait all to reach fence value
                WaitHandle.WaitAll(GetWaitHandles());

                UpdateLastCompletedFenceValue(fenceValue);
            }
        }

        private AutoResetEvent[] GetWaitHandles()
        {
            List<AutoResetEvent> queueWaitHandles = new List<AutoResetEvent>();
            queueWaitHandles.Add(m_GraphicsQueue.Fence.FenceEvent);
            queueWaitHandles.Add(m_ComputeQueue.Fence.FenceEvent);
            queueWaitHandles.Add(m_CopyQueue.Fence.FenceEvent);

            return queueWaitHandles.ToArray();
        }

        private void SetEventOnCompletion(Int64 fenceValue)
        {
            H1CommandQueue grahpicsQueue = GetQueue(H1CommandListType.Direct);
            H1CommandQueue computeQueue = GetQueue(H1CommandListType.Compute);
            H1CommandQueue copyQueue = GetQueue(H1CommandListType.Copy);

            grahpicsQueue.FenceValue.SetEventOnCompletion(fenceValue, grahpicsQueue.Fence.FenceEvent.Handle);
            computeQueue.FenceValue.SetEventOnCompletion(fenceValue, computeQueue.Fence.FenceEvent.Handle);
            copyQueue.FenceValue.SetEventOnCompletion(fenceValue, copyQueue.Fence.FenceEvent.Handle);
        }

        private void UpdateLastCompletedFenceValue(Int64 fenceValue)
        {
            m_GraphicsQueue.Fence.UpdateLastCompletedFenceValue(fenceValue);
            m_ComputeQueue.Fence.UpdateLastCompletedFenceValue(fenceValue);
            m_CopyQueue.Fence.UpdateLastCompletedFenceValue(fenceValue);
        }

        private Device m_DeviceRef = null;
    }
#endif
}
