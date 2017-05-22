using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD
{
    public class H1VertexBuffer : H1Resource
    {
        public H1VertexBuffer(H1GPUResourceManager manager, uint index, uint sizeInBytes, uint strideInBytes)
            : base(manager, sizeInBytes)
        {
            m_Index = index;
            m_StrideInBytes = strideInBytes;
        }

        ~H1VertexBuffer()
        {
            m_ResourceManagerRef.RemoveVertexBuffer(Convert.ToInt32(m_Index));
        }

        public uint Index
        {
            get { return m_Index; }
        }

        public Gen2Layer.H1VertexBufferView View
        {
            get
            {
                // create new vertex buffer view and return it
                Gen2Layer.H1VertexBufferView newView = new Gen2Layer.H1VertexBufferView(
                    null, // @TODO - need to set proper H1GpuResource reference
                    Resource.GPUVirtualAddress,
                    Convert.ToInt32(m_SizeInBytes),
                    Convert.ToInt32(m_StrideInBytes));

                return newView;
            }
        }

        public Resource Resource
        {
            get { return m_ResourceManagerRef.GetVertexBuffer(Convert.ToInt32(m_Index)); }
        }

        public void WriteData<T>(T[] data) where T : struct
        {
            IntPtr dataBegin = Resource.Map(0);
            Utilities.Write(dataBegin, data, 0, data.Length);
            Resource.Unmap(0);
        }

        static public H1VertexBuffer ProcessVertexBuffer<T>(T[] vertices) where T : struct
        {
            // create new vertex buffer
            int size = Utilities.SizeOf(vertices);
            int strideSize = Utilities.SizeOf<T>();
            H1VertexBuffer newVertexBuffer = H1Global<H1ManagedRenderer>.Instance.CreateVertexBuffer(Convert.ToUInt32(size), Convert.ToUInt32(strideSize));

            // copy the vertices to the vertex buffer
            newVertexBuffer.WriteData(vertices);

            return newVertexBuffer;
        }

        // index for vertex buffer container in GPUResourceManager
        private uint m_Index;
        // stride in bytes
        private uint m_StrideInBytes;
    }
}
