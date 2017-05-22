using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1DescriptorHandle
    {

    }

    public partial class H1DynamicDescriptorHeapManager
    {
        public static Int32 NumDescriptorsPerHeap
        {
            // constant number of descriptors per heap
            get { return 1024; }
        }   
        
        public H1DynamicDescriptorHeapManager()
        {
            InitializePlatformDependent();
        }
    }

    // this class is a linear allocation system for dynamically generated descriptor tables
    // it internally caches CPU descriptor handles so that when not enough space is available in the current heap, necessary descriptors can be re-copied to the new heap
    public partial class H1DynamicDescriptorHeap
    {
        public H1DynamicDescriptorHeap()
        {
            InitializePlatformDependent();
        }

        // derived class will be allocate new platform-specific instance
        protected static H1DynamicDescriptorHeapManager m_DynamicDescriptorHeapManager = null;
        protected static Int32 m_DescriptorSize = -1;

        // platform-dependent variables
        protected H1CommandContext m_CommandContextRef;
    }
}
