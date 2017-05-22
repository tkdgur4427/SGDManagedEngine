using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    /*
    public partial class H1GpuResAllocPageManager
    {        
        void CreateAllocPage(H1LinearAllocationType allocType, ref H1GpuResAllocPage gpuResAllocPage)
        {
            H1GpuResource newResource = new H1GpuResource();

            H1HeapType heapType = new H1HeapType();
            H1GpuResourceDesc resourceDesc = new H1GpuResourceDesc();
            resourceDesc.Alignment = 0;
            resourceDesc.Height = 1;
            resourceDesc.DepthOrArraySize = 1;
            resourceDesc.MipLevels = 1;
            resourceDesc.Format = H1PixelFormat.Unknown;
            resourceDesc.SampleDesc.Count = 1;
            resourceDesc.SampleDesc.Quality = 0;
            resourceDesc.Layout = H1TextureLayout.RowMajor;

            H1ResourceStates defaultUsage = new H1ResourceStates();
            if (allocType == H1LinearAllocationType.GpuExclusive)
            {
                heapType = H1HeapType.Default;
                resourceDesc.Width = Convert.ToUInt32(H1AllocatorPageSize.GpuAllocatorPageSize);
                resourceDesc.Flags = H1ResourceFlags.AllowUnorderedAccess;
                defaultUsage = H1ResourceStates.UnorderedAccess;
            }
            else if (allocType == H1LinearAllocationType.CpuWritable)
            {
                heapType = H1HeapType.Upload;
                resourceDesc.Width = Convert.ToUInt32(H1AllocatorPageSize.CpuAllocatorPageSize);
                resourceDesc.Flags = H1ResourceFlags.Unknown;
                defaultUsage = H1ResourceStates.GenericRead;
            }
            else
            {
                // error!
            }

            if (!newResource.CreateResource(heapType, resourceDesc, defaultUsage))
            {
                // failed to create resource
                gpuResAllocPage = null;
            }

            gpuResAllocPage = new H1GpuResAllocPage(allocType, newResource);
        }
    }
    */
#endif
}
