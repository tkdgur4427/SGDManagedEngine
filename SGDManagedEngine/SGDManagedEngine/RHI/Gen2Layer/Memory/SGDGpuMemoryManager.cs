using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer.Memory
{
    public class H1SegmentInfo
    {
        public Int64 Start;
        public Int64 Counts;
        public Int32 SegmentSize;
    }

    // H1GpuMemoryBlockInfo is a return-type (used for Allocator)
    public class H1GpuMemoryBlockInfo
    {
        public H1GpuMemoryPoolType PoolType = H1GpuMemoryPoolType.Invalid;
        public Int64 ResourceOrHeapIndex;
        public Boolean bIsPlacedResource;
        public H1GpuResourceHandle ResourceHandle = new H1GpuResourceHandle();
        public H1SegmentInfo SegmentInfo = new H1SegmentInfo();
    }

    public class H1GpuMemoryManager
    {
        public H1GpuMemoryManager()
        {

        }

        // memory pool for three types
        // 1. default
        // 2. readback
        // 3. upload
        private H1GpuMemoryPool[] m_MemoryPools = new H1GpuMemoryPool[Convert.ToInt32(H1GpuMemoryPoolType.Num)];
    }
}
