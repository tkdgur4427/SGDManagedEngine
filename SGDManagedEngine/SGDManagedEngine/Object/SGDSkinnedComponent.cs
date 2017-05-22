using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1SkinnedMeshComponent : H1MeshComponent
    {
        public H1SkeletalMesh SkeletalMesh
        {
            get { return m_SkeletalMesh; }
            set { m_SkeletalMesh = value; }
        }

        public H1SkeletalMeshObjectGPUSkin SkeletalMeshObjectGPUSkin
        {
            get { return m_SkeletalMeshObject as H1SkeletalMeshObjectGPUSkin; }
        }
        
        public Boolean GenerateSkeleltalMeshObjectGpuSkin()
        {
            // allocate new instance of SkeletalMeshGpuSkin
            m_SkeletalMeshObject = new H1SkeletalMeshObjectGPUSkin(m_SkeletalMesh.SkeletalMeshResource);
            H1SkeletalMeshObjectGPUSkin refSkeletalMeshObject = m_SkeletalMeshObject as H1SkeletalMeshObjectGPUSkin;

            // process soft vertices for each chunk to separate vertex buffers
            // @TODO - handle LOD variation
            List<H1SkelMeshChunk> skelChunks = SkeletalMesh.SkeletalMeshResource.GetLODModel(0).Chunks;

            // @TODO - temporary add LOD_0
            H1SkeletalMeshObjectGPUSkin.H1SkeletalMeshObjectLOD skeletalMeshObjectLOD_0 = refSkeletalMeshObject.AddSkeletalMeshObjectLOD();
            
            Int32 skelChunkIndex = 0;
            foreach (H1SkelMeshChunk skelChunk in skelChunks)
            {                
                H1SkeletalMeshVertexBuffers skeletalMeshVBsRef = skeletalMeshObjectLOD_0.SkelMeshResourceRef.GetLODModel(0).VertexBufferGPUSkin.SkeletalMeshVertexBuffers[skelChunkIndex];

                // add GPUSkinFactory
                skeletalMeshObjectLOD_0.GPUSkinVertexFactories.VertexFactories.Add(new H1GpuSkinVertexFactory());
                H1GpuSkinVertexFactory newGpuSkinVertexFactoryRef = skeletalMeshObjectLOD_0.GPUSkinVertexFactories.VertexFactories.Last();

                // offset tracking
                Int32 currOffset = 0;

                Int32 skelMeshVBsCount = skeletalMeshVBsRef.GetSkeletalMeshVertexDataCount();
                for (Int32 skelMeshVBIndex = 0; skelMeshVBIndex < skelMeshVBsCount; ++skelMeshVBIndex)
                {
                    H1SkeletalMeshVertexDataInterface skeletalMeshVertexData = skeletalMeshVBsRef.GetSkeletalMeshVertexData(skelMeshVBIndex);
                    switch (skeletalMeshVertexData.DataType)
                    {
                        case H1SkeletalMeshVertexDataInterface.VertexDataType.Position:
                            {
                                H1SkeletalMeshVertexData<Vector4> positionVertexData = skeletalMeshVertexData as H1SkeletalMeshVertexData<Vector4>;
                                newGpuSkinVertexFactoryRef.ShaderData.PositionBuffer = H1VertexBuffer.ProcessVertexBuffer(positionVertexData.VertexBuffer);
                                newGpuSkinVertexFactoryRef.PositionStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Position, H1VertexElementType.Float4, currOffset++);
                                break;
                            }
                        case H1SkeletalMeshVertexDataInterface.VertexDataType.TangentZ:
                            {
                                H1SkeletalMeshVertexData<Vector4> tangentZVertexData = skeletalMeshVertexData as H1SkeletalMeshVertexData<Vector4>;
                                newGpuSkinVertexFactoryRef.ShaderData.TangentZBuffer = H1VertexBuffer.ProcessVertexBuffer(tangentZVertexData.VertexBuffer);
                                newGpuSkinVertexFactoryRef.TangentZStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.TangentZ, H1VertexElementType.Float4, currOffset++);
                                break;
                            }
                        case H1SkeletalMeshVertexDataInterface.VertexDataType.TangentX:
                            {
                                H1SkeletalMeshVertexData<Vector4> tangentXVertexData = skeletalMeshVertexData as H1SkeletalMeshVertexData<Vector4>;
                                newGpuSkinVertexFactoryRef.ShaderData.TangentXBuffer = H1VertexBuffer.ProcessVertexBuffer(tangentXVertexData.VertexBuffer);
                                newGpuSkinVertexFactoryRef.TangentXStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.TangentX, H1VertexElementType.Float4, currOffset++);
                                break;
                            }
                        case H1SkeletalMeshVertexDataInterface.VertexDataType.InfluencedBones:
                            {
                                H1SkeletalMeshVertexData<Int4> boneIndicesVertexData = skeletalMeshVertexData as H1SkeletalMeshVertexData<Int4>;
                                newGpuSkinVertexFactoryRef.ShaderData.BoneIndices = H1VertexBuffer.ProcessVertexBuffer(boneIndicesVertexData.VertexBuffer);
                                newGpuSkinVertexFactoryRef.BoneIndicesStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.BoneIndices, H1VertexElementType.Int4, currOffset++);
                                break;
                            }
                        case H1SkeletalMeshVertexDataInterface.VertexDataType.InfluencedWeights:
                            {
                                H1SkeletalMeshVertexData<Vector4> blendWeightsVertexData = skeletalMeshVertexData as H1SkeletalMeshVertexData<Vector4>;
                                newGpuSkinVertexFactoryRef.ShaderData.BoneWeights = H1VertexBuffer.ProcessVertexBuffer(blendWeightsVertexData.VertexBuffer);
                                newGpuSkinVertexFactoryRef.BoneWeightsStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.BoneWeights, H1VertexElementType.Float4, currOffset++);
                                break;
                            }
                        // @TODO - support multiple texcoords
                        case H1SkeletalMeshVertexDataInterface.VertexDataType.Texcoord:
                            {
                                H1SkeletalMeshVertexData<Vector2> texcoordVertexData = skeletalMeshVertexData as H1SkeletalMeshVertexData<Vector2>;
                                newGpuSkinVertexFactoryRef.ShaderData.TexcoordBuffers.Add(H1VertexBuffer.ProcessVertexBuffer(texcoordVertexData.VertexBuffer));
                                newGpuSkinVertexFactoryRef.TexcoordStreamComponents.Add(new H1VertexStreamComponent(H1VertexStreamSematicType.Texcoord, H1VertexElementType.Float2, currOffset++));
                                break;
                            }
                        case H1SkeletalMeshVertexDataInterface.VertexDataType.Color:
                            {
                                H1SkeletalMeshVertexData<Vector4> colorVertexData = skeletalMeshVertexData as H1SkeletalMeshVertexData<Vector4>;
                                newGpuSkinVertexFactoryRef.ShaderData.ColorBuffer = H1VertexBuffer.ProcessVertexBuffer(colorVertexData.VertexBuffer);
                                newGpuSkinVertexFactoryRef.ColorStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Color, H1VertexElementType.Float4, currOffset++);
                                break;
                            }
                    }                    
                }

                // generate RHIVertexFormat Declaration
                newGpuSkinVertexFactoryRef.GenerateVertexDeclaration();

                skelChunkIndex++;
            }

            // add index buffer (containing multiple skeletal mesh chunks's indices)
            List<UInt32> indices = SkeletalMesh.SkeletalMeshResource.GetLODModel(0).MultiSizeIndexContainer.Indices;
            UInt32 bufferSize = Convert.ToUInt32(Utilities.SizeOf<UInt32>() * indices.Count);
            skeletalMeshObjectLOD_0.IndexBuffer = H1Global<H1ManagedRenderer>.Instance.CreateIndexBuffer(bufferSize);
            // write indices data
            skeletalMeshObjectLOD_0.IndexBuffer.WriteData(indices.ToArray());

            return true;
        }

        // the skeletal mesh used by this component
        protected H1SkeletalMesh m_SkeletalMesh = new H1SkeletalMesh();
        // if set, this skeletal mesh component will NOT use its SpaceBase for bone transform, but will use the SpaceBases array in the MaterPoseComponent
        protected H1SkinnedMeshComponent m_MasterPoseComponentRef;
        // temporary array of component space bone matrices, update each frame and used for rendering
        protected List<H1Transform>[] m_SpaceBonesArray = new List<H1Transform>[2];
        // the index for the space bases buffer we can currently read/write to
        protected Int32 m_CurrentEditableSpaceBases;
        protected Int32 m_CurrentReadSpaceBases;
        // if set, this component has slave pos components that are associated with this
        protected List<H1SkinnedMeshComponent> m_SlavePoseComponentRefs = new List<H1SkinnedMeshComponent>();
        // mapping between bone indices in this component and the parent one
        // each element is the index of the bone in the MasterPosComponent
        // size should be same as USkeletalMesh.RefSkeleton size
        protected List<Int32> m_MasterBoneMap = new List<int>();
        // if 0, auto-select LOD level
        // if > 0, force to (ForceLODModel - 1)
        protected Int32 m_ForceLODLevel;
        // the min LOD that this component will use
        protected Int32 m_MinLODModel;
        // best LOD that was 'predicted' by UpdateSkelPose
        protected Int32 m_PredictedLODModel;
        // to recalc required bones by detecting changes in LODs
        protected Int32 m_OldPredicateLODModel;
        // this is update frequency flag when our Owner has not been rendered recently
        //EMeshComponentUpdateFlag  

        // object responsible for sending bone transforms, vertex aim state etc to 'render-thread'
        protected H1SkeletalMeshObject m_SkeletalMeshObject;       
    }
}
