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
#include "SGDTaskScheduler.h"
using namespace SGD;

SGD::H1TaskScheduler* SGD::H1TaskSchedulerLayer::gTaskScheduler = nullptr;

H1TaskScheduler::H1TaskScheduler()
	: m_WaitFiberContextQueue(this)
	, m_MainThread(nullptr)
	, m_MainThreadId(-1)
{
	// setting nullptr for task queues
	for (uint32_t i = 0; i < ETaskQueuePriority::ETQP_Max; ++i)
	{
		m_TaskQueues[i] = nullptr;
	}
}

H1TaskScheduler::~H1TaskScheduler()
{

}

bool H1TaskScheduler::Initialize()
{
	// set main thread
	// suppose the task scheduler is initialized in the main thread
	m_MainThread = appGetCurrentThread();
	m_MainThreadId = appGetCurrentThreadId();
	m_MainThreadTaskCounter = new H1TaskCounter();

	// initialize fiber context pool
	if (!m_FiberContextPool.Initialize())
		return false;

	// initialize worker thread pool
	if (!m_WorkerThreadPool.Initialize(this))
		return false;

	// initialize wait-fiber-context queue
	if (!m_WaitFiberContextQueue.Initialize())
		return false;

	// initialize task queues
	m_TaskQueues[ETaskQueuePriority::ETQP_High] = new H1TaskQueue(ETaskQueuePriority::ETQP_High);
	m_TaskQueues[ETaskQueuePriority::ETQP_Mid] = new H1TaskQueue(ETaskQueuePriority::ETQP_Mid);
	m_TaskQueues[ETaskQueuePriority::ETQP_Low] = new H1TaskQueue(ETaskQueuePriority::ETQP_Low);

	return true;
}

void H1TaskScheduler::Destroy()
{
	// destroy fiber context pool
	m_FiberContextPool.Destroy();

	// destroy worker thread pool
	m_WorkerThreadPool.Destroy();

	// destroy wait-fiber-context queue
	m_WaitFiberContextQueue.Destroy();

	// deallocate task queue
	for (uint32_t i = 0; i < ETaskQueuePriority::ETQP_Max; ++i)
	{
		delete m_TaskQueues[i];
		m_TaskQueues[i] = nullptr;
	}		
}

H1WorkerThread* H1TaskScheduler::GetCurrentThread()
{	
	ThreadId currThreadId = appGetCurrentThreadId();
	return m_WorkerThreadPool.GetWorkerThreadById(currThreadId);
}

H1TaskScheduler* H1TaskSchedulerLayer::GetTaskScheduler()
{
	return gTaskScheduler;
}

bool H1TaskSchedulerLayer::InitializeTaskScheduler()
{
	gTaskScheduler = new SGD::H1TaskScheduler();
	return gTaskScheduler->Initialize();
}

void H1TaskSchedulerLayer::DestroyTaskScheduler()
{
	gTaskScheduler->Destroy();
	delete gTaskScheduler;
	gTaskScheduler = nullptr;
}

bool H1TaskSchedulerLayer::StartTaskScheduler()
{
	return GetTaskScheduler()->GetWorkerThreadPool().StartAll();
}

void H1TaskSchedulerLayer::WaitTaskScheduler()
{
	GetTaskScheduler()->GetWorkerThreadPool().WaitAll();
}

bool H1TaskSchedulerLayer::RunTasks(H1TaskDeclarationInterface** tasks, int32_t taskCounts, H1TaskCounter** ppTaskCounter)
{
	H1TaskScheduler* pTaskScheduler = GetTaskScheduler();
	if (pTaskScheduler == nullptr)
		return false; // error for creating task scheduler

	// check the calling object is main-thread or not (if it is main-thread, call first thread in worker thread pool)
	bool bIsMainThread = (appGetCurrentThreadId() == pTaskScheduler->GetMainThreadId());	

	// if this method currently executes in main thread
	if (bIsMainThread) 
	{
		// for readable code, I put similar code below in here (for the detail of code, refer to the below codes)
		*ppTaskCounter = pTaskScheduler->GetMainThreadTaskCounter();
		(*ppTaskCounter)->FetchAndAdd(taskCounts);

		for (int32_t i = 0; i < taskCounts; ++i)
		{
			(*tasks)[i].SetTaskCounter(*ppTaskCounter);
			(*tasks)[i].SetParent(nullptr);
		}

		pTaskScheduler->GetTaskQueue(ETQP_High)->EnqueueTaskRange(tasks, taskCounts);

		return true;
	}

	// get current running fiber and parent's task
	H1WorkerThread* currWorkerThread = pTaskScheduler->GetCurrentThread();

	bool bIsThreadFiberContext = false;
	H1FiberContext* currFiberContext = currWorkerThread->GetCurrentBindedFiberContext();
	if (currFiberContext == nullptr) // if no fiber context currently binded, return false
	{
		return false;
	}

	// set the task counter
	*ppTaskCounter = currFiberContext->GetTaskCounter();
	// add task counter with taskCounts
	(*ppTaskCounter)->FetchAndAdd(taskCounts);

	// set parent of triggered child tasks and task counter of fiber context attached parent's task
	for (int32_t i = 0; i < taskCounts; ++i)
	{
		(*tasks)[i].SetTaskCounter(*ppTaskCounter);
		(*tasks)[i].SetParent(currFiberContext->GetTaskSlot());
	}

	// add tasks to task queue
	// @TODO - temporary enqueue into high-priority queue
	pTaskScheduler->GetTaskQueue(ETQP_High)->EnqueueTaskRange(tasks, taskCounts);

	return true;
}

bool H1TaskSchedulerLayer::WaitForCounter(H1TaskCounter* pTaskCounter, H1TaskCounter::TaskCounterType value)
{
	// @TODO - currently, there is no functionality for matching certain value option (just handling only value == zero)
	H1TaskScheduler* pTaskScheduler = GetTaskScheduler();
	if (pTaskScheduler == nullptr)
		return false; // error for creating task scheduler
	
	// special handling running in the main thread
	bool bIsMainThread = (appGetCurrentThreadId() == pTaskScheduler->GetMainThreadId());
	if (bIsMainThread)
	{
		while (pTaskCounter->Get() != value) {}
		return true;
	}

	if (pTaskCounter->Get() == value)
		return false; // invalid counter is inserted
		
	// move current fiber context to wait queue
	H1WorkerThread* currWorkerThread = pTaskScheduler->GetCurrentThread();
	H1FiberContext* bindedFiberContext = currWorkerThread->GetCurrentBindedFiberContext();
	if (bindedFiberContext == nullptr)
		return false; // no binded fiber context exists!

	if (!pTaskScheduler->GetWaitFiberContextQueue().Enqueue(bindedFiberContext->GetFiberId(), bindedFiberContext->GetFiberType()))
		return false;

	// switch to thread-fiber
	currWorkerThread->SwitchThreadFiberContext();

	// wait until task counter reach to the value
	while (pTaskCounter->Get() != value) {}	

	return true;
}