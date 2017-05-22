using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public class H1GpuResAllocInfo
    {
        public H1GpuResAllocInfo(H1GpuResource buffer, Int64 offset, Int64 size, IntPtr dataPtr, long gpuAddress)
        {
            BufferRef = buffer; Offset = offset; Size = size; DataPtr = dataPtr; GpuAddress = gpuAddress;
        }

        public H1GpuResource BufferRef;
        public Int64 Offset;
        public Int64 Size;
        public IntPtr DataPtr;
        public long GpuAddress;
    }

    public class H1GpuResLinearAllocator
    {
        public H1GpuResLinearAllocator(H1LinearAllocationType allocType)
        {
            m_LinearAllocationType = allocType;
            switch (allocType)
            {
                case H1LinearAllocationType.GpuExclusive:
                    m_PageSize = Convert.ToInt64(H1AllocatorPageSize.GpuAllocatorPageSize);
                    break;

                case H1LinearAllocationType.CpuWritable:
                    m_PageSize = Convert.ToInt64(H1LinearAllocationType.CpuWritable);
                    break;
            }
        }

        public H1GpuResAllocInfo Allocate(Int64 sizeInBytes, Int64 alignment)
        {
            if (sizeInBytes > m_PageSize)
                return null;

            Int64 alignMask = alignment - 1;
            Int64 isPowerOfTwo = alignMask & alignment;
            if (isPowerOfTwo == 0)
            {
                return null;
            }

            // align the allocation
            Int64 alignedSize = SGD.MathCommon.H1Methods.AlignUpWithMask(sizeInBytes, alignMask);
            m_CurrOffset = SGD.MathCommon.H1Methods.AlignUp(m_CurrOffset, alignment);

            if (m_CurrOffset + alignedSize > m_PageSize)
            {
                m_RetiredPages.Add(m_CurrPageRef);
                m_CurrPageRef = null;
            }

            // if current page is null
            if (m_CurrPageRef == null)
            {
                m_CurrPageRef = m_GpuResPageManager.RequestPage(m_LinearAllocationType);
                m_CurrOffset = 0; // reset the current offset
            }

            H1GpuResAllocInfo result = new H1GpuResAllocInfo(m_CurrPageRef.GpuResource, m_CurrOffset, alignedSize
                , m_CurrPageRef.CpuVirtualAddress
                , m_CurrPageRef.GpuResource.GpuVirtualAddress);

            // update current offset
            m_CurrOffset += alignedSize;

            return result;
        }

        public void CleanupUsedPages(UInt64 fenceId)
        {
            // currently using page need to be discarded
            if (m_CurrPageRef != null)
            {
                m_RetiredPages.Add(m_CurrPageRef);
                // reset the following properties
                m_CurrPageRef = null;
                m_CurrOffset = 0;
            }

            // discard retired pages and clean up the retired pages
            m_GpuResPageManager.DiscardPages(m_LinearAllocationType, fenceId, m_RetiredPages);
            m_RetiredPages.Clear();
        }

        public static bool InitializeGpuResPageManager()
        {
            return m_GpuResPageManager.Initialize();
        }

        public static void DestroyGpuResPageManager()
        {
            m_GpuResPageManager.Destroy();
        }

        // static gpu resource allocation page manager
        private static H1GpuResAllocPageManager m_GpuResPageManager = new H1GpuResAllocPageManager();
        // member variables for gpu resource page allocator
        private H1LinearAllocationType m_LinearAllocationType = new H1LinearAllocationType();
        private Int64 m_PageSize;
        private Int64 m_CurrOffset;
        private H1GpuResAllocPage m_CurrPageRef;
        private List<H1GpuResAllocPage> m_RetiredPages = new List<H1GpuResAllocPage>();
    }
}
