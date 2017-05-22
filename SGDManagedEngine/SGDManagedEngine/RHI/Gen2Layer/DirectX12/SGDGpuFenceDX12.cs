using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12    
    public partial class H1GpuFence
    {
        public Fence FenceValue
        {
            get { return m_Fence; }
        }

        void InitializePlatformDependent()
        {
            m_Fence = H1Global<H1ManagedRenderer>.Instance.Device.CreateFence(0, FenceFlags.None);
            if (m_Fence == null)
            {
                // @TODO need to assert
            }
        }

        void DestroyPlatformDependent()
        {
            // platform-dependent dispose for DX12
            m_Fence.Dispose();
        }

        public Int64 UpdateLastCompletedFenceValue()
        {
            m_LastCompletedFenceValue = Math.Max(m_LastCompletedFenceValue, m_Fence.CompletedValue);
            return m_LastCompletedFenceValue;
        }

        private Fence m_Fence;
    }
#endif
}
