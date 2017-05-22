using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public enum H1LinearAllocationType
    {
        InvalidAllocator = -1,
        GpuExclusive = 0,
        CpuWritable = 1,
        NumAllocatorTypes,
    }

    public enum H1AllocatorPageSize
    {
        GpuAllocatorPageSize = 0x10000,     // 64KB
        CpuAllocatorPageSize = 0x200000,    // 2MB
    }

    public class H1GpuResAllocPage
    {
        public H1GpuResource GpuResource
        {
            get { return m_ResourceRef; }
        }

        public IntPtr CpuVirtualAddress
        {
            get { return m_CpuAddress; }
        }

        public H1GpuResAllocPage(H1LinearAllocationType linearAllocationType, H1GpuResource resourceRef)
        {
            m_LinearAllocationType = linearAllocationType;
            m_ResourceRef = resourceRef;
        }

        public Boolean Map()
        {
            if (m_ResourceRef == null)
                return false;

            //m_CpuAddress = m_ResourceRef.Map();
            return true;
        }

        public void Unmap()
        {
            //if (m_ResourceRef != null)
            //    m_ResourceRef.Unmap();
        }

        private H1LinearAllocationType m_LinearAllocationType = new H1LinearAllocationType();
        private H1GpuResource m_ResourceRef = null;
        private IntPtr m_CpuAddress = IntPtr.Zero;
    }

    public partial class H1GpuResAllocPageManager
    {        
        public H1GpuResAllocPageManager()
        {

        }

        public bool Initialize()
        {
            for (Int32 i = 0; i < Convert.ToInt32(H1LinearAllocationType.NumAllocatorTypes); ++i)
            {
                m_PagePool[i] = new List<H1GpuResAllocPage>();
                m_RetiredPages[i] = new Queue<Tuple<UInt64, H1GpuResAllocPage>>();
                m_AvailablePages[i] = new Queue<H1GpuResAllocPage>();
            }

            return true;
        }

        public void Destroy()
        {
            for (Int32 i = 0; i < Convert.ToInt32(H1LinearAllocationType.NumAllocatorTypes); ++i)
            {
                m_RetiredPages[i].Clear();
                m_AvailablePages[i].Clear();

                // clear page pool
                foreach (H1GpuResAllocPage allocPage in m_PagePool[i])
                {
                    // make sure page resource to unmap
                    allocPage.Unmap();
                    // destroy resource manually
                    //allocPage.GpuResource.DestroyResource();
                }
            }
        }

        public H1GpuResAllocPage RequestPage(H1LinearAllocationType allocType)
        {
            // check whether available page exists
            Queue<H1GpuResAllocPage> availablePageQueue = m_AvailablePages[Convert.ToInt32(allocType)];            
            if (availablePageQueue.Count > 0)
            {
                return availablePageQueue.Dequeue();
            }

            // crate new pages
            H1GpuResAllocPage newPage = null;
            //CreateAllocPage(allocType, ref newPage);
            //if (newPage == null) // failed to create alloc page
                return null;

            m_PagePool[Convert.ToInt32(allocType)].Add(newPage);
            return newPage;
        }

        public void DiscardPages(H1LinearAllocationType allocType, UInt64 fenceId, List<H1GpuResAllocPage> discardedPages)
        {
            for (Int32 i = 0; i < discardedPages.Count; ++i)
            {
                Queue<Tuple<UInt64, H1GpuResAllocPage>> retiredpagesQueue = m_RetiredPages[Convert.ToInt32(allocType)];
                retiredpagesQueue.Enqueue(new Tuple<UInt64, H1GpuResAllocPage>(fenceId, discardedPages[i]));
            }
        }

        public void SyncDiscardedPages(H1LinearAllocationType allocType, UInt64 fenceId)
        {
            Int32 allocatorTypeIndex = Convert.ToInt32(allocType);
            foreach (Tuple<UInt64, H1GpuResAllocPage> fencedAllocPage in m_RetiredPages[allocatorTypeIndex])
            {
                UInt64 allocPageFenceId = fencedAllocPage.Item1;
                if (allocPageFenceId < fenceId) // looping alloc page in retired pages, check whether current alloc page is allowed to release
                {
                    H1GpuResAllocPage newlyAvailablePage = fencedAllocPage.Item2;
                    m_AvailablePages[allocatorTypeIndex].Enqueue(newlyAvailablePage);
                    m_RetiredPages[allocatorTypeIndex].Dequeue();
                }                    
            }
        }

        protected virtual H1GpuResAllocPage CreateAllocPage(H1LinearAllocationType allocType)
        {
            // platform-specific override is needed
            return null;
        }

        private List<H1GpuResAllocPage>[] m_PagePool = new List<H1GpuResAllocPage>[Convert.ToInt32(H1LinearAllocationType.NumAllocatorTypes)];
        private Queue<Tuple<UInt64, H1GpuResAllocPage> >[] m_RetiredPages = new Queue<Tuple<UInt64, H1GpuResAllocPage>>[Convert.ToInt32(H1LinearAllocationType.NumAllocatorTypes)];
        private Queue<H1GpuResAllocPage>[] m_AvailablePages = new Queue<H1GpuResAllocPage>[Convert.ToInt32(H1LinearAllocationType.NumAllocatorTypes)];
    }
}
