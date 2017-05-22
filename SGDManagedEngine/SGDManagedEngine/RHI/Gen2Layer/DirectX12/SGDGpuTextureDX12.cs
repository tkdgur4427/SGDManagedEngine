using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1GpuTexture
    {
        public ResourceDescription Description
        {
            get { return m_Description; }
        }

        void ConstructPlatformDependentMembers()
        {
            // allocate description in advance
            m_Description = new ResourceDescription();
        }

        void DestructPlatformDependentMembers()
        {
            
        }

        public virtual Boolean CreateResourceDescription(H1GpuResourceDesc desc)
        {
            // API level specific resource description
            m_Description.Alignment = desc.Alignment;
            m_Description.DepthOrArraySize = Convert.ToInt16(desc.DepthOrArraySize);
            m_Description.Dimension = (ResourceDimension)H1GpuResource.DimensionMapper[Convert.ToInt32(desc.Dimension)];
            m_Description.Flags = H1RHIDefinitionHelper.FormatToResourceFlags(desc.Flags);
            m_Description.Format = H1RHIDefinitionHelper.ConvertToFormat(desc.Format);
            m_Description.Width = Convert.ToInt32(desc.Width);
            m_Description.Height = Convert.ToInt32(desc.Height);
            m_Description.Layout = (TextureLayout)H1GpuResource.TextureLayoutMapper[Convert.ToInt32(desc.Layout)];
            m_Description.MipLevels = Convert.ToInt16(desc.MipLevels);
            m_Description.SampleDescription.Count = Convert.ToInt32(desc.SampleDesc.Count);
            m_Description.SampleDescription.Quality = Convert.ToInt32(desc.SampleDesc.Quality);

            return true;            
        }

        // mirroring for RHI level resource description
        private ResourceDescription m_Description;

        // texture CPU descriptor handles
        // NOTE - the data for GPU usage should be copied before using this as shader resource
        protected CpuDescriptorHandle m_UAV;
        protected CpuDescriptorHandle m_SRV;
    }

    public partial class H1GpuTexture1D
    {

    }

    public partial class H1GpuTexture2D
    {
        static void CreatePlatformDependent(Vector4 clearValue, H1Texture2D.Description desc, H1SubresourceData initialData, ref H1GpuTexture2D result)
        {
            // get device for directX 12
            Device deviceDX12 = H1Global<H1ManagedRenderer>.Instance.Device;

            H1GpuResourceDesc desc12 = H1RHIDefinitionHelper.Texture2DDescToGpuResourceDesc(desc);
            H1HeapType heapType = H1RHIDefinitionHelper.GetHeapTypeFromTexture2DDesc(desc);
            H1ResourceStates resourceStates = H1RHIDefinitionHelper.GetResourceStatesFromTexture2DDesc(desc);
                      
            // generate resource
            //if (result != null)
            //    result.Resource.CreateResource(heapType, desc12, resourceStates);

            // generate RHI resource description (need resource description for generating UAV or SRV or etc.)
            result.CreateResourceDescription(desc12);            
        }
    }
}
