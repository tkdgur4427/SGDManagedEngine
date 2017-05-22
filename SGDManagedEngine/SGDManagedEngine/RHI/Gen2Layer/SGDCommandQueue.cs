using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public enum H1CommandListType
    {
        Direct = 0,
        Bundle,
        Compute,
        Copy,
        Num,
    }

    public partial class H1CommandQueue
    {
        public H1GpuFence Fence
        {
            get { return m_Fence; }
        }

        public H1CommandQueue(H1CommandListType type)
        {
            m_Type = type;
        }

        public Boolean Initialize()
        {
            InitializePlatformDependent();
            if (!HasCommandQueue())
                return false;

            return true;
        }

        public void Destroy()
        {
            DestroyPlatformDependent();
        }
        
        protected H1CommandListType m_Type;
        protected H1GpuFence m_Fence;
    }
}
