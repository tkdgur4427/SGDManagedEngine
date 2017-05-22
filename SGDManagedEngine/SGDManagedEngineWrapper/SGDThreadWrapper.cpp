#include "stdafx.h"
#include "SGDThreadWrapper.h"
using namespace SGDManagedEngineWrapper;

H1ManagedTaskCounterWrapper::H1ManagedTaskCounterWrapper()
{
	m_pTaskCounter = new SGDNativeCPP::H1TaskCounterNativeWrapper();
}

H1ManagedTaskCounterWrapper::~H1ManagedTaskCounterWrapper()
{

}

H1ManagedTaskCounterWrapper::!H1ManagedTaskCounterWrapper()
{
	delete m_pTaskCounter;
	m_pTaskCounter = nullptr;
}

H1ManagedTaskDeclaration::H1ManagedTaskDeclaration()
{
	m_TaskDeclarationWrapper = new H1TaskDeclarationWrapper(this);
}

H1ManagedTaskDeclaration::!H1ManagedTaskDeclaration()
{

}

H1ManagedTaskDeclaration::~H1ManagedTaskDeclaration()
{
	delete m_TaskDeclarationWrapper;
	m_TaskDeclarationWrapper = nullptr;
}

void H1ManagedTaskDeclaration::RunTask()
{
	m_TaskEntryPoint(m_TaskData);
}

H1TaskDeclarationWrapper::H1TaskDeclarationWrapper(H1ManagedTaskDeclaration^ managedTaskDeclaration)
{
	m_ManagedTaskDeclarationRef = managedTaskDeclaration;
}

void H1TaskDeclarationWrapper::RunTask()
{
	m_ManagedTaskDeclarationRef->RunTask();
}

bool H1ManagedTaskSchedulerLayerWrapper::InitializeTaskScheduler()
{
	return SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::InitializeTaskScheduler();
}

void H1ManagedTaskSchedulerLayerWrapper::DestroyTaskScheduler()
{
	SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::DestroyTaskScheduler();
}

bool H1ManagedTaskSchedulerLayerWrapper::StartTaskScheduler()
{
	return SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::StartTaskScheduler();
}

void H1ManagedTaskSchedulerLayerWrapper::WaitTaskScheduler()
{
	SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::WaitTaskScheduler();
}

bool H1ManagedTaskSchedulerLayerWrapper::RunTask(H1ManagedTaskDeclaration^ task, H1ManagedTaskCounterWrapper^ taskCounter)
{
	std::vector<SGDNativeCPP::H1TaskDeclarationNativeWrapper*> m_nativeTasks;
	m_nativeTasks.push_back(task->GetNativeTaskDeclaration());

	return SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::RunTasks(m_nativeTasks.data(), m_nativeTasks.size(), *taskCounter->GetTaskCounter());
}

bool H1ManagedTaskSchedulerLayerWrapper::RunTasks(List<H1ManagedTaskDeclaration^>^ tasks, H1ManagedTaskCounterWrapper^ taskCounter)
{
	std::vector<SGDNativeCPP::H1TaskDeclarationNativeWrapper*> m_nativeTasks;
	for each(H1ManagedTaskDeclaration^ task in tasks)
	{
		m_nativeTasks.push_back(task->GetNativeTaskDeclaration());
	}

	return SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::RunTasks(m_nativeTasks.data(), m_nativeTasks.size(), *taskCounter->GetTaskCounter());
}

bool H1ManagedTaskSchedulerLayerWrapper::WaitForCounter(H1ManagedTaskCounterWrapper^ taskCounter, Int32 value)
{
	return SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::WaitForCounter(*taskCounter->GetTaskCounter(), value);
}

void H1ManagedTaskSchedulerLayerWrapper::SignalQuitAll()
{
	SGDNativeCPP::H1TaskSchedulerLayerNativeWrapper::QuitAllThreads();
}