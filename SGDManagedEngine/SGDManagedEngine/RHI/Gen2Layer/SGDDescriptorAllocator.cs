using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    // this is unbounded resource descriptor allocator
    // it is intended to provide space for CPU-visible resource descriptors as resources are created
    // for those that need to be made shader-visible, they will need to be copied to a UserDescriptorHeap or DynamicDescriptorHeap
    public partial class H1DescriptorAllocator
    {
        public static Int32 NumDescriptorsPerHeap
        {
            get { return 256; }
        }

        public H1DescriptorAllocator(H1DescriptorHeapType type)
        {
            m_HeapType = type;
            m_RemainingFreeHandles = 0;

            ConstructPlatformDependentMembers();
        }

        private H1DescriptorHeapType m_HeapType;
        private Int32 m_DescriptorSize;
        private Int32 m_RemainingFreeHandles;
    }
}
