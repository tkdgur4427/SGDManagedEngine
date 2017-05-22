using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1SamplerState : IComparable
    {
        CpuDescriptorHandle m_DescriptorHandle;
        SamplerStateDescription m_Desc;

        public int CompareTo(object obj)
        {
            // just compare instance pointer (reference)
            return Convert.ToInt32(this != obj);
        }
    }
}
