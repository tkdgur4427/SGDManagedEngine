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

namespace SGD
{
	// forward declaration
	class H1TaskScheduler;

	class H1WorkerThread
	{
	public:
		H1WorkerThread();
		virtual ~H1WorkerThread();

		bool Initialize(H1TaskScheduler* taskScheduler, int32_t lockedCPUCoreId);
		void Destroy();

		bool Start();
		void Wait();

		bool IsQuit();
		void SignalQuit();

		void ConvertThreadToFiber();
		// switch to arbitrary fiber context
		void SwitchFiberContext(FiberId fiberId, EFiberType fiberType);
		// switch to thread fiber context
		void SwitchThreadFiberContext();
		// get current binded fiber context
		H1FiberContext* GetCurrentBindedFiberContext();

		inline int32_t GetCPUCoreId() { return m_CPUCoreId; }
		inline ThreadId GetThreadId() { return m_ThreadId; }
		inline ThreadType GetThreadHandle() { return m_ThreadHandle; }
		inline H1TaskScheduler* GetTaskScheduler() { return m_TaskScheduler; }
		inline H1FiberContext* GetThreadFiberContext() { return m_ThreadFiberContext; }

	private:
		// task scheduler reference
		H1TaskScheduler* m_TaskScheduler;
		// thread handle
		ThreadType m_ThreadHandle;
		// thread id
		ThreadId m_ThreadId;
		// CPU core id that this worker thread locked
		int32_t m_CPUCoreId;
		// fiber context slot currently attached
		EFiberType m_FiberContextType;
		FiberId m_FiberContextSlotId;
		// thread's fiber context
		H1FiberContext* m_ThreadFiberContext;
		// quit atomic counter
		std::atomic_bool m_IsQuit;
	};

	class H1WorkerThreadPool
	{
	public:
		H1WorkerThreadPool();
		~H1WorkerThreadPool();

		bool Initialize(H1TaskScheduler* taskScheduler);
		void Destroy();

		H1WorkerThread* GetWorkerThreadById(ThreadId threadId);
		inline H1WorkerThread* GetFirstThread() { return m_WorkerThreads.size() > 0 ? m_WorkerThreads[0] : nullptr; }

		UNIT_TEST_VIRTUAL bool StartAll();
		UNIT_TEST_VIRTUAL void WaitAll();
		UNIT_TEST_VIRTUAL void SignalQuitAll();

	private:
		std::vector<H1WorkerThread*> m_WorkerThreads;
	};

#if SGD_UNIT_TESTS 
	class H1WorkerThreadPoolUnitTestKnot : public H1WorkerThreadPool
	{
	public:
		H1WorkerThreadPoolUnitTestKnot();
		~H1WorkerThreadPoolUnitTestKnot();

		// overridden for unit-tests
		UNIT_TEST_VIRTUAL void WaitAll();

	public:
		// unit-test variables
		bool AllWorkerThreadTerminated;
	};
#endif
}