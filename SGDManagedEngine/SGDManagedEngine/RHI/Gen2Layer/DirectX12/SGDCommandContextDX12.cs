using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public partial class H1CommandContext
    {
        public CommandAllocator Allocator
        {
            get { return m_CurrentCommandAllocator; }
            set { m_CurrentCommandAllocator = value; }
        }

        public GraphicsCommandList CommandList
        {
            get { return m_CommandList; }
            set { m_CommandList = value; }
        }

        void InitializePlatformDependent(ref Boolean bSuccess)
        {
            // platform-specific assignment
            DynamicDescriptorHeap = new H1DynamicDescriptorHeap();
            bSuccess = CommandListManager.CreateCommandList(CommandListType, this);
        }        

        void DestroyPlatformDependent()
        {
            if (m_CommandList != null)
                m_CommandList.Dispose();
        }

        // @TODO - need to implmenent this again
        /*
        public Boolean CopyBuffer(H1GpuResource dest, H1GpuResource src)
        {
            H1GpuResource destDX12 = dest;
            H1GpuResource srcDX12 = src;
            if (destDX12 == null || srcDX12 == null)
                return false;

            TransitionResource(dest, H1ResourceStates.CopyDestination);
            TransitionResource(src, H1ResourceStates.CopySource);
            FlushResourceBarriers(); // execute accumulated barriers

            m_CommandList.CopyResource(destDX12.GpuResource, srcDX12.GpuResource);

            return true;
        }

        public Boolean CopyBufferRegion(H1GpuResource dest, Int64 destOffset, H1GpuResource src, Int64 srcOffset, Int64 numBytes)
        {
            H1GpuResource destDX12 = dest;
            H1GpuResource srcDX12 = src;
            if (destDX12 == null || srcDX12 == null)
                return false;

            TransitionResource(dest, H1ResourceStates.CopyDestination);
            FlushResourceBarriers();
            m_CommandList.CopyBufferRegion(destDX12.GpuResource, destOffset, srcDX12.GpuResource, srcOffset, numBytes);

            return true;
        }

        public Boolean CopySubresource(H1GpuResource dest, Int32 destSubIndex, H1GpuResource src, Int32 srcSubIndex)
        {
            H1GpuResource destDX12 = dest;
            H1GpuResource srcDX12 = src;
            if (destDX12 == null || srcDX12 == null)
                return false;

            FlushResourceBarriers();

            TextureCopyLocation destLocation = new TextureCopyLocation(
                destDX12.GpuResource,
                destSubIndex);

            TextureCopyLocation srcLocation = new TextureCopyLocation(
                srcDX12.GpuResource,
                srcSubIndex);

            m_CommandList.CopyTextureRegion(destLocation, 0, 0, 0, srcLocation, null);

            return true;
        }

        public Boolean WriteBuffer(H1GpuResource dest, Int64 destOffset, IntPtr bufferData, Int64 numBytes)
        {
            // allocate temporary space from cpu-linear allocator
            H1GpuResAllocInfo tempSpace = CpuLinearAllocator.Allocate(numBytes, 512);
            // copy the data ptr to the temporary space
            SharpDX.Utilities.CopyMemory(tempSpace.DataPtr, bufferData, Convert.ToInt32(MathCommon.H1Methods.DivideByMultiple(numBytes, 16)));

            // copy from cpu-memory to gpu-memory
            CopyBufferRegion(dest, destOffset, tempSpace.BufferRef, tempSpace.Offset, numBytes);

            return true;
        }
        */

        public Boolean SetDescriptorHeap(DescriptorHeapType type, DescriptorHeap heap)
        {
            Int32 toIndex = Convert.ToInt32(type);
            if (m_CurrDescriptorHeaps[toIndex] != heap)
            {
                m_CurrDescriptorHeaps[toIndex] = heap;
                BindDescriptorHeaps(); // update binded descriptor heaps
            }

            return true;
        }

        public Boolean SetDescirptorHeaps(Int32 heapCount, DescriptorHeapType[] types, DescriptorHeap[] heaps)
        {
            Boolean bAnyChanges = false;

            for (Int32 i = 0; i < heapCount; ++i)
            {
                Int32 typeToIndex = Convert.ToInt32(types[i]);
                if (m_CurrDescriptorHeaps[typeToIndex] != heaps[i])
                {
                    m_CurrDescriptorHeaps[typeToIndex] = heaps[i];
                    bAnyChanges = true;
                }
            }

            // if there is any change, bind descriptor heaps again(update)
            if (bAnyChanges)
                BindDescriptorHeaps();

            return true;
        }

        public void BindDescriptorHeaps()
        {
            Int32 nonNullHeaps = 0;
            List<DescriptorHeap> heapsToBind = new List<DescriptorHeap>();
            for (Int32 i = 0; i < Convert.ToInt32(DescriptorHeapType.NumTypes); ++i)
            {
                DescriptorHeap heap = m_CurrDescriptorHeaps[i];
                if (heap != null)
                {
                    heapsToBind.Add(heap);
                }                
            }

            if (heapsToBind.Count > 0)
                m_CommandList.SetDescriptorHeaps(heapsToBind.Count, heapsToBind.ToArray());
        }

        //@TODO - need to enable this again
        /*
        public Boolean TransitionResource(H1GpuResource resource, H1ResourceStates newState, Boolean flushImmediate = false)
        {
            H1ResourceStates oldState = resource.UsageStates;            

            if (oldState != newState)
            {
                if (m_NumBarriersToFlush >= 16)
                {
                    // failed to transition resource (exceeded arbitrary limit on buffered barriers)
                    return false;
                }

                H1GpuResource resourceDX12 = resource;

                ResourceBarrier barrierDesc = m_ResourceBarrierBuffer[m_NumBarriersToFlush];
                m_NumBarriersToFlush++;

                // initialize barrier description (for transition)
                barrierDesc.Type = ResourceBarrierType.Transition;

                ResourceTransitionBarrier resourceTransitionBarrier = new ResourceTransitionBarrier(
                    resourceDX12.GpuResource,
                    (ResourceStates)H1GpuResource.ResourceStateMapper[Convert.ToInt32(oldState)],
                    (ResourceStates)H1GpuResource.ResourceStateMapper[Convert.ToInt32(newState)]);

                barrierDesc.Transition = resourceTransitionBarrier;

                // check to see if we already started the transition
                if (newState == resource.TransitioningStates)
                {
                    barrierDesc.Flags = ResourceBarrierFlags.EndOnly;
                    resource.TransitioningStates = H1ResourceStates.Invalid;
                }
                else
                    barrierDesc.Flags = ResourceBarrierFlags.None;

                resource.UsageStates = newState;
            }
            else if (newState == H1ResourceStates.UnorderedAccess)
            {
                InsertUAVBarrier(resource, flushImmediate);
            }

            if (flushImmediate || m_NumBarriersToFlush == 16)
                FlushResourceBarriers();

            return true;
        }

        public Boolean BeginResourceTransition(H1GpuResource resource, H1ResourceStates newState, Boolean flushImmediate)
        {
            // if it's already transitioning, finish that transition
            if (resource.TransitioningStates != H1ResourceStates.Invalid)
            {
                TransitionResource(resource, newState);
            }

            H1ResourceStates oldState = resource.UsageStates;
            if (oldState != newState)
            {
                ResourceBarrier barrierDesc = m_ResourceBarrierBuffer[m_NumBarriersToFlush];
                m_NumBarriersToFlush++;

                H1GpuResource gpuResourceDX12 = resource;
                if (gpuResourceDX12 == null)
                    return false;

                barrierDesc.Type = ResourceBarrierType.Transition;
                barrierDesc.Transition = new ResourceTransitionBarrier(gpuResourceDX12.GpuResource
                    , (ResourceStates)H1GpuResource.ResourceStateMapper[Convert.ToInt32(oldState)]
                    , (ResourceStates)H1GpuResource.ResourceStateMapper[Convert.ToInt32(newState)]);

                barrierDesc.Flags = ResourceBarrierFlags.BeginOnly;

                resource.TransitioningStates = newState;
            }

            if (flushImmediate)
                FlushResourceBarriers();

            return true;
        }

        public Boolean InsertUAVBarrier(H1GpuResource resource, Boolean flushImmediate)
        {
            if (m_NumBarriersToFlush >= 16)
            {
                // failed to transition resource (exceeded arbitrary limit on buffered barriers)
                return false;
            }

            H1GpuResource resourceDX12 = resource;

            ResourceBarrier barrierDesc = m_ResourceBarrierBuffer[m_NumBarriersToFlush];
            m_NumBarriersToFlush++;

            barrierDesc.Type = ResourceBarrierType.UnorderedAccessView;
            barrierDesc.Flags = ResourceBarrierFlags.None;
            barrierDesc.UnorderedAccessView = new ResourceUnorderedAccessViewBarrier()
            {
                ResourcePointer = resourceDX12.GpuResource.NativePointer
            };

            if (flushImmediate)
            {
                // currently binded resource barriers are flushed
                FlushResourceBarriers();
            }

            return true;
        }

        public Boolean InsertAliasBarrier(H1GpuResource before, H1GpuResource after, Boolean flushImmediate)
        {
            if (m_NumBarriersToFlush >= 16)
                return false;

            ResourceBarrier barrierDesc = m_ResourceBarrierBuffer[m_NumBarriersToFlush++];
            barrierDesc.Type = ResourceBarrierType.Aliasing;
            barrierDesc.Flags = ResourceBarrierFlags.None;

            H1GpuResource beforeDX12 = before;
            H1GpuResource afterDX12 = after;
            barrierDesc.Aliasing = new ResourceAliasingBarrier(beforeDX12.GpuResource, afterDX12.GpuResource);

            if (flushImmediate)
                FlushResourceBarriers();

            return true;
        }
        */

        public void FlushResourceBarriers()
        {
            if (m_NumBarriersToFlush > 0)
            {
                List<ResourceBarrier> resourceBarriers = new List<ResourceBarrier>();
                for (Int32 i = 0; i < m_NumBarriersToFlush; ++i)
                    resourceBarriers.Add(m_ResourceBarrierBuffer[i]);

                // flush resource barriers
                CommandList.ResourceBarrier(resourceBarriers.ToArray());
            }
        }

        private GraphicsCommandList m_CommandList;
        private CommandAllocator m_CurrentCommandAllocator;
        private ResourceBarrier[] m_ResourceBarrierBuffer = new ResourceBarrier[16];
        private Int32 m_NumBarriersToFlush = 0;
        private DescriptorHeap[] m_CurrDescriptorHeaps = new DescriptorHeap[Convert.ToInt32(DescriptorHeapType.NumTypes)];
    }

    public partial class H1GraphicsCommandContext
    {
        public Boolean ClearUAV(H1GpuBuffer target)
        {             
            H1GpuBuffer targetDX12 = target as H1GpuBuffer;
            H1DynamicDescriptorHeap dynamicDescriptorHeapDX12 = DynamicDescriptorHeap;
            GpuDescriptorHandle gpuVisibleHandle = new GpuDescriptorHandle();
            //if (dynamicDescriptorHeapDX12.UploadDirect(targetDX12.UAV, ref gpuVisibleHandle) == false)
            //{
            //    return false;
            //}

            return true;
        }
    }

    public partial class H1ComputeCommandContext
    {

    }
#endif
}
