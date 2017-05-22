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
#include "SGDTask.h"
#include "SGDFiberContext.h"
#include "SGDTaskScheduler.h"
using namespace SGD;

H1TaskDeclarationInterface::H1TaskDeclarationInterface()
	: m_TaskCounter(nullptr)
	, m_Parent(nullptr)
	, m_Owner(nullptr)
{

}

void H1TaskDeclarationInterface::SetTaskCounter(H1TaskCounter* counter)
{
	m_TaskCounter = counter;
}

void H1TaskDeclarationInterface::SetParent(H1TaskDeclarationInterface* parent)
{
	m_Parent = parent;
}

H1TaskDeclaration::H1TaskDeclaration(TaskEntryPoint taskBody, void* taskData)
	: m_TaskBody(taskBody)
	, m_TaskData(taskData)
	, H1TaskDeclarationInterface()
{

}

void H1TaskDeclaration::SetTaskData(void* data)
{
	m_TaskData = data;
}

void H1TaskDeclaration::SetTaskEntryPoint(TaskEntryPoint taskBody)
{
	m_TaskBody = taskBody;
}

void H1TaskDeclaration::RunTask()
{
	m_TaskBody(m_TaskData);

	// @TODO - need to think about this portion of codes is appropriate to place here
	// when it finishes task, decrement assigned task counter (parent's fiber context counter)
	H1TaskCounter::TaskCounterType remainCounter = m_TaskCounter->Decrement();
	// @TODO - only handling when remain counter is zero (no destined value right now)
	if (remainCounter == 0)
	{
		//@TODO - need to find more elegant way to process this
		if (m_Parent == nullptr) // this task executes in the main thread
		{
			return;
		}

		FiberId fiberId = m_Parent->GetFiberContext()->GetFiberId();
		EFiberType fiberType = m_Parent->GetFiberContext()->GetFiberType();
		// @TODO - need to think about this... how to make it more elegant
		H1TaskSchedulerLayer::GetTaskScheduler()->GetWaitFiberContextQueue().MoveToReadyToResumeQueue(fiberId, fiberType);
	}
}

H1TaskCounter::H1TaskCounter()
	: m_RemainCounter(0)
{

}

H1TaskCounter::TaskCounterType H1TaskCounter::FetchAndAdd(TaskCounterType value)
{
	return m_RemainCounter.fetch_add(value);
}

H1TaskCounter::TaskCounterType H1TaskCounter::Reset(TaskCounterType value)
{
	m_RemainCounter.store(value);
	return m_RemainCounter.load();
}

H1TaskCounter::TaskCounterType H1TaskCounter::Decrement()
{
	--m_RemainCounter;
	return m_RemainCounter.load();
}

H1TaskCounter::TaskCounterType H1TaskCounter::Get()
{
	return m_RemainCounter.load();
}