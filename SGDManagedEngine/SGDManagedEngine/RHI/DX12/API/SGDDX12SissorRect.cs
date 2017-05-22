using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    struct DX12Rect
    {
        UInt32 Left, Top, Right, Bottom;
    }

    class H1DX12SissorRect : IComparable
    {
        DX12Rect m_Rect;

        public int CompareTo(object obj)
        {
            // just compare instance pointer (reference)
            return Convert.ToInt32(this != obj);
        }
    }
}
