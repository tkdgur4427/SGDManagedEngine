using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public partial class H1GpuView
    {
        public virtual Boolean CreateView()
        {
            return false;
        }

        protected CpuDescriptorHandle m_DescriptorHandle;
        protected Boolean m_bHasDesc = false;
        protected Int64 m_Size; // size in bytes

        protected static Int32 D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING = 5768;
    }

    public partial class H1VertexBufferView
    {
        public VertexBufferView View
        {
            get { return m_VBVDesc; }
        }

        void ConstructPlatformDependentMembers(Int64 bufferLocation, Int32 sizeInBytes, Int32 strideInBytes)
        {
            m_VBVDesc = new VertexBufferView()
            {
                BufferLocation = bufferLocation,
                SizeInBytes = sizeInBytes,
                StrideInBytes = strideInBytes,
            };
        }

        private VertexBufferView m_VBVDesc;
    }

    public partial class H1IndexBufferView
    {
        public IndexBufferView View
        {
            get { return m_IBVDesc; }
        }

        void ConstructPlatformDependentMembers(Int64 bufferLocation, H1PixelFormat format, Int32 sizeInBytes)
        {
            m_IBVDesc = new IndexBufferView()
            {
                BufferLocation = bufferLocation,
                Format = H1RHIDefinitionHelper.ConvertToFormat(format),
                SizeInBytes = sizeInBytes,
            };
        }

        private IndexBufferView m_IBVDesc;
    }

    public partial class H1ConstantBufferView
    {
        void ConstructPlatformDependentMembers(Int64 bufferLocation, Int32 sizeInBytes)
        {
            m_CBVDesc = new ConstantBufferViewDescription()
            {
                BufferLocation = bufferLocation,
                SizeInBytes = sizeInBytes,
            };
        }

        private ConstantBufferViewDescription m_CBVDesc;
    }

    public partial class H1ShaderResourceView
    {       
        public override Boolean CreateView()
        {
            // the description is in invalid state, please check
            if (!m_bHasDesc)
                return false;

            // get the device
            Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Device;

            // get the descriptor handle
            if (m_DescriptorHandle.Ptr == null)
            {
                m_DescriptorHandle = H1Global<H1ManagedRenderer>.Instance.Gen2Layer.AllocateDescriptor(H1DescriptorHeapType.ConstantBufferView_ShaderResourceView_UnorderedAccessView);
            }

            // convert platform-independent description to platform-dependent description
            m_SRVDesc.Format = H1RHIDefinitionHelper.ConvertToFormat(m_Description.Format);

            m_SRVDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;

            // depending on view dimension types, create the corresponding description
            switch (m_Description.ViewDimension)
            {
                case H1ShaderResourceViewDimension.Buffer:
                    {
                        m_SRVDesc.Dimension = ShaderResourceViewDimension.Buffer;
                        // if the buffer description is not filled up, invalid description
                        if (m_Description.Buffer == null)
                            return false;

                        m_SRVDesc.Buffer.FirstElement = m_Description.Buffer.FirstElement;
                        m_SRVDesc.Buffer.ElementCount = m_Description.Buffer.NumElement;
                        m_SRVDesc.Buffer.StructureByteStride = m_Description.Buffer.StructuredByteStride;
                        
                        switch (m_Description.Buffer.Flags)
                        {
                            case H1BufferShaderResourceViewFlags.None:
                                m_SRVDesc.Buffer.Flags = BufferShaderResourceViewFlags.None;
                                break;

                            case H1BufferShaderResourceViewFlags.Raw:
                                m_SRVDesc.Buffer.Flags = BufferShaderResourceViewFlags.Raw;
                                break;
                        }

                        break;
                    }

                case H1ShaderResourceViewDimension.Texture1D:
                    {
                        m_SRVDesc.Dimension = ShaderResourceViewDimension.Texture1D;
                        if (m_Description.Texture1D == null)
                            return false;

                        m_SRVDesc.Texture1D.MostDetailedMip = m_Description.Texture1D.MostDetailedMip;
                        m_SRVDesc.Texture1D.MipLevels = m_Description.Texture1D.MipLevels;
                        m_SRVDesc.Texture1D.ResourceMinLODClamp = m_Description.Texture1D.ResourceMinLODClamp;

                        break;
                    }

                case H1ShaderResourceViewDimension.Texture1DArray:
                    {
                        m_SRVDesc.Dimension = ShaderResourceViewDimension.Texture1DArray;
                        if (m_Description.Texture1DArray == null)
                            return false;

                        m_SRVDesc.Texture1DArray.MostDetailedMip = m_Description.Texture1DArray.MostDetailedMip;
                        m_SRVDesc.Texture1DArray.MipLevels = m_Description.Texture1DArray.MipLevels;
                        m_SRVDesc.Texture1DArray.FirstArraySlice = m_Description.Texture1DArray.FirstArraySlice;
                        m_SRVDesc.Texture1DArray.ArraySize = m_Description.Texture1DArray.ArraySize;
                        m_SRVDesc.Texture1DArray.ResourceMinLODClamp = m_Description.Texture1DArray.ResourceMinLODClamp;    

                        break;
                    }

                case H1ShaderResourceViewDimension.Texture2D:
                    {
                        m_SRVDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                        if (m_Description.Texture2D == null)
                            return false;

                        m_SRVDesc.Texture2D.MostDetailedMip = m_Description.Texture2D.MostDetailedMip;
                        m_SRVDesc.Texture2D.MipLevels = m_Description.Texture2D.MipLevels;
                        m_SRVDesc.Texture2D.PlaneSlice = m_Description.Texture2D.PlaneSlice;
                        m_SRVDesc.Texture2D.ResourceMinLODClamp = m_Description.Texture2D.ResourceMinLODClamp;

                        break;
                    }

                case H1ShaderResourceViewDimension.Texture2DArray:
                    {
                        m_SRVDesc.Dimension = ShaderResourceViewDimension.Texture2DArray;
                        if (m_Description.Texture2DArray == null)
                            return false;

                        m_SRVDesc.Texture2DArray.MostDetailedMip = m_Description.Texture2DArray.MostDetailedMip;
                        m_SRVDesc.Texture2DArray.MipLevels = m_Description.Texture2DArray.MipLevels;
                        m_SRVDesc.Texture2DArray.PlaneSlice = m_Description.Texture2DArray.PlaneSlice;
                        m_SRVDesc.Texture2DArray.ResourceMinLODClamp = m_Description.Texture2DArray.ResourceMinLODClamp;
                        m_SRVDesc.Texture2DArray.FirstArraySlice = m_Description.Texture2DArray.FirstArraySlice;
                        m_SRVDesc.Texture2DArray.ArraySize = m_Description.Texture2DArray.ArraySize;

                        break;
                    }

                case H1ShaderResourceViewDimension.Texture3D:
                    {
                        m_SRVDesc.Dimension = ShaderResourceViewDimension.Texture3D;
                        if (m_Description.Texture3D == null)
                            return false;

                        m_SRVDesc.Texture3D.MostDetailedMip = m_Description.Texture3D.MostDetailedMip;
                        m_SRVDesc.Texture3D.MipLevels = m_Description.Texture3D.MipLevels;
                        m_SRVDesc.Texture3D.ResourceMinLODClamp = m_Description.Texture3D.ResourceMinLODClamp;

                        break;
                    }

                default:
                    // @TODO - not implemented yet!
                    return false;
            }

            // create view descriptor
            deviceDX12.CreateShaderResourceView(m_ResourceRef.GpuResource, m_SRVDesc, m_DescriptorHandle);

            return true;
        }

        private ShaderResourceViewDescription m_SRVDesc;
    }

    public partial class H1UnorderedAccessView
    {
        public override Boolean CreateView()
        {
            // the description is in invalid state, please check
            if (!m_bHasDesc)
                return false;

            // get the device
            Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Device;

            // get the descriptor handle
            if (m_DescriptorHandle.Ptr == null)
            {
                m_DescriptorHandle = H1Global<H1ManagedRenderer>.Instance.Gen2Layer.AllocateDescriptor(H1DescriptorHeapType.ConstantBufferView_ShaderResourceView_UnorderedAccessView);
            }

            // convert platform-independent description to platform-dependent description
            m_UAVDesc.Format = H1RHIDefinitionHelper.ConvertToFormat(m_Description.Format);            

            // depending on view dimension types, create the corresponding description
            switch (m_Description.ViewDimension)
            {
                case H1UnorderedAccesViewDimension.Buffer:
                    {
                        m_UAVDesc.Dimension = UnorderedAccessViewDimension.Buffer;
                        // if the buffer description is not filled up, invalid description
                        if (m_Description.Buffer == null)
                            return false;

                        m_UAVDesc.Buffer.FirstElement = m_Description.Buffer.FirstElement;
                        m_UAVDesc.Buffer.ElementCount = m_Description.Buffer.NumElements;
                        m_UAVDesc.Buffer.StructureByteStride = m_Description.Buffer.StructureByteStride;
                        m_UAVDesc.Buffer.CounterOffsetInBytes = m_Description.Buffer.CounterOffsetInBytes;

                        switch (m_Description.Buffer.Flags)
                        {
                            case H1UnorderedAccessViewFlags.None:
                                m_UAVDesc.Buffer.Flags = BufferUnorderedAccessViewFlags.None;
                                break;

                            case H1UnorderedAccessViewFlags.Raw:
                                m_UAVDesc.Buffer.Flags = BufferUnorderedAccessViewFlags.Raw;
                                break;
                        }

                        break;
                    }

                case H1UnorderedAccesViewDimension.Texture1D:
                    {
                        m_UAVDesc.Dimension = UnorderedAccessViewDimension.Texture1D;
                        if (m_Description.Texture1D == null)
                            return false;

                        m_UAVDesc.Texture1D.MipSlice = m_Description.Texture1D.MipSlice;
                        
                        break;
                    }

                case H1UnorderedAccesViewDimension.Texture1DArray:
                    {
                        m_UAVDesc.Dimension = UnorderedAccessViewDimension.Texture1DArray;
                        if (m_Description.Texture1DArray == null)
                            return false;

                        m_UAVDesc.Texture1DArray.MipSlice = m_Description.Texture1DArray.MipSlice;
                        m_UAVDesc.Texture1DArray.FirstArraySlice = m_Description.Texture1DArray.FirstArraySlice;
                        m_UAVDesc.Texture1DArray.ArraySize = m_Description.Texture1DArray.ArraySize;

                        break;
                    }

                case H1UnorderedAccesViewDimension.Texture2D:
                    {
                        m_UAVDesc.Dimension = UnorderedAccessViewDimension.Texture2D;
                        if (m_Description.Texture2D == null)
                            return false;

                        m_UAVDesc.Texture2D.MipSlice = m_Description.Texture2D.MipSlice;
                        m_UAVDesc.Texture2D.PlaneSlice = m_Description.Texture2D.PlaneSlice;                        

                        break;
                    }

                case H1UnorderedAccesViewDimension.Texture2DArray:
                    {
                        m_UAVDesc.Dimension = UnorderedAccessViewDimension.Texture2DArray;
                        if (m_Description.Texture2DArray == null)
                            return false;
                                                
                        m_UAVDesc.Texture2DArray.MipSlice = m_Description.Texture2DArray.MipSlice;
                        m_UAVDesc.Texture2DArray.PlaneSlice = m_Description.Texture2DArray.PlaneSlice;                        
                        m_UAVDesc.Texture2DArray.FirstArraySlice = m_Description.Texture2DArray.FirstArraySlice;
                        m_UAVDesc.Texture2DArray.ArraySize = m_Description.Texture2DArray.ArraySize;

                        break;
                    }

                case H1UnorderedAccesViewDimension.Texture3D:
                    {
                        m_UAVDesc.Dimension = UnorderedAccessViewDimension.Texture3D;
                        if (m_Description.Texture3D == null)
                            return false;

                        m_UAVDesc.Texture3D.MipSlice = m_Description.Texture3D.MipSlice;
                        m_UAVDesc.Texture3D.FirstWSlice = m_Description.Texture3D.FirstWSlice;
                        m_UAVDesc.Texture3D.WSize = m_Description.Texture3D.WSize;

                        break;
                    }
            }

            // create view descriptor
            deviceDX12.CreateUnorderedAccessView(m_ResourceRef.GpuResource, m_CounterResourceRef == null ? null : m_CounterResourceRef.GpuResource, m_UAVDesc, m_DescriptorHandle);

            return true;
        }                
        
        private UnorderedAccessViewDescription m_UAVDesc;
    }

    public partial class H1DepthStencilView
    {
        public override Boolean CreateView()
        {
            // the description is in invalid state, please check
            if (!m_bHasDesc)
                return false;

            // get the device
            Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Device;

            // get the descriptor handle
            if (m_DescriptorHandle.Ptr == null)
            {
                m_DescriptorHandle = H1Global<H1ManagedRenderer>.Instance.Gen2Layer.AllocateDescriptor(H1DescriptorHeapType.ConstantBufferView_ShaderResourceView_UnorderedAccessView);
            }

            // convert platform-independent description to platform-dependent description
            m_DSVDesc.Format = H1RHIDefinitionHelper.ConvertToFormat(m_Description.Format);            

            // depending on view dimension types, create the corresponding description
            switch (m_Description.ViewDimension)
            {               
                case H1DepthStencilViewDimension.Texture1D:
                    {
                        m_DSVDesc.Dimension = DepthStencilViewDimension.Texture1D;
                        if (m_Description.Texture1D == null)
                            return false;

                        m_DSVDesc.Texture1D.MipSlice = m_Description.Texture1D.MipSlice;
                        
                        break;
                    }

                case H1DepthStencilViewDimension.Texture1DArray:
                    {
                        m_DSVDesc.Dimension = DepthStencilViewDimension.Texture1DArray;
                        if (m_Description.Texture1DArray == null)
                            return false;
                        
                        m_DSVDesc.Texture1DArray.MipSlice = m_Description.Texture1DArray.MipSlice;
                        m_DSVDesc.Texture1DArray.FirstArraySlice = m_Description.Texture1DArray.FirstArraySlice;
                        m_DSVDesc.Texture1DArray.ArraySize = m_Description.Texture1DArray.ArraySize;                        

                        break;
                    }

                case H1DepthStencilViewDimension.Texture2D:
                    {
                        m_DSVDesc.Dimension = DepthStencilViewDimension.Texture2D;
                        if (m_Description.Texture2D == null)
                            return false;

                        m_DSVDesc.Texture2D.MipSlice = m_Description.Texture2D.MipSlice;                        

                        break;
                    }

                case H1DepthStencilViewDimension.Texture2DArray:
                    {
                        m_DSVDesc.Dimension = DepthStencilViewDimension.Texture2DArray;
                        if (m_Description.Texture2DArray == null)
                            return false;

                        m_DSVDesc.Texture2DArray.MipSlice = m_Description.Texture2DArray.MipSlice;                        
                        m_DSVDesc.Texture2DArray.FirstArraySlice = m_Description.Texture2DArray.FirstArraySlice;
                        m_DSVDesc.Texture2DArray.ArraySize = m_Description.Texture2DArray.ArraySize;

                        break;
                    }                

                default:
                    // @TODO - not implemented yet!
                    return false;
            }

            // create view descriptor
            deviceDX12.CreateDepthStencilView(m_ResourceRef.GpuResource, m_DSVDesc, m_DescriptorHandle);

            return true;
        }

        private DepthStencilViewDescription m_DSVDesc;
    }

    public partial class H1RenderTargetView
    {
        public override Boolean CreateView()
        {
            // the description is in invalid state, please check
            if (!m_bHasDesc)
                return false;

            // get the device
            Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Device;

            // get the descriptor handle
            if (m_DescriptorHandle.Ptr == null)
            {
                m_DescriptorHandle = H1Global<H1ManagedRenderer>.Instance.Gen2Layer.AllocateDescriptor(H1DescriptorHeapType.ConstantBufferView_ShaderResourceView_UnorderedAccessView);
            }

            // convert platform-independent description to platform-dependent description
            m_RTVDesc.Format = H1RHIDefinitionHelper.ConvertToFormat(m_Description.Format);            

            // depending on view dimension types, create the corresponding description
            switch (m_Description.ViewDimension)
            {
                case H1RenderTargetViewDimension.Buffer:
                    {
                        m_RTVDesc.Dimension = RenderTargetViewDimension.Buffer;
                        // if the buffer description is not filled up, invalid description
                        if (m_Description.Buffer == null)
                            return false;

                        m_RTVDesc.Buffer.FirstElement = m_Description.Buffer.FirstElement;
                        m_RTVDesc.Buffer.ElementCount = m_Description.Buffer.NumElements;                                               

                        break;
                    }

                case H1RenderTargetViewDimension.Texture1D:
                    {
                        m_RTVDesc.Dimension = RenderTargetViewDimension.Texture1D;
                        if (m_Description.Texture1D == null)
                            return false;

                        m_RTVDesc.Texture1D.MipSlice = m_Description.Texture1D.MipSlice;                       

                        break;
                    }

                case H1RenderTargetViewDimension.Texture1DArray:
                    {
                        m_RTVDesc.Dimension = RenderTargetViewDimension.Texture1DArray;
                        if (m_Description.Texture1DArray == null)
                            return false;

                        m_RTVDesc.Texture1DArray.MipSlice = m_Description.Texture1DArray.MipSlice;                        
                        m_RTVDesc.Texture1DArray.FirstArraySlice = m_Description.Texture1DArray.FirstArraySlice;
                        m_RTVDesc.Texture1DArray.ArraySize = m_Description.Texture1DArray.ArraySize;
                        
                        break;
                    }

                case H1RenderTargetViewDimension.Texture2D:
                    {
                        m_RTVDesc.Dimension = RenderTargetViewDimension.Texture2D;
                        if (m_Description.Texture2D == null)
                            return false;

                        m_RTVDesc.Texture2D.MipSlice = m_Description.Texture2D.MipSlice;                        
                        m_RTVDesc.Texture2D.PlaneSlice = m_Description.Texture2D.PlaneSlice;                        

                        break;
                    }

                case H1RenderTargetViewDimension.Texture2DArray:
                    {
                        m_RTVDesc.Dimension = RenderTargetViewDimension.Texture2DArray;
                        if (m_Description.Texture2DArray == null)
                            return false;

                        m_RTVDesc.Texture2DArray.MipSlice = m_Description.Texture2DArray.MipSlice;                        
                        m_RTVDesc.Texture2DArray.PlaneSlice = m_Description.Texture2DArray.PlaneSlice;                        
                        m_RTVDesc.Texture2DArray.FirstArraySlice = m_Description.Texture2DArray.FirstArraySlice;
                        m_RTVDesc.Texture2DArray.ArraySize = m_Description.Texture2DArray.ArraySize;

                        break;
                    }

                case H1RenderTargetViewDimension.Texture3D:
                    {
                        m_RTVDesc.Dimension = RenderTargetViewDimension.Texture3D;
                        if (m_Description.Texture3D == null)
                            return false;
                                                
                        m_RTVDesc.Texture3D.MipSlice = m_Description.Texture3D.MipSlice;
                        m_RTVDesc.Texture3D.FirstDepthSlice = m_Description.Texture3D.FirstWSlice;
                        m_RTVDesc.Texture3D.DepthSliceCount = m_Description.Texture3D.WSize;

                        break;
                    }

                default:
                    // @TODO - not implemented yet!
                    return false;
            }

            // create view descriptor
            deviceDX12.CreateRenderTargetView(m_ResourceRef.GpuResource, m_RTVDesc, m_DescriptorHandle);

            return true;
        }

        private RenderTargetViewDescription m_RTVDesc; 
    }
#endif
}
