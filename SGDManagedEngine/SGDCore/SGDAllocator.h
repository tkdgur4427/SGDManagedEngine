#pragma once

#include "SGDTaggedHeap.h"

namespace SGD
{
	class H1Allocator
	{
	public:
		enum {
			BLOCK_SIZE = H1TaggedHeap::BLOCK_SIZE,
			DATA_OFFSET = sizeof(H1TaggedHeap::H1BlockHeader),
			DATA_SIZE = BLOCK_SIZE - DATA_OFFSET,
		};

		struct H1PageHeader
		{
			H1PageHeader()
				: Next(nullptr), Prev(nullptr)
			{}

			H1Page* Next;
			H1Page* Prev;
		};

		struct H1Page
		{
			H1Page()
				: MemoryBlock(nullptr)
			{}

			H1PageHeader PageHeader;
			H1TaggedHeap::H1Block* MemoryBlock;
		};

		H1Allocator(H1TaggedHeap& owner)
		{
			m_Owner = owner;
		}

		virtual RAW_PTR Allocate(uint32_t size) = 0;
		// deallocate is done by manually in H1TaggedHeap by tagged id combination check
		// virtual void Deallocate(RAW_PTR pointer) {}

	private:
		H1TaggedHeap& m_Owner;
	};
}