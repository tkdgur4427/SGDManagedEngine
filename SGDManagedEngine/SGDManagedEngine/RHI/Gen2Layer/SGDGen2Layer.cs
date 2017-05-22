using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1Gen2Layer
    {
        public H1Gen2Layer()
        {
            
        }

        public Boolean Initialize()
        {
            // create descriptor allocators for all descriptor heap types
            Int32 descriptorHeapTypeIndex = Convert.ToInt32(H1DescriptorHeapType.ConstantBufferView_ShaderResourceView_UnorderedAccessView);
            m_DescriptorAllocators[descriptorHeapTypeIndex] = new H1DescriptorAllocator(H1DescriptorHeapType.ConstantBufferView_ShaderResourceView_UnorderedAccessView);

            descriptorHeapTypeIndex = Convert.ToInt32(H1DescriptorHeapType.Sampler);
            m_DescriptorAllocators[descriptorHeapTypeIndex] = new H1DescriptorAllocator(H1DescriptorHeapType.Sampler);

            descriptorHeapTypeIndex = Convert.ToInt32(H1DescriptorHeapType.DepthStencilView);
            m_DescriptorAllocators[descriptorHeapTypeIndex] = new H1DescriptorAllocator(H1DescriptorHeapType.DepthStencilView);

            descriptorHeapTypeIndex = Convert.ToInt32(H1DescriptorHeapType.RenderTargetView);
            m_DescriptorAllocators[descriptorHeapTypeIndex] = new H1DescriptorAllocator(H1DescriptorHeapType.RenderTargetView);

            return true;
        }

        public void Destroy()
        {

        }

        private H1DescriptorAllocator[] m_DescriptorAllocators = new H1DescriptorAllocator[Convert.ToInt32(H1DescriptorHeapType.TotalNum)];
    }
}
