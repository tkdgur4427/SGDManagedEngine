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
#include "SGDTask.h"

namespace SGD
{
	enum EFiberType
	{
		EFT_Big,	// fiber stack size is big (512KB)
		EFT_Small,	// fiber stack size is small (64KB)
		EFT_Thread,	// thread fiber type
		EFT_Max,
	};

	typedef uint32_t FiberId;

	// forward declaration
	class H1WorkerThread;

	class H1FiberContext
	{
	public:
		H1FiberContext();
		virtual ~H1FiberContext();

		bool Initialize(int32_t fiberIndex, EFiberType fiberType, int32_t stackSize = 0);
		void Destroy();

		void SwitchSlot(H1TaskDeclarationInterface* newSlot);
		void RunSlot();

		// pure virtual functions to override in derived classes
		virtual bool CreateFiberContext(int32_t stackSize) = 0;
		virtual void DestroyFiberContext() = 0;
		virtual void SwitchFiberContext() = 0;
		// this method creates currently binded thread's fiber context
		virtual void ConvertThreadToFiber() = 0;

		// inline methods
		inline FiberId GetFiberId() const { return m_Index; }
		inline EFiberType GetFiberType() const { return m_Type; }
		inline void* GetFiberInstance() const { return m_FiberInstance; }
		inline H1TaskCounter* GetTaskCounter() { return m_TaskCounter; }
		inline H1TaskDeclarationInterface* GetTaskSlot() { return m_TaskSlot; }
		
		inline H1WorkerThread* GetOwner() { return m_Owner; }
		inline void SetOwner(H1WorkerThread* owner) { m_Owner = owner; }

	protected:
		// the index of fiber context pool
		FiberId m_Index;
		// the fiber context type
		EFiberType m_Type;		
		// task slot to execute in this fiber context
		H1TaskDeclarationInterface* m_TaskSlot;
		// fiber context task counter
		H1TaskCounter* m_TaskCounter;
		// fiber instance
		void* m_FiberInstance;
		// worker thread holding this fiber-context
		H1WorkerThread* m_Owner;
	};

	class H1FiberContextWindow : public H1FiberContext
	{
	public:
		H1FiberContextWindow();
		virtual ~H1FiberContextWindow();

		virtual bool CreateFiberContext(int32_t stackSize);
		virtual void DestroyFiberContext();
		virtual void SwitchFiberContext();
		virtual void ConvertThreadToFiber();

	private:
	};

	class H1FiberContextPool
	{
	public:
		H1FiberContextPool();
		~H1FiberContextPool();

		bool Initialize();
		void Destroy();

		bool EnqueueFreeFiberContext(FiberId fiberId, EFiberType fiberType);
		FiberId DequeueFreeFiberContext(EFiberType fiberType);

		bool ConstructFiberContext(FiberId newFiberId, EFiberType fiberType, H1TaskDeclarationInterface* newTask);

		inline H1FiberContext* GetFiberContextSmall(FiberId fiberId) { return m_SmallFiberContexts[fiberId]; }
		inline H1FiberContext* GetFiberContextBig(FiberId fiberId) { return m_BigFiberContexts[fiberId]; }
		inline size_t GetFiberContextSmallCounts() { return m_SmallFiberContexts.size(); }
		inline size_t GetFiberContextBigCounts() { return m_BigFiberContexts.size(); }

	private:
		// total 160 fiber contexts exist
		// 128 - small fiber contexts(64KB) & 32 big fiber contexts(512KB)
		std::vector<H1FiberContext*> m_SmallFiberContexts;
		std::vector<H1FiberContext*> m_BigFiberContexts;

		// this shared concurrent queue is only managed by through this class public methods!
		//	- please whenever modify this queues, please leave the place in comment
#if USE_MS_CONCURRENT_QUEUE
		concurrency::concurrent_queue<FiberId> m_FreeFiberContexts[EFiberType::EFT_Max];
#else
		moodycamel::ConcurrentQueue<FiberId> m_FreeFiberContexts[EFiberType::EFT_Max];
#endif
	};
}