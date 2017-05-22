using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.DXGI;
using SharpDX.Direct3D12;

using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public class H1AsyncCommandQueue
    {
        enum TaskType
        {
            ExecuteCommandList,
            ResetCommandList,
            PresentBackBuffer,
            SignalFence,
            WaitForFence,
            WaitForFences,
        }

        public class TaskArgs
        {
            CommandQueue CmdQueueRef;
            volatile object QueueFramesCounter;     // UInt32
            volatile object SignalledFenceValue;    // UInt64
        }

        class TaskBody
        {
            public virtual void Process(TaskArgs args)
            {

            }
        }

        class ExecuteCommandlist : TaskBody
        {
            CommandList CommandListRef;
            public override void Process(TaskArgs args)
            {

            }
        }

        class ResetCommandlist : TaskBody
        {
            CommandList CommandListRef;
            public override void Process(TaskArgs args)
            {

            }
        }

        class SignalFence : TaskBody
        {
            Fence FenceRef;
            UInt64 FenceValue;

            public override void Process(TaskArgs args)
            {

            }
        }

        class WaitForFence : TaskBody
        {
            Fence FenceRef;
            UInt64 FenceValue;

            public override void Process(TaskArgs args)
            {

            }
        }

        class WaitForFences : TaskBody
        {
            Fence[] FencesRef;
            UInt64[] FenceValue = new UInt64[H1GlobalDX12Definitions.CMDQUEUE_NUM];

            public override void Process(TaskArgs args)
            {

            }
        }

        class PresentBackbuffer : TaskBody
        {
            SwapChain3 SwapChainRef;
            UInt32 SyncInterval;
            UInt32 Flags;

            public override void Process(TaskArgs args)
            {

            }
        }

        class SubmissionTask
        {
            TaskType m_Type;

            [StructLayout(LayoutKind.Explicit)]
            struct Data
            {
                [FieldOffset(0)]
                SignalFence SignalFenceData;

                [FieldOffset(0)]
                WaitForFence WaitForFenceData;

                [FieldOffset(0)]
                WaitForFences WaitForFencesData;

                [FieldOffset(0)]
                ExecuteCommandlist ExecuteCommandlistData;

                [FieldOffset(0)]
                ResetCommandlist ResetCommandlistData;

                [FieldOffset(0)]
                PresentBackbuffer PresentBackbufferData;
            }

            Data m_Data;

            void Process<T>(TaskArgs args) where T : TaskBody
            {
                T Task = m_Data as T;
                Task?.Process(args);
            }
        }

        void AddTask<T>(SubmissionTask task) where T : TaskBody
        {
            //...
        }

        UInt64 m_SignalledFenceValue; // need memory barrier
        volatile Int32 m_QueuedFramesCounter;
        volatile Boolean m_bStopRequested;
        volatile Boolean m_bSleeping;

        H1CommandListPool CmdListPoolRef;
        ConcurrentQueue<SubmissionTask> m_TaskQueue = new ConcurrentQueue<SubmissionTask>();
        // semaphore m_TaskEvent   
    }
}
