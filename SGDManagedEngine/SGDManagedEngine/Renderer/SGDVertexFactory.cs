using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{  
    public class H1VertexStream
    {
        public String Name
        {
            get { return m_Name; }
        }

        public String SemanticName
        {
            get { return m_SemanticName; }
        }

        public int SemanticIndex
        {
            get { return m_SemanticIndex; }
        }

        public H1VertexElementType VertexElementType
        {
            get { return m_ElementType; }
        }

        public uint Stride
        {
            get { return m_Stride; }
        }

        public uint Offset
        {
            get { return m_Offset; }
        }

        public H1VertexStream(String name, String semanticName, int semanticIndex, H1VertexElementType elementType, int stride, int offset, H1VertexBuffer vertexBufferRef)
        {
            m_Name = name;
            m_SemanticName = semanticName;
            m_SemanticIndex = semanticIndex;
            m_ElementType = elementType;
            m_Stride = Convert.ToUInt32(stride);
            m_Offset = Convert.ToUInt32(offset);
            m_VertexBufferRef = vertexBufferRef;
        }

        private readonly String m_Name;
        private readonly String m_SemanticName;
        private readonly int m_SemanticIndex;
        private readonly H1VertexElementType m_ElementType;
        
        private readonly uint m_Stride;
        private readonly uint m_Offset; // offset to the vertex buffer (we could have one big vertex buffer having different offsets)
        private readonly H1VertexBuffer m_VertexBufferRef;
    }

    public class H1VertexFactoryType
    {       
        public String Name
        {
            get { return m_Name; }
        }

        public H1VertexFactoryType(String name, String shaderFileName, H1VertexFactory.ModifyCompilationEnvironmentDelegate modifyCompilationEnvironmentRef,
            H1VertexFactory.ShouldCacheDelegate shouldCacheRef)
        {
            m_Name = name;
            m_ShaderFileName = shaderFileName;
            m_ModifyCompilationEnvironmentRef = modifyCompilationEnvironmentRef;
            m_ShouldCacheRef = shouldCacheRef;                  
        }

        public static H1VertexFactoryType[] GetTypeList()
        {
            return H1Global<H1ManagedRenderer>.Instance.ShaderManager.VertexFactoryTypes.ToArray();
        }

        public void ModifyCompilationEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {
            String vertexfactoryIncludeString = String.Format("#include \"{0}\"", m_ShaderFileName);
            outEnvironment.SetIncludeFileContent("VertexFactory.hlsl", vertexfactoryIncludeString);

            // call delegate ModifyCompilationEnvironmentRef
            m_ModifyCompilationEnvironmentRef(material, outEnvironment);
        }

        public bool ShouldCache(H1MaterialResource material, H1ShaderType shaderType)
        {
            return m_ShouldCacheRef(material, shaderType);
        }

        private String m_Name;
        private String m_ShaderFileName;        
        private H1VertexFactory.ModifyCompilationEnvironmentDelegate m_ModifyCompilationEnvironmentRef;
        private H1VertexFactory.ShouldCacheDelegate m_ShouldCacheRef;
    }

    public class H1VertexFactory
    {
        // delegates (function pointers)
        public delegate void ModifyCompilationEnvironmentDelegate(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment);
        public delegate bool ShouldCacheDelegate(H1MaterialResource material, H1ShaderType shaderType);

        public H1VertexFactory(H1VertexFactoryType type)
        {
            m_TypeRef = type;
        }

        public H1VertexFactoryType VertexFactoryType
        {
            get { return m_TypeRef; }
        }

        public H1VertexDeclaration VertexDeclaration
        {
            get { return m_Declaration; }
        }

        public void AddVertexStream(H1VertexStream stream)
        {
            m_Streams.Add(stream);
        }

        protected bool GenerateVertexDeclaration()
        {
            if (m_Streams.Count == 0)
                return false;

            m_Declaration = new H1VertexDeclaration(m_Streams.ToArray());
            return true;
        }

        public static void ModifyCompilationEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {
            
        }

        public static bool ShouldCache(H1MaterialResource material, H1ShaderType shaderType)
        {
            return true;
        }

        private readonly List<H1VertexStream> m_Streams = new List<H1VertexStream>();
        private H1VertexDeclaration m_Declaration;
        private H1VertexFactoryType m_TypeRef;
    } 
}
