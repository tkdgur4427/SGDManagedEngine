#pragma once

// we put SGDTask.h explicitly in here, to override H1TaskDeclarationInterface
#include <atomic> // this headers for 'SGDTask.h'
#include "SGDTask.h"

namespace SGDNativeCPP
{	
	class H1TaskCounterNativeWrapper
	{
	public:
		H1TaskCounterNativeWrapper();
		~H1TaskCounterNativeWrapper();

		SGD::H1TaskCounter** GetTaskCounterRef() { return &m_pTaskCounter; }
		SGD::H1TaskCounter* GetTaskCounter() { return m_pTaskCounter; }

	private:
		SGD::H1TaskCounter* m_pTaskCounter;
	};

	// wrapper to define managed TaskDeclaration derived class
	class H1TaskDeclarationNativeWrapper : public SGD::H1TaskDeclarationInterface
	{
		virtual void RunTask() {}
	};

	class H1TaskSchedulerLayerNativeWrapper
	{
	public:
		static bool InitializeTaskScheduler();
		static void DestroyTaskScheduler();

		static bool StartTaskScheduler();
		static void WaitTaskScheduler();

		static bool RunTasks(H1TaskDeclarationNativeWrapper** tasks, int taskCounts, H1TaskCounterNativeWrapper& taskCounter);
		static bool WaitForCounter(H1TaskCounterNativeWrapper& taskCounter, int value = 0);

		static void QuitAllThreads();
	};
}