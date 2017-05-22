#pragma once

using namespace System;
using namespace System::Collections::Generic;

#include "SGDThreadNativeWrapper.h"

namespace SGDManagedEngineWrapper
{
	// forward declaration
	class H1TaskDeclarationWrapper;

	public ref class H1ManagedTaskCounterWrapper
	{
	public:
		H1ManagedTaskCounterWrapper();
		~H1ManagedTaskCounterWrapper();
		!H1ManagedTaskCounterWrapper();

		SGDNativeCPP::H1TaskCounterNativeWrapper* GetTaskCounter() { return m_pTaskCounter; }

	private:
		SGDNativeCPP::H1TaskCounterNativeWrapper* m_pTaskCounter;
	};

	public ref class H1ManagedTaskData
	{
	public:

	private:		
	};

	public delegate void ManagedTaskEntryPoint(H1ManagedTaskData^ data);

	// manage life time for H1TaskDeclarationWrapper
	public ref class H1ManagedTaskDeclaration
	{
	public:
		H1ManagedTaskDeclaration();
		!H1ManagedTaskDeclaration();
		~H1ManagedTaskDeclaration();

		// properties
		property H1ManagedTaskData^ TaskData
		{
			void set(H1ManagedTaskData^ value) { m_TaskData = value; }
			H1ManagedTaskData^ get() { return m_TaskData; }
		}

		property ManagedTaskEntryPoint^ TaskEntryPoint
		{
			void set(ManagedTaskEntryPoint^ value) { m_TaskEntryPoint = value; }
			ManagedTaskEntryPoint^ get() { return m_TaskEntryPoint; }
		}

		void RunTask();
		H1TaskDeclarationWrapper* GetNativeTaskDeclaration() { return m_TaskDeclarationWrapper; }

	private:
		H1ManagedTaskData^ m_TaskData;
		ManagedTaskEntryPoint^ m_TaskEntryPoint;
		H1TaskDeclarationWrapper* m_TaskDeclarationWrapper;
	};

	// subsided to H1ManagedTaskDeclaration
	class H1TaskDeclarationWrapper : public SGDNativeCPP::H1TaskDeclarationNativeWrapper
	{
	public:
		H1TaskDeclarationWrapper(H1ManagedTaskDeclaration^ managedTaskDeclaration);

		// virtual methods
		virtual void RunTask();
	private:
		gcroot<H1ManagedTaskDeclaration^> m_ManagedTaskDeclarationRef;
	};

	public ref class H1ManagedTaskSchedulerLayerWrapper
	{
	public:
		static bool InitializeTaskScheduler();
		static void DestroyTaskScheduler();

		static bool StartTaskScheduler();
		static void WaitTaskScheduler();

		static bool RunTask(H1ManagedTaskDeclaration^ task, H1ManagedTaskCounterWrapper^ taskCounter);
		static bool RunTasks(List<H1ManagedTaskDeclaration^>^ tasks, H1ManagedTaskCounterWrapper^ taskCounter);
		static bool WaitForCounter(H1ManagedTaskCounterWrapper^ taskCounter, Int32 value);

		static void SignalQuitAll();
	};
}
