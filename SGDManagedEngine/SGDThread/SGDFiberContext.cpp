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
#include "SGDFiberContext.h"
#include "SGDWorkerThread.h"
using namespace SGD;

H1FiberContext::H1FiberContext()
	: m_TaskSlot(nullptr)
	, m_FiberInstance(nullptr)
	, m_Index(-1)
	, m_Owner(nullptr)
{

}

H1FiberContext::~H1FiberContext()
{

}

bool H1FiberContext::Initialize(int32_t fiberIndex, EFiberType fiberType, int32_t stackSize)
{
	// set fiber index (unique id)
	m_Index = fiberIndex;
	// set fiber type
	m_Type = fiberType;
	// create fiber task counter
	m_TaskCounter = new H1TaskCounter();
	// create fiber instance
	return CreateFiberContext(stackSize);
}

void H1FiberContext::Destroy()
{
	// deallocate fiber task counter
	delete m_TaskCounter;
	m_TaskCounter = nullptr;

	// destroy fiber instance
	DestroyFiberContext();
}

void H1FiberContext::SwitchSlot(H1TaskDeclarationInterface* newSlot)
{
	// set task slot
	m_TaskSlot = newSlot;
	// set fiber context (currently binded with H1TaskDeclaration)
	m_TaskSlot->SetFiberContext(this);
}

void H1FiberContext::RunSlot()
{
	// execute task slot of H1TaskDeclaration
	m_TaskSlot->RunTask();
}

H1FiberContextWindow::H1FiberContextWindow()
	: H1FiberContext()
{

}

H1FiberContextWindow::~H1FiberContextWindow()
{

}

#if WIN32
// function body - entry point for fiber context
void __stdcall H1FiberContextEntryPoint(void* Data)
{
	// run the task slot
	H1FiberContext* fiberContext = reinterpret_cast<H1FiberContext*>(Data);
	fiberContext->RunSlot();

	// return to main thread, nullifying owner thread for later usage
	H1WorkerThread* owner = fiberContext->GetOwner();
	fiberContext->SetOwner(nullptr);

	owner->SwitchThreadFiberContext();
}
#endif

bool H1FiberContextWindow::CreateFiberContext(int32_t stackSize)
{
	m_FiberInstance = CreateFiber(stackSize, H1FiberContextEntryPoint, this);
	if (m_FiberInstance == nullptr)
		return false;
	return true;
}

void H1FiberContextWindow::DestroyFiberContext()
{
	DeleteFiber(m_FiberInstance);
	m_FiberInstance = nullptr;
}

void H1FiberContextWindow::SwitchFiberContext()
{
	SwitchToFiber(m_FiberInstance);
}

void H1FiberContextWindow::ConvertThreadToFiber()
{
	// thread fiber only have type as 'EFT_Thread' and rest of properties like m_Slot and m_Index is null (or -1)
	m_Type = EFiberType::EFT_Thread; // set thread fiber type
	m_FiberInstance = ConvertThreadToFiberEx(nullptr, FIBER_FLAG_FLOAT_SWITCH);
}

H1FiberContextPool::H1FiberContextPool()
{

}

H1FiberContextPool::~H1FiberContextPool()
{

}

bool H1FiberContextPool::Initialize()
{
	// create small fiber contexts
	const int32_t smallFiberContextCount = 128;
	const int32_t smallFiberContextStackSize = 64 * 1024;
	for (int32_t i = 0; i < smallFiberContextCount; ++i)
	{
		H1FiberContextWindow* newFiberContext = new H1FiberContextWindow;
		if (!newFiberContext->Initialize(i, EFiberType::EFT_Small, smallFiberContextStackSize))
			return false;
		m_SmallFiberContexts.push_back(newFiberContext);
	}

	// create big fiber contexts
	const int32_t bigFiberContextCount = 32;
	const int32_t bigFiberContextStackSize = 512 * 1024;
	for (int32_t i = 0; i < bigFiberContextCount; ++i)
	{
		H1FiberContextWindow* newFiberContext = new H1FiberContextWindow;
		if (!newFiberContext->Initialize(i, EFiberType::EFT_Big, bigFiberContextStackSize))
			return false;
		m_BigFiberContexts.push_back(newFiberContext);
	}

	// make free lists
	for (int32_t i = 0; i < smallFiberContextCount; ++i)
	{
#if USE_MS_CONCURRENT_QUEUE
		m_FreeFiberContexts[EFiberType::EFT_Small].push(i);
#else
		m_FreeFiberContexts[EFiberType::EFT_Small].enqueue(i);
#endif
	}			

	for (int32_t i = 0; i < bigFiberContextCount; ++i)
	{
#if USE_MS_CONCURRENT_QUEUE
		m_FreeFiberContexts[EFiberType::EFT_Big].push(i);
#else
		m_FreeFiberContexts[EFiberType::EFT_Big].enqueue(i);
#endif
	}		

	return true;
}

void H1FiberContextPool::Destroy()
{
	// destroy all small/big fiber contexts
	for (uint32_t i = 0; i < m_SmallFiberContexts.size(); ++i)
	{
		m_SmallFiberContexts[i]->Destroy();
	}

	for (uint32_t i = 0; i < m_BigFiberContexts.size(); ++i)
	{
		m_BigFiberContexts[i]->Destroy();
	}

	// clear fiber context containers
	m_SmallFiberContexts.clear();
	m_BigFiberContexts.clear();
}

bool H1FiberContextPool::EnqueueFreeFiberContext(FiberId fiberId, EFiberType fiberType)
{
#if USE_MS_CONCURRENT_QUEUE
	m_FreeFiberContexts[fiberType].push(fiberId);
#else
	m_FreeFiberContexts[fiberType].enqueue(fiberId);
#endif
	return true;
}

FiberId H1FiberContextPool::DequeueFreeFiberContext(EFiberType fiberType)
{
	FiberId result = -1;
#if USE_MS_CONCURRENT_QUEUE
	if (!m_FreeFiberContexts[fiberType].try_pop(result))
#else
	if (!m_FreeFiberContexts[fiberType].try_dequeue(result))
#endif
		return result;
	return result;
}

bool H1FiberContextPool::ConstructFiberContext(FiberId newFiberId, EFiberType fiberType, H1TaskDeclarationInterface* newTask)
{
	H1FiberContext* pFiberContext = nullptr;
	if (fiberType == EFiberType::EFT_Small)
		pFiberContext = m_SmallFiberContexts[newFiberId];
	else
		pFiberContext = m_BigFiberContexts[newFiberId];

	pFiberContext->SwitchSlot(newTask);

	return true;
}