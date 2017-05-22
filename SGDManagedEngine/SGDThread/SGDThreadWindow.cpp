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

namespace SGD
{
	inline bool appCreateThread(ThreadType* threadHandle, ThreadId* threadId, uint32_t stackSize, ThreadEntryPoint threadEntryPoint, void* data, uint32_t coreAffinity)
	{
		// put the option 'SUSPENDED' to set the thread to arbitrary CPU core
		*threadHandle = reinterpret_cast<void*>(_beginthreadex(nullptr, stackSize, threadEntryPoint, data, CREATE_SUSPENDED, nullptr));
		if (threadHandle == nullptr)
			return false;

		// set thread id
		*threadId = GetThreadId(*threadHandle);
		// set this thread to CPU core
		DWORD_PTR mask = 1ull << coreAffinity;
		SetThreadAffinityMask(*threadHandle, mask);
		// resume this thread
		ResumeThread(*threadHandle);

		return true;
	}

	inline void appDestroyThread()
	{
		_endthreadex(0);
	}

	inline void appJoinThread(uint32_t numThreads, ThreadType* threads)
	{
		WaitForMultipleObjects(numThreads, threads, true, INFINITE);
	}

	inline uint32_t appGetNumHardwareThreads()
	{
#if ENABLE_SINGLE_THREAD_DEBUG
		return 1;
#else
#if ENABLE_ARBITRARY_THREAD_NUMBER
		return ARBITRARY_THREAD_NUMBER;
#else
		SYSTEM_INFO sysInfo;
		GetSystemInfo(&sysInfo);
		return sysInfo.dwNumberOfProcessors;
#endif
#endif
	}

	inline ThreadType appGetCurrentThread()
	{
		ThreadType result = nullptr;
		// convert pseudo handle to real handle
		DuplicateHandle(GetCurrentProcess(),
			GetCurrentThread(),
			GetCurrentProcess(),
			&result,
			0,
			FALSE,
			DUPLICATE_SAME_ACCESS);
		return result;
	}

	inline ThreadId appGetCurrentThreadId()
	{
		return GetCurrentThreadId();
	}

	inline void appSetCurrentThreadAffinity(uint32_t coreAffinity)
	{
		SetThreadAffinityMask(GetCurrentThread(), coreAffinity);
	}

	inline void appCreateEvent(EventType* event)
	{
		event->Event = CreateEvent(nullptr, TRUE, FALSE, nullptr);
		event->CountWaiters = 0;
	}

	inline void appCloseEvent(EventType event)
	{
		CloseHandle(event.Event);
	}

	inline bool appWaitForEvent(EventType& event, uint32_t milliseconds)
	{
		event.CountWaiters.fetch_add(1u);
		DWORD retval = WaitForSingleObject(event.Event, milliseconds);
		uint32_t prev = event.CountWaiters.fetch_sub(1u);
		if (1 == prev)
			ResetEvent(event.Event); // we were the last to awaken, so reset event

		if (retval != WAIT_FAILED) return false;
		if (prev != 0) return false;
		return true;
	}

	inline void appSignalEvent(EventType event)
	{
		SetEvent(event.Event);
	}
}
