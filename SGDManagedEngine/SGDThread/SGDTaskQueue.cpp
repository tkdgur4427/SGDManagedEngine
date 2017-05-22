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
#include "SGDTaskQueue.h"
using namespace SGD;

H1TaskQueue::H1TaskQueue(ETaskQueuePriority priority)
	: m_Priority(priority)
{
	
}

H1TaskQueue::~H1TaskQueue()
{

}

bool H1TaskQueue::EnqueueTask(H1TaskDeclarationInterface* pTask)
{
#if USE_MS_CONCURRENT_QUEUE
	m_QueuedTasks.push(pTask);
#else
	m_QueuedTasks.enqueue(pTask);
#endif
	// successfully enqueue the task
	return true;
}

bool H1TaskQueue::EnqueueTaskRange(H1TaskDeclarationInterface** tasks, int32_t taskCounts)
{
	// @TODO - why try_enqueue_bulk is not working?
	//if (!m_QueuedTasks.try_enqueue_bulk(&tasks, taskCounts))
	//	return false; // additional memory allocation failed (not enough memory)
	for (int32_t taskIdx = 0; taskIdx < taskCounts; ++taskIdx)
	{
		H1TaskDeclarationInterface* task = tasks[taskIdx];
#if USE_MS_CONCURRENT_QUEUE
		m_QueuedTasks.push(task);
#else
		m_QueuedTasks.enqueue(task);
#endif
	}

	// successfully enqueue the task
	return true;
}

H1TaskDeclarationInterface* H1TaskQueue::DequeueTask()
{
	H1TaskDeclarationInterface* pDequeuedTask = nullptr;
#if USE_MS_CONCURRENT_QUEUE
	if (!m_QueuedTasks.try_pop(pDequeuedTask))
#else
	if (!m_QueuedTasks.try_dequeue(pDequeuedTask))
#endif
		return nullptr; // if the queue is empty, return nullptr
	return pDequeuedTask;
}
