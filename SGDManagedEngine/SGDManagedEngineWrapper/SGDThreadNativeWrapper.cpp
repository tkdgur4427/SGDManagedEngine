// this file is disabled using precompiled header & clr feature
#include "SGDThreadPCH.h"
#include "SGDThreadNativeWrapper.h"

// SGDThread headers
#include "SGDTaskScheduler.h"

using namespace SGDNativeCPP;

H1TaskCounterNativeWrapper::H1TaskCounterNativeWrapper()
	: m_pTaskCounter(nullptr)
{
	m_pTaskCounter = new SGD::H1TaskCounter();
}

H1TaskCounterNativeWrapper::~H1TaskCounterNativeWrapper()
{
	delete m_pTaskCounter;
}

bool H1TaskSchedulerLayerNativeWrapper::InitializeTaskScheduler()
{
	return SGD::H1TaskSchedulerLayer::InitializeTaskScheduler();
}

void H1TaskSchedulerLayerNativeWrapper::DestroyTaskScheduler()
{
	SGD::H1TaskSchedulerLayer::InitializeTaskScheduler();
}

bool H1TaskSchedulerLayerNativeWrapper::StartTaskScheduler()
{
	return SGD::H1TaskSchedulerLayer::StartTaskScheduler();
}

void H1TaskSchedulerLayerNativeWrapper::WaitTaskScheduler()
{
	SGD::H1TaskSchedulerLayer::WaitTaskScheduler();
}

bool H1TaskSchedulerLayerNativeWrapper::RunTasks(H1TaskDeclarationNativeWrapper** tasks, int taskCounts, H1TaskCounterNativeWrapper& taskCounter)
{
	std::vector<SGD::H1TaskDeclarationInterface*> taskInterfaces;
	for (int i = 0; i < taskCounts; ++i)
		taskInterfaces.push_back(static_cast<SGD::H1TaskDeclarationInterface*>(tasks[i]));

	return SGD::H1TaskSchedulerLayer::RunTasks(taskInterfaces.data(), taskCounts, taskCounter.GetTaskCounterRef());
}

bool H1TaskSchedulerLayerNativeWrapper::WaitForCounter(H1TaskCounterNativeWrapper& taskCounter, int value)
{
	return SGD::H1TaskSchedulerLayer::WaitForCounter(taskCounter.GetTaskCounter(), value);
}

void H1TaskSchedulerLayerNativeWrapper::QuitAllThreads()
{
	SGD::H1TaskSchedulerLayer::GetTaskScheduler()->GetWorkerThreadPool().SignalQuitAll();
}