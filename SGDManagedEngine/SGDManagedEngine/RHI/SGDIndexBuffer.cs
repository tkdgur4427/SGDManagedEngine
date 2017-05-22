using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD
{
    public class H1IndexBuffer : H1Resource
    {
        public H1IndexBuffer(H1GPUResourceManager manager, uint index, uint sizeInBytes)
            : base(manager, sizeInBytes)
        {
            m_Index = index;
            m_IndicesCount = sizeInBytes / sizeof(UInt32);
        }

        ~H1IndexBuffer()
        {
            m_ResourceManagerRef.RemoveIndexBuffer(Convert.ToInt32(m_Index));
        }

        public uint Index
        {
            get { return m_Index; }
        }

        public UInt32 Count
        {
            get { return m_IndicesCount; }
        }

        public Gen2Layer.H1IndexBufferView View
        {
            get
            {
                Gen2Layer.H1IndexBufferView newView = new Gen2Layer.H1IndexBufferView(
                    null, // @TODO - need to set proper H1GpuResource reference
                    Resource.GPUVirtualAddress, 
                    H1PixelFormat.R32_UINT, 
                    Convert.ToInt32(m_SizeInBytes));
                return newView;
            }
        }

        public Resource Resource
        {
            get { return m_ResourceManagerRef.GetIndexBuffer(Convert.ToInt32(m_Index)); }
        }

        public void WriteData<T>(T[] data) where T : struct
        {
            IntPtr dataBegin = Resource.Map(0);
            Utilities.Write(dataBegin, data, 0, data.Length);
            Resource.Unmap(0);
        }

        public static H1IndexBuffer ProcessIndexBuffer(uint[] indices)
        {
            int size = Utilities.SizeOf(indices);

            // create index buffer
            H1IndexBuffer newIndexBuffer = H1Global<H1ManagedRenderer>.Instance.CreateIndexBuffer(Convert.ToUInt32(size));

            // write the data to the index buffer
            newIndexBuffer.WriteData(indices);

            return newIndexBuffer;
        }

        // index for vertex buffer container in GPUResourceManager
        private uint m_Index;
        // indices count
        private UInt32 m_IndicesCount;
    }
}
