using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1MultiSizeIndexContainer
    {
        public List<UInt32> Indices
        {
            get { return m_Indices; }
        }

        private List<UInt32> m_Indices = new List<UInt32>();
    }

    public abstract class H1SkeletalMeshVertexDataInterface
    {
        public enum VertexDataType
        {
            Position,
            TangentZ,
            TangentX,
            Texcoord,
            InfluencedBones,
            InfluencedWeights,
            Color,
        }

        public VertexDataType DataType
        {
            get { return m_VertexDataType; }
            set { m_VertexDataType = value; }
        }

        // skeletal mesh vertex data type
        protected VertexDataType m_VertexDataType;
                
        // properties        
        protected UInt32 m_Stride;
        protected UInt32 m_NumVertices;
        protected UInt32 m_NumTexcoords;
        protected Boolean m_bNeedCPUAccess;
    }

    public class H1SkeletalMeshVertexData<VertexType> : H1SkeletalMeshVertexDataInterface
    {
        public VertexType[] VertexBuffer
        {
            get { return m_VertexData; }
        }

        public H1SkeletalMeshVertexData()
        {
            m_Stride = Convert.ToUInt32(Vector4.SizeInBytes);
        }

        public void SetVertexData(VertexType[] vertexData, VertexDataType dataType, Boolean bNeedCPUAccess = false)
        {
            UInt32 numVertexData = Convert.ToUInt32(vertexData.Count());
            if (dataType != VertexDataType.Texcoord)
                m_NumVertices = numVertexData;
            else
                m_NumTexcoords = numVertexData;

            m_bNeedCPUAccess = bNeedCPUAccess;
            m_VertexDataType = dataType;

            // copy the vertex data
            m_VertexData = new VertexType[vertexData.Count()];
            vertexData.CopyTo(m_VertexData, 0);
        }

        // vertex data
        private VertexType[] m_VertexData; 
    }

    public class H1SkeletalMeshVertexBuffers // should inherit vertex buffer : H1VertexBuffer
    {
        public H1SkeletalMeshVertexBuffers(Int32 skelMeshChunkIndex)
        {
            m_SkelMeshChunkIndex = skelMeshChunkIndex;
        }

        public Int32 SkelMeshChunkIndex
        {
            get { return m_SkelMeshChunkIndex; }
        }

        public H1SkeletalMeshVertexDataInterface GetSkeletalMeshVertexData(Int32 skelChunkIndex)
        {
            return m_VertexDataList[skelChunkIndex];
        }

        public Int32 GetSkeletalMeshVertexDataCount()
        {
            return m_VertexDataList.Count;
        }

        public void AddSkeletalMeshVertexData(H1SkeletalMeshVertexDataInterface newVertexData)
        {
            m_VertexDataList.Add(newVertexData);
        }

        // chunk index
        private Int32 m_SkelMeshChunkIndex;
        // the vertex data storage
        private List<H1SkeletalMeshVertexDataInterface> m_VertexDataList = new List<H1SkeletalMeshVertexDataInterface>();                
    }

    public class H1SkeletalChunkVertexBufferSet
    {
        public List<H1SkeletalMeshVertexBuffers> SkeletalMeshVertexBuffers
        {
            get { return m_SkelMeshVertexBuffers; }
        }
                
        private List<H1SkeletalMeshVertexBuffers> m_SkelMeshVertexBuffers = new List<H1SkeletalMeshVertexBuffers>();
    }

    public class H1StaticLODModel
    {
        public List<H1SkelMeshChunk> Chunks
        {
            get { return m_Chunks; }
        }

        public List<H1SkelMeshSection> Sections
        {
            get { return m_Sections; }
        }

        public H1MultiSizeIndexContainer MultiSizeIndexContainer
        {
            get { return m_MultiSizeIndexContainer; }
        }

        public H1SkeletalChunkVertexBufferSet VertexBufferGPUSkin
        {
            get { return m_VertexBufferGPUSkin; }
        } 

        private String m_Name;
        private H1Transform m_LocalTransform;

        // a set of skeletal mesh triangles which use the same material and chunks
        private List<H1SkelMeshSection> m_Sections = new List<H1SkelMeshSection>();
        // the vertex chunks which made up this LOD
        private List<H1SkelMeshChunk> m_Chunks = new List<H1SkelMeshChunk>();
        // bone hierarchical subset active for this chunk
        private List<UInt16> m_ActiveBoneIndices = new List<UInt16>();
        // bones that should be updated when rendering this LOD
        private List<UInt16> m_RequiredBones = new List<UInt16>();
        // index buffer
        private H1MultiSizeIndexContainer m_MultiSizeIndexContainer = new H1MultiSizeIndexContainer();
        private UInt32 m_Size;
        private UInt32 m_NumVertices;
        private UInt32 m_NumTexcoords;
        // static vertices from chunks for skinning on GPU
        //private H1SkeletalMeshVertexBuffers m_VertexBufferGPUSkin = new H1SkeletalMeshVertexBuffers();
        private H1SkeletalChunkVertexBufferSet m_VertexBufferGPUSkin = new H1SkeletalChunkVertexBufferSet();
        // mapping from final mesh vertex index to raw import vertex index
        private List<Int32> m_MeshToImportVertexMap;
    }
}
