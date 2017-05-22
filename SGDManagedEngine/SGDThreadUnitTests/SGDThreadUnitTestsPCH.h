#pragma once

#include <windows.h>
#include <stdio.h>

// google test
#include "gtest\gtest.h"

#if _DEBUG
#pragma comment(lib, "gtestd.lib")
#else
#pragma comment(lib, "gtest.lib")
#endif

// SGDThread precompiled header
#include "SGDThreadPCH.h"
#pragma comment(lib, "SGDThread.lib")