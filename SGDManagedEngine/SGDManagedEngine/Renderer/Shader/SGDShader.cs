using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{     
    public class H1Shader
    {               
        public H1ShaderId ShaderId
        {
            get { return m_Id; }
        }

        public H1ShaderType ShaderType
        {
            get { return m_ShaderTypeRef; }
        }

        public byte[] ShaderByteCode
        {
            get { return m_DX12Shader.ShaderByteCode; }
        }
                
        public H1Shader(H1ShaderType type, byte[] compiledByteCode, H1ShaderParameterMap shaderParameterMap, Boolean isCreateShaderType = false)
        {
            if (isCreateShaderType)
                return; // triggered by just shader type creation, skip it

            // assign new H1ShaderId
            m_Id = new H1ShaderId();

            m_CompiledByteCode = compiledByteCode;
            m_ShaderTypeRef = type;                        
            m_ParameterMap = shaderParameterMap;

            Direct3D12.H1ResourceRanges resourceRanges = new Direct3D12.H1ResourceRanges();
            foreach (KeyValuePair<String, H1ParameterAllocation> parameter in shaderParameterMap.ParameterMap)
            {
                String parameterName = parameter.Key;
                H1ParameterAllocation allocation = parameter.Value;

                // 1. constant buffer
                if (allocation.Type == H1ParameterType.ConstantBuffer)
                {
                    // insert the new range to the constant buffers
                    Direct3D12.H1BindRanges.H1Range range = new Direct3D12.H1BindRanges.H1Range();
                    range.Start = Convert.ToByte(allocation.BufferIndex);
                    range.Length = Convert.ToByte(1);
                    resourceRanges.ConstantBuffers.AddRange(range);
                    continue;
                }         

                // handling textures, samplers,...
            }

            // 2. create new shader instance
            m_DX12Shader = new Direct3D12.H1Shader(compiledByteCode, resourceRanges);
        }

        public override bool Equals(object obj)
        {
            H1Shader shader = obj as H1Shader;
            if (shader == null)
                return false;

            if (this.m_Id != shader.m_Id) // comparing ids
                return false;

            if (this.m_ShaderTypeRef != shader.m_ShaderTypeRef) // comparing shader type
                return false;

            if (!(this.m_CompiledByteCode.SequenceEqual(shader.m_CompiledByteCode))) // comparing code bytes
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            // use default hash generated code
            return base.GetHashCode();
        }
                
        private H1ShaderId m_Id;
        private H1ShaderType m_ShaderTypeRef;
        private byte[] m_CompiledByteCode;
        private H1ShaderParameterMap m_ParameterMap;

        // new implementation
        private Direct3D12.H1Shader m_DX12Shader;
    }

    public class H1GlobalShader : H1Shader
    {       
        public H1GlobalShader(H1ShaderType type, byte[] compiledByteCode, H1ShaderParameterMap shaderParameterMap)
            : base(type, compiledByteCode, shaderParameterMap)
        {

        }  
    }

    // base class of all shaders that need material parameters
    public class H1MaterialShader : H1Shader
    {
        public H1MaterialShader(H1ShaderType type, byte[] compiledByteCode, H1ShaderParameterMap shaderParameterMap, Boolean isCreateShaderType = false)
            : base(type, compiledByteCode, shaderParameterMap, isCreateShaderType)
        {

        }

        // material parameters
        // ....
    }

    public class H1MeshMaterialShader : H1MaterialShader
    {
        public H1MeshMaterialShader(H1ShaderType type, byte[] compiledByteCode, H1ShaderParameterMap shaderParameterMap, Boolean isCreateShaderType = false)
            : base(type, compiledByteCode, shaderParameterMap, isCreateShaderType)
        {

        }

        // vertex factory shader parameter
        // ... (ex. flocalvertexfactoryparameter, fgpuskinvertexfactoryshaderparameter, ...)
    }

    public class H1ShaderMap<ShaderMetaType> where ShaderMetaType : H1ShaderType
    {
        public Dictionary<H1ShaderType, H1Shader> Shaders
        {
            get { return m_Shaders; }
        }

        public void AddShader(H1ShaderType shaderType, H1Shader shader)
        {
            m_Shaders.Add(shaderType, shader);
        }

        // @TODO - we need more structured Id class for shader-map
        private int m_ShaderMapId;
        private readonly Dictionary<H1ShaderType, H1Shader> m_Shaders = new Dictionary<H1ShaderType, H1Shader>();
    }    
}
