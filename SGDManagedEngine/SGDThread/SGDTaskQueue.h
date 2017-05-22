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

#include "SGDTask.h"

namespace SGD
{
	enum ETaskQueuePriority
	{
		ETQP_High,
		ETQP_Mid,
		ETQP_Low,
		ETQP_Max,
	};

	// the wrapper for concurrent task queue
	class H1TaskQueue
	{
	public:
		H1TaskQueue(ETaskQueuePriority priority);
		~H1TaskQueue();

		bool EnqueueTask(H1TaskDeclarationInterface* pTask);
		H1TaskDeclarationInterface* DequeueTask();

		bool EnqueueTaskRange(H1TaskDeclarationInterface** tasks, int32_t taskCounts);

	private:
		// task queue priority
		ETaskQueuePriority m_Priority;
		// task concurrent queue consumed by H1FiberContext
#if USE_MS_CONCURRENT_QUEUE
		concurrency::concurrent_queue<H1TaskDeclarationInterface*> m_QueuedTasks;
#else
		moodycamel::ConcurrentQueue<H1TaskDeclarationInterface*> m_QueuedTasks;
#endif
	};
}