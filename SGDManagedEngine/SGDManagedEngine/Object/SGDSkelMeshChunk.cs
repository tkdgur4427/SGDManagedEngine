using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1SoftSkinVertex
    {
        public Vector3 Position;
        public Vector3 TangentX;
        public Vector3 TangentY;
        public Vector3 TangentZ;
        public Vector2[] UVs = new Vector2[H1ObjectGlobalDefinitions.MAX_TEXCOORDS]; // MAX_TEXCOORDS
        public Color4 Color;
        public Byte[] InfluenceBones = new Byte[H1ObjectGlobalDefinitions.MAX_TOTAL_INFLUENCES];
        public Byte[] InfluenceWeights = new Byte[H1ObjectGlobalDefinitions.MAX_TOTAL_INFLUENCES];
    }

    public class H1SkelMeshChunk
    {
        public List<H1SoftSkinVertex> SoftVertices
        {
            get { return m_SoftVertices; }
        }

        public List<UInt16> BoneMap
        {
            get { return m_BoneMap; }
        }

        public UInt32 BaseVertexIndex
        {
            get { return m_BaseVertexIndex; }
            set { m_BaseVertexIndex = value; }
        }

        public Int32 MaxBoneInfluences
        {
            get { return m_MaxBoneInfluences; }
            set { m_MaxBoneInfluences = value; }
        }
        
        // the offset into the LOD's vertex buffer of this chunk's vertices
        private UInt32 m_BaseVertexIndex;
        // the rigid vertices of this chunks
        //private List<H1RigidSkinVertex>
        // the soft vertices of this chunk
        private List<H1SoftSkinVertex> m_SoftVertices = new List<H1SoftSkinVertex>();
        // the bones which are used by the vertices of this chunk
        private List<UInt16> m_BoneMap = new List<UInt16>();
        private Int32 m_MaxBoneInfluences;
    }
}
