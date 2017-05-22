using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1DX12Resource : IComparable
    {
        public Resource Resource
        {
            get { return m_Resource; }
        }

        public H1DX12Resource()
        {

        }

        public H1DX12Resource(Resource resource, ResourceStates initialState, ResourceDescription desc, H1SubresourceData[] initialData = null, UInt32 numInitialData = 0)
        {
            m_Resource = resource;
            m_Desc = desc;

            if (m_Resource != null)
            {
                HeapProperties heapProperties;
                HeapFlags heapFlag;
                m_Resource.GetHeapProperties(out heapProperties, out heapFlag);

                m_HeapType = heapProperties.Type;

                // certain heaps are restricted to certain 'ResourceStates'
                if (m_HeapType == HeapType.Upload)
                {
                    m_CurrentState = ResourceStates.GenericRead;
                }
                else if ( m_HeapType == HeapType.Readback)
                {
                    m_CurrentState = ResourceStates.CopyDestination;                
                }
                else
                {
                    m_CurrentState = initialState;
                }

                if (desc.Dimension == ResourceDimension.Buffer)
                {
                    m_GPUVirtualAddress.Ptr = resource.GPUVirtualAddress;
                }
            }   
            // null resource put on the UPLOAD heap to prevent attemps to transition the resource 
            else
            {
                m_HeapType = HeapType.Upload;
            }

            // defaultly set it as false
            m_bCompressed = false;

            if (initialData != null && numInitialData > 0)
            {
                //@TODO - I need to change this by H1DX12DeviceContext
                //@TODO - I need to move this chunck code to H1DX12DeviceContext as 'UploadResource'
                Device device = H1Global<H1ManagedRenderer>.Instance.Device;

                // create instance of InitialData
                m_InitialData.Size = GetSize(0, numInitialData);
                // create new instance of SubResourceData
                m_InitialData.SubResourceData = new List<H1SubresourceData>();
                
                for (UInt32 i = 0; i < numInitialData; ++i)
                {
                    H1SubresourceData newInitialData = initialData[i];

                    UInt64 curSize = GetSize(i, 1);
                    UInt64 cpySize = curSize;

                    if (desc.Dimension != ResourceDimension.Texture3D && newInitialData.SlicePitch != 0)
                    {
                        cpySize = newInitialData.SlicePitch;
                        newInitialData.SlicePitch = 0;
                    }
                    else if (desc.Dimension == ResourceDimension.Texture3D && newInitialData.SlicePitch != 0)
                    {
                        cpySize = Convert.ToUInt64((Convert.ToInt32(desc.DepthOrArraySize) >> Convert.ToInt32(i))) * newInitialData.SlicePitch;
                    }

                    newInitialData.pData = SharpDX.Utilities.AllocateMemory(Convert.ToInt32(curSize));
                    SharpDX.Utilities.CopyMemory(newInitialData.pData, initialData[i].pData, Convert.ToInt32(cpySize));
                    m_InitialData.SubResourceData.Add(newInitialData);
                }
            }            
        }    
        
        public UInt64 GetSize(UInt32 firstSubresource, UInt32 numSubresources)
        {
            Int64 size;
            // @TODO - need to get the device pointer from the context
            Device device = H1Global<H1ManagedRenderer>.Instance.Device;
            // GetCopyableFootprint - get a resource that can be copied
            device.GetCopyableFootprints(ref m_Desc, Convert.ToInt32(firstSubresource), Convert.ToInt32(numSubresources), 0, null, null, null, out size);
            return Convert.ToUInt64(size);
        }    

        // initialization of contents
        struct InitialData
        {
            public List<H1SubresourceData> SubResourceData;
            public UInt64 Size;
        }

        struct CPUAccessibleData
        {
            struct SubresourceData
            {
                UInt32 Subresource;
                UInt64 Size;
                H1MemcpyDest Data;
                Boolean IsDirty;
            }

            List<SubresourceData> m_SubResourceData;
        }

        // never changes after construction
        private ResourceDescription m_Desc;
        private HeapType m_HeapType;
        private GpuDescriptorHandle m_GPUVirtualAddress;
        private Resource m_Resource;
        private Resource m_UploadResource;
        private Resource m_DownloadResource;

        private H1SwapChain m_SwapChainOwnerRef;
        private Boolean m_bCompressed;

        // potentially changes on every resource-use
        ResourceStates m_CurrentState;
        ResourceStates m_AnnouncedState;
        UInt64[,] m_FenceValues = new UInt64[H1GlobalDX12Definitions.CMDTYPE_NUM, H1GlobalDX12Definitions.CMDQUEUE_NUM];

        // used when using the resource the first time
        InitialData m_InitialData;

        // used when data is transferred to/from the CPU
        CPUAccessibleData m_CPUAccessibleData;

        public int CompareTo(object obj)
        {
            // just compare instance pointer (reference)
            return Convert.ToInt32(this != obj);
        }
    }
}
