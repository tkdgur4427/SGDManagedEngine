using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{   
    public class H1StaticSection
    {
        //@TODO temporary, I need to fix this properly
        private int m_FirstIndex;
        private int m_Counts;
    }

    public class H1StaticMeshLODResource
    {
        public H1StaticMeshLODResource(H1MeshContext context)
        {
            // defaultly pointing first material
            m_MaterialIndex = 0;

            // create local vertex factory
            m_LocalVertexFactory = new H1LocalVertexFactory();

            // process mesh context for H1StaticMeshLODResource
            ProcessMeshContext(context);
        }

        public H1LocalVertexFactory LocalVertexFactory
        {
            get { return m_LocalVertexFactory; }            
        }

        public H1IndexBuffer IndexBuffer
        {
            get { return m_IndexBuffer; }
        }

        // private methods
        private void ProcessMeshContext(H1MeshContext context)
        {            
            // offset tracking
            int currOffset = 0;

            // process vertex buffers and index buffer
            // 1. positions
            var positions = context.Positions.ToArray();
            if (positions.Count() != 0)
            {
                LocalVertexFactory.PositionVertexBuffer = H1VertexBuffer.ProcessVertexBuffer(positions);
                LocalVertexFactory.PositionStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Position, H1VertexElementType.Float3, currOffset++);
            }            

            // 2. normals
            var normals = context.Normals.ToArray();
            if (normals.Count() != 0)
            {
                LocalVertexFactory.NormalVertexBuffer = H1VertexBuffer.ProcessVertexBuffer(normals);
                LocalVertexFactory.NormalStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Normal, H1VertexElementType.Float3, currOffset++);
            }            

            // 3. texcoords
            // 3-1. looping UVBuffers
            foreach (var texcoords in context.UVBuffers)
            {
                if (texcoords.Buffer.Count() != 0)
                {
                    LocalVertexFactory.TexcoordVertexBuffers.Add(H1VertexBuffer.ProcessVertexBuffer(texcoords.Buffer.ToArray()));
                    LocalVertexFactory.AddTexcoordStreamComponent(new H1VertexStreamComponent(H1VertexStreamSematicType.Texcoord, H1VertexElementType.Float2, currOffset++));
                }
            }

            // 3-2. looping UVWBuffers
            foreach (var texcoords in context.UVWBuffers)
            {
                if (texcoords.Buffer.Count() != 0)
                {
                    LocalVertexFactory.TexcoordVertexBuffers.Add(H1VertexBuffer.ProcessVertexBuffer(texcoords.Buffer.ToArray()));
                    LocalVertexFactory.AddTexcoordStreamComponent(new H1VertexStreamComponent(H1VertexStreamSematicType.Texcoord, H1VertexElementType.Float3, currOffset++));
                }
            }
            
            // @TODO - temporal color component         
            int numVertices = context.Positions.Count;
            Vector4[] colors = new Vector4[numVertices];
            for (int i = 0; i < numVertices; ++i)
            {
                colors[i].X = 1.0f; colors[i].Y = 0.0f; colors[i].Z = 0.0f; colors[i].W = 1.0f;
            }

            LocalVertexFactory.ColorVertexBuffer = H1VertexBuffer.ProcessVertexBuffer(colors);
            LocalVertexFactory.ColorStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Color, H1VertexElementType.Float4, currOffset++);

            // generate RHIVertexFormatDeclaration
            LocalVertexFactory.GenerateVertexDeclaration();

            // 4. indices
            var indices = context.Indices.ToArray();
            m_IndexBuffer = H1IndexBuffer.ProcessIndexBuffer(indices);
        }       

        private H1LocalVertexFactory m_LocalVertexFactory;        
        private H1IndexBuffer m_IndexBuffer;
        private int m_MaterialIndex; // material index in H1StaticMesh

        private readonly List<H1StaticSection> m_Sections = new List<H1StaticSection>();
    }

    public class H1StaticMeshData
    {
        public H1StaticMeshData(H1ModelContext context)
        {
            // looping the mesh context in H1ModelContext
            foreach (H1MeshContext meshContext in context.Meshes)
            {
                // @TODO - need to change this after applying quad splitting
                if (meshContext.Indices.Count == 0)
                    continue;

                H1StaticMeshLODResource meshLOD = new H1StaticMeshLODResource(meshContext);
                m_LODResources.Add(meshLOD);
            }
        }

        public H1StaticMeshLODResource GetLODResource(int index)
        {
            return m_LODResources[index];
        }

        private readonly List<H1StaticMeshLODResource> m_LODResources = new List<H1StaticMeshLODResource>();
    }

    public class H1StaticMesh
    {    
        public H1StaticMeshData StaticMeshData
        {
            get { return m_StaticMeshData; }
        }
            
        public H1StaticMesh(H1ModelContext context)
        {
            m_StaticMeshData = new H1StaticMeshData(context);

            // @TODO - temporary one unified material
            H1MaterialInterface material = H1Material.DefaultMaterial;
            m_Materials.Add(material);          
        }

        private readonly List<H1MaterialInterface> m_Materials = new List<H1MaterialInterface>();
        private H1StaticMeshData m_StaticMeshData;
    }
}
