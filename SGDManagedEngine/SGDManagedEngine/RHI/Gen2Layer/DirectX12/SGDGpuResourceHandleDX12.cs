using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer.Memory
{
#if SGD_DX12
    public partial class H1GpuResourceHandle
    {
        private long m_GpuVirtualAddress;
    }
#endif
}
