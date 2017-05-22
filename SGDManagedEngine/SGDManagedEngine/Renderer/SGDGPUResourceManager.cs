using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD
{
    public class H1GPUResourceManager
    {
        public Device Device
        {
            get { return m_RendererRef.Device; }
        }

        public H1GPUResourceManager()
        {

        }

        public void Initialize(H1ManagedRenderer renderer)
        {
            m_RendererRef = renderer;
        }

        public void Destroy()
        {
            foreach (VertexBufferWrapper vertexBufferWrapper in m_VertexBuffers)
                vertexBufferWrapper.Dispose();
            foreach (IndexBufferWrapper indexBufferWrapper in m_IndexBuffers)
                indexBufferWrapper.Dispose();

            m_VertexBuffers.Clear();
            m_IndexBuffers.Clear();
        }

        public Resource GetVertexBuffer(int index)
        {
            return m_VertexBuffers[index].ResourcePtr;
        }

        public void RemoveVertexBuffer(Int32 index)
        {
            //m_VertexBuffers[index].Dispose();
            m_VertexBuffers[index] = null;
        }

        public Resource GetIndexBuffer(int index)
        {
            return m_IndexBuffers[index].ResourcePtr;
        }

        public void RemoveIndexBuffer(Int32 index)
        {
            //m_IndexBuffers[index].Dispose();
            m_IndexBuffers[index] = null;
        }

        public Resource GetGeneralBuffer(int index)
        {
            return m_GeneralBuffers[index].ResourcePtr;
        }

        public Resource GetTexture2D(int index)
        {
            return m_Texture2Ds[index].Resource;
        }

        public int CreateVertexBuffer(uint bufferSize)
        {
            Resource vertexBuffer = CreateCommonBuffer(bufferSize);

            // try to find out available index in VertexBuffers
            Int32 newIndex = m_VertexBuffers.FindIndex(x => { return x == null; });

            // if there is no available index
            if (newIndex == -1)
            {
                newIndex = m_VertexBuffers.Count;
                m_VertexBuffers.Add(new VertexBufferWrapper() { ResourcePtr = vertexBuffer });
            }
            else // if the available index exists
            {
                m_VertexBuffers[newIndex] = new VertexBufferWrapper() { ResourcePtr = vertexBuffer };
            }

            return newIndex;   
        }
        
        public int CreateIndexBuffer(uint bufferSize)
        {
            Resource indexBuffer = CreateCommonBuffer(bufferSize);

            Int32 newIndex = m_IndexBuffers.FindIndex(x => { return x == null; });

            // if there is no available index
            if (newIndex == -1)
            {
                newIndex = m_IndexBuffers.Count;
                m_IndexBuffers.Add(new IndexBufferWrapper() { ResourcePtr = indexBuffer });
            }
            else // if the available index exists
            {
                m_IndexBuffers[newIndex] = new IndexBufferWrapper() { ResourcePtr = indexBuffer };
            }

            return newIndex;
        }

        public int CreateGeneralBuffer(uint bufferSize)
        {
            Resource generalBuffer = CreateCommonBuffer(bufferSize);

            // get the new index
            int newIndex = m_GeneralBuffers.Count;
            m_GeneralBuffers.Add(new GeneralBufferWrapper() { ResourcePtr = generalBuffer });

            return newIndex;
        }

        public void DeleteGeneralBuffer(int index)
        {
            Resource generalBuffer = m_GeneralBuffers[index].ResourcePtr;

            // dispose the resource and remove from the container
            generalBuffer.Dispose();
            m_GeneralBuffers.RemoveAt(index);
        }

        Resource CreateCommonBuffer(uint resourceSize)
        {
            // @TODO - I will change change this more flexible memory management with 'Heap' class in SharpDX
            HeapProperties heapProperties = new HeapProperties(
                HeapType.Upload // specifies a heap for uploading, it has CPU access optimized for uploading to the GPU - best for CPU-write-once, GPU-read-once 
                );
            // create new resource
            return m_RendererRef.Device.CreateCommittedResource(heapProperties, HeapFlags.None, ResourceDescription.Buffer(resourceSize), ResourceStates.GenericRead);
        }
        
        public int CreateTexture2D(H1PixelFormat elementType, int width, int height, Vector4 clearValue, H1SubresourceData[] subResources)
        {
            // @TODO - I will change change this more flexible memory management with 'Heap' class in SharpDX
            HeapProperties heapProperties = new HeapProperties(
                HeapType.Upload // specifies a heap for uploading, it has CPU access optimized for uploading to the GPU - best for CPU-write-once, GPU-read-once 
                );

            // HeapType (from msdn)
            // 1. Upload Heaps - a heap for loading data from the CPU to the GPU, typically containing vertex, index or constant buffers or textures and other resources
            // 2. Readback Heaps - a heap for loading data back from the GPU to the CPU. Typically data that would be collected by the GPU and downloaded to the CPU would be image data from
            //                     a captured screen shot and statistical and performance data such as counters and timings
            // 3. Default Heaps - a heap which supports all GPU operations. This resource heap is focused on containing data that is persistently required by the GPU, such as index and vertex data that maybe required for many frames

            // Resources (from msdn)
            // 1. Commited resources
            //    - commited resources are the most common idea of D3D resources over the generations.
            //    - creating such a resource allocates virtual address range, an implicit heap large enough to fit the whole resource and commits the virtual address range to the physical memory encapsulated by the heap
            //    - the implicit heap properties must be passed to match functional parity with previous D3D versions.
            // 2. Reserved resources
            //    - Reserved resources are equivalent to D3D11 tiled resources.
            //    - On their creation, only a virtual address range is allocated and not mapped to any heap.
            //    - the application will map such resources to heaps latere
            //    - the capabilities of such resources are currently unchanged over D3D11, as they can be mapped to a heap at a 64KB tile granularity with 'UpdateTileMappings'
            // 3. Placed resources
            //    - new for D3D12, applications may create heaps seperate from resources.
            //    - afterward, the application may locate multiple resources within a single heap.
            //    - this can be done without creating tiled or reserved resources, enabling the capabilities for all resource types able to be created directly by applications.
            //    - multiple resources may overlap, and the application must use the 'TiledResourceBarrier' to re-use physical memory correctly.

            // create new resource
            H1Texture2D.Description desc = new H1Texture2D.Description();
            desc.Width = Convert.ToUInt32(width);
            desc.Height = Convert.ToUInt32(height);
            desc.Format = elementType;
            desc.SampleDesc.Count = 1;
            desc.SampleDesc.Quality = 0;
            desc.MipLevels = 0;
            desc.ArraySize = 1;                                 

            Direct3D12.H1DX12Texture2D texture2D = Direct3D12.H1DX12Texture2D.Create(Device, clearValue, desc, subResources);

            Int32 index = m_Texture2Ds.Count;
            m_Texture2Ds.Add(texture2D);

            return index;       
        }   

        public Int32 CreateShaderResourceView()
        {
            return -1;
        }

        protected class ResourceWrapper : IDisposable
        {
            public Resource ResourcePtr = null;

            #region IDisposable Support
            protected bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).                        
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.
                    if (ResourcePtr != null) ResourcePtr.Dispose();

                    // base class dispose
                    // base.Dispose(disposing);

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            ~ResourceWrapper()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(false);
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                GC.SuppressFinalize(this);
            }
            #endregion
        }

        // disposable buffers which automatically collected resources (DX12 Resource)
        private class VertexBufferWrapper : ResourceWrapper { }

        private class IndexBufferWrapper : ResourceWrapper { }

        private class GeneralBufferWrapper : ResourceWrapper { }

        // @TODO - I need to convert this into H1DX12Resource
        private readonly List<VertexBufferWrapper> m_VertexBuffers = new List<VertexBufferWrapper>();
        private readonly List<IndexBufferWrapper> m_IndexBuffers = new List<IndexBufferWrapper>();        
        private readonly List<GeneralBufferWrapper> m_GeneralBuffers = new List<GeneralBufferWrapper>();

        private readonly List<Direct3D12.H1DX12Texture2D> m_Texture2Ds = new List<Direct3D12.H1DX12Texture2D>();

        private H1ManagedRenderer m_RendererRef;

        //---------------------------------------------------------------------
        // Gen2Layer for GPUResourceManager


    }
}
