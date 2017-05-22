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

#include "SGDThreadPCH.h"
#include "SGDWorkerThread.h"
// thread utils
#include "SGDThreadUtil.h"
// task scheduler
#include "SGDTaskScheduler.h"
using namespace SGD;

#if _WIN32
uint32_t __stdcall WorkerThreadEntryPoint(void* Data)
#endif
{
	// get worker thread instance
	H1WorkerThread* pWorkerThread = reinterpret_cast<H1WorkerThread*>(Data);
	// create thread fiber type
	pWorkerThread->ConvertThreadToFiber();
	
	// execute thread-main loop
	while (true)
	{
		// handling exit event
		if (pWorkerThread->IsQuit())
			break;

		// 1. look up wait-queue to find ready-to-execute task
		H1TaskScheduler* pTaskScheduler = pWorkerThread->GetTaskScheduler();
		// if there is no allocated task allocator, just skip it and looping infinitely until current thread get signaled to be quit
		// for unit-testing purpose
		if (pTaskScheduler == nullptr) 
			continue;

		H1WaitFiberContextQueue& rWaitFiberContextQueue = pTaskScheduler->GetWaitFiberContextQueue();

		// @TODO - how to decide whether we dequeue fiber-context[big or small]? : temporary use small right now
		H1FiberContext* fiberContextToProcess = nullptr;
		FiberId newFiberContextId = pTaskScheduler->GetWaitFiberContextQueue().Dequeue(EFiberType::EFT_Small);
		if (newFiberContextId != -1)
		{
			
		}
		// 2. if there is no available task in wait queue, get the task from task queue
		else
		{
			H1TaskDeclarationInterface* pNewTask = nullptr;
			// high-priority queue
			pNewTask = pTaskScheduler->GetTaskQueue(ETaskQueuePriority::ETQP_High)->DequeueTask();

			// mid-priority queue
			if (pNewTask == nullptr)
				pNewTask = pTaskScheduler->GetTaskQueue(ETaskQueuePriority::ETQP_Mid)->DequeueTask();

			// low-priority queue
			if (pNewTask == nullptr)
				pNewTask = pTaskScheduler->GetTaskQueue(ETaskQueuePriority::ETQP_Low)->DequeueTask();
			
			// there is no available task right now, skip to create and execute new fiber context
			if (pNewTask == nullptr)
				continue;

			// construct new fiber context adding newly popped task
			// @TODO - only handling small one right now
			// 1) dequeue free fiber context
			newFiberContextId = pTaskScheduler->GetFiberContextPool().DequeueFreeFiberContext(EFT_Small);
			// 2) construct new fiber context with new task
			pTaskScheduler->GetFiberContextPool().ConstructFiberContext(newFiberContextId, EFT_Small, pNewTask);
		}
		
		// 3. switch to fiber context with the task that we got
		// process the fiber context
		//	- it successfully finished current fiber-context 
		//	- or it is suspended to wait child tasks to be finished
		pWorkerThread->SwitchFiberContext(newFiberContextId, EFT_Small);
	}

	// successfully quit the thread entry point
	return 1;
}

H1WorkerThread::H1WorkerThread()
	: m_CPUCoreId(-1)
	, m_ThreadHandle(nullptr)
	, m_TaskScheduler(nullptr)
	, m_FiberContextSlotId(-1)
	, m_ThreadFiberContext(nullptr)
{
	
}

H1WorkerThread::~H1WorkerThread()
{

}

bool H1WorkerThread::Initialize(H1TaskScheduler* taskScheduler, int32_t lockedCPUCoreId)
{	
	// set quit atomic counter to false
	m_IsQuit.store(false);

	// set CPU core locked
	m_CPUCoreId = lockedCPUCoreId;

	// set task scheduler
	m_TaskScheduler = taskScheduler;

	return true;
}

void H1WorkerThread::Destroy()
{
	// deallocate ThreadFiberContext
	if (m_ThreadFiberContext != nullptr)
	{
		delete m_ThreadFiberContext;
		m_ThreadFiberContext = nullptr;
	}		

	// in case, still worker thread is running, signal to quit
	SignalQuit();
}

bool H1WorkerThread::Start()
{
	// start worker thread
	return appCreateThread(&m_ThreadHandle, &m_ThreadId, 0, WorkerThreadEntryPoint, this, m_CPUCoreId);
}

void H1WorkerThread::Wait()
{
	// join worker thread
	appJoinThread(1, &m_ThreadHandle);
}

bool H1WorkerThread::IsQuit()
{
	return m_IsQuit.load();
}

void H1WorkerThread::SignalQuit()
{
	m_IsQuit.store(true);
}

