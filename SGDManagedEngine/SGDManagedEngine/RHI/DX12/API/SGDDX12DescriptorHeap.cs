using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1CursorHost
    {
        protected UInt32 m_Cursor;
    }

    public class H1DescriptorHeap : H1CursorHost
    {       
        public H1DescriptorHeap()
        {

        }

        public void Initialize(Device device, Int32 descriptorCount, Boolean isShaderVisible, H1ViewType descriptorHeapType)        
        {
            DescriptorHeapType? type = null;
            switch (descriptorHeapType)
            {
                case H1ViewType.DepthStencilView:
                    type = DescriptorHeapType.DepthStencilView;
                    break;
                case H1ViewType.RenderTargetView:
                    type = DescriptorHeapType.RenderTargetView;
                    break;
                case H1ViewType.ShaderResourceView:
                case H1ViewType.UnorderedAccessView:
                case H1ViewType.ConstantBufferView:
                    type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView;
                    break;                
            }

            if (type == null)
                return; // @TODO - excpetion handling

            // create descriptorheap desc
            m_Desc = new DescriptorHeapDescription()
            {
                DescriptorCount = descriptorCount,
                Flags = isShaderVisible ? DescriptorHeapFlags.ShaderVisible : DescriptorHeapFlags.None,
                Type = type.Value 
            };

            // create descriptor heap and set the descriptor incremental size
            m_DescriptorHeap = device.CreateDescriptorHeap(m_Desc);
            m_DescSize = Convert.ToUInt32(device.GetDescriptorHandleIncrementSize(type.Value));

            // set the heap start CPU/GPU
            m_HeapStartCPU = m_DescriptorHeap.CPUDescriptorHandleForHeapStart;
            m_HeapStartGPU = m_DescriptorHeap.GPUDescriptorHandleForHeapStart;

            // set the cursor to 0
            m_Cursor = 0; // tracing next allocation flags
        }        

        public CpuDescriptorHandle GetCpuAddressByOffset(Int32 Index)
        {
            return m_HeapStartCPU + Index * Convert.ToInt32(m_DescSize);
        }

        public GpuDescriptorHandle GetGpuAddressByOffset(Int32 Index)
        {
            return m_HeapStartGPU + Index * Convert.ToInt32(m_DescSize);
        }

        public CpuDescriptorHandle GetAvailableAllocCpuAddress(Int32 allocCounts = 1)
        {
            CpuDescriptorHandle allocStartCpu = m_HeapStartCPU + Convert.ToInt32(m_Cursor * m_DescSize);
            m_Cursor += Convert.ToUInt32(allocCounts); // update cursor
            return allocStartCpu;
        }        

        private DescriptorHeap m_DescriptorHeap;
        private DescriptorHeapDescription m_Desc;
        UInt32 m_DescSize;
        private CpuDescriptorHandle m_HeapStartCPU;
        private GpuDescriptorHandle m_HeapStartGPU;        
    }

    public class H1DescriptorBlock : H1CursorHost
    {
        public H1DescriptorBlock(H1DescriptorHeap refDescriptorHeap)
        {
            m_DescriptorHeapRef = refDescriptorHeap;
        }

        private H1DescriptorHeap m_DescriptorHeapRef;
        private UInt32 m_BlockStart;
        private UInt32 m_Capacity;
    }
}
