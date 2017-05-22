// Simplified BSD license:
// Copyright (c) 2016-2016, SangHyeok Hong.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice, this list of
// conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice, this list of
// conditions and the following disclaimer in the documentation and/or other materials
// provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
// THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
// OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
// TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#pragma once

#include "SGDFiberContext.h"
#include "SGDWorkerThread.h"
#include "SGDTaskQueue.h"
#include "SGDWaitFiberContextQueue.h"

namespace SGD
{
	class H1TaskScheduler
	{
	public:		
		H1TaskScheduler();
		~H1TaskScheduler();

		bool Initialize();
		void Destroy();

		H1WorkerThread* GetCurrentThread();

		inline ThreadId GetMainThreadId() { return m_MainThreadId; }
		inline H1TaskCounter* GetMainThreadTaskCounter() { return m_MainThreadTaskCounter; }

		inline H1WorkerThreadPool& GetWorkerThreadPool() { return m_WorkerThreadPool;}
		inline H1FiberContextPool& GetFiberContextPool() { return m_FiberContextPool; }
		inline H1WaitFiberContextQueue& GetWaitFiberContextQueue() { return m_WaitFiberContextQueue; }
		inline H1TaskQueue* GetTaskQueue(ETaskQueuePriority tqPriority) { return m_TaskQueues[tqPriority]; }

	private:
		// fiber context pool
		H1FiberContextPool m_FiberContextPool;
		// worker thread pool
		H1WorkerThreadPool m_WorkerThreadPool;
		// wait fiber context queue
		// @TODO - need to think about the design for concurrent wait fiber context queue
		//	- there are big problems to make it concurrent container with wait-queue
		H1WaitFiberContextQueue m_WaitFiberContextQueue;
		// task queues (high, mid, low) - concurrent task queue
		//	- multiple threads access these queues
		H1TaskQueue* m_TaskQueues[ETaskQueuePriority::ETQP_Max];
		// main thread
		ThreadType m_MainThread;
		ThreadId m_MainThreadId;
		H1TaskCounter* m_MainThreadTaskCounter;
	};

	class H1TaskSchedulerLayer
	{
	public:
		// task scheduler getter 
		static H1TaskScheduler* GetTaskScheduler();

		// these methods (initialization & destruction of task scheduler) is NOT thread-safe method
		// only execute this in one particular thread!!
		// NOTE THAT - if you really want to use this in multi-thread environment, you must use lock to execute these methods
		static bool InitializeTaskScheduler();
		static void DestroyTaskScheduler();

		static bool StartTaskScheduler();
		static void WaitTaskScheduler();

		// public methods (utility functions) used for TaskScheduler(fiber-based)
		static bool RunTasks(H1TaskDeclarationInterface** tasks, int32_t taskCounts, H1TaskCounter** ppTaskCounter);
		static bool WaitForCounter(H1TaskCounter* pTaskCounter, H1TaskCounter::TaskCounterType value = 0);

	private:
		static H1TaskScheduler* gTaskScheduler;
	};
}