void H1WorkerThread::ConvertThreadToFiber()
{
	// create new fiber context, setting thread fiber type
	m_ThreadFiberContext = new H1FiberContextWindow();
	m_ThreadFiberContext->ConvertThreadToFiber();
}

void H1WorkerThread::SwitchFiberContext(FiberId fiberId, EFiberType fiberType)
{
	H1FiberContext* pFiberContext = nullptr;
	if (fiberType == EFiberType::EFT_Small)
		pFiberContext = m_TaskScheduler->GetFiberContextPool().GetFiberContextSmall(fiberId);
	else
		pFiberContext = m_TaskScheduler->GetFiberContextPool().GetFiberContextBig(fiberId);

	// update fiber id and fiber-type
	m_FiberContextType = fiberType;
	m_FiberContextSlotId = fiberId;

	// set owner
	pFiberContext->SetOwner(this);

	// switch to fiber
	pFiberContext->SwitchFiberContext();
}

void H1WorkerThread::SwitchThreadFiberContext()
{
	// udpate fiber id and fiber-type
	m_FiberContextSlotId = -1;
	m_FiberContextType = EFiberType::EFT_Thread;

	// switch to thread fiber context
	m_ThreadFiberContext->SwitchFiberContext();
}

H1FiberContext* H1WorkerThread::GetCurrentBindedFiberContext()
{
	if (m_FiberContextSlotId == -1)
		return nullptr; // currently no binded fiber (which means it binded with thread-fiber context)

	H1FiberContextPool& rFiberContextPool = m_TaskScheduler->GetFiberContextPool();
	H1FiberContext* pFiberContext = m_FiberContextType == EFiberType::EFT_Small ? rFiberContextPool.GetFiberContextSmall(m_FiberContextSlotId) : rFiberContextPool.GetFiberContextBig(m_FiberContextSlotId);
	return pFiberContext;
}

H1WorkerThreadPool::H1WorkerThreadPool()
{

}

H1WorkerThreadPool::~H1WorkerThreadPool()
{

}

bool H1WorkerThreadPool::Initialize(H1TaskScheduler* taskScheduler)
{
	// initialize thread pools
	int32_t cpuCoreCount = appGetNumHardwareThreads();
	m_WorkerThreads.resize(cpuCoreCount);

	for (int32_t i = 0; i < cpuCoreCount; ++i)
	{
		// initialize thread with core number by 'cpuCoreCount'
		m_WorkerThreads[i] = new H1WorkerThread();
		if (!m_WorkerThreads[i]->Initialize(taskScheduler, i))
			return false;
	}
	return true;
}

void H1WorkerThreadPool::Destroy()
{
	for (uint32_t i = 0; i < m_WorkerThreads.size(); ++i)
	{
		m_WorkerThreads[i]->Destroy();
		delete m_WorkerThreads[i];
	}
}

H1WorkerThread* H1WorkerThreadPool::GetWorkerThreadById(ThreadId threadId)
{
	for (H1WorkerThread* pWorkerThread : m_WorkerThreads)
	{
		if (pWorkerThread->GetThreadId() == threadId)
			return pWorkerThread;
	}
	return nullptr;
}

bool H1WorkerThreadPool::StartAll()
{
	// trigger each worker thread
	for (H1WorkerThread* pWorkerThread : m_WorkerThreads)
		if (!pWorkerThread->Start())
			return false;
	return true;
}

void H1WorkerThreadPool::WaitAll()
{
	// not single wait object call - wait all worker threads at once
	std::vector<ThreadType> groupThreads(m_WorkerThreads.size());
	for (uint32_t i = 0; i < m_WorkerThreads.size(); ++i)
		groupThreads[i] = m_WorkerThreads[i]->GetThreadHandle();
	appJoinThread(m_WorkerThreads.size(), groupThreads.data());
}

void H1WorkerThreadPool::SignalQuitAll()
{
	for (H1WorkerThread* pWorkerThread : m_WorkerThreads)
		pWorkerThread->SignalQuit(); // signal to quit thread
}

#if SGD_UNIT_TESTS
H1WorkerThreadPoolUnitTestKnot::H1WorkerThreadPoolUnitTestKnot()
	: H1WorkerThreadPool()
	, AllWorkerThreadTerminated(false)
{

}

H1WorkerThreadPoolUnitTestKnot::~H1WorkerThreadPoolUnitTestKnot()
{

}

void H1WorkerThreadPoolUnitTestKnot::WaitAll()
{
	H1WorkerThreadPool::WaitAll();

	// finally all thread successfully finished
	AllWorkerThreadTerminated = true;
}
#endif