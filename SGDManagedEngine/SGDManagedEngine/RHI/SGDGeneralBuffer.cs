using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD
{
    public class H1GeneralBuffer : H1Resource
    {
        public H1GeneralBuffer(H1GPUResourceManager manager, uint index, uint sizeInBytes)
            : base(manager, sizeInBytes)
        {
            m_Index = index;
        }

        public uint Index
        {
            get { return m_Index; }
        }

        public Resource Resource
        {
            get { return m_ResourceManagerRef.GetGeneralBuffer(Convert.ToInt32(m_Index)); }
        }

        public void WriteData<T>(T[] data) where T : struct
        {
            IntPtr dataBegin = Resource.Map(0);
            Utilities.Write(dataBegin, data, 0, data.Length);
            Resource.Unmap(0);
        }

        public void WriteData(IntPtr dataPointer, Int32 dataSize)
        {
            IntPtr dataBegin = Resource.Map(0);
            Utilities.CopyMemory(dataBegin, dataPointer, dataSize);
            Resource.Unmap(0);
        }

        private uint m_Index;
    }
}
