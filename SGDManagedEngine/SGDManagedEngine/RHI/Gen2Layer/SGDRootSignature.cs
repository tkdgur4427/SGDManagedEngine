using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1RootSignature
    {
        public Int32 NumParameters
        {
            get { return m_NumParameters; }
        }

        public BitArray DescriptorTableBitArray
        {
            get { return m_DescriptorTableBitMap; }
        }

        public Int32[] DescriptorTableSize
        {
            get { return m_DescriptorTableSize; }
        }

        public H1RootSignature(Int32 numRootParameters, Int32 numStaticSamplers)
        {
            m_Finalized = false;
            m_NumParameters = numRootParameters;
            m_NumSamplers = numStaticSamplers;
            m_NumInitializedStaticSamplers = 0;

            ConstructPlatformDependentMembers();
        }

        protected Boolean m_Finalized;
        protected Int32 m_NumParameters;
        protected Int32 m_NumSamplers;
        protected Int32 m_NumInitializedStaticSamplers;

        // one bit is set for root parameters that are (non-sampler) descriptor tables
        protected BitArray m_DescriptorTableBitMap = new BitArray(32);
        // non-sampler descriptor tables need to know their descriptor count
        protected Int32[] m_DescriptorTableSize = new Int32[16];
        // the sum of all non-sampler descriptor table counts
        protected Int32 m_MaxDescriptorCacheHandleCount;
    }
}
