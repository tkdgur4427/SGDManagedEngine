using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public class H1GpuBuffer : H1GpuResource
    {
        protected H1GpuBuffer(H1ResourceFlags resourceFlags, H1GpuViewTypes allowedViews)
        {

        }

        protected virtual Boolean CreateViews()
        {
            // @TODO implemented
            return false;
        }

        protected void SetValidAllocViewBits(H1GpuViewTypes allowViews)
        {
            Int32 endIndex = Convert.ToInt32(H1GpuViewTypes.TotalNum);
            for (Int32 searchIndex = 0; searchIndex < endIndex; searchIndex++)
            {

            }
        }
                
        protected H1ResourceFlags m_ResourceFlags = H1ResourceFlags.Unknown;

        // Gpu views are determined by the type of H1GpuBuffer
        // 1. H1VertexBuffer
        //      - Vertex Buffer View
        // 2. H1IndexBuffer
        //      - Index Buffer View
        // 3. H1UniformBuffer (constant buffer)
        //      - Constant Buffer View
        // 4. H1ResourceBuffer
        //      - Shader Resource View
        //      - Unordered Access View
        //      - Derived Buffers
        //          - H1ByteAddressBuffer
        //          - H1StructuredBuffer
        //          - H1TypedBuffer
        // - (1.)(2.)(3.) can be made by sub-buffer allocation
        // - But, (4.) can't be made by sub-buffer 
        //      - Because, it should have UAV and SRV descriptors with Resource Ptr (DX12)

        // Gpu view alloc bits
        protected BitArray m_GpuViewAllocBits = new BitArray(Convert.ToInt32(H1GpuViewTypes.TotalNum), false);
        // container for views
        protected H1GpuView[] m_GpuViews = new H1GpuView[Convert.ToInt32(H1GpuViewTypes.TotalNum)];        
    }

    public enum H1GpuBufferAllocType
    {
        Single,
        Segmented,
    }
    /*
    // segmented buffer which can be sub-allocated 
    //  - only allowed to sub-allocate with same type (type-specific allocation)
    //  - only allowed to create these types
    //      - Vertex Buffer
    //      - Index Buffer
    //      - Uniform Buffer
    public class H1GpuBufferSegmented : H1GpuBuffer
    {
        static public H1GpuBufferSegmented Create(H1HeapType heapType, H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {

        }

        protected H1GpuBufferAllocType m_GpuBufferAllocType = H1GpuBufferAllocType.Segmented;
    }

    // single buffer which can NOT be sub-allocated
    //  - all types of buffer are allowed 
    public class H1GpuBufferSingle : H1GpuBuffer
    {
        protected H1GpuBufferAllocType m_GpuBufferAllocType = H1GpuBufferAllocType.Single;
    }

    
    public class H1VertexBuffer : H1GpuBuffer
    {
        protected H1VertexBuffer()
        {
            
        }        

        public static H1VertexBuffer CreateVertexBuffer()
        {
            // @TODO - not implemented yet (after implementing GpuMemoryManager)
            return null;
        }

        protected override bool CreateViews()
        {
            if (!SetValidAllocViewBits())
                return false;

            // not range is specified
            if (m_SubBufferRange == null)
                return false;

            // vertex buffer should be typed
            if (m_SubBufferRange.Type != SubBufferRangeType.Typed)
                return false;     

            // create vertex buffer view
            Int32 vertexBufferViewBitIndex = Convert.ToInt32(H1ViewType.VertexBufferView);
            m_GpuViews[vertexBufferViewBitIndex] = new H1VertexBufferView(m_ResourceRef, m_SubBufferRange.BufferOffset, m_SubBufferRange.BufferSize, m_SubBufferRange.ElementSize);

            return true;
        }

        protected override Boolean SetValidAllocViewBits()
        {
            // only enable 'Vertex Buffer View'
            Int32 bitIndex = Convert.ToInt32(H1ViewType.VertexBufferView);
            if (m_GpuViewAllocBits.Get(bitIndex) == true)
                return false; // it is already created

            // set alloc bits
            m_GpuViewAllocBits.Set(bitIndex, true);

            return true;
        }
    }    
    public class H1IndexBuffer : H1GpuBuffer
    {
        public H1IndexBuffer(H1GpuResource resourceRef, SubBufferRange range)
        {
            m_ResourceRef = resourceRef;
            m_SubBufferRange = range;
        }

        public static H1IndexBuffer CreateIndexBuffer()
        {
            // @TODO - not implemented yet (after implementing GpuMemoryManager)
            return null;
        }

        protected override bool CreateViews()
        {
            if (!SetValidAllocViewBits())
                return false;

            // not range is specified
            if (m_SubBufferRange == null)
                return false;

            // index buffer should be typed
            if (m_SubBufferRange.Type != SubBufferRangeType.Typed)
                return false;

            H1PixelFormat indexBufferFormat = H1PixelFormat.INVALID;
            if (m_SubBufferRange.ElementSize == H1RHIDefinitionHelper.ElementTypeToSize(H1PixelFormat.R16_UINT))
            {
                indexBufferFormat = H1PixelFormat.R16_UINT;
            }
            else if (m_SubBufferRange.ElementSize == H1RHIDefinitionHelper.ElementTypeToSize(H1PixelFormat.R32_UINT))
            {
                indexBufferFormat = H1PixelFormat.R32_UINT;
            }
            else // not valid element size for index buffer
                return false;

            // create index buffer view
            Int32 indexBufferViewBitIndex = Convert.ToInt32(H1ViewType.IndexBufferView);
            m_GpuViews[indexBufferViewBitIndex] = new H1IndexBufferView(m_ResourceRef, m_SubBufferRange.BufferOffset, indexBufferFormat, m_SubBufferRange.BufferSize);

            return true;
        }

        protected override Boolean SetValidAllocViewBits()
        {
            // only enable 'Index Buffer View'
            Int32 bitIndex = Convert.ToInt32(H1ViewType.IndexBufferView);
            if (m_GpuViewAllocBits.Get(bitIndex) == true)
                return false; // it is already created

            // set alloc bits
            m_GpuViewAllocBits.Set(bitIndex, true);

            return true;
        }
    }
    */
    /*
    public partial class H1ByteAddressBuffer : H1GpuBuffer
    {
        protected override Boolean CreateViews()
        {
            // 1. create shader resource views
            H1ShaderResourceViewDescription descSRV = new H1ShaderResourceViewDescription();
            descSRV.Format = H1PixelFormat.R32_TYPELESS;
            descSRV.ViewDimension = H1ShaderResourceViewDimension.Buffer;

            descSRV.Buffer = new H1BufferSharderResourceViewDesc();
            descSRV.Buffer.FirstElement = 0;
            descSRV.Buffer.NumElement = m_BufferSize / 4; // the element size is 4 bytes (R32)
            descSRV.Buffer.Flags = H1BufferShaderResourceViewFlags.Raw; // view buffer srv as 'raw'

            m_GpuViews[Convert.ToInt32(H1ViewType.ShaderResourceView)] = new H1ShaderResourceView(m_ResourceRef);

            H1ShaderResourceView gpuViewSRV = m_GpuViews[Convert.ToInt32(H1ViewType.ShaderResourceView)] as H1ShaderResourceView;
            gpuViewSRV.Description = descSRV;
            if (!gpuViewSRV.CreateView())
                return false;

            // 2. create unordered access views
            H1UnorderedAccessViewDescription descUAV = new H1UnorderedAccessViewDescription();
            descUAV.Format = H1PixelFormat.R32_TYPELESS;
            descUAV.ViewDimension = H1UnorderedAccesViewDimension.Buffer;

            descUAV.Buffer.FirstElement = 0;
            descUAV.Buffer.CounterOffsetInBytes = 0;
            descUAV.Buffer.StructureByteStride = 0;

            descUAV.Buffer.NumElements = m_BufferSize / 4;
            descUAV.Buffer.Flags = H1UnorderedAccessViewFlags.Raw;

            m_GpuViews[Convert.ToInt32(H1ViewType.UnorderedAccessView)] = new H1UnorderedAccessView(m_ResourceRef);
            H1UnorderedAccessView gpuViewUAV = m_GpuViews[Convert.ToInt32(H1ViewType.UnorderedAccessView)] as H1UnorderedAccessView;
            gpuViewUAV.Description = descUAV;
            if (!gpuViewSRV.CreateView())
                return false;

            return true;
        }
    } 

    public class H1IndirectArgsBuffer : H1ByteAddressBuffer
    {
        
    }
    
    public partial class H1StructuredBuffer : H1GpuBuffer
    {
        public override void Destroy()
        {
            base.Destroy();
            m_CounterBuffer.Destroy();
        }

        protected override Boolean CreateViews()
        {
            // 1. create shader resource views
            H1ShaderResourceViewDescription descSRV = new H1ShaderResourceViewDescription();
            descSRV.Format = H1PixelFormat.Unknown;
            descSRV.ViewDimension = H1ShaderResourceViewDimension.Buffer;

            descSRV.Buffer = new H1BufferSharderResourceViewDesc();
            descSRV.Buffer.FirstElement = 0;
            descSRV.Buffer.StructuredByteStride = m_ElementSize; // StructuredByteStride holds element size
            descSRV.Buffer.NumElement = m_ElementCount; 
            descSRV.Buffer.Flags = H1BufferShaderResourceViewFlags.None; // view buffer srv as 'none'

            m_GpuViews[Convert.ToInt32(H1ViewType.ShaderResourceView)] = new H1ShaderResourceView(m_ResourceRef);

            H1ShaderResourceView gpuViewSRV = m_GpuViews[Convert.ToInt32(H1ViewType.ShaderResourceView)] as H1ShaderResourceView;
            gpuViewSRV.Description = descSRV;
            if (!gpuViewSRV.CreateView())
                return false;

            // 2. create unordered access views
            H1UnorderedAccessViewDescription descUAV = new H1UnorderedAccessViewDescription();
            descUAV.Format = H1PixelFormat.Unknown;
            descUAV.ViewDimension = H1UnorderedAccesViewDimension.Buffer;

            descUAV.Buffer.FirstElement = 0;
            descUAV.Buffer.CounterOffsetInBytes = 0;

            descUAV.Buffer.StructureByteStride = m_ElementSize;
            descUAV.Buffer.NumElements = m_ElementCount;
            descUAV.Buffer.Flags = H1UnorderedAccessViewFlags.None;

            m_GpuViews[Convert.ToInt32(H1ViewType.UnorderedAccessView)] = new H1UnorderedAccessView(m_ResourceRef);
            H1UnorderedAccessView gpuViewUAV = m_GpuViews[Convert.ToInt32(H1ViewType.UnorderedAccessView)] as H1UnorderedAccessView;
            gpuViewUAV.Description = descUAV;
            if (!gpuViewSRV.CreateView())
                return false;

            // @TODO - need to create counter buffer
            // 3. create counter buffer            

            return true;
        }

        private H1ByteAddressBuffer m_CounterBuffer = new H1ByteAddressBuffer();
    }   
    
    public partial class H1TypedBuffer : H1GpuBuffer
    {
        H1TypedBuffer(H1PixelFormat format)
        {
            m_Format = format;
        }

        protected override Boolean CreateViews()
        {
            // 1. create shader resource views
            H1ShaderResourceViewDescription descSRV = new H1ShaderResourceViewDescription();
            descSRV.Format = m_Format;
            descSRV.ViewDimension = H1ShaderResourceViewDimension.Buffer;

            descSRV.Buffer = new H1BufferSharderResourceViewDesc();
            descSRV.Buffer.FirstElement = 0;
            descSRV.Buffer.StructuredByteStride = 0;

            descSRV.Buffer.NumElement = m_ElementCount;
            descSRV.Buffer.Flags = H1BufferShaderResourceViewFlags.None; // view buffer srv as 'none'

            m_GpuViews[Convert.ToInt32(H1ViewType.ShaderResourceView)] = new H1ShaderResourceView(m_ResourceRef);

            H1ShaderResourceView gpuViewSRV = m_GpuViews[Convert.ToInt32(H1ViewType.ShaderResourceView)] as H1ShaderResourceView;
            gpuViewSRV.Description = descSRV;
            if (!gpuViewSRV.CreateView())
                return false;

            // 2. create unordered access views
            H1UnorderedAccessViewDescription descUAV = new H1UnorderedAccessViewDescription();
            descUAV.Format = m_Format;
            descUAV.ViewDimension = H1UnorderedAccesViewDimension.Buffer;

            descUAV.Buffer.FirstElement = 0;
            descUAV.Buffer.CounterOffsetInBytes = 0;
            descUAV.Buffer.StructureByteStride = 0;

            descUAV.Buffer.NumElements = m_ElementCount;
            descUAV.Buffer.Flags = H1UnorderedAccessViewFlags.None;

            m_GpuViews[Convert.ToInt32(H1ViewType.UnorderedAccessView)] = new H1UnorderedAccessView(m_ResourceRef);
            H1UnorderedAccessView gpuViewUAV = m_GpuViews[Convert.ToInt32(H1ViewType.UnorderedAccessView)] as H1UnorderedAccessView;
            gpuViewUAV.Description = descUAV;
            if (!gpuViewSRV.CreateView())
                return false;

            return true;
        }

        private H1PixelFormat m_Format;
    } 
    */
}
