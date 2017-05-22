using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1DX12Texture2D
    {
        public Resource Resource
        {
            get { return m_Resource.Resource; }
        }

        public static H1DX12Texture2D Create(Device device, Vector4 clearValue, H1Texture2D.Description desc, H1SubresourceData[] initialData)
        {
            // converting description to DX12 description
            ResourceDescription desc12 = new ResourceDescription
            {
                Format = H1RHIDefinitionHelper.ConvertToFormat(desc.Format),
                Width = Convert.ToInt32(desc.Width),
                Height = Convert.ToInt32(desc.Height),
                DepthOrArraySize = Convert.ToInt16(desc.ArraySize),
                MipLevels = Convert.ToInt16(desc.MipLevels),    
                Flags = ResourceFlags.None,
                Layout = TextureLayout.Unknown,
                Dimension = ResourceDimension.Texture2D,
            };

            desc12.SampleDescription.Count = Convert.ToInt32(desc.SampleDesc.Count);
            desc12.SampleDescription.Quality = Convert.ToInt32(desc.SampleDesc.Quality);

            HeapProperties heapProperties = new HeapProperties(HeapType.Default);
            ResourceStates resourceUsage = ResourceStates.CopyDestination;
            ClearValue value = H1GlobalDX12Definitions.GetDXGIFormatClearValue(desc12.Format, (desc.BindFlags & Convert.ToUInt32(H1BindFlag.DepthStencil)) != 0);
            Boolean allowClearValue = desc12.Dimension == ResourceDimension.Buffer;

            if (desc.Usage == H1Usage.Immutable)
            {
                heapProperties = new HeapProperties(HeapType.Default);
                resourceUsage = ResourceStates.CopyDestination;
            }

            else if (desc.Usage == H1Usage.Staging)
            {
                heapProperties = new HeapProperties(HeapType.Readback);
                resourceUsage = ResourceStates.CopyDestination;
            }

            else if (desc.Usage == H1Usage.Dynamic)
            {
                heapProperties = new HeapProperties(HeapType.Upload);
                resourceUsage = ResourceStates.GenericRead;
            }

            if (desc.CPUAccessFlags != 0)
            {
                if (desc.CPUAccessFlags == Convert.ToUInt32(H1CPUAccessFlag.Write))
                {
                    heapProperties = new HeapProperties(HeapType.Upload);
                    resourceUsage = ResourceStates.GenericRead;
                }

                else if (desc.CPUAccessFlags == Convert.ToUInt32(H1CPUAccessFlag.Read))
                {
                    heapProperties = new HeapProperties(HeapType.Readback);
                    resourceUsage = ResourceStates.CopyDestination;
                }

                else
                {
                    return null;
                }
            }

            if (clearValue != null)
            {
                if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.DepthStencil)) != 0)
                {
                    value.DepthStencil.Depth = clearValue.X;
                    value.DepthStencil.Stencil = Convert.ToByte(clearValue.Y);
                }
                else
                {
                    value.Color = clearValue;
                }
            }

            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.UnorderedAccess)) != 0)
            {
                desc12.Flags |= ResourceFlags.AllowUnorderedAccess;
                resourceUsage = ResourceStates.UnorderedAccess;
            }

            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.DepthStencil)) != 0)
            {
                desc12.Flags |= ResourceFlags.AllowDepthStencil;
                allowClearValue = true;
                resourceUsage = ResourceStates.DepthWrite;
            }

            if ((desc.BindFlags & Convert.ToUInt32(H1BindFlag.RenderTarget)) != 0)
            {
                desc12.Flags |= ResourceFlags.AllowRenderTarget;
                allowClearValue = true;
                resourceUsage = ResourceStates.RenderTarget;
            }

            Resource resource = device.CreateCommittedResource(
                heapProperties,
                HeapFlags.None,
                desc12,
                resourceUsage,
                allowClearValue ? value : default(ClearValue?));

            H1DX12Texture2D newTexture = new H1DX12Texture2D();
            newTexture.m_Resource = new H1DX12Resource(resource, resourceUsage, resource.Description, initialData, Convert.ToUInt32(desc12.DepthOrArraySize * desc12.MipLevels));
            return newTexture;
        }

        private H1DX12Resource m_Resource;
    }
}
