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

#include <atomic>
#include <vector>

#if _WIN32
#include <Windows.h>
#endif

#include <cassert>

#define ENABLE_SINGLE_THREAD_DEBUG 0

#define ENABLE_ARBITRARY_THREAD_NUMBER 1
#if ENABLE_ARBITRARY_THREAD_NUMBER
#define ARBITRARY_THREAD_NUMBER 3
#endif

// thread util
#include "SGDThreadUtil.h"

#if _DEBUG	// debug mode
#define SGD_UNIT_TESTS 1
#else		// release mode
#define SGD_UNIT_TESTS 0
#define NDEBUG // for cassert header, disable
#endif

#if SGD_UNIT_TESTS
#define UNIT_TEST_VIRTUAL virtual
#else
#define UNIT_TEST_VIRTUAL
#endif

#define USE_MS_CONCURRENT_QUEUE 0

#if USE_MS_CONCURRENT_QUEUE
#include "concurrent_queue.h"
#else
// concurrent queue
#include "concurrentqueue.h"
#endif
