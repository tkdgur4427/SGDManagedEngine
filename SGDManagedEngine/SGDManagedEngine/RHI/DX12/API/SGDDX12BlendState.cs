using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    class H1BlendState : IComparable
    {
        BlendStateDescription m_Desc;

        public int CompareTo(object obj)
        {
            // just compare instance pointer (reference)
            return Convert.ToInt32(this != obj);
        }
    }
}
