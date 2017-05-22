#include "SGDThreadUnitTestsPCH.h"
#include "SGDTaskScheduler.h"
#include "SGDWorkerThread.h"

// the fixture for testing class
class TaskSchedulerTest : public ::testing::Test 
{
protected:
	// you can remove any or all of the following functions if its body is empty
	TaskSchedulerTest()
	{
		// you can do set-up work for each test here
	}

	virtual ~TaskSchedulerTest()
	{
		// you can do clean-up work that doesn't throw exceptions here
	}

	// if the constructor and destructor are not enough for setting up and cleaning up each test, you can define the following methods:
	virtual void SetUp()
	{
		// code here will be called immediately after the constructor (right before each test)
	}

	virtual void TearDown()
	{
		// code here will be called immediately after each test (right before the destructor)
		
	}

	// objects declared here can be used all tests in the test case
	
};

// tests that the above class method does Abc
TEST_F(TaskSchedulerTest, ThreadPoolSuccessfullyQuit)
{
	SGD::H1WorkerThreadPoolUnitTestKnot threadPoolKnot;
	threadPoolKnot.Initialize(nullptr);
	threadPoolKnot.StartAll();
	threadPoolKnot.SignalQuitAll();
	threadPoolKnot.WaitAll();

	EXPECT_EQ(true, threadPoolKnot.AllWorkerThreadTerminated);

	threadPoolKnot.Destroy();
}

TEST_F(TaskSchedulerTest, TaskSchedulerInitialization)
{
	SGD::H1TaskScheduler taskScheduler;
	bool bSuccessInitialization = taskScheduler.Initialize();
	EXPECT_EQ(true, bSuccessInitialization);

	taskScheduler.Destroy();
}

TEST_F(TaskSchedulerTest, TaskSchedulerLayerInitialization)
{
	SGD::H1TaskSchedulerLayer::InitializeTaskScheduler();
	EXPECT_EQ(true, SGD::H1TaskSchedulerLayer::GetTaskScheduler() != nullptr);

	SGD::H1TaskSchedulerLayer::DestroyTaskScheduler();
	EXPECT_EQ(true, SGD::H1TaskSchedulerLayer::GetTaskScheduler() == nullptr);
}

START_TASK_ENTRY_POINT(TerminateAllThreads)
{
	SGD::H1TaskSchedulerLayer::GetTaskScheduler()->GetWorkerThreadPool().SignalQuitAll();
}

TEST_F(TaskSchedulerTest, TaskSchedulerLayerExecute)
{
	SGD::H1TaskSchedulerLayer::InitializeTaskScheduler();
	EXPECT_EQ(true, SGD::H1TaskSchedulerLayer::GetTaskScheduler() != nullptr);

	SGD::H1TaskSchedulerLayer::GetTaskScheduler()->GetWorkerThreadPool().StartAll();
	
	SGD::H1TaskDeclaration task(TaskEntryPoint_TerminateAllThreads, nullptr);
	SGD::H1TaskDeclarationInterface* taskInteface = &task;
	SGD::H1TaskCounter* counter = nullptr;
	SGD::H1TaskSchedulerLayer::RunTasks(&taskInteface, 1, &counter);
	SGD::H1TaskSchedulerLayer::WaitForCounter(counter);

	SGD::H1TaskSchedulerLayer::GetTaskScheduler()->GetWorkerThreadPool().WaitAll();

	SGD::H1TaskSchedulerLayer::DestroyTaskScheduler();
	EXPECT_EQ(true, SGD::H1TaskSchedulerLayer::GetTaskScheduler() == nullptr);
}

struct AddNumberData
{
	int32_t a, b;
	int32_t* result;
};

START_TASK_ENTRY_POINT(AddNumber)
{
	AddNumberData* pData = reinterpret_cast<AddNumberData*>(pTaskData_AddNumber);
	*(pData->result) = pData->a + pData->b;
}

START_TASK_ENTRY_POINT(MainLoop)
{
	int32_t arrNumber[10] = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
	int32_t arrResults[5] = { 0, };
	AddNumberData arrNumberData[5];
	for (int32_t i = 0; i < 5; ++i)
	{
		arrNumberData[i].a = arrNumber[i * 2];
		arrNumberData[i].b = arrNumber[i * 2 + 1];
		arrNumberData[i].result = &arrResults[0] + i;
	}

	SGD::H1TaskDeclaration tasks[5];
	SGD::H1TaskDeclarationInterface* taskInterfaces[5];
	for (int32_t i = 0; i < 5; ++i)
	{
		tasks[i].SetTaskEntryPoint(TaskEntryPoint_AddNumber);
		tasks[i].SetTaskData(reinterpret_cast<void*>(&arrNumberData[i]));

		taskInterfaces[i] = &tasks[i];
	}

	SGD::H1TaskCounter* counter = nullptr;
	SGD::H1TaskSchedulerLayer::RunTasks(taskInterfaces, 5, &counter);
	SGD::H1TaskSchedulerLayer::WaitForCounter(counter);
	
	EXPECT_EQ(45, arrResults[0] + arrResults[1] + arrResults[2] + arrResults[3] + arrResults[4]);
}

TEST_F(TaskSchedulerTest, TaskSchedulerLayerExecuteRecursiveAndMultipleTasks)
{
	SGD::H1TaskSchedulerLayer::InitializeTaskScheduler();
	EXPECT_EQ(true, SGD::H1TaskSchedulerLayer::GetTaskScheduler() != nullptr);

	SGD::H1TaskSchedulerLayer::GetTaskScheduler()->GetWorkerThreadPool().StartAll();

	SGD::H1TaskDeclaration task(TaskEntryPoint_MainLoop, nullptr);
	SGD::H1TaskDeclarationInterface* taskInterface = &task;
	SGD::H1TaskCounter* counter = nullptr;
	SGD::H1TaskSchedulerLayer::RunTasks(&taskInterface, 1, &counter);
	SGD::H1TaskSchedulerLayer::WaitForCounter(counter);

	// terminate all threads
	SGD::H1TaskDeclaration terminateThreadsTask(TaskEntryPoint_TerminateAllThreads, nullptr);
	taskInterface = &terminateThreadsTask;
	SGD::H1TaskSchedulerLayer::RunTasks(&taskInterface, 1, &counter);
	SGD::H1TaskSchedulerLayer::WaitForCounter(counter);

	SGD::H1TaskSchedulerLayer::GetTaskScheduler()->GetWorkerThreadPool().WaitAll();

	SGD::H1TaskSchedulerLayer::DestroyTaskScheduler();
	EXPECT_EQ(true, SGD::H1TaskSchedulerLayer::GetTaskScheduler() == nullptr);
}