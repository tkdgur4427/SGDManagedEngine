using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    class H1DX12Object<T> 
    {
        public H1DX12Object()
        {

        }

        ~H1DX12Object()
        {
            // when it will be released, dispose the DX12 object
            DeviceChild deviceChild = m_DX12Object as DeviceChild;
            deviceChild?.Dispose();
        }

        T m_DX12Object;
    }
}
