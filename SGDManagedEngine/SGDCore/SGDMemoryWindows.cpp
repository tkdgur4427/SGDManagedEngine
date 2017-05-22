namespace SGD
{
	inline void* appMalloc(uint32_t memorySize)
	{
		return malloc(memorySize);
	}

	inline void appFree(void* memoryPtr)
	{
		free(memoryPtr);
	}
}
