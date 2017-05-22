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

	class H1WaitFiberContextQueue
	{
	public:
		struct H1FiberContextWaitState
		{
			H1FiberContextWaitState(H1FiberContext* fiberContext)
				: bIsInWaitQueue(false)
				, FiberContextReference(fiberContext)
			{}

			std::atomic<int64_t> bIsInWaitQueue;
			H1FiberContext* FiberContextReference;
			// @TODO - for optimization, we could add padding with size of cache line
		};

		H1WaitFiberContextQueue(H1TaskScheduler* owner);
		~H1WaitFiberContextQueue();

		bool Initialize();
		void Destroy();

		bool Enqueue(FiberId fiberContextId, EFiberType fiberType);
		FiberId Dequeue(EFiberType fiberType);

		// this method is only used by H1TaskDeclaration (at the end of RunTask())
		void MoveToReadyToResumeQueue(FiberId fiberContextId, EFiberType fiberType);

	private:
		// owner
		H1TaskScheduler* m_Owner;
		// concurrent wait queue which contains fiber contexts ready-to-start
		//	- this concurrent queue is changed by child task who reach to parent's reference count to 0, put FiberId to m_ReadyToResumeQueue
		//	- other than the child task, don't have right to modify this m_ReadyToResumeQueue!
		// @TODO - need to design to assign priority to modify this queue to arbitrary class instance!
#if USE_MS_CONCURRENT_QUEUE
		concurrency::concurrent_queue<FiberId> m_ReadyToResumeQueue[EFiberType::EFT_Max];
#else
		moodycamel::ConcurrentQueue<FiberId> m_ReadyToResumeQueue[EFiberType::EFT_Max];
#endif
		// fiber context wait states
		std::vector<H1FiberContextWaitState*> m_FiberContextWaitStates[EFiberType::EFT_Max];
	};
}