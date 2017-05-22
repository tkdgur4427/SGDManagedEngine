using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public partial class H1DescriptorHandle
    {
        public CpuDescriptorHandle CpuHandle;
        public GpuDescriptorHandle GpuHandle;

        public static H1DescriptorHandle operator + (H1DescriptorHandle descriptorHandle, Int64 offsetScaledByDescriptorSize)
        {
            descriptorHandle.CpuHandle += Convert.ToInt32(offsetScaledByDescriptorSize);
            descriptorHandle.GpuHandle += Convert.ToInt32(offsetScaledByDescriptorSize);
            return descriptorHandle;
        }
    }

    // describes a descriptor table entry; a region of the handle cache and which handles have been set
    public class H1DescriptorTableCache
    {
        public BitArray AssignedHandlesBitMap = new BitArray(32);
        // indicates (references) m_HandleCaches in H1DescriptorHandleCache
        public Int32 TableStart;
        public Int32 TableSize;
    }

    public class H1DescirptorHandleCache
    {
        public H1DescirptorHandleCache()
        {
            ClearCache();
        }

        public void ClearCache()
        {
            // clear root descriptor table bit map
            for (Int32 i = 0; i < m_RootDescriptorTableBitMap.Count; ++i)
                m_RootDescriptorTableBitMap[i] = false;

            m_MaxCachedDescriptors = 0;
        }

        public Int64 ComputeStagedSize()
        {
            // sum the maximum assigned offsets of stale descriptor tables to determine total needed space
            Int64 neededSpace = 0;
            BitArray staleParams = m_StaleRootParamsBitMap;
            for(Int32 rootIndex = 0; rootIndex < staleParams.Count; ++rootIndex) 
            {
                if (staleParams[rootIndex] == true) // forward bit scanning
                {
                    staleParams[rootIndex] = false; // XOR for current bit element

                    BitArray AssignedHandlesForRootDescriptorTable = m_RootDescriptorTable[rootIndex].AssignedHandlesBitMap;
                    for (Int32 maxSetHandle = AssignedHandlesForRootDescriptorTable.Count - 1; maxSetHandle >= 0; --maxSetHandle)
                    {
                        // reverse bit scanning
                        if (AssignedHandlesForRootDescriptorTable[maxSetHandle] == true)
                        {
                            neededSpace += maxSetHandle + 1;
                            break;
                        }
                    }
                }
            }

            return neededSpace;
        }

        public Boolean CopyAndBindStaleTables(H1DescriptorHandle destHandleStart, GraphicsCommandList cmdList, Boolean bGraphicsRootDescriptorTable = true)
        {
            Int64 staleParamCount = 0;
            Int64[] tableSize = new Int64[MaxNumDescriptorsTables];
            Int64[] rootIndices = new Int64[MaxNumDescriptorsTables];
            Int64 neededSpace = 0;
            Int32 rootIndex;

            // sum the maximum assigned offsets of stale descriptor tables to determine total needed space
            BitArray staleParams = m_StaleRootParamsBitMap;
            for (rootIndex = 0; rootIndex < staleParams.Count; ++rootIndex)
            {
                // forward bit scanning
                if (staleParams[rootIndex] == true)
                {
                    // cache stale parameter index to the root indices
                    rootIndices[staleParamCount] = rootIndex;
                    // XOR to staleParams
                    staleParams[rootIndex] = false;

                    BitArray AssignedHandlesForRootDescriptorTable = m_RootDescriptorTable[rootIndex].AssignedHandlesBitMap;
                    for (Int32 maxSetHandle = AssignedHandlesForRootDescriptorTable.Count - 1; maxSetHandle >= 0; --maxSetHandle)
                    {
                        // bit reverse traversing
                        if (AssignedHandlesForRootDescriptorTable[maxSetHandle] == true)
                        {
                            neededSpace += maxSetHandle + 1;
                            tableSize[staleParamCount] = maxSetHandle + 1;
                            break; // only take an account of maximum handle index
                        }
                    }

                    // increase stale param count
                    staleParamCount++;
                }
            }

            if (staleParamCount > MaxNumDescriptorsTables)
                return false; // only equipped to handle so many descriptor tables

            // clear stale root param bit map
            for (int i = 0; i < m_StaleRootParamsBitMap.Count; ++i)
                m_StaleRootParamsBitMap[i] = false;

            const Int32 MaxDescriptorPerCopy = 16;
            Int32 numDestDescriptorRanges = 0;
            CpuDescriptorHandle[] destDescriptorRangeStart = new CpuDescriptorHandle[MaxDescriptorPerCopy];
            Int32[] destDescriptorRangeSizes = new Int32[MaxDescriptorPerCopy];

            Int32 numSrcDescriptorRanges = 0;
            CpuDescriptorHandle[] srcDescriptorRangeStart = new CpuDescriptorHandle[MaxDescriptorPerCopy];
            Int32[] srcDescriptorRangeSizes = new Int32[MaxDescriptorPerCopy];

            Int32 descriptorSize = H1DynamicDescriptorHeap.GetDescriptorSize();

            for (Int32 i = 0; i < staleParamCount; ++i)
            {
                // get the root index and set root descriptor table
                rootIndex = Convert.ToInt32(rootIndices[i]);
                if (bGraphicsRootDescriptorTable)
                {
                    cmdList.SetGraphicsRootDescriptorTable(rootIndex, destHandleStart.GpuHandle);
                }
                else
                {
                    cmdList.SetComputeRootDescriptorTable(rootIndex, destHandleStart.GpuHandle);
                }

                H1DescriptorTableCache rootDescriptorTable = m_RootDescriptorTable[rootIndex];

                // cached local variables 
                Int32 srcHandles = rootDescriptorTable.TableStart;
                BitArray setHandles = rootDescriptorTable.AssignedHandlesBitMap;
                CpuDescriptorHandle currDest = destHandleStart.CpuHandle;

                // move dest handle start ptr by table size scaled by descriptor size
                destHandleStart = destHandleStart + (tableSize[i] * descriptorSize);

                for (Int32 skipCount = 0; skipCount < setHandles.Count; ++skipCount)
                {
                    // forward bit scanning
                    if (setHandles[skipCount] == true)
                    {
                        // skip over unset descriptor handles
                        srcHandles += skipCount;
                        currDest.Ptr += skipCount * descriptorSize;

                        Int32 descriptorCount;
                        for (descriptorCount = skipCount; descriptorCount < setHandles.Count; ++descriptorCount)
                        {
                            // find unset descriptor handles (skipping consecutive set descriptor handles)
                            if (setHandles[descriptorCount] == false)
                            {
                                // update descriptorCount as consecutive set descriptor handles
                                descriptorCount -= skipCount;
                                // update skipCount
                                skipCount += descriptorCount;
                                break;
                            }
                        }

                        // if we run out of temp room, copy that we've got so far
                        if (numDestDescriptorRanges + descriptorCount > MaxDescriptorPerCopy)
                        {
                            H1Global<H1ManagedRenderer>.Instance.Device.CopyDescriptors(
                                numDestDescriptorRanges, destDescriptorRangeStart, destDescriptorRangeSizes,
                                numSrcDescriptorRanges, srcDescriptorRangeStart, srcDescriptorRangeSizes,
                                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

                            // reset the number of [src|dest] descriptor range numbers
                            numSrcDescriptorRanges = 0;
                            numDestDescriptorRanges = 0;
                        }

                        // set up destination range
                        destDescriptorRangeStart[numDestDescriptorRanges] = currDest;
                        destDescriptorRangeSizes[numDestDescriptorRanges] = descriptorCount;
                        ++numDestDescriptorRanges;

                        // set up source ranges (one descriptor each because we don't assume they are contiguous)
                        for (Int32 j = 0; j < descriptorCount; ++j)
                        {
                            srcDescriptorRangeStart[numSrcDescriptorRanges] = m_HandleCaches[srcHandles + j];
                            srcDescriptorRangeSizes[numSrcDescriptorRanges] = 1;
                            ++numSrcDescriptorRanges;
                        }

                        // move the destination pointer forward by the number of descriptors we will copy
                        srcHandles += descriptorCount;
                        currDest.Ptr += descriptorCount * descriptorCount;
                    }
                }
            }

            H1Global<H1ManagedRenderer>.Instance.Device.CopyDescriptors(
                numDestDescriptorRanges, destDescriptorRangeStart, destDescriptorRangeSizes,
                numSrcDescriptorRanges, srcDescriptorRangeStart, srcDescriptorRangeSizes,
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

            return true;
        }

        public void UnbindAllValid()
        {
            // clear staleRootParamsBitMap
            for (Int32 rootIndex = 0; rootIndex < m_StaleRootParamsBitMap.Count; ++rootIndex)
                m_StaleRootParamsBitMap[rootIndex] = false;

            BitArray tableParams = m_RootDescriptorTableBitMap;
            for (Int32 rootIndex = 0; rootIndex < tableParams.Count; ++rootIndex)
            {
                // forward bit scanning
                if (tableParams[rootIndex] == true)
                {
                    tableParams[rootIndex] = false;
                    BitArray assignedHandlesBitMapForRootDescriptorTable = m_RootDescriptorTable[rootIndex].AssignedHandlesBitMap;
                    for (Int32 handleIndex = 0; handleIndex < assignedHandlesBitMapForRootDescriptorTable.Count; ++handleIndex)
                    {
                        // forward bit scanning
                        if (assignedHandlesBitMapForRootDescriptorTable[handleIndex] == true)
                        {
                            // update staleRootParameterBitMap
                            m_StaleRootParamsBitMap[rootIndex] = true;
                            break;
                        }
                    }
                }
            }
        }

        public Boolean StageDescriptorHandles(Int32 rootIndex, Int32 offset, Int32 numHandles, CpuDescriptorHandle[] handles)
        {
            if (m_RootDescriptorTableBitMap[rootIndex] == false)
            {
                // root parameter is not a CBV_SRV_UAV descriptor table
                return false;
            }

            // descriptor table is out of boundary by handles(descriptor handles)
            if (offset + numHandles > m_RootDescriptorTable[rootIndex].TableSize)
                return false;

            H1DescriptorTableCache tableCache = m_RootDescriptorTable[rootIndex];

            // set handles to handle caches for descriptor table cache
            Int32 copyDestIndex = tableCache.TableStart + offset;
            for (Int32 i = 0; i < numHandles; ++i)
            { 
                m_HandleCaches[copyDestIndex + i] = handles[i];
                tableCache.AssignedHandlesBitMap[offset + i] = true;
            }                

            // set root param bit map as true
            m_StaleRootParamsBitMap[rootIndex] = true;

            return true;
        }

        public Boolean ParseRootSignature(H1RootSignature rootSignature)
        {
            Int32 currentOffset = 0;

            // maybe we need to support something greater
            if (rootSignature.NumParameters > 16)
                return false;

            for (Int32 i = 0; i < m_StaleRootParamsBitMap.Count; ++i)
                m_StaleRootParamsBitMap[i] = false;

            for (Int32 i = 0; i < rootSignature.DescriptorTableBitArray.Count; ++i)
                m_RootDescriptorTableBitMap[i] = rootSignature.DescriptorTableBitArray[i];

            BitArray tableParams = m_RootDescriptorTableBitMap;
            for (Int32 rootIndex = 0; rootIndex < tableParams.Count; ++rootIndex)
            {
                // bit scanning forward
                if (tableParams[rootIndex] == true)
                {
                    Int32 tableSize = rootSignature.DescriptorTableSize[rootIndex];

                    // the invalid table size is smaller and equal than zero
                    if (tableSize <= 0)
                        return false;

                    H1DescriptorTableCache rootDescriptorTable = m_RootDescriptorTable[rootIndex];
                    rootDescriptorTable.TableSize = tableSize;
                    rootDescriptorTable.TableStart = currentOffset;
                    for (Int32 i = 0; i < rootDescriptorTable.AssignedHandlesBitMap.Count; ++i)
                        rootDescriptorTable.AssignedHandlesBitMap[i] = false;

                    currentOffset++;
                }
            }

            m_MaxCachedDescriptors = currentOffset;

            return true;
        }

        public static Int32 MaxNumDescriptors = 256;
        public static Int32 MaxNumDescriptorsTables = 16;

        private BitArray m_RootDescriptorTableBitMap = new BitArray(32);
        private BitArray m_StaleRootParamsBitMap = new BitArray(32);
        private Int32 m_MaxCachedDescriptors;        
        private H1DescriptorTableCache[] m_RootDescriptorTable = new H1DescriptorTableCache[MaxNumDescriptorsTables];
        private CpuDescriptorHandle[] m_HandleCaches = new CpuDescriptorHandle[MaxNumDescriptors];
    }

    public partial class H1DynamicDescriptorHeapManager
    {
        public Int32 DescriptorSize
        {
            get { return m_DescriptorSize; }
        }

        void InitializePlatformDependent()
        {
            m_DescriptorSize = H1Global<H1ManagedRenderer>.Instance.Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
        }

        public void AddRetiredDescriptorHeap(Int64 fenceValue, DescriptorHeap retiredHeap)
        {
            m_RetiredDescriptorHeaps.Enqueue(new KeyValuePair<Int64, DescriptorHeap>(fenceValue, retiredHeap));
        }

        public DescriptorHeap RequestDescriptorHeap()
        {
            // @TODO - need to be thread-safe

            if (m_AvailableDescriptorHeaps.Count > 0)
            {
                DescriptorHeap heap = m_AvailableDescriptorHeaps.Dequeue();
                return heap;
            }
            else
            {
                DescriptorHeapDescription descriptorHeapDesc = new DescriptorHeapDescription();
                descriptorHeapDesc.Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView;
                descriptorHeapDesc.DescriptorCount = NumDescriptorsPerHeap;
                descriptorHeapDesc.Flags = DescriptorHeapFlags.ShaderVisible;
                descriptorHeapDesc.NodeMask = 1;

                // create descriptor heap and add heap to the pool
                DescriptorHeap heap = H1Global<H1ManagedRenderer>.Instance.Device.CreateDescriptorHeap(descriptorHeapDesc);
                m_DescriptorHeapPool.Add(heap);
                return heap;
            }
        }

        private List<DescriptorHeap> m_DescriptorHeapPool = new List<DescriptorHeap>();
        private Queue<DescriptorHeap> m_AvailableDescriptorHeaps = new Queue<DescriptorHeap>();
        private Queue<KeyValuePair<Int64, DescriptorHeap>> m_RetiredDescriptorHeaps = new Queue<KeyValuePair<Int64, DescriptorHeap>>();

        private Int32 m_DescriptorSize;
    }

    public partial class H1DynamicDescriptorHeap
    {
        void InitializePlatformDependent()
        {
            // static dynamic descriptor heap manager is not allocated
            if (m_DynamicDescriptorHeapManager == null) 
                m_DynamicDescriptorHeapManager = new H1DynamicDescriptorHeapManager();

            // platform-dependent member variables allocations            
        }

        public static Int32 GetDescriptorSize()
        {
            return m_DynamicDescriptorHeapManager.DescriptorSize;
        }

        public Boolean HasSpace(Int32 count)
        {
            return m_CurrDescriptorHeapRef != null && m_CurrOffset + count <= H1DynamicDescriptorHeapManager.NumDescriptorsPerHeap;
        }

        public void RetireCurrentHeap()
        {
            // don't retire unused heaps
            if (m_CurrOffset == 0)
                return;

            m_RetiredDescriptorHeapRefs.Add(m_CurrDescriptorHeapRef);
            m_CurrDescriptorHeapRef = null;
            m_CurrOffset = 0;
        }

        public static void DiscardDescriptorHeaps(Int64 fenceValue, List<DescriptorHeap> usedHeaps)
        {
            // @TODO - need to be thread-safe            
            foreach (DescriptorHeap heap in usedHeaps)
                m_DynamicDescriptorHeapManager.AddRetiredDescriptorHeap(fenceValue, heap);
        }

        public void RetireUsedHeaps(Int64 fenceValue)
        {
            DiscardDescriptorHeaps(fenceValue, m_RetiredDescriptorHeapRefs);
            m_RetiredDescriptorHeapRefs.Clear();
        }

        public void CleanupUsedHeaps(Int64 fenceValue)
        {
            RetireCurrentHeap();
            RetireUsedHeaps(fenceValue);
            m_GraphicsDescriptorHandleCache.ClearCache();
            m_ComputeDescriptorHandleCache.ClearCache();
        }

        public DescriptorHeap GetHeap()
        {
            H1DynamicDescriptorHeapManager dynamicDescriptorHeapManagerDX12 = m_DynamicDescriptorHeapManager;

            if (m_CurrDescriptorHeapRef == null)
            {
                m_CurrDescriptorHeapRef = dynamicDescriptorHeapManagerDX12.RequestDescriptorHeap();
                m_FirstDescriptor = new H1DescriptorHandle()
                {
                    CpuHandle = m_CurrDescriptorHeapRef.CPUDescriptorHandleForHeapStart,
                    GpuHandle = m_CurrDescriptorHeapRef.GPUDescriptorHandleForHeapStart,
                };
            }

            return m_CurrDescriptorHeapRef;
        }

        public Boolean UploadDirect(CpuDescriptorHandle handle, ref GpuDescriptorHandle result)
        {
            if (!HasSpace(1))
            {
                // @TODO - currently just return and make it false
                return false;
            }

            H1CommandContext commandContextDX12 = m_CommandContextRef;
            commandContextDX12.SetDescriptorHeap(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView, GetHeap());

            H1DescriptorHandle destHandle = m_FirstDescriptor + m_CurrOffset * GetDescriptorSize();
            m_CurrOffset += 1;

            H1Global<H1ManagedRenderer>.Instance.Device.CopyDescriptorsSimple(1, destHandle.CpuHandle, handle, DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
            result = destHandle.GpuHandle;

            return true;
        }

        // @TODO - currently manually clean up dynamic descriptor heap! need to sync them
        private DescriptorHeap m_CurrDescriptorHeapRef;
        private Int32 m_CurrOffset;
        private H1DescriptorHandle m_FirstDescriptor;
        private List<DescriptorHeap> m_RetiredDescriptorHeapRefs = new List<DescriptorHeap>();
        public H1DescirptorHandleCache m_GraphicsDescriptorHandleCache = new H1DescirptorHandleCache();
        public H1DescirptorHandleCache m_ComputeDescriptorHandleCache = new H1DescirptorHandleCache();
    }
#endif
}
