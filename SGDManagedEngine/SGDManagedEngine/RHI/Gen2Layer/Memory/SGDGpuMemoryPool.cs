using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer.Memory
{   
    public enum H1GpuMemoryPoolType
    {
        Invalid,
        Default,
        Readback,
        Upload,
        Num
    }

    public class H1GpuMemoryPool
    {
        public H1GpuMemoryPoolType Type
        {
            get { return m_Type; }
        }

        protected H1GpuMemoryPool(H1GpuMemoryPoolType type)
        {
            m_Type = type;
        }

        private H1GpuMemoryPoolType m_Type = H1GpuMemoryPoolType.Invalid;

        private List<Memory.H1GpuHeap> m_Heaps = new List<Memory.H1GpuHeap>();
        private List<H1GpuResourceSingle> m_Resources = new List<H1GpuResourceSingle>();
        private List<H1GpuResourceSegmented> m_SegmentedResources = new List<H1GpuResourceSegmented>();
    }
}
