using System;
using System.Collections;
using System.Collections.Generic;

namespace SGDManagedEngine.SGD.Gen2Layer.Memory
{
    public class H1GpuResAllocRange
    {
        // the constructors of H1GpuResAllocRange is protected, which means you can't instantiate this class with constructor, only by static method
        protected H1GpuResAllocRange()
        {
            // alloc range info for H1GpuResAllocPolicySingle            
            m_bSegmented = false;
            m_StartOffset = -1;
            m_Count = -1;
        }

        protected H1GpuResAllocRange(Int32 startOffset, Int32 count)
        {
            // alloc range info for H1GpuResAllocPolicySegmented
            m_bSegmented = true;
            m_StartOffset = startOffset;
            m_Count = count;
        }

        public static H1GpuResAllocRange CreateAllocRangeForSingle()
        {
            return new H1GpuResAllocRange();
        }

        public static H1GpuResAllocRange CreateAllocRangeForSegmented(Int32 offset, Int32 count)
        {
            H1GpuResAllocRange newAllocRange = new H1GpuResAllocRange(offset, count);
            return newAllocRange;
        }

        public Boolean IsSegmented
        {
            get { return m_bSegmented; }
        }

        public Int32 StartOffset
        {
            get { return m_StartOffset; }
        }

        public Int32 Count
        {
            get { return m_Count; }
        }

        private Boolean m_bSegmented;
        private Int32 m_StartOffset;
        private Int32 m_Count;
    }

    public enum H1GpuResAllocPolicyType
    {
        Invalid,
        Single,
        Segmented,
    }

    public class H1GpuResAllocPolicy
    {
        public H1GpuResAllocPolicy(Int64 resourceSizeInBytes)
        {
            m_ResourceSize = resourceSizeInBytes;
        }

        public virtual H1GpuResAllocRange Allocate(Int64 sizeInBytes = 0)
        {
            return null;
        }

        public virtual void Deallocate(H1GpuResAllocRange allocRange)
        {
            throw new NotImplementedException("Need to override and proper call needed!");
        }

        protected H1GpuResAllocPolicyType m_PolicyType = H1GpuResAllocPolicyType.Invalid;
        protected Int64 m_ResourceSize;
    }

    public class H1GpuResAllocPolicySingle : H1GpuResAllocPolicy
    {
        public H1GpuResAllocPolicySingle(Int64 resourceSizeInBytes)
            : base(resourceSizeInBytes)
        {
            m_PolicyType = H1GpuResAllocPolicyType.Single;
        }

        public override H1GpuResAllocRange Allocate(long sizeInBytes = 0)
        {
            return H1GpuResAllocRange.CreateAllocRangeForSingle();
        }

        public override void Deallocate(H1GpuResAllocRange allocRange = null)
        {
            // nothing to do
        }
    }

    public class H1GpuResAllocPolicySegmented : H1GpuResAllocPolicy
    {
        // minimum alignment size in DX12 is 256
        // @TODO - need to consider other platform minimum alignment size like Vulkan
        protected const Int64 DefaultSegmentSize = 256;

        // for heap segment management
        // heap segment size alignment should be at least 512 bytes (for texture)
        public const Int64 DefaultSegmentSizeForHeap = 512;

        public H1GpuResAllocPolicySegmented(Int64 resourceSizeInBytes, Boolean bIsHeap = false, Int64 segmentSize = 0)
            : base(resourceSizeInBytes)
        {
            m_PolicyType = H1GpuResAllocPolicyType.Segmented;
            m_bResourceOrHeap = bIsHeap;

            if (segmentSize == 0) // use default setting for segment size
            {
                if (m_bResourceOrHeap == false) // for resource
                    m_SegmentSize = DefaultSegmentSize;
                else // for heap (GPU Heap)
                    m_SegmentSize = DefaultSegmentSizeForHeap;
            }
            else
            {
                m_SegmentSize = segmentSize;
            }

            // check segment size is aligned of power of 2
            if (MathCommon.H1Methods.IsPowerofTwo(segmentSize) == false)
            {
                throw new InvalidOperationException("segment size is not power of 2, check it!");
            }

            // create alloc bits
            m_SegmentCount = (resourceSizeInBytes + m_SegmentSize - 1) / m_SegmentSize;
            m_Segments = new List<H1GpuResSegment>(Convert.ToInt32(m_SegmentCount));
            m_AllocBits = new BitArray(Convert.ToInt32(m_SegmentCount), false);
        }

