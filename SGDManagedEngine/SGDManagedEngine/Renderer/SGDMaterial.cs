using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    // vertex factory related material map (like vertex shader)
    public class H1MeshMaterialShaderMap : H1ShaderMap<H1MeshMaterialShaderType>
    {
        public H1VertexFactoryType VertexFactoryType
        {
            get { return m_VertexFactoryTypeRef; }
        }            

        public H1MeshMaterialShaderMap(H1VertexFactoryType type)
        {
            m_VertexFactoryTypeRef = type;
        }

        // note that - for this 'BeginCompile' call is called by H1MaterialMap usually!
        public void BeginCompile(uint shaderMapId, H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment materialEnvironment)
        {
            // iterating shader type
            var shaderTypes = H1ShaderType.GetTypeList();
            foreach (var shaderType in shaderTypes)
            {
                H1MeshMaterialShaderType meshMaterialShaderType = shaderType.GetMeshMaterialShaderType();
                if (meshMaterialShaderType != null && 
                    m_VertexFactoryTypeRef != null && 
                    meshMaterialShaderType.ShouldCache(material, m_VertexFactoryTypeRef) &&
                    material.ShouldCache(meshMaterialShaderType, m_VertexFactoryTypeRef) &&
                    m_VertexFactoryTypeRef.ShouldCache(material, meshMaterialShaderType))
                {
                    // compile mesh material shader type (ex. vertex shader)
                    H1Shader shader = meshMaterialShaderType.BeginCompileShader(shaderMapId, material, materialEnvironment, m_VertexFactoryTypeRef);

                    // add shader to shader map
                    AddShader(meshMaterialShaderType, shader);
                }
            }
        }

        private H1VertexFactoryType m_VertexFactoryTypeRef;
    }

    // material map independent of vertex factory (but connected to the H1MeshMaterialMap)
    class H1MaterialShaderMap : H1ShaderMap<H1MaterialShaderType>
    {
        public H1MaterialShaderMap()
        {

        }

        public void Compile(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment materialEnvironment)
        {
            // iterate over all vertex factory types
            var vertexFactoryTypes = H1VertexFactoryType.GetTypeList();
            foreach (var vertexFactoryType in vertexFactoryTypes)
            {
                H1MeshMaterialShaderMap meshMaterialMap = null;

                // look for existing map for this vertex factory map
                foreach (var shaderMap in m_MeshMaterialMaps)
                {
                    if (shaderMap.VertexFactoryType == vertexFactoryType)
                    {
                        meshMaterialMap = shaderMap;
                        break;
                    }
                }

                if (meshMaterialMap == null)
                {
                    // create a new mesh material shader map
                    meshMaterialMap = new H1MeshMaterialShaderMap(vertexFactoryType);
                    m_MeshMaterialMaps.Add(meshMaterialMap);
                }

                // compile mesh material map
                meshMaterialMap.BeginCompile(0, // @TODO - I need to change this appropriately!
                    material, materialEnvironment);
            }

            // iterate over all material shader types
            var shaderTypes = H1ShaderType.GetTypeList();
            foreach (var shaderType in shaderTypes)
            {
                H1MaterialShaderType materialShaderType = shaderType.GetMaterialShaderType();
                if (materialShaderType != null &&
                    materialShaderType.ShouldCache(material) &&
                    material.ShouldCache(materialShaderType, null))
                {
                    materialShaderType.BeginCompileShader(0, // @TODO - I need to change this appropriately! 
                        material, materialEnvironment);
                }
            }
        }

        public H1MeshMaterialShaderMap GetMeshShaderMap(H1VertexFactoryType vertexFactoryType)
        {
            foreach (H1MeshMaterialShaderMap meshMaterialShaderMap in m_MeshMaterialMaps)
            {
                if (meshMaterialShaderMap.VertexFactoryType == vertexFactoryType)
                    return meshMaterialShaderMap;
            }

            return null;
        }

        private readonly List<H1MeshMaterialShaderMap> m_MeshMaterialMaps = new List<H1MeshMaterialShaderMap>();        
    }

    // material resource
    public class H1MaterialResource
    {
        public String FriendlyName
        {
            // @TODO - implement this!
            get { return m_FriendlyName; }
        }

        public bool DrawWithColor
        {
            get
            {
                // if DrawWithMaterial is on, return false
                return (m_DrawWithColor) && !(m_DrawWithMaterial);
            }
        }

        public H1MaterialResource(String friendlyName)
        {
            m_FriendlyName = friendlyName;
        }

        public bool BeginCompileShaderMap()
        {
            H1MaterialShaderMap newMaterialShaderMap = new H1MaterialShaderMap();

            // generate the material shader code
            // @TODO - H1HLSLMaterialTranslator

            // create a shader compiler environment for the material
            H1ShaderCompileHelper.H1ShaderCompilerEnvironment materialEnvironment = new H1ShaderCompileHelper.H1ShaderCompilerEnvironment();

            // H1HLSLMaterialTranslator get environment and shader code
            String materialShaderCode = ""; // @TODO - material shader code
            materialEnvironment.SetIncludeFileContent("Material.hlsl", materialShaderCode);

            // compile shader map
            newMaterialShaderMap.Compile(this, materialEnvironment);

            m_MaterialShaderMap = newMaterialShaderMap;

            return m_MaterialShaderMap != null ? true : false;
        }

        public virtual bool ShouldCache(H1ShaderType shaderType, H1VertexFactoryType vertexFactoryType)
        {
            return true;
        }

        // explicitly draw with color not with material (this enable vertex factory compiling with H1_COLOR)
        public void SetDrawWithColor()
        {
            m_DrawWithMaterial = false;
            m_DrawWithColor = true;
        }

        public H1Shader GetShader(String shaderTypeName, H1VertexFactoryType vertexFactoryType)
        {
            H1MeshMaterialShaderType shaderType = H1Global<H1ManagedRenderer>.Instance.ShaderManager.GetShaderType(shaderTypeName) as H1MeshMaterialShaderType;

            if (shaderType != null)
            {
                H1MeshMaterialShaderMap meshShaderMap = m_MaterialShaderMap.GetMeshShaderMap(vertexFactoryType);
                H1Shader shader = meshShaderMap != null ? meshShaderMap.Shaders[shaderType] : null;
                return shader;
            }
            else
                return null;
        }

        // friendly name
        private String m_FriendlyName;

        // flags
        private bool m_DrawWithColor = false; // if color draw is on, m_DrawWithMaterial should be off
        private bool m_DrawWithMaterial = true; 

        // material shader map
        private H1MaterialShaderMap m_MaterialShaderMap; // @TODO - I need to differentiate for using rendering thread
    }

    // material interface
    public abstract class H1MaterialInterface
    {

    }

    // definition of how to interact the lighting with material
    // we can think of 'Material Template'
    public class H1Material : H1MaterialInterface
    {   
        public static H1Material DefaultMaterial
        {
            get { return m_DefaultMaterial; }
        }
        
        public H1MaterialResource MaterialResource
        {
            get { return m_MaterialResource; }
        }       
                            
        public H1Material()
        {

        }

        private static H1Material CreateDefaultMaterial()
        {
            H1Material outDefaultMaterial = new H1Material() { m_MaterialResource = new H1MaterialResource("DefaultMaterial") };
            outDefaultMaterial.m_MaterialResource.SetDrawWithColor(); // enable drawing color instances
            outDefaultMaterial.m_MaterialResource.BeginCompileShaderMap();

            return outDefaultMaterial;
        }

        // default material as static member variable
        private static H1Material m_DefaultMaterial = CreateDefaultMaterial();

        private H1MaterialResource m_MaterialResource;
    }

    // material interface instance handling parameters for material
    class H1MaterialInstance : H1MaterialInterface
    {
        // pointer to the parent (H1Material)
        private H1MaterialInterface m_Parent;
    }
}
