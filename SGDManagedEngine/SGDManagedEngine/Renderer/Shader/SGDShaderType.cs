using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{         
    // abstract H1ShaderType - the inherited this class instances has shader compilation method 
    public abstract class H1ShaderType
    {      
        public String Name
        {
            get { return m_Name; }
        }

        public String SourceFileName
        {
            get { return m_SourceFileName; }
        }

        public String FunctionName
        {
            get { return m_FunctionName; }
        }

        public H1ShaderTarget ShaderTarget
        {
            get { return m_ShaderTarget; }
        }

        public Dictionary<H1ShaderId, H1Shader> ShaderIdMap
        {
            get { return m_ShaderIdMap; }
        }            

        public H1ShaderType(String name, String fileName, String functionName, H1ShaderTarget target)
        {
            m_Name = name;
            m_SourceFileName = fileName;
            m_FunctionName = functionName;
            m_ShaderTarget = target;            
        }

        public static H1ShaderType[] GetTypeList()
        {
            return H1Global<H1ManagedRenderer>.Instance.ShaderManager.ShaderTypes;
        }
        
        public virtual H1Shader FinishCompileShader(H1ShaderCompileHelper.H1ShaderCompileOutput compiledOutput)
        {
            return null;
        }
        
        public virtual H1MeshMaterialShaderType GetMeshMaterialShaderType()
        {
            return null;
        }

        public virtual H1MaterialShaderType GetMaterialShaderType()
        {
            return null;
        }

        public virtual H1GlobalShaderType GetGlobalShaderType()
        {
            return null;
        }

        private String m_Name;
        private String m_SourceFileName;
        private String m_FunctionName;
        private H1ShaderTarget m_ShaderTarget;        
        private readonly Dictionary<H1ShaderId, H1Shader> m_ShaderIdMap = new Dictionary<H1ShaderId, H1Shader>();
    }

    public class H1GlobalShaderType : H1ShaderType
    {
        public delegate void ModifyCompilationEnvironmentDelegate(H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment);

        public H1GlobalShaderType(String name, String fileName, String functionName, H1ShaderTarget target,
            ModifyCompilationEnvironmentDelegate modifyCompilationEnvironmentRef)
            : base(name, fileName, functionName, target)
        {
            m_ModifyCompilationEnvironmentRef = modifyCompilationEnvironmentRef;
        }

        public void SetupEnvironment(H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {
            m_ModifyCompilationEnvironmentRef(outEnvironment);
        }

        public void BeginCompileShader()
        {           
            // process shared/single environment 
            H1ShaderCompileHelper.H1ShaderCompilerEnvironment environment = new H1ShaderCompileHelper.H1ShaderCompilerEnvironment();
            H1ShaderCompileHelper.H1ShaderCompilerEnvironment sharedEnvironment = new H1ShaderCompileHelper.H1ShaderCompilerEnvironment();
            SetupEnvironment(sharedEnvironment);

            // create shader compile input
            H1ShaderCompileHelper.H1ShaderCompileInput input = new H1ShaderCompileHelper.H1ShaderCompileInput();
            input.SourceFileName = SourceFileName;
            input.EntryPointName = FunctionName;
            input.Environment = environment;
            input.SharedEnvironment = sharedEnvironment;
            input.Target = ShaderTarget;

            // compile
            H1Global<H1ManagedRenderer>.Instance.ShaderManager.CompileShader(m_GlobalName, null, this, input);
        }

        public override H1Shader FinishCompileShader(H1ShaderCompileHelper.H1ShaderCompileOutput compiledOutput)
        {
            // based on output, generate shader instance
            H1GlobalShader globalShader = new H1GlobalShader(this, compiledOutput.Code, compiledOutput.ParameterMap);

            // add the created shader to the shader map
            ShaderIdMap.Add(globalShader.ShaderId, globalShader);

            return globalShader;
        }

        public override H1GlobalShaderType GetGlobalShaderType()
        {
            return this;
        }

        private readonly String m_GlobalName = "Global";
        private ModifyCompilationEnvironmentDelegate m_ModifyCompilationEnvironmentRef;
    }

    public class H1MeshMaterialShaderType : H1ShaderType
    {
        public delegate void ModifyCompilationEnvironmentDelegate(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment);
        public delegate bool ShouldCacheDelegate(H1MaterialResource material, H1VertexFactoryType vertexFactoryType);        

        public H1MeshMaterialShaderType(String name, String fileName, String functionName, H1ShaderTarget target, ModifyCompilationEnvironmentDelegate modifyCompilationEnvironmentRef,
            ShouldCacheDelegate shouldCacheRef)
            : base(name, fileName, functionName, target)
        {
            m_ModifyCompilationEnvironmentRef = modifyCompilationEnvironmentRef;
            m_ShouldCacheRef = shouldCacheRef;
        }

        public void SetupEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {
            m_ModifyCompilationEnvironmentRef(material, outEnvironment);
        }

        public bool ShouldCache(H1MaterialResource material, H1VertexFactoryType vertexFactoryType)
        {
            return m_ShouldCacheRef(material, vertexFactoryType);
        }

        public H1Shader BeginCompileShader(uint shaderMapId, H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment materialEnvironment, H1VertexFactoryType vertexFactoryType)
        {
            H1ShaderCompileHelper.H1ShaderCompilerEnvironment environment = new H1ShaderCompileHelper.H1ShaderCompilerEnvironment();
            H1ShaderCompileHelper.H1ShaderCompilerEnvironment sharedEnvironment = H1ObjectCopier.Clone(materialEnvironment);            

            // set the environment by 'vertex factory type'
            vertexFactoryType.ModifyCompilationEnvironment(material, sharedEnvironment);

            // modify the shader type by the current compile environment
            SetupEnvironment(material, sharedEnvironment);

            // create shader compile input
            H1ShaderCompileHelper.H1ShaderCompileInput input = new H1ShaderCompileHelper.H1ShaderCompileInput();
            input.SourceFileName = SourceFileName;
            input.EntryPointName = FunctionName;
            input.Environment = environment;
            input.SharedEnvironment = sharedEnvironment;
            input.Target = ShaderTarget;               

            // compile shader
            return H1Global<H1ManagedRenderer>.Instance.ShaderManager.CompileShader(material.FriendlyName, null, this, input);
        }

        public override H1Shader FinishCompileShader(H1ShaderCompileHelper.H1ShaderCompileOutput compiledOutput)
        {
            H1MeshMaterialShader materialShader = new H1MeshMaterialShader(this, compiledOutput.Code, compiledOutput.ParameterMap);

            // add the shader to shaderIdMap
            ShaderIdMap.Add(materialShader.ShaderId, materialShader);

            return materialShader;
        }

        public override H1MeshMaterialShaderType GetMeshMaterialShaderType()
        {
            return this;
        }

        private ModifyCompilationEnvironmentDelegate m_ModifyCompilationEnvironmentRef;
        private ShouldCacheDelegate m_ShouldCacheRef;
    }

    public class H1MaterialShaderType : H1ShaderType
    {
        public delegate void ModifyCompilationEnvironmentDelegate(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment);
        public delegate bool ShouldCacheDelegate(H1MaterialResource material);

        public H1MaterialShaderType(String name, String fileName, String functionName, H1ShaderTarget target, ModifyCompilationEnvironmentDelegate modifyCompilationEnvironmentRef,
            ShouldCacheDelegate shouldCacheRef)
            : base(name, fileName, functionName, target)
        {
            m_ModifyCompilationEnvironmentRef = modifyCompilationEnvironmentRef;
            m_ShouldCacheRef = shouldCacheRef;
        }

        public void SetupEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {
            m_ModifyCompilationEnvironmentRef(material, outEnvironment);
        }

        public bool ShouldCache(H1MaterialResource material)
        {
            return m_ShouldCacheRef(material);
        }

        public H1Shader BeginCompileShader(int shaderMapId, H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment materialEnvironment)
        {
            H1ShaderCompileHelper.H1ShaderCompilerEnvironment environment = new H1ShaderCompileHelper.H1ShaderCompilerEnvironment();
            H1ShaderCompileHelper.H1ShaderCompilerEnvironment sharedEnvironment = materialEnvironment;

            // set input environment
            SetupEnvironment(material, sharedEnvironment);

            // create shader compile input
            H1ShaderCompileHelper.H1ShaderCompileInput input = new H1ShaderCompileHelper.H1ShaderCompileInput();
            input.SourceFileName = SourceFileName;
            input.EntryPointName = FunctionName;
            input.Environment = environment;
            input.SharedEnvironment = sharedEnvironment;
            input.Target = ShaderTarget;

            // begin compile shader without vertex factory type
            return H1Global<H1ManagedRenderer>.Instance.ShaderManager.CompileShader(material.FriendlyName, null, this, input);
        }

        // this method is called by H1ShaderManager (after successfully compiling a shader)
        public override H1Shader FinishCompileShader(H1ShaderCompileHelper.H1ShaderCompileOutput compiledOutput)
        {
            H1MaterialShader materialShader = new H1MaterialShader(this, compiledOutput.Code, compiledOutput.ParameterMap);

            // add the shader to shaderIdMap
            ShaderIdMap.Add(materialShader.ShaderId, materialShader);

            return materialShader;
        }

        public override H1MaterialShaderType GetMaterialShaderType()
        {
            return this;
        }

        private ModifyCompilationEnvironmentDelegate m_ModifyCompilationEnvironmentRef;
        private ShouldCacheDelegate m_ShouldCacheRef;
    }
}
