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

namespace SGD
{
	typedef void (*TaskEntryPoint)(void* pTaskData);
	
	// forward declaration
	class H1FiberContext;

	class H1TaskCounter
	{
	public:
		typedef int32_t TaskCounterType;

		H1TaskCounter();

		//@TODO - further optimization with memory access flags
		TaskCounterType FetchAndAdd(TaskCounterType value);
		TaskCounterType Reset(TaskCounterType value);
		TaskCounterType Decrement();
		TaskCounterType Get();

	private:
		std::atomic<TaskCounterType> m_RemainCounter;
	};

	class H1TaskDeclarationInterface
	{
	public:
		H1TaskDeclarationInterface();

		void SetParent(H1TaskDeclarationInterface* parent);
		void SetTaskCounter(H1TaskCounter* counter);

		// virtual methods
		virtual void RunTask() = 0;

		// inline functionalities
		inline void SetFiberContext(H1FiberContext* pFiberContext) { m_Owner = pFiberContext; }
		inline H1FiberContext* GetFiberContext() { return m_Owner; }

	protected:
		// fiber context has task slot for this instance
		H1FiberContext* m_Owner;
		// the task triggered by 'parent' task
		// when m_Parent is null, it triggered by the main thread
		H1TaskDeclarationInterface* m_Parent;
		// reference for task-counter in H1FiberContext which spawned this task
		H1TaskCounter* m_TaskCounter;
	};

	class H1TaskDeclaration : public H1TaskDeclarationInterface
	{
	public:
		H1TaskDeclaration(TaskEntryPoint taskBody = nullptr, void* taskData = nullptr);
					
		void SetTaskData(void* data);
		void SetTaskEntryPoint(TaskEntryPoint taskBody);
		virtual void RunTask();

	private:		
		// task-body and data
		TaskEntryPoint m_TaskBody;
		void* m_TaskData;
	};
}

#define START_TASK_ENTRY_POINT(TaskName)							\
void TaskEntryPoint_##TaskName(void* pTaskData_##TaskName)			