        protected class H1GpuResSegment
        {
            // segment info can be contained   
        }

        protected Int32 SearchAllocBits(Int32 startIndex, Int32 Count, Int32 numOfBlocksRequired)
        {
            // if failed to find proper contiguous memory blocks, return -1
            Int32 foundStartIndex = -1;

            Boolean bIsContiguous = false;
            Int32 contiguousBlockCount = 0;

            Int32 endIndex = startIndex + Count;
            for (Int32 searchIndex = startIndex; searchIndex < endIndex; ++searchIndex)
            {
                Boolean allocBit = m_AllocBits[searchIndex];
                if (allocBit == false)
                {
                    bIsContiguous = true;
                    contiguousBlockCount++;
                }
                else
                {
                    bIsContiguous = false;
                    contiguousBlockCount = 0;
                }

                if (bIsContiguous == true && contiguousBlockCount > numOfBlocksRequired)
                {
                    foundStartIndex = searchIndex;
                    break;
                }
            }            

            return foundStartIndex;
        }

        private void MarkRangeAllocBits(Boolean value, Int32 startOffset, Int32 count)
        {
            Int32 endIndex = startOffset + count;
            for (Int32 searchIndex = startOffset; searchIndex < endIndex; searchIndex++)
            {
                m_AllocBits.Set(searchIndex, value);
            }
        }

        public override H1GpuResAllocRange Allocate(Int64 sizeInBytes)
        {
            // calculate number of segments needed to allocate
            Int32 numberOfSegments = Convert.ToInt32((sizeInBytes + m_SegmentSize - 1) / m_SegmentSize);

            // find contiguous blocks matching number of segments needed           
            Int32 startOffset = -1;

            // 1. starting from m_NextBitSearchStartIndex to the end
            startOffset = SearchAllocBits(m_NextBitSearchStartIndex, Convert.ToInt32(m_SegmentCount), numberOfSegments);            

            // 2. still didn't find proper contiguous blocks starting from 0 to m_NextBitSearchStartIndex
            if (startOffset == -1)
            {
                startOffset = SearchAllocBits(0, m_NextBitSearchStartIndex, numberOfSegments);
            }

            // return result
            if (startOffset == -1) // still failed to find the blocks
                return null;

            // mark as allocated
            MarkRangeAllocBits(true, startOffset, numberOfSegments);

            // update next bit to search
            m_NextBitSearchStartIndex = startOffset + numberOfSegments;            

            return H1GpuResAllocRange.CreateAllocRangeForSegmented(startOffset, numberOfSegments);
        }

        public override void Deallocate(H1GpuResAllocRange allocRange)
        {
            // mark as deallocated
            MarkRangeAllocBits(false, allocRange.StartOffset, allocRange.Count);

            // update next bit to search
            m_NextBitSearchStartIndex = allocRange.StartOffset;
        }

        // single segment size
        private Int64 m_SegmentSize;
        private Int64 m_SegmentCount;      

        // deferred creation after setting on resource size and segment size
        private BitArray m_AllocBits = null;
        // for fast tracking available alloc block, caching index next search index
        private Int32 m_NextBitSearchStartIndex = 0;

        private List<H1GpuResSegment> m_Segments = null;

        // whether it is for resource or heap?
        // if it is == 0, it is for resource
        // otherwise (== 1), it is for heap
        private Boolean m_bResourceOrHeap = false;
    }
}
