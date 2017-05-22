using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public partial class H1GpuResource
    {
        public Resource GpuResource
        {
            get { return m_GpuResource; }            
        }

        Int32 CreateResourcePlatformDependent(H1HeapType heapType, H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        { 
            HeapProperties heapProps = new HeapProperties();            
            heapProps.CPUPageProperty = CpuPageProperty.Unknown;
            heapProps.MemoryPoolPreference = MemoryPool.Unknown;
            heapProps.CreationNodeMask = 1;
            heapProps.VisibleNodeMask = 1;

            // convert to H1HeapType to HeapType for dx12
            switch (heapType)
            {
                case H1HeapType.Default:
                    heapProps.Type = HeapType.Default;
                    break;
                case H1HeapType.Readback:
                    heapProps.Type = HeapType.Readback;
                    break;
                case H1HeapType.Upload:
                    heapProps.Type = HeapType.Upload;
                    break;
            }

            // convert H1ResourceDesc to ResourceDesc for dx12
            ResourceDescription resourceDescDX12 = ConvertToResourceDescDx12(resourceDesc);
            ResourceStates defaultStates = (ResourceStates)ResourceStateMapper[Convert.ToInt32(defaultUsage)];

            Device deviceDX11 = H1Global<H1ManagedRenderer>.Instance.Device;
            m_GpuResource = deviceDX11.CreateCommittedResource(heapProps, HeapFlags.None, resourceDescDX12, defaultStates);

            Int32 elementSize = Convert.ToInt32(H1RHIDefinitionHelper.ElementTypeToSize(resourceDesc.Format));
            if (elementSize == 0)
                throw new InvalidOperationException("there is no appropriate format is implemented in ElementTypeToSize()!");

            Int32 totalSizeInBytes = elementSize * Convert.ToInt32(resourceDesc.Width * resourceDesc.Height * resourceDesc.DepthOrArraySize);
            return totalSizeInBytes;
        }

        void DestroyResourcePlatformDependent()
        {
            // manually dispose
            m_GpuResource.Dispose();
        }

        void MapPlatformDependent(ref IntPtr outPtr, Int32 subResource)
        {
            outPtr = m_GpuResource.Map(subResource);
        }

        void UnmapPlatformDependent(Int32 subResource)
        {
            m_GpuResource.Unmap(subResource);
        }

        public void SetPlacedResourcePlatformDependent(Resource resource)
        {
            m_GpuResource = resource;
            m_GpuVirtualAddress = resource.GPUVirtualAddress;
        }

        static void InitializeMappers()
        {
            // resource state mappers
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.Common)] = Convert.ToInt32(ResourceStates.Common);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.Present)] = Convert.ToInt32(ResourceStates.Present);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.VertexAndConstantBuffer)] = Convert.ToInt32(ResourceStates.VertexAndConstantBuffer);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.IndexBuffer)] = Convert.ToInt32(ResourceStates.IndexBuffer);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.RenderTarget)] = Convert.ToInt32(ResourceStates.RenderTarget);

            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.UnorderedAccess)] = Convert.ToInt32(ResourceStates.UnorderedAccess);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.DepthWrite)] = Convert.ToInt32(ResourceStates.DepthWrite);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.DepthRead)] = Convert.ToInt32(ResourceStates.DepthRead);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.NonPixelShaderResource)] = Convert.ToInt32(ResourceStates.NonPixelShaderResource);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.PixelShaderResource)] = Convert.ToInt32(ResourceStates.PixelShaderResource);

            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.StreamOut)] = Convert.ToInt32(ResourceStates.StreamOut);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.IndirectArgument)] = Convert.ToInt32(ResourceStates.IndirectArgument);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.Predication)] = Convert.ToInt32(ResourceStates.Predication);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.CopyDestination)] = Convert.ToInt32(ResourceStates.CopyDestination);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.CopySource)] = Convert.ToInt32(ResourceStates.CopySource);

            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.GenericRead)] = Convert.ToInt32(ResourceStates.GenericRead);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.ResolveDestination)] = Convert.ToInt32(ResourceStates.ResolveDestination);
            ResourceStateMapper[Convert.ToInt32(H1ResourceStates.ResolveSource)] = Convert.ToInt32(ResourceStates.ResolveSource);

            // dimension mappers
            DimensionMapper[Convert.ToInt32(H1Dimension.Buffer)] = Convert.ToInt32(ResourceDimension.Buffer);
            DimensionMapper[Convert.ToInt32(H1Dimension.Texture1D)] = Convert.ToInt32(ResourceDimension.Texture1D);
            DimensionMapper[Convert.ToInt32(H1Dimension.Texture2D)] = Convert.ToInt32(ResourceDimension.Texture2D);
            DimensionMapper[Convert.ToInt32(H1Dimension.Texture3D)] = Convert.ToInt32(ResourceDimension.Texture3D);

            // texture layout mappers
            TextureLayoutMapper[Convert.ToInt32(H1TextureLayout.RowMajor)] = Convert.ToInt32(TextureLayout.RowMajor);
            TextureLayoutMapper[Convert.ToInt32(H1TextureLayout.StandardSwizzle64KB)] = Convert.ToInt32(TextureLayout.StandardSwizzle64kb);
            TextureLayoutMapper[Convert.ToInt32(H1TextureLayout.UndefinedSwizzle64KB)] = Convert.ToInt32(TextureLayout.UndefinedSwizzle64kb);

            // resource flags
            ResourceFlagsMapper[Convert.ToInt32(H1ResourceFlags.Unknown)] = Convert.ToInt32(ResourceFlags.None);
            ResourceFlagsMapper[Convert.ToInt32(H1ResourceFlags.AllowRenderTarget)] = Convert.ToInt32(ResourceFlags.AllowRenderTarget);
            ResourceFlagsMapper[Convert.ToInt32(H1ResourceFlags.AllowDepthStencil)] = Convert.ToInt32(ResourceFlags.AllowDepthStencil);
            ResourceFlagsMapper[Convert.ToInt32(H1ResourceFlags.AllowUnorderedAccess)] = Convert.ToInt32(ResourceFlags.AllowUnorderedAccess);
            ResourceFlagsMapper[Convert.ToInt32(H1ResourceFlags.DenyShaderResource)] = Convert.ToInt32(ResourceFlags.DenyShaderResource);
            ResourceFlagsMapper[Convert.ToInt32(H1ResourceFlags.AllowCrossAdapter)] = Convert.ToInt32(ResourceFlags.AllowCrossAdapter);
            ResourceFlagsMapper[Convert.ToInt32(H1ResourceFlags.AllowSimultaneousAccess)] = Convert.ToInt32(ResourceFlags.AllowSimultaneousAccess);
        }

        public static ResourceDescription ConvertToResourceDescDx12(H1GpuResourceDesc resourceDesc)
        {
            ResourceDescription result = new ResourceDescription();
            result.Dimension = (ResourceDimension)DimensionMapper[Convert.ToInt32(resourceDesc.Dimension)];
            result.Alignment = resourceDesc.Alignment;
            result.Width = resourceDesc.Width;
            result.Height = Convert.ToInt32(resourceDesc.Height);
            result.MipLevels = Convert.ToInt16(resourceDesc.MipLevels);
            result.SampleDescription.Count = Convert.ToInt32(resourceDesc.SampleDesc.Count);
            result.SampleDescription.Quality = Convert.ToInt32(resourceDesc.SampleDesc.Quality);
            result.Format = H1RHIDefinitionHelper.ConvertToFormat(resourceDesc.Format);
            result.Flags = (ResourceFlags)ResourceFlagsMapper[Convert.ToInt32(resourceDesc.Flags)];
            result.DepthOrArraySize = Convert.ToInt16(resourceDesc.DepthOrArraySize);

            return result;
        }

        private Resource m_GpuResource;        
    }
#endif
}
