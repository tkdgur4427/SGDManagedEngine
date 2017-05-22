#pragma once

namespace SGD
{
	// size in MB
	template <uint32_t totalSize, uint32_t blockSize = 2>
	class H1TaggedHeap
	{
	public:
		enum { 
			TOTAL_SIZE = 1024 * 1024 * totalSize, 
			BLOCK_SIZE = 1024 * 1024 * 2,
			BLOCK_COUNT = TOTAL_SIZE / BLOCK_SIZE,
		};

		enum BlockTagType
		{
			// Game
			Game_Logic		= 1 << 0,
			Render_Logic	= 1 << 1,
			GPU_Exec		= 1 << 2,,
		};
		
		/*
		- H1Block
			- each block is owned by a tag
			- no 'free(ptr)' interface
			- free all blocks associated with a specific tag
		*/
		struct H1BlockHeader
		{
			// BlockTagType bit-combination
			uint64_t TagId;
			// block index (fast tracing purpose)
			uint32_t BlockIndex;
		};

		struct H1Block
		{
			// memory block header
			H1BlockHeader Header
			// data pointer
			byte DataPtr[1];
		};

		bool Initialize()
		{
			// alloc big chunk for tagged heap
			m_StartPtr = appMalloc(TOTAL_SIZE);

			// set all block pointers
			m_Blocks[0] = m_StartPtr;
			for (int32_t blockIndex = 1; blockIndex < BLOCK_COUNT; ++blockIndex)
			{
				m_Blocks[blockIndex] = reinterpret_cast<void*>(reinterpret_cast<byte*>(m_Blocks[blockIndex]) + BLOCK_SIZE);
			}

			// fill block's properties
			for (int32_t blockIndex = 0; blockIndex < BLOCK_COUNT; ++blockIndex)
			{
				m_Blocks[blockIndex]->Header.TagId = 0;
				m_Blocks[blockIndex]->Header.BlockIndex = blockIndex;
				m_Blocks[blockIndex]->DataPtr = *(reinterpret_cast<byte*>(m_Blocks[blockIndex]) + sizeof(H1BlockHeader));
			}

			// initialize block alloc bits
			for (int32_t blockIndex = 0; blockIndex < BLOCK_COUNT; ++blockIndex)
			{
				m_BlockAllocBits[blockIndex] = false;
			}

			return true;
		}

		void Destroy()
		{
			// free chunk memory
			appFree(m_StartPtr);
		}

	private:
		RAW_PTR m_StartPtr;
		// memory blocks pointer in tagged heap (different from data pointer in H1Block)
		H1Block* m_Blocks[BLOCK_COUNT];
		// if 1, current block is allocated, otherwise it is not being used
		std::bitset<BLOCK_COUNT> m_BlockAllocBits;
	};
}