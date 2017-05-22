using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1CommandListFenceHelper
    {
        public static Int64 MaxFenceValue(ref Int64 a, Int64 b)
        {
            Int64 utilizedValue = b;
            Int64 previousValue = Interlocked.Read(ref a);
            while (previousValue < utilizedValue &&
                 previousValue != Interlocked.CompareExchange(ref a, utilizedValue, previousValue)) ;

            return b;
        }
    }    

    public class H1CommandListFence
    {
        public Fence Fence
        {
            get { return m_Fence; }
        }

        public Int64 CurrentValue
        {
            get { return Interlocked.Read(ref m_CurrentValue); }
        }

        public H1CommandListFence(H1DX12Device refDX12Device)
        {
            m_DeviceRef = refDX12Device;
            Interlocked.Exchange(ref m_LastCompletedValue, 0);
            Interlocked.Exchange(ref m_CurrentValue, 0);
        }

        public Boolean Initialize()
        {
            Fence fence = null;
            fence = m_DeviceRef.Device.CreateFence(0, FenceFlags.None);
            if (fence == null)
                return false;

            m_Fence = fence;
            fence.Dispose();

            m_Fence.Signal(Interlocked.Read(ref m_LastCompletedValue));
            m_FenceEvent = new AutoResetEvent(false);

            return true;
        }

        public void SetCurrentValue(Int64 fenceValue)
        {
            H1CommandListFenceHelper.MaxFenceValue(ref m_CurrentValue, fenceValue);
        }

        public Int64 AdvanceCompletion()
        {
            Int64 currentCompletedValue = m_Fence.CompletedValue;            
            H1CommandListFenceHelper.MaxFenceValue(ref m_LastCompletedValue, currentCompletedValue);

            return currentCompletedValue;
        }

        public Boolean IsCompleted(Int64 fenceValue)
        {
            return (Interlocked.Read(ref m_LastCompletedValue) >= fenceValue) || (AdvanceCompletion() >= fenceValue);
        }

        public void WaitForFence(Int64 fenceValue)
        {
            if (!IsCompleted(fenceValue))
            {
                m_Fence.SetEventOnCompletion(fenceValue, m_FenceEvent.SafeWaitHandle.DangerousGetHandle());
                m_FenceEvent.WaitOne();

                AdvanceCompletion();
            }
        }        

        private H1DX12Device m_DeviceRef;
        private Fence m_Fence;
        private AutoResetEvent m_FenceEvent;

        private Int64 m_CurrentValue;
        private Int64 m_LastCompletedValue;
    }

    class H1CommandListFenceSet
    {
        public H1CommandListFenceSet(H1DX12Device refDevice)
        {
            m_DeviceRef = refDevice;
        }

        public Boolean Initialize()
        {
            for (Int32 i = 0; i < H1GlobalDX12Definitions.CMDQUEUE_NUM; ++i)
            {
                Fence fence = null;
                fence = m_DeviceRef.Device.CreateFence(0, FenceFlags.None);
                if (fence == null)
                    return false;

                m_Fences[i] = fence;
                m_Fences[i].Signal(m_LastComputedValues[i]);
                m_FenceEvents[i] = new AutoResetEvent(false);
            }

            return true;
        }

        public Fence[] GetFences()
        {
            return m_Fences;
        }

        public Fence GetFence(Int32 id)
        {
            return m_Fences[id];
        }

        public Int64 GetSubmittedValue(Int32 id)
        {
            return m_SubmittedValues[id];
        }

        public void SetSubmittedValue(Int64 fenceValue, Int32 id)
        {
            H1CommandListFenceHelper.MaxFenceValue(ref m_SubmittedValues[id], fenceValue);
        }

        public void GetSubmittedValues(ref Int64[] fenceValues /*size = H1GlobalDX12Definition.CMDQUEUE_NUM*/)
        {
            fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS] = Interlocked.Read(ref m_SubmittedValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]);
            fenceValues[H1GlobalDX12Definitions.CMDQUEUE_COMPUTE] = Interlocked.Read(ref m_SubmittedValues[H1GlobalDX12Definitions.CMDQUEUE_COMPUTE]);
            fenceValues[H1GlobalDX12Definitions.CMDQUEUE_COPY] = Interlocked.Read(ref m_SubmittedValues[H1GlobalDX12Definitions.CMDQUEUE_COPY]);
        }        

        public Int64 GetCurrentValue(Int32 id)
        {
            return Interlocked.Read(ref m_CurrentValues[id]);
        }

        public void GetCurrentValues(ref Int64[] fenceValues /*size = H1GlobalDX12Definition.CMDQUEUE_NUM*/)
        {
            fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS] = Interlocked.Read(ref m_CurrentValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]);
            fenceValues[H1GlobalDX12Definitions.CMDQUEUE_COMPUTE] = Interlocked.Read(ref m_CurrentValues[H1GlobalDX12Definitions.CMDQUEUE_COMPUTE]);
            fenceValues[H1GlobalDX12Definitions.CMDQUEUE_COPY] = Interlocked.Read(ref m_CurrentValues[H1GlobalDX12Definitions.CMDQUEUE_COPY]);
        }

        public void SetCurrentValue(Int64 fenceValue, Int32 id)
        {
            H1CommandListFenceHelper.MaxFenceValue(ref m_CurrentValues[id], fenceValue);
        }

        public Int64 AdvancedCompletion(Int32 id)
        {
            Int64 currentCompletedValue = m_Fences[id].CompletedValue;

            H1CommandListFenceHelper.MaxFenceValue(ref m_LastComputedValues[id], currentCompletedValue);

            return currentCompletedValue;
        }

        public Boolean IsCompleted(Int64 fenceValue, Int32 id)
        {
            return (m_LastComputedValues[id] >= fenceValue) || (AdvancedCompletion(id) >= fenceValue);
        }

        public Boolean IsCompleted(Int64[] fenceValues)
        {
            return
                (Interlocked.Read(ref m_LastComputedValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) >= fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) || (AdvancedCompletion(Convert.ToInt32(H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS)) >= fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) & 
                (Interlocked.Read(ref m_LastComputedValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) >= fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) || (AdvancedCompletion(Convert.ToInt32(H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS)) >= fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) &
                (Interlocked.Read(ref m_LastComputedValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) >= fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]) || (AdvancedCompletion(Convert.ToInt32(H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS)) >= fenceValues[H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS]);
        }

        public void WaitForFence(Int64 fenceValue, Int32 id)
        {
            m_Fences[id].SetEventOnCompletion(fenceValue, m_FenceEvents[id].SafeWaitHandle.DangerousGetHandle());
            m_FenceEvents[id].WaitOne();

            // completed CPU on fence
            AdvancedCompletion(id);
        }

        public void AdvancedCompletions()
        {
            AdvancedCompletion(Convert.ToInt32(H1GlobalDX12Definitions.CMDQUEUE_GRAPHICS));
            AdvancedCompletion(Convert.ToInt32(H1GlobalDX12Definitions.CMDQUEUE_COMPUTE));
            AdvancedCompletion(Convert.ToInt32(H1GlobalDX12Definitions.CMDQUEUE_COPY));
        }

        void WaitForFence(Int64[] fenceValues)
        {
            Int32 numObjects = 0;            
            for (Int32 i = 0; i < H1GlobalDX12Definitions.CMDQUEUE_NUM; ++i)
            {
                if (fenceValues[i] != 0 && (Interlocked.Read(ref m_LastComputedValues[i]) < fenceValues[i]))
                {                    
                    m_Fences[i].SetEventOnCompletion(fenceValues[i], m_FenceEvents[numObjects++].SafeWaitHandle.DangerousGetHandle());
                }
            }
            
            WaitHandle.WaitAll(m_FenceEvents.Take(numObjects).ToArray());
            AdvancedCompletions();
        }

        H1DX12Device m_DeviceRef;
        Fence[] m_Fences = new Fence[H1GlobalDX12Definitions.CMDQUEUE_NUM];
        AutoResetEvent[] m_FenceEvents = new AutoResetEvent[H1GlobalDX12Definitions.CMDQUEUE_NUM];

        // maximum fence-value of all command-lists currently in flight (allocated, running or free)
        Int64[] m_CurrentValues = new Int64[H1GlobalDX12Definitions.CMDQUEUE_NUM];
        // maximum fence-value of all command-lists passed to the driver (running only)
        Int64[] m_SubmittedValues = new Int64[H1GlobalDX12Definitions.CMDQUEUE_NUM];
        // maximum fence-value of all command-lists executed by the driver (free only)
        Int64[] m_LastComputedValues = new Int64[H1GlobalDX12Definitions.CMDQUEUE_NUM];
    }
}
