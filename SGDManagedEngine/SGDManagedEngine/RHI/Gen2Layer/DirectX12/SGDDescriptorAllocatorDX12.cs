using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1DescriptorAllocator
    {
        void ConstructPlatformDependentMembers()
        {
            m_Device = H1Global<H1ManagedRenderer>.Instance.Device;

            m_CurrentHeap = null;
            m_CurrentHandle.Ptr = null;

            // set descriptor size
            m_DescriptorSize = m_Device.GetDescriptorHandleIncrementSize(H1RHIDefinitionHelper.ConvertToDescriptorHeapType(m_HeapType));
        }

        protected DescriptorHeap RequestNewHeap(H1DescriptorHeapType type)
        {
            DescriptorHeapType typeInDX12 = H1RHIDefinitionHelper.ConvertToDescriptorHeapType(type);

            // @TODO - need to be thread-safe
            DescriptorHeapDescription desc = new DescriptorHeapDescription();
            desc.Type = typeInDX12;
            desc.DescriptorCount = NumDescriptorsPerHeap;

            // this part is none, but for usage in command context, you need to create new descriptor for shader-visible by UserDescriptorHeap or DynamicDescriptorHeap
            desc.Flags = DescriptorHeapFlags.None;
            desc.NodeMask = 1;

            DescriptorHeap newHeap = m_Device.CreateDescriptorHeap(desc);
            m_DescriptorHeapPool.Add(newHeap);

            return newHeap;
        }

        public CpuDescriptorHandle Allocate(Int32 count)
        {
            if (m_CurrentHeap == null || m_RemainingFreeHandles < count)
            {
                m_CurrentHeap = RequestNewHeap(m_HeapType);
                m_CurrentHandle = m_CurrentHeap.CPUDescriptorHandleForHeapStart;
                m_RemainingFreeHandles = NumDescriptorsPerHeap;                
            }

            m_CurrentHandle += count * m_DescriptorSize;
            m_RemainingFreeHandles -= count;

            return m_CurrentHandle;
        }

        public void DestroyAll()
        {
            m_DescriptorHeapPool.Clear();
        }

        private Device m_Device;
        private static List<DescriptorHeap> m_DescriptorHeapPool = new List<DescriptorHeap>();
        private DescriptorHeap m_CurrentHeap;
        private CpuDescriptorHandle m_CurrentHandle;
    }
}
