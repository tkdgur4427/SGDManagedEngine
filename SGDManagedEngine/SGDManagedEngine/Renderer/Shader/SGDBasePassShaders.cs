using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1BasePassVertexShader : H1MeshMaterialShader
    {
        // default values (for 'null') is only for triggering generations for mesh material shader types! , don't use default constructor!
        public H1BasePassVertexShader(byte[] compiledByteCode = null, H1ShaderParameterMap shaderParameterMap = null, Boolean isCreateShaderType = false)
            : base(H1Global<H1ManagedRenderer>.Instance.ShaderManager.CacheShaderType(
                new H1MeshMaterialShaderType("H1BasePassVertexShader", "SGDBasePassVS.hlsl", "Main", new H1ShaderTarget() { Freqency = H1ShaderFrequency.VertexShader, Platform = H1ShaderPlatform.SM5_1}, ModifyCompilationEnvironment, ShouldCache)), 
                compiledByteCode,
                shaderParameterMap,
                isCreateShaderType)
        {

        }

        public static void ModifyCompilationEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {

        }

        public static bool ShouldCache(H1MaterialResource material, H1VertexFactoryType vertexFactoryType)
        {
            return true;
        }
    }

    public class H1BasePassPixelShader : H1MeshMaterialShader
    {
        // default values (for 'null') is only for triggering generations for mesh material shader types! , don't use default constructor!
        public H1BasePassPixelShader(byte[] compiledByteCode = null, H1ShaderParameterMap shaderParameterMap = null, Boolean isCreateShaderType = false)
            : base(H1Global<H1ManagedRenderer>.Instance.ShaderManager.CacheShaderType(
                new H1MeshMaterialShaderType("H1BasePassPixelShader", "SGDBasePassPS.hlsl", "Main", new H1ShaderTarget() { Freqency = H1ShaderFrequency.PixelShader, Platform = H1ShaderPlatform.SM5_1}, ModifyCompilationEnvironment, ShouldCache)), 
                compiledByteCode,
                shaderParameterMap,
                isCreateShaderType)
        {

        }

        public static void ModifyCompilationEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {

        }

        public static bool ShouldCache(H1MaterialResource material, H1VertexFactoryType vertexFactoryType)
        {
            return true;
        }
    }
}
