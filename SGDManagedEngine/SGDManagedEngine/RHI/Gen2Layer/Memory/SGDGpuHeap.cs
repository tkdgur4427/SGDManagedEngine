using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer.Memory
{
    // to support tier 1 hardware spec
    // manage three types of GPU Heaps (in-place memory allocation)
    public enum H1GpuHeapType
    {
        Buffers,
        NonGpuWritableTextures,
        GpuWriteableTexture
    }
        
    public partial class H1GpuHeap
    {
        public class H1GpuHeapResourceInfo
        {                  
            // Resource can be either:
            // 1. H1GpuResourceSingle
            // 2. H1GpuResourceSegmented
            public H1GpuResource Resource = null;
            public H1GpuResAllocRange ResourceAllocRange = null;
        }

        protected H1GpuHeap(H1GpuHeapType type, Int64 totalSizeInBytes)
        {
            m_Type = type;
            m_TotalSizeInBytes = totalSizeInBytes;
        }

        public static H1GpuHeap CreateGpuHeap(H1GpuMemoryPool ownerRef, H1GpuHeapType type, Int64 sizeInBytes)
        {
            H1GpuHeap newHeap = new H1GpuHeap(type, sizeInBytes);
            newHeap.m_Owner = ownerRef;

            if (!newHeap.CreateHeapPlatformDependent())
                return null; // failed to create heap instance

            // create heap allocation policy (segmented)
            newHeap.m_HeapResAllocPolicy = new H1GpuResAllocPolicySegmented(newHeap.m_TotalSizeInBytes, true);

            return newHeap;
        }

        public H1GpuResourceSingle CreatePlacedResourceSingle(H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {
            H1GpuHeapResourceInfo newResSingle = CreatePlacedResourceSinglePlatformDependent(resourceDesc, defaultUsage);
            m_ResourceSingles.Add(newResSingle);
            
            return newResSingle.Resource as H1GpuResourceSingle;
        }

        public H1GpuResourceSegmented CreatePlacedResourceSegmented(H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {
            H1GpuHeapResourceInfo newResSingle = CreatePlacedResourceSegmentedPlatformDependent(resourceDesc, defaultUsage);
            m_ResourceSegmenteds.Add(newResSingle);

            return newResSingle.Resource as H1GpuResourceSegmented;
        }

        private H1GpuMemoryPool m_Owner = null;
        private H1GpuHeapType m_Type;
        private Int64 m_TotalSizeInBytes;

        // resource managements
        private List<H1GpuHeapResourceInfo> m_ResourceSingles = new List<H1GpuHeapResourceInfo>();
        private List<H1GpuHeapResourceInfo> m_ResourceSegmenteds = new List<H1GpuHeapResourceInfo>();

        // heap allocation policy
        private H1GpuResAllocPolicy m_HeapResAllocPolicy = null;
    }
}
