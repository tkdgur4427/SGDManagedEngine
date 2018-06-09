using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    class H1VertexBufferContext
    {
        public List<Vector3> Buffer
        {
            get { return m_Buffer; }
        }

        private readonly List<Vector3> m_Buffer = new List<Vector3>();
    }

    public class H1Texcoord2DBufferContext
    {
        public List<Vector2> Buffer
        {
            get { return m_Buffer; }
        }

        private readonly List<Vector2> m_Buffer = new List<Vector2>();
    }

    public class H1Texcoord3DBufferContext
    {
        public List<Vector3> Buffer
        {
            get { return m_Buffer; }
        }

        private readonly List<Vector3> m_Buffer = new List<Vector3>();
    }

    class H1IndexBufferContext
    {
        public List<uint> Buffer
        {
            get { return m_Buffer; }
        }

        private readonly List<uint> m_Buffer = new List<uint>();
    }

    public class H1MeshContext
    {
        public String Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public List<Vector3> Positions
        {
            get { return m_Positions.Buffer; }
        }

        public List<Vector3> Normals
        {
            get { return m_Normals.Buffer; }
        }        

        public List<Vector3> BiTangents
        {
            get { return m_BiTangents.Buffer; }
        }

        public List<Vector3> Tangents
        {
            get { return m_Tangents.Buffer; }
        }

        public List<uint> Indices
        {
            get { return m_Indices.Buffer; }
        }

        public List<H1Texcoord3DBufferContext> UVWBuffers
        {
            get { return m_UVWBuffers; }
        }

        public List<H1Texcoord2DBufferContext> UVBuffers
        {
            get { return m_UVBuffers; }
        }

        public H1Transform LocalToGlobalTransform
        {
            get { return m_LocalToGlobalTransform; }
            set { m_LocalToGlobalTransform = value; }
        }

        public List<Vector2> AddTexcoord2DBufferContext()
        {
            H1Texcoord2DBufferContext newBuffer = new H1Texcoord2DBufferContext();
            m_UVBuffers.Add(newBuffer);

            return newBuffer.Buffer;
        }

        public List<Vector3> AddTexcoord3DBufferContext()
        {
            H1Texcoord3DBufferContext newBuffer = new H1Texcoord3DBufferContext();
            m_UVWBuffers.Add(newBuffer);

            return newBuffer.Buffer;
        }       

        private readonly H1VertexBufferContext m_Positions = new H1VertexBufferContext();
        private readonly H1VertexBufferContext m_Normals = new H1VertexBufferContext();
        private readonly H1VertexBufferContext m_BiTangents = new H1VertexBufferContext();
        private readonly H1VertexBufferContext m_Tangents = new H1VertexBufferContext();
        private readonly List<H1Texcoord3DBufferContext> m_UVWBuffers = new List<H1Texcoord3DBufferContext>();
        private readonly List<H1Texcoord2DBufferContext> m_UVBuffers = new List<H1Texcoord2DBufferContext>();
        private readonly H1IndexBufferContext m_Indices = new H1IndexBufferContext();
        private String m_Name;

        // this transform matrix is needed for converting all vertices resided in particular node space to root node space (world space)
        // this matrix is also used for transforming offset matrix to root node space not mesh node space
        private H1Transform m_LocalToGlobalTransform = new H1Transform();
    }

    public class H1AnimationContext
    {
        public class QuaternionKey
        {
            public double Time;
            public Quaternion Value;
        }

        public class PositionKey
        {
            public double Time;
            public Vector3 Value;
        }

        public class ScalingKey
        {
            public double Time;
            public Vector3 Value;
        }

        public class JointAnimation
        {
            public String BoneName;
            public List<QuaternionKey> RotKeys = new List<QuaternionKey>();
            public List<PositionKey> PosKeys = new List<PositionKey>();
            public List<ScalingKey> ScaleKeys = new List<ScalingKey>();            
        }

        public class AnimSequence
        {
            public String AnimSeqName;
            public List<JointAnimation> BoneAnimations = new List<JointAnimation>();
            public double Duration;
            public double TicksPerSecond;
        }

        public List<AnimSequence> AnimSequences = new List<AnimSequence>();
    }

    public class H1SkeletalContext
    {
        public JointNode Root
        {
            get { return m_RootJointNode; }
            set { m_RootJointNode = value; }
        }

        public List<JointNode> JointNodes
        {
            get { return m_JointNodes; }
        }

        public class WeightedVertex
        {
            public void CopyFrom(WeightedVertex vertex)
            {
                Weight = vertex.Weight;
                VertexIndex = vertex.VertexIndex;
            }

            public float Weight;
            public Int32 VertexIndex;
        }

        public class Transformation
        {
            // @TODO - remove this method!
            public void CopyFrom(Transformation transformation)
            {
                Translation = new Vector3(transformation.Translation.ToArray());
                Rotation = new Quaternion(transformation.Rotation.ToArray());
                Scaling = new Vector3(transformation.Scaling.ToArray());
            }

            public Vector3 Translation = new Vector3();
            public Quaternion Rotation = new Quaternion();
            public Vector3 Scaling = new Vector3(1);
        }

        public class Joint
        {
            // temporary value for indexing vertex properly from mesh context
            public Int32 MeshContextIndex = -1;
            // offset transformation for joint node currently resided in
            // - transforming mesh node space to bone space (bone local space)
            public Transformation OffsetTransformation = new Transformation();
            // list of vertex weighting (vertex blending for bone smooth transition)
            public List<WeightedVertex> WeightedVertices = new List<WeightedVertex>();

            // temporary usage to transform 'OffsetTransformation' to root node space (as following vertices transformation which is applied to reside in root node space)
            public H1Transform MeshContextLocalToGlobal = new H1Transform();
        }        

        public class JointNode
        {
            // same joint name and multiple joint data exists
            public String JointName;
            // same joint node name, but multiple Joint(data) exists            
            public List<Joint> JointDataList = new List<Joint>();            
            // local transform relative to parent node
            // - node local space is just scene graph node space NOT bone space (bone space info is stored in 'Joint.OffsetTransformation(mesh space to bone local space)'
            public Transformation NodeLocalTransform = new Transformation();
            // list of children
            public List<String> ChildNodeNames = new List<String>();
            public List<JointNode> Children = new List<JointNode>();
            // parent joint node
            public JointNode Parent = null;
            // to extract exact bone space, we need this marking flag for later process
            public Boolean MarkedAsBoneSpace = false;
        }

        private JointNode m_RootJointNode;
        private List<H1SkeletalContext.JointNode> m_JointNodes = new List<JointNode>();
    }    

    public class H1ModelContext
    {
        public List<H1MeshContext> Meshes
        {
            get { return m_Meshes; }
        }

        public List<H1SkeletalContext> SkeletalContexts
        {
            get { return m_SkeletalContexts; }
        }

        public H1AnimationContext AnimationContext
        {
            get { return m_AnimationContext; }
            set { m_AnimationContext = value; }
        }

        private List<H1MeshContext> m_Meshes = new List<H1MeshContext>();
        // if skeletal context exists, the number of m_SeletalContexts should be same to m_Meshes
        private List<H1SkeletalContext> m_SkeletalContexts = new List<H1SkeletalContext>();
        private H1AnimationContext m_AnimationContext; 
    }    

    public class H1AssetContext
    {
        public H1ModelContext AddModel()
        {
            H1ModelContext model = new H1ModelContext();
            m_Models.Add(model);

            return model;
        }

        public H1ModelContext GetModel(int index)
        {
            if (m_Models.Count > 0)
            {
                return m_Models[index];
            }

            return null;
        }

        private readonly List<H1ModelContext> m_Models = new List<H1ModelContext>();
    }
}
