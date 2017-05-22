using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// auto-reset event
using System.Threading;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1GpuFence
    {
        public Int64 NextFrameFenceValue
        {
            get { return m_NextFrameFenceValue; }
            set { m_NextFrameFenceValue = value; }
        }

        public Int64 LastCompletedFenceValue
        {
            get { return m_LastCompletedFenceValue; }
        }

        public AutoResetEvent FenceEvent
        {
            get { return m_FenceEvent; }
        }

        public H1GpuFence()
        {
            
        }

        Boolean IntializePlatformIndependent()
        {
            m_NextFrameFenceValue = 0;
            m_LastCompletedFenceValue = 0;
            m_FenceEvent = new AutoResetEvent(false);

            return true;
        }
        
        public Boolean Initialize()
        {
            InitializePlatformDependent();
            return IntializePlatformIndependent();
        }

        public void DestroyPlatformIndependent()
        {
            // manually dispose event instance
            m_FenceEvent.Dispose();
        }

        public void Destroy()
        {
            DestroyPlatformDependent();
            DestroyPlatformIndependent();
        }

        public Int64 IncreNextFenceValue()
        {
            // increment next frame fence value atomically
            Int64 prevValue = Interlocked.Increment(ref m_NextFrameFenceValue);
            return prevValue;
        }

        public Int64 UpdateLastCompletedFenceValue(Int64 newFenceValue)
        {
            Int64 prevValue = Interlocked.Exchange(ref m_LastCompletedFenceValue, newFenceValue);
            return prevValue;
        }

        protected Int64 m_NextFrameFenceValue;
        protected Int64 m_LastCompletedFenceValue;
        protected AutoResetEvent m_FenceEvent;
    }
}
