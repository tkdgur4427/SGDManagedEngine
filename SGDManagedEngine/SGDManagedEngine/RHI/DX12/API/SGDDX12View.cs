using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public enum H1ViewType
    {
        Unknown = 0,
        VertexBufferView,
        IndexBufferView,
        ConstantBufferView,
        ShaderResourceView,
        UnorderedAccessView,
        DepthStencilView,
        RenderTargetView,
    }

    class H1View : IComparable
    {
        H1DX12Resource m_ResourceRef;
        H1ViewType m_Type;
        CpuDescriptorHandle m_DescriptorHandle;

        [StructLayout(LayoutKind.Explicit)]
        struct Desc
        {
            [FieldOffset(0)]
            public VertexBufferView m_VBVDesc;

            [FieldOffset(0)]
            public IndexBufferView m_IBVDesc;

            [FieldOffset(0)]
            public ConstantBufferViewDescription m_CBVDesc;

            [FieldOffset(0)]
            public ShaderResourceViewDescription m_SRVDesc;

            [FieldOffset(0)]
            public UnorderedAccessViewDescription m_UAVDesc;

            [FieldOffset(0)]
            public DepthStencilViewDescription m_DSVDesc;

            [FieldOffset(0)]
            public RenderTargetViewDescription m_RTVDesc;
        }

        Desc m_Desc;

        // some views can be created without descriptor (DSV)
        Boolean m_HasDesc;

        // view size in bytes
        UInt64 m_Size;

        public int CompareTo(object obj)
        {
            // just compare instance pointer (reference)
            return Convert.ToInt32(this != obj);
        }
    }
}
