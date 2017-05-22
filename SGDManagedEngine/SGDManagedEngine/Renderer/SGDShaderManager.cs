using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{  
    // the only manager handling all instances for shader objects
    public class H1ShaderManager
    {
        public String ShaderDirectory
        {
            get { return m_ShaderDirectory; }
        }

        public H1ShaderType[] ShaderTypes
        {
            get { return m_ShaderTypes.Values.ToArray(); }
        }
        
        public H1VertexFactoryType[] VertexFactoryTypes
        {
            get { return m_VertexFactoryTypes.Values.ToArray(); }
        }

        public void Initialize(H1ManagedRenderer renderer)
        {
            // cache all vertex factory types and shader types
            CacheAllVertexFactoryTypesAndShaderTypes();

            m_RendererRef = renderer;

            // set the directory
            // @TODO - temporary set the directory, in the future , I need to change it appropriately
            m_ShaderDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Shaders\\");
        }

        public void Destroy()
        {
            
        }

        private void RegisterShaderType(String name, H1ShaderType shaderType)
        {
            m_ShaderTypes.Add(name, shaderType);
        }

        private void RegisterVertexFactoryType(String name, H1VertexFactoryType vertexFactoryType)
        {
            m_VertexFactoryTypes.Add(name, vertexFactoryType);
        }

        //@TODO - I need to think about how to solve this problem!
        private static void CacheAllVertexFactoryTypesAndShaderTypes()
        {
            // 1. vertex factory types
            new H1LocalVertexFactory();
            new H1GpuSkinVertexFactory();

            // 2. mesh material shader types
            new H1BasePassVertexShader(null, null, true);
            new H1BasePassPixelShader(null, null, true);
        }

        public H1ShaderType CacheShaderType(H1ShaderType type)
        {
            if (!m_ShaderTypes.ContainsKey(type.Name))
            {
                RegisterShaderType(type.Name, type);
            }

            return m_ShaderTypes[type.Name];
        }

        public H1VertexFactoryType CacheVertexFactoryType(H1VertexFactoryType type)
        {
            if (!m_VertexFactoryTypes.ContainsKey(type.Name))
            {                
                RegisterVertexFactoryType(type.Name, type);
            }

            return m_VertexFactoryTypes[type.Name];
        }        
                
        public H1Shader CompileShader(String debugGroupName, H1VertexFactoryType vertexFactoryType, H1ShaderType shaderType, H1ShaderCompileHelper.H1ShaderCompileInput input)
        {           
            // compile the shader
            H1ShaderCompileHelper.H1ShaderCompileOutput shaderCompileOutput = new H1ShaderCompileHelper.H1ShaderCompileOutput();
            H1ShaderCompileHelper.CompileD3D12Shader(input, shaderCompileOutput);

            if (!shaderCompileOutput.IsSucceed)
            {
                throw new Exception("failed to compile the shader! please check!");
            }

            H1Shader outShader = null;
            if (shaderType != null)
            {
                // create shader instance
                outShader = shaderType.FinishCompileShader(shaderCompileOutput); 

                if (shaderType is H1GlobalShaderType) // only for global shader, add global shaders
                {
                    m_GlobalShaders.Add(outShader);
                }
                else // otherwise add local shaders
                {
                    m_LocalShaders.Add(outShader);
                }
            }

            return outShader;
        }

        public H1ShaderType GetShaderType(String name)
        {
            return m_ShaderTypes[name];
        }

        public H1VertexFactoryType GetVertexFactoryType(String name)
        {
            return m_VertexFactoryTypes[name];
        }

        // manages all local shader's instances
        private readonly List<H1Shader> m_LocalShaders = new List<H1Shader>();
        // manages all global shader's instances
        private readonly List<H1Shader> m_GlobalShaders = new List<H1Shader>();

        // manage all shader types and vertex factory types
        private readonly Dictionary<String, H1ShaderType> m_ShaderTypes = new Dictionary<String, H1ShaderType>();
        private readonly Dictionary<String, H1VertexFactoryType> m_VertexFactoryTypes = new Dictionary<String, H1VertexFactoryType>();

        // directory for shader files
        private String m_ShaderDirectory;

        private H1ManagedRenderer m_RendererRef;
    }
}
