using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public enum H1GpuMemoryType
    {
        Invalid,
        CpuWritable,    // upload, read-back
        GpuWritable,    // default
        TotalNum,
    }

    public enum H1GpuMemoryPageSize
    {
        GpuWritablePageSize = 0x10000,     // 64KB
        CpuWritablePageSize = 0x200000,    // 2MB
    }    

    public enum H1GpuMemoryPageType
    {
        Invalid,
        // sub-allocation is not allowed
        // EX) SRV, RTV, UAV or DSV
        Normal,
        // sub-allocation is allowed
        // EX) CBV, VBV or IBV
        Segmented,
    }

    // gpu memory page resource type
    public enum H1GpuMemoryPageResourceType
    {
        Invalid,
        Buffer,
        Texture1D,
        Texture2D,
        Texture3D,
    }

    public class H1GpuMemoryChunk
    {
        public H1GpuMemoryChunk(H1GpuMemoryType type, H1GpuMemoryPageType pageType)
        {
            m_Type = type;
            m_PageType = pageType;
        }

        public H1GpuMemoryType Type
        {
            get { return m_Type; }
        }

        public H1GpuMemoryPageType PageType
        {
            get { return m_PageType; }
        }

        protected Boolean UpdateAvailableMemoryPageSegmented()
        {
            Int32 maxEvaluatedValue = Int32.MinValue;
            Int32 maxPageIndex = -1;
            for(Int32 pageIndex = 0; pageIndex < m_Pages.Count; ++pageIndex)
            {
                H1GpuMemoryPageSegmented page = m_Pages[pageIndex] as H1GpuMemoryPageSegmented;
                if (page == null) // invalid memory page type (should be segmented type)
                    return false;

                Int32 evaluatedValue = page.LargestAvailableSegmentCounts;
                if (maxEvaluatedValue < evaluatedValue)
                {
                    maxPageIndex = pageIndex;
                    maxEvaluatedValue = evaluatedValue;
                }
            }

            // set available memory page which has largest available memory
            m_AvailablePage = m_Pages[maxPageIndex] as H1GpuMemoryPageSegmented;
            return true;
        }

        protected void CreateNewPageSegmented()
        {
            H1GpuMemoryPageSegmented newPage = H1GpuMemoryPageSegmented.CreatePage(this);
            m_Pages.Add(newPage);

            // update available page as newly created page
            m_AvailablePage = newPage;
        }

        protected H1GpuMemoryAllocInfo AllocateWithSegmented(Int32 size)
        {
            if (m_Pages.Count == 0)
            {
                CreateNewPageSegmented();
            }

            // allocate memory
            H1GpuMemoryBlock newBlock = m_AvailablePage.Allocate(size);
            // not enough memory we need to make new page
            if (newBlock == null)
            {
                // create new page and allocate newly assigned available page
                CreateNewPageSegmented();
                newBlock = m_AvailablePage.Allocate(size);
            }

            // update available memory page
            UpdateAvailableMemoryPageSegmented();

            return new H1GpuMemoryAllocInfo(newBlock);
        }

        protected H1GpuMemoryAllocInfo AllocateWithNormal(Int32 size)
        {            
            throw new NotImplementedException();
        }

        public H1GpuMemoryAllocInfo Allocate(Int32 size)
        {
            H1GpuMemoryAllocInfo result = null;
            if (m_PageType == H1GpuMemoryPageType.Segmented)
            {
                result = AllocateWithSegmented(size);
            }
            else if (m_PageType == H1GpuMemoryPageType.Normal)
            {
                result = AllocateWithNormal(size);
            }
            else // currently memory page type is NOT specified
                return null;

            return result;
        }        

        public void DeallocateWithSegmented(H1GpuMemoryAllocInfo info)
        {
            // deallocate memory
            info.MemoryBlock.MemoryPage.Deallocate(info.MemoryBlock);

            // update available memory page
            UpdateAvailableMemoryPageSegmented();
        }

        public void DeallocateWithNormal(H1GpuMemoryAllocInfo info)
        {
            throw new NotImplementedException();
        }

        public void Deallocate(H1GpuMemoryAllocInfo info)
        {
            if (m_PageType == H1GpuMemoryPageType.Segmented)
            {
                DeallocateWithSegmented(info);
            }
            else if (m_PageType == H1GpuMemoryPageType.Normal)
            {
                DeallocateWithNormal(info);
            }
            else
                throw new InvalidOperationException();
        }

        // the collection of memory pages
        protected List<H1GpuMemoryPage> m_Pages = new List<H1GpuMemoryPage>();
        // available memory page (only for memory page segmented type) - when the sub allocation is possible
        protected H1GpuMemoryPageSegmented m_AvailablePage = null;
        // managing memory type
        protected H1GpuMemoryType m_Type = H1GpuMemoryType.Invalid;
        // memory page type
        protected H1GpuMemoryPageType m_PageType = H1GpuMemoryPageType.Invalid;
    }

    public class H1GpuMemoryPage
    {
        public H1GpuMemoryChunk Owner
        {
            get { return m_ChunkRef; }
        }

        protected H1GpuResource Resource
        {
            get { return m_Resource; }
        }

        public H1GpuMemoryPageType PageType
        {
            get { return m_Type; }
        }

        public H1GpuMemoryType MemoryType
        {
            get { return Owner.Type; }
        }

        // H1GpuMemoryPage is only initialized by static method called 'CreatePage'
        protected H1GpuMemoryPage(H1GpuMemoryChunk chunkRef)
        {
            m_ChunkRef = chunkRef;
        }
        
        public class H1OptionalParameters
        {            
            public H1ResourceStates ResourceStates = H1ResourceStates.Invalid;
            public H1ResourceFlags ResourceFlags = H1ResourceFlags.Unknown;
        }        

        // generic version for page creation
        protected virtual Boolean CreatePage(Int64 sizeInBytes, H1OptionalParameters options = null)
        {
            // invalid call for create page
            if (Owner.PageType != H1GpuMemoryPageType.Normal)
                return false;            

            // create resource encapsulated GPU API layer
            H1HeapType heapType = H1HeapType.Unknown;
            H1ResourceStates usage = H1ResourceStates.Invalid;

            H1GpuResourceDesc resourceDesc = new H1GpuResourceDesc();
            resourceDesc.Alignment = 0; // depends on GDI decision for choosing appropriate alignment
            resourceDesc.Height = 1;
            resourceDesc.DepthOrArraySize = 1;
            resourceDesc.MipLevels = 1;
            resourceDesc.Format = H1PixelFormat.Unknown;
            resourceDesc.SampleDesc.Count = 1;
            resourceDesc.SampleDesc.Quality = 0;
            resourceDesc.Layout = H1TextureLayout.RowMajor;

            if (Owner.Type != H1GpuMemoryType.GpuWritable)
            {
                // invalid gpu memory type
                // CpuWritable type should be H1GpuMemoryPageSegmented type
                return false;
            }

            // GPU-writable
            heapType = H1HeapType.Default;            
            resourceDesc.Width = Convert.ToUInt32(sizeInBytes);

            // apply optional parameters
            if (options != null)
            {
                resourceDesc.Flags = options.ResourceFlags;
                usage = options.ResourceStates;
            }            

            // create new page resource
            //if (!Resource.CreateResource(heapType, resourceDesc, usage))
            {
                return false;
            }

            //return true;
        }

        protected H1GpuMemoryChunk m_ChunkRef = null;
        // the resource instance for this GpuMemoryPage
        protected H1GpuResource m_Resource = null;
        // the gpu page type
        protected H1GpuMemoryPageType m_Type = H1GpuMemoryPageType.Normal;
        // the gpu page resource type
        protected H1GpuMemoryPageResourceType m_ResourceType = H1GpuMemoryPageResourceType.Invalid;
    }

    public class H1GpuMemoryPageBuffer : H1GpuMemoryPage
    {
        protected H1GpuMemoryPageBuffer(H1GpuMemoryChunk owner)
            : base (owner)
        {
            m_ResourceType = H1GpuMemoryPageResourceType.Buffer;
        }

        public static H1GpuMemoryPageBuffer CreatePage(H1GpuMemoryChunk memoryChunk, Int64 sizeInBytes, H1OptionalParameters options = null)
        {
            H1GpuMemoryPageBuffer newGpuMemoryPageBuffer = new H1GpuMemoryPageBuffer(memoryChunk);
            if (!newGpuMemoryPageBuffer.CreatePage(sizeInBytes, options))
                return null;

            return newGpuMemoryPageBuffer;
        }
    }

    // not allowed to instantiate this class explicitly (note : protected class and no static function)
    public class H1GpuMemoryPageTexture : H1GpuMemoryPage
    {
        protected H1GpuMemoryPageTexture(H1GpuMemoryChunk owner)
            : base (owner)
        {
            
        }

        public class H1OptionalParametersForTexture : H1OptionalParameters
        {
            public H1TextureLayout Layout = H1TextureLayout.RowMajor;
            public H1SampleDescription SampleDesc = new H1SampleDescription()
            {
                // default initialization values
                Count = 1, Quality = 0
            };
            public UInt16 MipLevels = 1;
        }

        // static method for more general creat page for texture resided in GPU memory
        protected virtual Boolean CreatePage(H1PixelFormat format, Int32 width, Int32 height, Int32 depthOrArraySize, H1OptionalParametersForTexture options = null)
        {
            // invalid call for create page
            if (Owner.PageType != H1GpuMemoryPageType.Normal)
                return false;

            // create resource encapsulated GPU API layer
            H1HeapType heapType = H1HeapType.Unknown;
            H1ResourceStates usage = H1ResourceStates.Invalid;
                       
            if (Owner.Type != H1GpuMemoryType.GpuWritable)
            {
                // invalid gpu memory type
                // CpuWritable type should be H1GpuMemoryPageSegmented type
                return false;
            }

            // GPU-writable
            heapType = H1HeapType.Default;
            H1GpuResourceDesc resourceDesc = new H1GpuResourceDesc();

            resourceDesc.Alignment = 0; // depends on GDI decision for choosing appropriate alignment
            resourceDesc.Width = Convert.ToUInt32(width);
            resourceDesc.Height = Convert.ToUInt32(height);
            resourceDesc.DepthOrArraySize = Convert.ToUInt16(depthOrArraySize);            
            resourceDesc.Format = format;

            // these description properties are default values
            resourceDesc.SampleDesc.Count = 1;
            resourceDesc.SampleDesc.Quality = 0;
            resourceDesc.Layout = H1TextureLayout.RowMajor;
            resourceDesc.MipLevels = 1;

            // apply optional parameters
            if (options != null)
            {
                resourceDesc.Flags = options.ResourceFlags;
                usage = options.ResourceStates;

                // resource description optional parameters
                resourceDesc.MipLevels = options.MipLevels;
                resourceDesc.Layout = options.Layout;

                // struct copy (shallow copy should be occurred)
                resourceDesc.SampleDesc = options.SampleDesc;
            }

            // create new page resource
            //if (!Resource.CreateResource(heapType, resourceDesc, usage))
            {
                return false;
            }

            //return true;
        }
    }

    public class H1GpuMemoryPageTexture1D : H1GpuMemoryPageTexture
    {
        protected H1GpuMemoryPageTexture1D (H1GpuMemoryChunk owner)
            : base (owner)
        {

        }

        public static H1GpuMemoryPageTexture1D CreatePage(H1GpuMemoryChunk owner, H1Texture1D.Description desc)
        {
            H1GpuMemoryPageTexture1D newTexture1D = new H1GpuMemoryPageTexture1D(owner);

            // convert texture1D description to appropriate input parameters to create texture1D
            H1PixelFormat format = desc.Format;
            Int32 width = Convert.ToInt32(desc.Width);
            Int32 height = 1;
            Int32 depthOrArraySize = Convert.ToInt32(desc.ArraySize);

            H1OptionalParametersForTexture options = null;
            if (desc.MipLevels != 1) // if there is any optional property to apply
            {
                options = new H1OptionalParametersForTexture()
                {
                    MipLevels = Convert.ToUInt16(desc.MipLevels)
                };
            }

            if (!newTexture1D.CreatePage(format, width, height, depthOrArraySize, options))
                return null;

            return newTexture1D;
        }
    }

    public class H1GpuMemoryPageTexture2D : H1GpuMemoryPageTexture
    {
        protected H1GpuMemoryPageTexture2D(H1GpuMemoryChunk owner)
            : base (owner)
        {

        }

        public static H1GpuMemoryPageTexture2D CreatePage(H1GpuMemoryChunk owner, H1Texture2D.Description desc)
        {
            H1GpuMemoryPageTexture2D newTexture2D = new H1GpuMemoryPageTexture2D(owner);

            // convert texture1D description to appropriate input parameters to create texture1D
            H1PixelFormat format = desc.Format;
            Int32 width = Convert.ToInt32(desc.Width);
            Int32 height = Convert.ToInt32(desc.Height);
            Int32 depthOrArraySize = Convert.ToInt32(desc.ArraySize);

            H1OptionalParametersForTexture options = null;
            if (desc.MipLevels != 1 || desc.SampleDesc.Count != 1 || desc.SampleDesc.Quality != 0) // if there is any optional property to apply
            {
                options = new H1OptionalParametersForTexture()
                {
                    MipLevels = Convert.ToUInt16(desc.MipLevels),
                    SampleDesc = new H1SampleDescription()
                    {
                        Count = desc.SampleDesc.Count,
                        Quality = desc.SampleDesc.Quality,
                    }
                };
            }

            if (!newTexture2D.CreatePage(format, width, height, depthOrArraySize, options))
                return null;

            return newTexture2D;
        }
    }

    public class H1GpuMemoryPageTexture3D : H1GpuMemoryPageTexture
    {
        protected H1GpuMemoryPageTexture3D(H1GpuMemoryChunk owner)
            : base(owner)
        {

        }

        public static H1GpuMemoryPageTexture3D CreatePage(H1GpuMemoryChunk owner, H1Texture3D.Description desc)
        {
            H1GpuMemoryPageTexture3D newTexture3D = new H1GpuMemoryPageTexture3D(owner);

            // convert texture1D description to appropriate input parameters to create texture1D
            H1PixelFormat format = desc.Format;
            Int32 width = Convert.ToInt32(desc.Width);
            Int32 height = Convert.ToInt32(desc.Height);
            Int32 depthOrArraySize = Convert.ToInt32(desc.Depth);

            H1OptionalParametersForTexture options = null;
            if (desc.MipLevels != 1) // if there is any optional property to apply
            {
                options = new H1OptionalParametersForTexture()
                {
                    MipLevels = Convert.ToUInt16(desc.MipLevels),                    
                };
            }

            if (!newTexture3D.CreatePage(format, width, height, depthOrArraySize, options))
                return null;

            return newTexture3D;
        }
    }

    // for efficient memory management, the segmented memory design is introduced
    public class H1GpuMemoryPageSegmented : H1GpuMemoryPage
    {
        public class PageSegment
        {
            // offset of page segment
            //  - to calculate memory address:
            //      StartAddress + Offset * H1GpuMemoryPage::m_PageSegmentSize
            public Int32 Offset;
        }

        public Int32 LargestAvailableSegmentCounts
        {
            get
            {
                if (m_AvailableMemoryBlocks[0] == null)
                    return -1;
                return m_AvailableMemoryBlocks[0].Counts;
            }
        }

        protected H1GpuMemoryPageSegmented(H1GpuMemoryChunk chunkRef)
            : base (chunkRef)
        {
            m_Type = H1GpuMemoryPageType.Segmented;
        }        
        
        protected static Boolean SetupPageSegments(H1GpuMemoryPageSegmented memoryPageRef, Int32 segmentCounts)
        {
            if (memoryPageRef == null)
                return false;

            if (segmentCounts <= 0)
                return false;
                 
            // initialize all related page segments properties
            memoryPageRef.m_AllocBits = new BitArray(segmentCounts, false);
            memoryPageRef.m_PageSegments = new PageSegment[segmentCounts];

            // initialize page segments
            Int32 currOffset = 0;
            foreach (PageSegment pageSegment in memoryPageRef.m_PageSegments)
            {
                pageSegment.Offset = currOffset++;
            }

            return true;
        }

        // newly override method called 'CreatePage(...)'
        public new static H1GpuMemoryPageSegmented CreatePage(H1GpuMemoryChunk memoryChunk)
        {
            H1GpuMemoryPageSegmented newPage = new H1GpuMemoryPageSegmented(memoryChunk);

            // create resource encapsulated GPU API layer
            H1HeapType heapType = H1HeapType.Unknown;
            H1ResourceStates usage = H1ResourceStates.Invalid;

            H1GpuResourceDesc resourceDesc = new H1GpuResourceDesc();
            resourceDesc.Alignment = 0;
            resourceDesc.Height = 1;
            resourceDesc.DepthOrArraySize = 1;
            resourceDesc.MipLevels = 1;
            resourceDesc.Format = H1PixelFormat.Unknown;
            resourceDesc.SampleDesc.Count = 1;
            resourceDesc.SampleDesc.Quality = 0;
            resourceDesc.Layout = H1TextureLayout.RowMajor;

            // CPU-writable
            if (newPage.Owner.Type == H1GpuMemoryType.GpuWritable)
            {
                heapType = H1HeapType.Default;

                resourceDesc.Width = Convert.ToUInt32(H1GpuMemoryPageSize.GpuWritablePageSize);
                resourceDesc.Flags = H1ResourceFlags.AllowUnorderedAccess;
                resourceDesc.Flags = H1ResourceFlags.Unknown;

                usage = H1ResourceStates.UnorderedAccess;
            }

            // GPU-writable
            else if (newPage.Owner.Type == H1GpuMemoryType.CpuWritable)
            {
                // cpu writable is mainly for upload page (it could be 'readback' property)
                heapType = H1HeapType.Upload;

                resourceDesc.Width = Convert.ToUInt32(H1GpuMemoryPageSize.CpuWritablePageSize);
                resourceDesc.Flags = H1ResourceFlags.Unknown;

                usage = H1ResourceStates.GenericRead;
            }

            // @TODO - handle 'Readback' heap type
            else
            {
                // invalid type is assigned!
                return null;
            }

            // create new page resource
            //if (!newPage.Resource.CreateResource(heapType, resourceDesc, usage))
            {
                return null;
            }

            // initialize the properties of the gpu memory page
            Int32 segmentCounts = -1;

            // calculate segment counts
            // @TODO - need to consider further for choosing segment counts (kinda magic number)
            if (newPage.Owner.Type == H1GpuMemoryType.GpuWritable)
            {
                segmentCounts = 1 << 8; // 256 segments
                newPage.m_PageSegmentCounts = segmentCounts;
                newPage.m_PageSegmentSize = Convert.ToInt32(H1GpuMemoryPageSize.GpuWritablePageSize) / newPage.m_PageSegmentCounts;
            }

            else if (newPage.Owner.Type == H1GpuMemoryType.CpuWritable)
            {
                segmentCounts = 1 << 10; // 1024 segments
                newPage.m_PageSegmentCounts = segmentCounts;
                newPage.m_PageSegmentSize = Convert.ToInt32(H1GpuMemoryPageSize.CpuWritablePageSize) / newPage.m_PageSegmentCounts;
            }

            if (!SetupPageSegments(newPage, segmentCounts))
                return null;

            return newPage;
        }

        protected void UpdateAvailableMemoryBlocks()
        {
            Boolean isContiguous = false;
            Int32 contiguousAllocBitsNum = 0;
            List<H1GpuMemoryBlock> allAvailableMemoryBlocksInPage = new List<H1GpuMemoryBlock>();

            // find all available memory blocks
            Int32 startIndex = 0;
            Int32 currIndex = 0;
            foreach (Boolean allocBit in m_AllocBits)
            {
                if (allocBit == false && isContiguous == false)
                {
                    isContiguous = true;
                    startIndex = currIndex;
                    contiguousAllocBitsNum = 1;
                }

                else if (allocBit == true && isContiguous == true)
                {
                    // if previously contiguous segments exists
                    if (contiguousAllocBitsNum > 0)
                    {
                        // add available memory block
                        H1GpuMemoryBlock newBlock = new H1GpuMemoryBlock(this, startIndex, contiguousAllocBitsNum);
                        allAvailableMemoryBlocksInPage.Add(newBlock);
                    }

                    // reset tracking temp values
                    isContiguous = false;
                    contiguousAllocBitsNum = 0;
                }

                else if (allocBit == false && isContiguous == true)
                {
                    contiguousAllocBitsNum++;
                }

                else // allocBit == true && isContiguous == false
                {
                    // do nothing
                }

                // update index
                currIndex++;
            }

            // sort allAvailableMemoryBlocksInPage by memory segment counts
            allAvailableMemoryBlocksInPage.OrderBy(x => x.Counts);

            // extract largest three memory blocks
            // zero out the m_AvailableMemoryBlocks
            Array.Clear(m_AvailableMemoryBlocks, 0, m_AvailableMemoryBlocks.Count());
            // reuse currIndex variable
            currIndex = 0; 
            foreach (H1GpuMemoryBlock block in allAvailableMemoryBlocksInPage)
            {
                if (currIndex > 3)
                    break;
                
                m_AvailableMemoryBlocks[currIndex] = block;
            }
        }        

        public H1GpuMemoryBlock Allocate(Int32 size)
        {
            // calculate number of blocks we need to allocate
            Int32 numSegments = -1;
            numSegments = (size + m_PageSegmentSize) / m_PageSegmentSize;

            // if available memory blocks are not initialized yet, update them
            if (m_AvailableMemoryBlocks[0] == null)
                UpdateAvailableMemoryBlocks();

            // among available memory blocks, find the best fit block        
            Int32 minEvaluatedValue = Int32.MaxValue;
            Int32 minEvaluatedIndex = -1;

            Int32 currIndex = 0;
            foreach (H1GpuMemoryBlock memoryBlock in m_AvailableMemoryBlocks)
            {
                Int32 evaluatedValue = memoryBlock.Counts - numSegments;
                if (evaluatedValue > 0 && minEvaluatedValue > evaluatedValue)
                {
                    // update the values
                    minEvaluatedIndex = currIndex;
                    minEvaluatedValue = evaluatedValue;
                }

                currIndex++;
            }

            // there is no available memory for this page
            if (minEvaluatedIndex == -1)
                return null;

            H1GpuMemoryBlock newBlock = m_AvailableMemoryBlocks[minEvaluatedIndex];

            // update available memory blocks
            UpdateAvailableMemoryBlocks();

            return newBlock;
        }

        public void Deallocate(H1GpuMemoryBlock block)
        {
            // apply alloc bits as deallocated
            Int32 startIndex = block.Start;
            Int32 counts = block.Counts;

            for (Int32 index = startIndex; index < counts; ++index)
            {
                // reset alloc bit as deallocated (free space)
                m_AllocBits.Set(index, false);
            }

            // invalidate memory block
            block.Invalidate();

            // update available memory blocks
            UpdateAvailableMemoryBlocks();
        }
          
        // page properties
        protected Int32 m_CurrPageSegmentIndex = 0;
        protected Int32 m_PageSegmentSize = -1;
        protected Int32 m_PageSegmentCounts = 0;

        // deferred creation with page segment number
        protected BitArray m_AllocBits = null;
        // fixed size of array of page segments
        protected PageSegment[] m_PageSegments = null;
        // tracking available memory blocks in increasing order
        //  - to reduce memory fragmentation, track multiple gpu memory blocks to make it best fit
        protected readonly static Int32 TrackingAvailableMemoryBlockCounts = 3;
        protected H1GpuMemoryBlock[] m_AvailableMemoryBlocks = new H1GpuMemoryBlock[TrackingAvailableMemoryBlockCounts];
    }

    public class H1GpuMemoryBlock
    {
        public Int32 Start
        {
            get { return m_PageSegmentStart; }
        }

        public Int32 Counts
        {
            get { return m_PageSegmentCounts; }
        }

        public H1GpuMemoryPageSegmented MemoryPage
        {
            get { return m_PageRef; }
        }

        // 'end' as parameter is assigned value
        public H1GpuMemoryBlock(H1GpuMemoryPageSegmented pageRef, Int32 start, Int32 counts)
        {
            m_PageRef = pageRef;
            m_PageSegmentStart = start;            
            m_PageSegmentEnd = m_PageSegmentStart + counts - 1;
            m_PageSegmentCounts = counts;
        }

        public void Invalidate()
        {
            m_PageRef = null;
            m_PageSegmentCounts = 0;
            m_PageSegmentStart = -1;
            m_PageSegmentEnd = -1;
        }        

        protected H1GpuMemoryPageSegmented m_PageRef;
        // consecutive collection of page segments (range : start - end)
        protected Int32 m_PageSegmentStart = -1;
        protected Int32 m_PageSegmentEnd = -1;
        protected Int32 m_PageSegmentCounts = 0;
    }

    [Flags]
    public enum H1GpuMemoryAllocInfoProperties
    {
        GpuWritable = 1,
        CpuWritable = 1 << 1,
        Normal      = 1 << 2,
        Segmented   = 1 << 3,
    }

    public class H1GpuMemoryAllocInfo
    {
        public H1GpuMemoryBlock MemoryBlock
        {
            get { return m_MemoryBlockRef; }
        }

        // constructor for memory page with segmented type
        public H1GpuMemoryAllocInfo(H1GpuMemoryBlock newBlock)
        {
            m_MemoryBlockRef = newBlock;            

            H1GpuMemoryPage page = m_MemoryBlockRef.MemoryPage;

            // this constructor should trigger by segmented type only
            if (page.PageType != H1GpuMemoryPageType.Segmented)
                throw new InvalidOperationException("Invalid parameter for this constructor!"); 

            // set appropriate properties with memory chunk
            H1GpuMemoryChunk chunk = page.Owner;
            SetProperties(chunk);
        }

        // constructor for memory page with normal type
        public H1GpuMemoryAllocInfo(H1GpuMemoryPage newPage)
        {
            m_MemoryPageRef = newPage;

            // this constructor should trigger by normal type only
            if (m_MemoryPageRef.PageType != H1GpuMemoryPageType.Normal)
                throw new InvalidOperationException("Invalid parameter for this constructor!");

            // set appropriate properties with memory chunk
            H1GpuMemoryChunk chunk = m_MemoryPageRef.Owner;
            SetProperties(chunk);
        }

        protected void SetProperties(H1GpuMemoryChunk chunkRef)
        {
            switch (chunkRef.PageType)
            {
                case H1GpuMemoryPageType.Normal:
                    m_Properties |= H1GpuMemoryAllocInfoProperties.Normal;
                    break;

                case H1GpuMemoryPageType.Segmented:
                    m_Properties |= H1GpuMemoryAllocInfoProperties.Segmented;
                    break;
            }

            switch (chunkRef.Type)
            {
                case H1GpuMemoryType.CpuWritable:
                    m_Properties |= H1GpuMemoryAllocInfoProperties.CpuWritable;
                    break;

                case H1GpuMemoryType.GpuWritable:
                    m_Properties |= H1GpuMemoryAllocInfoProperties.GpuWritable;
                    break;
            }
        }

        protected H1GpuMemoryBlock m_MemoryBlockRef = null;
        protected H1GpuMemoryPage m_MemoryPageRef = null;
        protected H1GpuMemoryAllocInfoProperties m_Properties = 0;
    }
}
