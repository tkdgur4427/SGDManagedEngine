using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;
using SGDManagedEngine.SGD.Gen2Layer;

namespace SGDManagedEngine.SGD.Gen2Layer.Memory
{
#if SGD_DX12
    public partial class H1GpuHeap
    {
        protected Boolean CreateHeapPlatformDependent()
        {
            // get device ptr from the renderer
            Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Device;

            // create dx12 heap description
            m_HeapDesc = new HeapDescription();
                       
            // depending on H1GpuHeap type setting flags
            switch (m_Type)
            {
                case H1GpuHeapType.Buffers:
                    m_HeapDesc.Flags |= HeapFlags.AllowOnlyBuffers;
                    break;

                case H1GpuHeapType.GpuWriteableTexture:
                    m_HeapDesc.Flags |= HeapFlags.AllowOnlyRtDomainShaderTextureS;
                    break;

                case H1GpuHeapType.NonGpuWritableTextures:
                    m_HeapDesc.Flags |= HeapFlags.AllowOnlyNonRtDomainShaderTextureS;
                    break;
            }

            m_HeapDesc.Alignment = 0;
            m_HeapDesc.SizeInBytes = m_TotalSizeInBytes;

            HeapType heapType = HeapType.Custom;
            switch (m_Owner.Type)
            {
                case H1GpuMemoryPoolType.Default:
                    heapType = HeapType.Default;
                    break;

                case H1GpuMemoryPoolType.Readback:
                    heapType = HeapType.Readback;
                    break;

                case H1GpuMemoryPoolType.Upload:
                    heapType = HeapType.Upload;
                    break;
            }

            if (heapType == HeapType.Custom)
                throw new InvalidOperationException("you can not creat custom H1GpuHeap, please check owner's type!");
            
            m_HeapDesc.Properties = new HeapProperties(heapType);

            // creat heap instance
            m_HeapInstance = deviceDX12.CreateHeap(m_HeapDesc);

            if (m_HeapInstance != null)
                return false;

            return true;
        }

        H1GpuHeapResourceInfo CreatePlacedResourceSinglePlatformDependent(H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {
            // get device ptr from the renderer
            Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Device;

            H1GpuResourceSingle newRes = H1GpuResourceSingle.CreateEmptyGpuResource();
                        
            // convert H1ResourceDesc to ResourceDesc for dx12
            ResourceDescription resourceDescDX12 = H1GpuResource.ConvertToResourceDescDx12(resourceDesc);
            ResourceStates defaultStates = (ResourceStates)H1GpuResource.ResourceStateMapper[Convert.ToInt32(defaultUsage)];

            // get resource allocation info and find proper resource size and alignments
            ResourceAllocationInformation allocInfo = deviceDX12.GetResourceAllocationInfo(0, resourceDescDX12);
            Int64 sizeInBytes = allocInfo.SizeInBytes;
            Int64 alignment = allocInfo.Alignment;

            if (alignment > 512)
                throw new InvalidOperationException("alignment is bigger than 512 bytes, please check!");

            // request number of segments to resource alloc policy (segments)
            H1GpuResAllocRange newRange = m_HeapResAllocPolicy.Allocate(sizeInBytes);
            if (newRange == null) // there is no current available blocks to allocate
                return null;

            // with new range, 
            Int64 offsetInBytes = newRange.StartOffset * H1GpuResAllocPolicySegmented.DefaultSegmentSizeForHeap;
            Resource newResDX12 = deviceDX12.CreatePlacedResource(m_HeapInstance, offsetInBytes, resourceDescDX12, defaultStates);
            if (newResDX12 == null) // failed to create placed resource
                return null;

            // set GpuResource for newly created placed resource            
            newRes.SetPlacedResourcePlatformDependent(newResDX12);

            // create heap resource info
            H1GpuHeapResourceInfo newResourceInfo = new H1GpuHeapResourceInfo()
            {
                Resource = newRes,
                ResourceAllocRange = newRange
            };

            return newResourceInfo;
        }

        H1GpuHeapResourceInfo CreatePlacedResourceSegmentedPlatformDependent(H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {
            // @TODO - need to implemented
            throw new NotImplementedException();
        }

        protected HeapDescription m_HeapDesc;
        protected Heap m_HeapInstance = null;
    }
#endif
}
