using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    class H1DataStreamer
    {
        public UInt64 RequestUpload(H1DX12Resource destResource, UInt32 subresource, H1SubresourceData desc, UInt64 size)
        {
            return 0;
        }

        struct Request
        {
            public UInt64 FenceValue;
            public Resource TemporaryResourceRef;

            H1DX12Resource Resource;
            UInt32 Subresource;
            H1SubresourceData Data;
        }

        private H1DX12Device m_DeviceRef;
        private List<Request> m_Requests;
    }
}
