using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    // render data for a GPU skinned mesh
    public class H1SkeletalMeshObjectGPUSkin : H1SkeletalMeshObject
    {
        public H1SkeletalMeshObjectGPUSkin(H1SkeletalMeshResource skeletalMeshResourceRef)
            : base(skeletalMeshResourceRef)
        {
           
        }

        public H1SkeletalMeshObjectLOD AddSkeletalMeshObjectLOD()
        {
            m_LODs.Add(new H1SkeletalMeshObjectLOD());
            H1SkeletalMeshObjectLOD newSkeletalMeshObjectLOD = m_LODs.Last();
            // newly add the reference for SkeletalMeshResource reference
            newSkeletalMeshObjectLOD.SkelMeshResourceRef = m_SkeletalMeshResourceRef;
            return newSkeletalMeshObjectLOD;
        }

        public H1SkeletalMeshObjectLOD GetSkeletalMeshObjectLODByIndex(Int32 index)
        {
            return m_LODs[index];
        }

        // vertex factories and their matrix array
        public class H1VertexFactoryData
        {
            // one vertex factory for each chunk
            public List<H1GpuSkinVertexFactory> VertexFactories = new List<H1GpuSkinVertexFactory>();
            // ... other various GpuSkin vertex factories
        }

        // vertex data for rendering a single LOD
        public class H1SkeletalMeshObjectLOD
        {
            public H1SkeletalMeshResource SkelMeshResourceRef;
            // index into H1SkeletalMEshResource::LODModels[]
            public Int32 LODIndex;
            // vertex buffer that stores the morph target vertex updated on CPU
            // FMorphVertexBuffer MorphVertexBuffer
            // ...

            // default GPU skinning vertex factories and matrices
            public H1VertexFactoryData GPUSkinVertexFactories = new H1VertexFactoryData();

            // index buffer
            public H1IndexBuffer IndexBuffer;
        }

        // render data for each LOD
        private List<H1SkeletalMeshObjectLOD> m_LODs = new List<H1SkeletalMeshObjectLOD>();
        // data that is updated dynamically and is needed for rendering
        // FDynamicSkelMeshObjectDataGPUSkin
        // fence for dynamic data
        // FGraphEventRef RHITHreadFenceForDynamicData
    }
}
