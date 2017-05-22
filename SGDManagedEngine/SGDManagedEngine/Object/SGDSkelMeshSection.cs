using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1SkelMeshSection
    {
        public UInt16 MaterialIndex
        {
            set { m_MaterialIndex = value; }
            get { return m_MaterialIndex; }
        }

        public UInt16 ChunkIndex
        {
            set { m_ChunkIndex = value; }
            get { return m_ChunkIndex; }
        }

        public UInt32 NumTriangles
        {
            set { m_NumTriangles = value; }
            get { return m_NumTriangles; }
        }

        private UInt16 m_MaterialIndex;
        private UInt16 m_ChunkIndex;
        private UInt32 m_NumTriangles;
        // ETriangleSortOption
        // bDisabled - this section can be disabled for cloth simulation
        // bSelected
        private UInt16 m_CorrespondClothSectionIndex;
    }
}
