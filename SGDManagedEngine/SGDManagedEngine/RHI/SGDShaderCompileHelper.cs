using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using SharpDX;
using SharpDX.D3DCompiler;

namespace SGDManagedEngine.SGD
{    
    public class H1ShaderCompileHelper
    {        
        public class H1ShaderCompilerEnvironment : H1DeepCopyable
        {  
            public String[] IncludeFileContentMap
            {
                get { return m_IncludeFileNameToContentMap.Values.ToArray(); }
            }

            public Dictionary<String, String> Definitions
            {
                get { return m_Definitions;}
            }

            public void SetIncludeFileContent(String fileName, String content)
            {
                m_IncludeFileNameToContentMap.Add(fileName, content);
            }
                              
            public void SetDefine<T>(String name, T value) where T : struct
            {
                m_Definitions.Add(name, value.ToString());
            }

            public override object DeepCopy()
            {
                var newObj = new H1ShaderCompilerEnvironment();
                newObj = this.MemberwiseClone() as H1ShaderCompilerEnvironment;

                // 1. definitions
                newObj.m_Definitions.Clear(); // making sure empty (for preventing shallow copy by memberwiseclone
                foreach (var element in this.m_Definitions)
                {
                    newObj.m_Definitions.Add(element.Key, element.Value);
                }

                // 2. include mapping
                newObj.m_IncludeFileNameToContentMap.Clear();
                foreach (var element in this.m_IncludeFileNameToContentMap)
                {
                    newObj.m_IncludeFileNameToContentMap.Add(element.Key, element.Value);
                }

                return newObj;
            }

            // environment like definition macro and various includes
            private readonly Dictionary<String, String> m_IncludeFileNameToContentMap = new Dictionary<String, String>();
            private readonly Dictionary<String, String> m_Definitions = new Dictionary<String, String>();
        }

        public class H1ShaderCompileInput
        {
            public H1ShaderTarget Target { get; set; }
            public String SourceFileName { get; set; }
            public String EntryPointName { get; set; }
            public H1ShaderCompilerEnvironment Environment { get; set; }
            public H1ShaderCompilerEnvironment SharedEnvironment { get; set; }
        }

        public class H1ShaderCompileOutput
        {
            public H1ShaderParameterMap ParameterMap { get; set; }
            public ShaderBytecode Code { get; set; }
            public bool IsSucceed { get; set; }
        }

        private static String LoadShaderFile(String shaderFileName)
        {
            String shaderDirectory = H1Global<H1ManagedRenderer>.Instance.ShaderManager.ShaderDirectory;
            String shaderPath = shaderDirectory + shaderFileName;

            // researched for average fastest read text is 'System.IO.File.ReadXXX' - from http://cc.davelozinski.com/c-sharp/fastest-way-to-read-text-files
            String content = System.IO.File.ReadAllText(shaderPath);

            return content;
        }

        private static void ProcessShaderCompilerEnvironment(H1ShaderCompilerEnvironment environment, ref String outContent, List<SharpDX.Direct3D.ShaderMacro> outMacros)
        {
            // 1. process include files to the 'outContent'            
            foreach (String content in environment.IncludeFileContentMap)
            {
                outContent += content;
            }

            // 2. process macros            
            foreach (var definition in environment.Definitions)
            {
                outMacros.Add(new SharpDX.Direct3D.ShaderMacro(definition.Key, definition.Value));
            }            
        }

        class H1SharpDXCompileInclude : Include
        {
            public IDisposable Shadow
            {
                get; set;
            }

            public void Close(Stream stream)
            {
                stream.Close();
                stream.Dispose();
            }

            public void Dispose()
            {
                
            }

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {                
                if (type == IncludeType.Local)
                {
                    // get the shader directory
                    String shaderDirectory = H1Global<H1ManagedRenderer>.Instance.ShaderManager.ShaderDirectory;
                    return new FileStream(shaderDirectory + fileName, FileMode.Open);
                }

                else if (type == IncludeType.System)
                {
                    throw new Exception("include type is invalid (as 'System') please check");                    
                }

                return null;
            }
        }

        public static void CompileD3D12Shader(H1ShaderCompileInput input, H1ShaderCompileOutput output)
        {
            // process shared/single environments
            String includeContent = "";
            List<SharpDX.Direct3D.ShaderMacro> macros = new List<SharpDX.Direct3D.ShaderMacro>();

            // 1. shared environment
            ProcessShaderCompilerEnvironment(input.SharedEnvironment, ref includeContent, macros);

            // 2. single environment
            ProcessShaderCompilerEnvironment(input.Environment, ref includeContent, macros);

            // load shader file content
            String sourceContent = includeContent + "\n" + LoadShaderFile(input.SourceFileName);

            // preprocess the shader file
            sourceContent = ShaderBytecode.Preprocess(sourceContent, macros.ToArray(), new H1SharpDXCompileInclude());
#if DEBUG
            var shader = ShaderBytecode.Compile(sourceContent, input.EntryPointName, input.Target.ToFormat, SharpDX.D3DCompiler.ShaderFlags.Debug | SharpDX.D3DCompiler.ShaderFlags.SkipOptimization);
#else
            var shader = ShaderBytecode.Compile(sourceContent, input.EntryPointName, input.Target.ToFormat);
#endif
            if (shader.Message != null) // failed to compile the shader
            {
                // @TODO - should log the error for failing compiling shader
                output.IsSucceed = false;
                return;
            }

            // assign the resultant byte code
            output.Code = shader;
            // create shader parameter map
            output.ParameterMap = new H1ShaderParameterMap();

            // reflection for the compiled shader
            ShaderReflection shaderReflect = new ShaderReflection(shader);
            ShaderDescription shaderDesc = shaderReflect.Description;

            int bindResCounts = shaderDesc.BoundResources;
            for (int resIdx = 0; resIdx < bindResCounts; ++resIdx)
            {
                InputBindingDescription bindDesc = shaderReflect.GetResourceBindingDescription(resIdx);

                // for constant buffers
                if (bindDesc.Type == ShaderInputType.ConstantBuffer)
                {
                    int cbIndex = bindDesc.BindPoint;
                    ConstantBuffer cb = shaderReflect.GetConstantBuffer(cbIndex);

                    ConstantBufferDescription cbDesc;
                    cbDesc = cb.Description;

                    // track all variables in this constant buffer
                    for (int varIdx = 0; varIdx < cbDesc.VariableCount; varIdx++)
                    {
                        ShaderReflectionVariable variable = cb.GetVariable(varIdx);
                        ShaderVariableDescription variableDesc = variable.Description;

                        output.ParameterMap.ParameterMap.Add(variableDesc.Name,
                            new H1ParameterAllocation(H1ParameterType.Variable, cbIndex, variableDesc.StartOffset, variableDesc.Size));
                    }

                    // add constant buffer parameter
                    output.ParameterMap.ParameterMap.Add(cbDesc.Name,
                        new H1ParameterAllocation(H1ParameterType.ConstantBuffer, cbIndex, -1, cbDesc.Size));
                }

                // texture, samplers .... other various GDI data
            }

            // release shader reflection
            shaderReflect.Dispose();
            output.IsSucceed = true; // successfully compiled            
        }
    }
}
