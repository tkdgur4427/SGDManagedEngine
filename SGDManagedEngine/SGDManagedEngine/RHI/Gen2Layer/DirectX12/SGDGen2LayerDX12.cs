using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1Gen2Layer
    {
        public CpuDescriptorHandle AllocateDescriptor(H1DescriptorHeapType type, Int32 count = 1)
        {
            return m_DescriptorAllocators[Convert.ToInt32(type)].Allocate(count);
        }
    }
}
