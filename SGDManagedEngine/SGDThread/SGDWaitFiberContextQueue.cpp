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
#include "SGDWaitFiberContextQueue.h"
#include "SGDTaskScheduler.h"
using namespace SGD;

H1WaitFiberContextQueue::H1WaitFiberContextQueue(H1TaskScheduler* owner)
	: m_Owner(owner)
{

}

H1WaitFiberContextQueue::~H1WaitFiberContextQueue()
{

}

bool H1WaitFiberContextQueue::Initialize()
{
	// initialize m_FiberContextWaitStates
	// 1. small fiber context
	for (uint32_t i = 0; i < m_Owner->GetFiberContextPool().GetFiberContextSmallCounts(); ++i)
	{
		H1FiberContext* currFiberContext = m_Owner->GetFiberContextPool().GetFiberContextSmall(i);
		m_FiberContextWaitStates[EFiberType::EFT_Small].push_back(new H1FiberContextWaitState(currFiberContext));
	}		

	// 2. big fiber context
	for (uint32_t i = 0; i < m_Owner->GetFiberContextPool().GetFiberContextBigCounts(); ++i)
	{
		H1FiberContext* currFiberContext = m_Owner->GetFiberContextPool().GetFiberContextBig(i);
		m_FiberContextWaitStates[EFiberType::EFT_Big].push_back(new H1FiberContextWaitState(currFiberContext));
	}		

	return true;
}

void H1WaitFiberContextQueue::Destroy()
{

}

bool H1WaitFiberContextQueue::Enqueue(FiberId fiberContextId, EFiberType fiberType)
{
#if _DEBUG
	if (m_FiberContextWaitStates[fiberType][fiberContextId]->bIsInWaitQueue.load())
	{
		assert("[invalid] it is already in wait queue, please check again");
		return false;
	}
#endif

	// change atomic boolean flags
	m_FiberContextWaitStates[fiberType][fiberContextId]->bIsInWaitQueue.store(true);

	return true;
}

FiberId H1WaitFiberContextQueue::Dequeue(EFiberType fiberType)
{
	FiberId result = -1;
#if USE_MS_CONCURRENT_QUEUE
	if (!m_ReadyToResumeQueue[fiberType].try_pop(result))
#else
	if (!m_ReadyToResumeQueue[fiberType].try_dequeue(result))
#endif
		return -1; // when m_ReadyToResumeQueue is empty, return false
	return result;
}

void H1WaitFiberContextQueue::MoveToReadyToResumeQueue(FiberId fiberContextId, EFiberType fiberType)
{
	m_FiberContextWaitStates[fiberType][fiberContextId]->bIsInWaitQueue = false;

	// put the corresponding fiber context to ReadyToResumeQueue
#if USE_MS_CONCURRENT_QUEUE
	m_ReadyToResumeQueue[fiberType].push(fiberContextId);
#else
	m_ReadyToResumeQueue[fiberType].enqueue(fiberContextId);
#endif
}