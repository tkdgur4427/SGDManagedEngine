using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1SkeletalMaterial
    {
        private H1MaterialInterface m_MaterialInterfaceRef;
        private Boolean m_bShadowCasting;
    }

    public class H1SkeletalMeshLODInfo
    {
        // indicates when to use this LOD (a smaller number means use this LOD when further array)
        private float m_ScreenSize;
        // used to avoid flickering when on LOD boundary
        private float m_LODHystersis;
        // mapping table from this LOD's materials to the USkeletalMesh materials array
        private List<Int32> m_LODMaterialMap = new List<Int32>();
    }

    public class H1SkeletalMeshSocket
    {
        private String m_SocketName;
        private String m_BoneName;
        private Vector3 m_RelativeLocation;
        private Vector3 m_RelativeRotation;
        private Vector3 m_RelativeScale;
    }

    public class H1SkeletalMesh : H1Object
    {
        public H1SkeletalMeshResource SkeletalMeshResource
        {
            get { return m_SkeletalMeshResource; }
        }

        #region Import Methods

        private class VertexInfo
        {
            public struct InfluenceBoneData
            {
                public Int32 BoneIndex;
                public float BoneWeight;
            }

            public Int32 VertexIndex;
            public List<InfluenceBoneData> InfluencedBones = new List<InfluenceBoneData>();
            public List<Int32> TriangleIndices = new List<Int32>();
        }

        // to preprocess asset-context to handle multiple JointData
        class BoneOffsetTransformSet
        {
            public List<H1Transform> BoneOffsetTransforms = new List<H1Transform>();
        }

        public H1StaticLODModel PrepareProcessAssetContext(H1SkeletalContext skeletalContext)
        {
            if (m_SkeletalMeshResource == null)
                return null;

            // @TODO - temporary low LOD model, fix this
            H1StaticLODModel staticLODModel = m_SkeletalMeshResource.AddLODModel();

            // add root node that controls all animation motion
            H1ReferenceSkeleton refSkeleton = m_Skeleton.RefSkeleton;

            // construct refSkeleton
            List<H1SkeletalContext.JointNode> jointNodes = skeletalContext.JointNodes;

            // to construct local transforms for each bone            
            List<BoneOffsetTransformSet> boneOffsetTransforms = new List<BoneOffsetTransformSet>();

            foreach (H1SkeletalContext.JointNode jointNode in jointNodes)
            {
                // if is not marked as bone node space, skip
                if (!jointNode.MarkedAsBoneSpace)
                    continue;

                // prepare data to process bone node
                H1MeshBoneInfo meshBoneInfo = new H1MeshBoneInfo();
                meshBoneInfo.Name = jointNode.JointName;

                // recursively find bone-space marked parent node (based on H1SkeletalContext.JointNode)
                // we need to reapply parent index based on 'refSkeleton.RefBoneInfoList'
                Int32 currParentIndex = jointNodes.FindIndex(x =>
                {
                    if (jointNode.Parent == null)
                        return false;
                    return x.JointName == jointNode.Parent.JointName;
                });
                while (currParentIndex != -1 // if not reaching to root node
                     && !jointNodes[currParentIndex].MarkedAsBoneSpace) // and if is not marked by bone-space
                {
                    currParentIndex = jointNodes.FindIndex(x =>
                    {
                        if (jointNodes[currParentIndex].Parent == null)
                            return false;
                        return x.JointName == jointNodes[currParentIndex].Parent.JointName;
                    });
                }
                meshBoneInfo.ParentIndex = currParentIndex;

                // the new index should be same with RefBoneBases
                Int32 newBoneInfoIndex = refSkeleton.RefBoneInfoList.Count;
                Int32 newBoneBaseIndex = refSkeleton.RefBoneBases.Count;
                if (newBoneBaseIndex != newBoneInfoIndex)
                    return null;

                refSkeleton.RefBoneInfoList.Add(meshBoneInfo);

                // what I learned from this disabled code section
                // 1. ASSIMP has separate node space and bone node space (transformation matrix)
                // 2. we need to handle those differently
                //  - vertices resided in particular node space (mesh space we called) should be transformed into root node space (world space)
                //  - localTransform in refSkeleton should be set by offsetMatrix in JointNode.JointData
                H1Transform boneBase = new H1Transform();
                boneBase.Translation = jointNode.NodeLocalTransform.Translation;
                boneBase.Scaling = jointNode.NodeLocalTransform.Scaling;
                boneBase.Rotation = jointNode.NodeLocalTransform.Rotation;    

                // allocate the space in advance
                refSkeleton.RefBoneBases.Add(boneBase);

                refSkeleton.AddNameToIndexPair(meshBoneInfo.Name, newBoneInfoIndex);

                // add bone offset transform set
                boneOffsetTransforms.Add(new BoneOffsetTransformSet());

                // looping joint data
                foreach (H1SkeletalContext.Joint jointData in jointNode.JointDataList)
                {
                    H1Transform meshContextLocalToGlobalTransform = jointData.MeshContextLocalToGlobal;
                    
                    // convert 'offsetTransformation' to H1Transform
                    H1SkeletalContext.Transformation offsetTransformation = jointData.OffsetTransformation;
                    H1Transform offsetTransform = new H1Transform();
                    offsetTransform.Translation = offsetTransformation.Translation;
                    offsetTransform.Rotation = offsetTransformation.Rotation;
                    offsetTransform.Scaling = offsetTransformation.Scaling;

                    // apply meshContextLocalToGlobal
                    Matrix result = Matrix.Multiply(Matrix.Invert(meshContextLocalToGlobalTransform.Transformation), offsetTransform.Transformation);
                    Vector3 resultTranslation;
                    Vector3 resultScaling;
                    Quaternion resultRotation;
                    result.Decompose(out resultScaling, out resultRotation, out resultTranslation);

                    offsetTransform.Translation = resultTranslation;
                    offsetTransform.Rotation = resultRotation;
                    offsetTransform.Scaling = resultScaling;

                    boneOffsetTransforms.Last().BoneOffsetTransforms.Add(offsetTransform);
                }
            }

            // construct parent index for H1MeshBoneInfo
            foreach (H1MeshBoneInfo meshBoneInfo in refSkeleton.RefBoneInfoList)
            {
                if (meshBoneInfo.ParentIndex == -1) // not handling root node case
                    continue; 

                String parentName = jointNodes[meshBoneInfo.ParentIndex].JointName;
                meshBoneInfo.ParentIndex = refSkeleton.RefBoneInfoList.FindIndex(x =>
                {
                    return x.Name == parentName;
                });
            }                

            // @TODO - handle multiple offset transformations
            // set offset matrix
            Int32 currBoneIndex = 0;
            foreach (H1MeshBoneInfo meshBoneInfo in refSkeleton.RefBoneInfoList)
            {
                // add to global offset transform
                H1Transform globalOffsetTransform = boneOffsetTransforms[currBoneIndex].BoneOffsetTransforms.Count > 0 ? boneOffsetTransforms[currBoneIndex].BoneOffsetTransforms[0] : new H1Transform();
                refSkeleton.RefOffsetBases.Add(globalOffsetTransform);

                currBoneIndex++;
            }

            return staticLODModel;
        }

        public Boolean ProcessAssetContext(H1StaticLODModel staticLODModel, H1MeshContext[] meshContexts, H1SkeletalContext skeletalContext)
        {            
            if (m_Skeleton == null)
                return false;
                        
            // 1. process skeletalContext (H1SkeletalContext)
            H1SkeletalContext.JointNode rootNode = skeletalContext.Root;
            H1ReferenceSkeleton refSkeleton = m_Skeleton.RefSkeleton;

            #region Disabled
            // process recursively from root node
            //Dictionary<String, H1SkeletalContext.JointNode> mapNodeNameToJointNode = new Dictionary<string, H1SkeletalContext.JointNode>();
            //if (!ProcessAssetContextRecursive(rootNode, ref refSkeleton, ref mapNodeNameToJointNode))
            //    return false; // failed to process nodes from root nodes
            #endregion

            // 2. process meshContexts (H1MeshContext)
            // collect all vertices for all different mesh contexts
            Int32[] meshContextOffsetsList = new Int32[meshContexts.Count()];

            List<Vector3> positionsList = new List<Vector3>();
            List<Vector3> tangentXList = new List<Vector3>();
            List<Vector3> tangentYList = new List<Vector3>();
            List<Vector3> tangentZList = new List<Vector3>();
            List<Vector2> texcoordsList = new List<Vector2>();

            Int32 currMeshContextIndex = 0;
            foreach (H1MeshContext meshContext in meshContexts)
            {
                meshContextOffsetsList[currMeshContextIndex] = positionsList.Count;

                positionsList.AddRange(meshContext.Positions);
                tangentXList.AddRange(meshContext.Tangents);
                tangentYList.AddRange(meshContext.Normals);
                tangentZList.AddRange(meshContext.BiTangents);
                texcoordsList.AddRange(meshContext.UVBuffers[0].Buffer);

                currMeshContextIndex++;
            }

            Vector3[] positions = positionsList.ToArray();            
            Vector3[] tangentX = tangentXList.ToArray();
            Vector3[] tangentY = tangentYList.ToArray();
            Vector3[] tangentZ = tangentZList.ToArray();
            Vector2[] texcoords = texcoordsList.ToArray();

            Int32 numPositions = positions.Count();
            if (numPositions != texcoords.Count())
            {
                return false;
            }

            // create the list of vertex info
            List<VertexInfo> vertexInfoList = new List<VertexInfo>();
#if DEBUG
            List<Boolean> taggedVertexInfoList = new List<Boolean>();
#endif

            for (Int32 i = 0; i < numPositions; ++i)
            {
                vertexInfoList.Add(new VertexInfo());
                vertexInfoList[i].VertexIndex = i;
#if DEBUG
                taggedVertexInfoList.Add(false);
#endif
            }

            // set the triangle index
            // create collection of index for all mesh contexts
            List<UInt32> indexList = new List<UInt32>();
            Int32 meshContextIndex = 0;
            foreach (H1MeshContext meshContext in meshContexts)
            {
                // using meshContextOffsetsList information, remap indices correctly
                foreach (UInt32 vertexIndex in meshContext.Indices)
                {
                    indexList.Add(Convert.ToUInt32(meshContextOffsetsList[meshContextIndex]) + vertexIndex);
                }

                meshContextIndex++;
            }
            
            UInt32[] indices = indexList.ToArray();
#if DEBUG
            Boolean[] taggedIndices = new Boolean[indexList.Count];
#endif

            // looping index, set triangle index for each vertexInfoList
            Int32 indexOfIndex = 0;
            foreach (UInt32 index in indices)
            {
                Int32 vertexIndex = Convert.ToInt32(index);
                Int32 triangleIndex = indexOfIndex / 3;

                // set the triangle index
                vertexInfoList[vertexIndex].TriangleIndices.Add(triangleIndex);
#if DEBUG
                taggedIndices[indexOfIndex] = false;
#endif
                indexOfIndex++;
            }

            // process vertex weights and bone index in advance by looping all H1MeshBoneInfo
            Int32 meshBoneInfoIndex = 0;
            foreach (H1MeshBoneInfo meshBoneInfo in refSkeleton.RefBoneInfoList)
            {
                Int32 jointNodeIndex = skeletalContext.JointNodes.FindIndex(x => x.JointName == meshBoneInfo.Name);
                H1SkeletalContext.JointNode node = skeletalContext.JointNodes[jointNodeIndex];

                foreach (H1SkeletalContext.Joint nodeData in node.JointDataList)
                {
                    // looping weighted vertices
                    Int32 vertexIndexOffset = nodeData.MeshContextIndex == -1 ? 0 : meshContextOffsetsList[nodeData.MeshContextIndex];
                    // process vertex weights and bone index
                    foreach (H1SkeletalContext.WeightedVertex weightedVertex in nodeData.WeightedVertices)
                    {
                        VertexInfo.InfluenceBoneData influencedBoneData = new VertexInfo.InfluenceBoneData();
                        influencedBoneData.BoneIndex = meshBoneInfoIndex;
                        influencedBoneData.BoneWeight = weightedVertex.Weight;

                        vertexInfoList[vertexIndexOffset + weightedVertex.VertexIndex].InfluencedBones.Add(influencedBoneData);
                    }
                }
                meshBoneInfoIndex++;
            }

#if DEBUG
            // verification code for vertex weights            
            List<float> taggedVertexWeights = new List<float>();            
            foreach (VertexInfo vertexInfo in vertexInfoList)
            {
                float totalWeight = 0.0f;
                foreach (VertexInfo.InfluenceBoneData influencedBoneData in vertexInfo.InfluencedBones)
                {
                    totalWeight += influencedBoneData.BoneWeight;
                }
                taggedVertexWeights.Add(totalWeight);                
            }

            List<Int32> invalidVertexIndices = new List<Int32>();
            Int32 currVertexIndex = 0;
            foreach (float totalWeight in taggedVertexWeights)
            {
                if (totalWeight < 0.99f)
                    invalidVertexIndices.Add(currVertexIndex);
                currVertexIndex++;
            }

            if (invalidVertexIndices.Count > 0)
            {
                return false;
            }
#endif

            // looping meshBoneInfo, set bone index and bone weight
            foreach (H1MeshBoneInfo meshBoneInfo in refSkeleton.RefBoneInfoList)
            {
                Int32 jointNodeIndex = skeletalContext.JointNodes.FindIndex(x => x.JointName == meshBoneInfo.Name);
                H1SkeletalContext.JointNode node = skeletalContext.JointNodes[jointNodeIndex];
                List<UInt32> ChunkVertexIndices = new List<UInt32>(); // vertex indices for this chunk (bone node)

                foreach (H1SkeletalContext.Joint nodeData in node.JointDataList)
                {
                    // looping weighted vertices
                    Int32 vertexIndexOffset = nodeData.MeshContextIndex == -1 ? 0 : meshContextOffsetsList[nodeData.MeshContextIndex];
                    // add chunk vertex indices
                    foreach (H1SkeletalContext.WeightedVertex weightedVertex in nodeData.WeightedVertices)
                    {                     
                        ChunkVertexIndices.Add(Convert.ToUInt32(vertexIndexOffset + weightedVertex.VertexIndex));
                    }
                }

                // exceptional handling for MeshBoneInfo which doesn't contain any mesh data, no need to generate chunk for this BasePose
                if (ChunkVertexIndices.Count == 0)
                    continue;

                // process skeletal mesh chunk
                H1SkelMeshChunk skelMeshChunk = new H1SkelMeshChunk();
                // calculate triangles for this mesh chunk
                List<UInt32> triangles = new List<UInt32>();
                                
                foreach (Int32 vertexIndex in ChunkVertexIndices)
                {
                    VertexInfo vertexInfo = vertexInfoList[vertexIndex];

                    H1SoftSkinVertex newSoftSkinVertex = new H1SoftSkinVertex();
                    newSoftSkinVertex.Position = positions[vertexInfo.VertexIndex];
                    newSoftSkinVertex.TangentX = tangentX[vertexInfo.VertexIndex];
                    newSoftSkinVertex.TangentY = tangentY[vertexInfo.VertexIndex];
                    newSoftSkinVertex.TangentZ = tangentZ[vertexInfo.VertexIndex];

                    // @TODO - in the future, it could increase texcoords (ex. light map texcoord)
                    newSoftSkinVertex.UVs[0] = texcoords[vertexInfo.VertexIndex];
                    
                    for (Int32 i = 0; i < H1ObjectGlobalDefinitions.MAX_TOTAL_INFLUENCES; ++i)
                    {
                        if (vertexInfo.InfluencedBones.Count <= i)
                        {
                            // insert the default value
                            newSoftSkinVertex.InfluenceWeights[i] = 0;
                            newSoftSkinVertex.InfluenceBones[i] = 0;
                            continue;
                        }

                        VertexInfo.InfluenceBoneData boneData = vertexInfo.InfluencedBones[i];
                        newSoftSkinVertex.InfluenceWeights[i] = Convert.ToByte(boneData.BoneWeight * 255.0f);
                        // @TODO - for this bone index, we can replace with 'm_BoneMap'
                        newSoftSkinVertex.InfluenceBones[i] = Convert.ToByte(boneData.BoneIndex);
                    }

                    skelMeshChunk.SoftVertices.Add(newSoftSkinVertex);

                    // process triangle indices
                    foreach (Int32 triangleIndex in vertexInfo.TriangleIndices)
                    {
                        UInt32 triangleIndexUInt32 = Convert.ToUInt32(triangleIndex);
                        if (!triangles.Exists(x => { return x == triangleIndexUInt32; }))
                        {
                            triangles.Add(triangleIndexUInt32);
                        }
                    }                    
                }

                // build vertex buffers
                List<Vector4> chunkPositions = new List<Vector4>();
                List<Vector4> chunkTangentZs = new List<Vector4>();
                List<Vector4> chunkTangentXs = new List<Vector4>();
                List<Vector2> chunkTexcoords = new List<Vector2>();
                List<Int4> chunkBoneIndices = new List<Int4>();
                List<Vector4> chunkBoneWeights = new List<Vector4>();
                List<Vector4> chunkColors = new List<Vector4>();

                Dictionary<Int32, Int32> vertexToChunkVertexMap = new Dictionary<Int32, Int32>();
                foreach (Int32 triangleIndex in triangles)
                {
                    for(Int32 faceIndex = 0; faceIndex < 3; ++faceIndex)
                    {
                        Int32 vertexIndex = Convert.ToInt32(indices[triangleIndex * 3 + faceIndex]);

#if DEBUG
                        taggedVertexInfoList[vertexIndex] = true;
#endif

                        // determine if the same vertex exists based on chunkPositions
                        //if (chunkPositions.Contains(new Vector4(positions[vertexIndex], 1.0f)))
                        if (vertexToChunkVertexMap.ContainsKey(vertexIndex))
                        {
                            continue;
                        }

                        // position index is base-index
                        Int32 newIndex = chunkPositions.Count;
                        vertexToChunkVertexMap.Add(vertexIndex, newIndex);

                        chunkPositions.Add(new Vector4(positions[vertexIndex], 1.0f));
                        chunkTangentZs.Add(new Vector4(tangentZ[vertexIndex], 1.0f));
                        chunkTangentXs.Add(new Vector4(tangentX[vertexIndex], 1.0f));
                        chunkTexcoords.Add(new Vector2(texcoords[vertexIndex].X, texcoords[vertexIndex].Y));

                        VertexInfo vertexInfo = vertexInfoList[vertexIndex];
                        Int4 influencedBones = new Int4(0);
                        Vector4 influencedWeights = new Vector4(0.0f);

                        Int32 insertedIndex = 0;
                        foreach (VertexInfo.InfluenceBoneData boneData in vertexInfo.InfluencedBones)
                        {
                            influencedBones[insertedIndex] = boneData.BoneIndex;
                            influencedWeights[insertedIndex] = boneData.BoneWeight;
                            insertedIndex++;
                        }

                        chunkBoneIndices.Add(influencedBones);
                        chunkBoneWeights.Add(influencedWeights);

                        //@TODO - temporary set it as 'RED'
                        chunkColors.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                    }                    
                }

                // create skeletal mesh vertex data
                // 1. position
                H1SkeletalMeshVertexData<Vector4> positionVertexData = new H1SkeletalMeshVertexData<Vector4>();
                positionVertexData.SetVertexData(chunkPositions.ToArray(), H1SkeletalMeshVertexDataInterface.VertexDataType.Position, false);
                // 2. tangentZ
                H1SkeletalMeshVertexData<Vector4> tangentZVertexData = new H1SkeletalMeshVertexData<Vector4>();
                tangentZVertexData.SetVertexData(chunkTangentZs.ToArray(), H1SkeletalMeshVertexDataInterface.VertexDataType.TangentZ, false);
                // 3. tangentX
                H1SkeletalMeshVertexData<Vector4> tangentXVertexData = new H1SkeletalMeshVertexData<Vector4>();
                tangentXVertexData.SetVertexData(chunkTangentXs.ToArray(), H1SkeletalMeshVertexDataInterface.VertexDataType.TangentX, false);
                // 4. bone indices
                H1SkeletalMeshVertexData<Int4> boneIndicesVertexData = new H1SkeletalMeshVertexData<Int4>();
                boneIndicesVertexData.SetVertexData(chunkBoneIndices.ToArray(), H1SkeletalMeshVertexDataInterface.VertexDataType.InfluencedBones, false);
                // 5. bone weights
                H1SkeletalMeshVertexData<Vector4> boneWeightsVertexData = new H1SkeletalMeshVertexData<Vector4>();
                boneWeightsVertexData.SetVertexData(chunkBoneWeights.ToArray(), H1SkeletalMeshVertexDataInterface.VertexDataType.InfluencedWeights, false);
                // 6. texcoord
                H1SkeletalMeshVertexData<Vector2> texcoordVertexData = new H1SkeletalMeshVertexData<Vector2>();
                texcoordVertexData.SetVertexData(chunkTexcoords.ToArray(), H1SkeletalMeshVertexDataInterface.VertexDataType.Texcoord, false);
                // 7. colors
                H1SkeletalMeshVertexData<Vector4> colorVertexData = new H1SkeletalMeshVertexData<Vector4>();
                colorVertexData.SetVertexData(chunkColors.ToArray(), H1SkeletalMeshVertexDataInterface.VertexDataType.Color);

                Int32 newSkelMeshChunkIndex = staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers.Count;
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers.Add(new H1SkeletalMeshVertexBuffers(newSkelMeshChunkIndex));
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers[newSkelMeshChunkIndex].AddSkeletalMeshVertexData(positionVertexData);
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers[newSkelMeshChunkIndex].AddSkeletalMeshVertexData(tangentZVertexData);
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers[newSkelMeshChunkIndex].AddSkeletalMeshVertexData(tangentXVertexData);
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers[newSkelMeshChunkIndex].AddSkeletalMeshVertexData(boneIndicesVertexData);
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers[newSkelMeshChunkIndex].AddSkeletalMeshVertexData(boneWeightsVertexData);
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers[newSkelMeshChunkIndex].AddSkeletalMeshVertexData(texcoordVertexData);
                staticLODModel.VertexBufferGPUSkin.SkeletalMeshVertexBuffers[newSkelMeshChunkIndex].AddSkeletalMeshVertexData(colorVertexData);

                // build index buffer
                List<UInt32> chunkIndices = new List<UInt32>();
                foreach (Int32 triangleIndex in triangles)
                {
                    for (Int32 faceIndex = 0; faceIndex < 3; ++faceIndex)
                    {
#if DEBUG
                        taggedIndices[triangleIndex * 3 + faceIndex] = true;
#endif
                        Int32 vertexIndex = Convert.ToInt32(indices[triangleIndex * 3 + faceIndex]);
                        chunkIndices.Add(Convert.ToUInt32(vertexToChunkVertexMap[vertexIndex]));
                    }
                }

                staticLODModel.MultiSizeIndexContainer.Indices.AddRange(chunkIndices.ToArray());

                // add new chunk
                Int32 newChunkIndex = staticLODModel.Chunks.Count;
                staticLODModel.Chunks.Add(skelMeshChunk);

                // process skeletal mesh section
                H1SkelMeshSection skelMeshSection = new H1SkelMeshSection();
                skelMeshSection.ChunkIndex = Convert.ToUInt16(newChunkIndex);
                
                // @TODO - fix this num triangle inconsistency
                //skelMeshSection.NumTriangles = Convert.ToUInt16(triangles.Count);
                skelMeshSection.NumTriangles = Convert.ToUInt16(chunkIndices.Count / 3);

                //@TODO - add material index

                // add skeletal sections
                staticLODModel.Sections.Add(skelMeshSection);
            }

#if DEBUG
            // debugging code for detecting missing vertex to draw meshes
            List<Int32> notTaggedVertexIndex = new List<Int32>();
            Int32 currTaggedVertexIndex = 0;
            foreach (Boolean isTaggedVertex in taggedVertexInfoList)
            {
                if (isTaggedVertex == false)
                    notTaggedVertexIndex.Add(currTaggedVertexIndex);
                currTaggedVertexIndex++;
            }
            if (notTaggedVertexIndex.Count > 0)
            {
                return false;
            }

            // debugging code for detecting missing index to draw meshes
            List<Int32> notTaggedIndex = new List<Int32>();
            Int32 currIndexOfIndex = 0;
            foreach (Boolean isTaggedIndex in taggedIndices)
            {
                if (isTaggedIndex == false)
                    notTaggedIndex.Add(currIndexOfIndex);
                currIndexOfIndex++;
            }
            if (notTaggedIndex.Count > 0)
            {
                return false;
            }
#endif

            return true;
        }

        #region Disabled
        public Boolean ProcessAssetContextRecursive(H1SkeletalContext.JointNode node, ref H1ReferenceSkeleton refSkeleton, ref Dictionary<String, H1SkeletalContext.JointNode> mapBoneNameToBoneNode)
        {
            if (node == null) // successfully reach to the leaf nodes
                return true;

            if (!ProcessRefSkeletonFromAssetContext(node, ref refSkeleton))
                return false;

            // add pair for BoneNameToBoneNode
            mapBoneNameToBoneNode.Add(node.JointName, node);

            foreach (H1SkeletalContext.JointNode child in node.Children)
            {
                if (!ProcessAssetContextRecursive(child, ref refSkeleton, ref mapBoneNameToBoneNode))
                    return false;
            }

            return true;
        }

        public Boolean ProcessRefSkeletonFromAssetContext(H1SkeletalContext.JointNode node, ref H1ReferenceSkeleton refSkeleton)
        {
            H1SkeletalContext.Joint jointData = null; // node.JointData;

            // add mesh bone info
            H1MeshBoneInfo meshBoneInfo = new H1MeshBoneInfo();
            //meshBoneInfo.Name = jointData.JointName;

            if (refSkeleton.NameToIndexMapIsEmpty())
            {
                // should be root node, so parent index is '0'
                meshBoneInfo.ParentIndex = 0;
            }
            else if (node.Parent == null)
            {
                // should be sub-root node, so parent index is '0' (root motion node)
                meshBoneInfo.ParentIndex = refSkeleton.GetIndexByBoneName("RootMotionNode");
            }
            else
                //meshBoneInfo.ParentIndex = refSkeleton.GetIndexByBoneName(node.Parent.JointData.JointName);

            if (meshBoneInfo.ParentIndex == -1)
                return false;

            // the new index should be same with RefBoneBases
            Int32 newBoneInfoIndex = refSkeleton.RefBoneInfoList.Count;
            Int32 newBoneBaseIndex = refSkeleton.RefBoneBases.Count;
            if (newBoneBaseIndex != newBoneInfoIndex)
                return false;

            refSkeleton.RefBoneInfoList.Add(meshBoneInfo);

            // add bone base
            H1Transform boneBase = new H1Transform();
            boneBase.Translation = jointData.OffsetTransformation.Translation;
            boneBase.Scaling = jointData.OffsetTransformation.Scaling;
            boneBase.Rotation = jointData.OffsetTransformation.Rotation;

            refSkeleton.RefBoneBases.Add(boneBase);

            // add NameToIndex
            refSkeleton.AddNameToIndexPair(meshBoneInfo.Name, newBoneInfoIndex);

            return true;
        }
        #endregion

        #endregion

        // @TODO - change this delivering directly command list from the renderer after implementing overall rendering system
        public void SkeletonDebugDrawing(SharpDX.Direct3D12.GraphicsCommandList commandList, Boolean bDebugDraw = true)
        {
            // @TODO - temporary to check if animation is working
            H1SkeletalMeshComponent skeletalMeshComponent = H1Global<H1World>.Instance.PersistentLevel.GetActor(0).GetActorComponent<H1SkeletalMeshComponent>();

            List<H1AnimInstance.InterpolatedLocalTransform> interpolatedLocalTransforms = new List<H1AnimInstance.InterpolatedLocalTransform>();
            for (Int32 boneIndex = 0; boneIndex < m_Skeleton.RefSkeleton.RefBoneInfoList.Count; ++boneIndex)
            {
                H1AnimInstance.InterpolatedLocalTransform interpolatedLocalTransform = new H1AnimInstance.InterpolatedLocalTransform();
                H1MeshBoneInfo meshBoneInfo = m_Skeleton.RefSkeleton.RefBoneInfoList[boneIndex];

                interpolatedLocalTransform.BoneName = meshBoneInfo.Name;
                interpolatedLocalTransform.LocalTransform = m_Skeleton.RefSkeleton.RefBoneBases[boneIndex];

                interpolatedLocalTransforms.Add(interpolatedLocalTransform);
            }

            // extract interpolated local transforms
            skeletalMeshComponent.AnimScriptInstance.ExtractInterpolatedLocalTransform(0.0f, ref interpolatedLocalTransforms);

            H1Transform[] globalTransforms = new H1Transform[m_Skeleton.RefSkeleton.RefBoneBases.Count];
            // tracking only upper parent index by one
            Int32[] transformParentIndices = new Int32[m_Skeleton.RefSkeleton.RefBoneBases.Count];

            for (Int32 boneIndex = 0; boneIndex < m_Skeleton.RefSkeleton.RefBoneInfoList.Count; ++boneIndex)
            {
                H1MeshBoneInfo meshBoneInfo = m_Skeleton.RefSkeleton.RefBoneInfoList[boneIndex];
                
                //H1Transform localTransform = m_Skeleton.RefSkeleton.RefBoneBases[boneIndex];
                H1Transform localTransform = interpolatedLocalTransforms[boneIndex].LocalTransform;

                // extract parent locals
                List<H1Transform> parentLocalTransforms = new List<H1Transform>();
                H1MeshBoneInfo currMeshBoneInfo = meshBoneInfo;
                while (currMeshBoneInfo.ParentIndex != -1)
                {
                    //parentLocalTransforms.Add(m_Skeleton.RefSkeleton.RefBoneBases[currMeshBoneInfo.ParentIndex]);
                    parentLocalTransforms.Add(interpolatedLocalTransforms[currMeshBoneInfo.ParentIndex].LocalTransform);
                    currMeshBoneInfo = m_Skeleton.RefSkeleton.RefBoneInfoList[currMeshBoneInfo.ParentIndex];
                }

                // generate global transform for current bone space
                // 1. start to transform current local transformation matrix
                Matrix globalTransformMtx = localTransform.Transformation;
                // 2. from curr bone space to root space by going up to each parent node space
                for (Int32 index = 0; index < parentLocalTransforms.Count; ++index)
                {
                    Matrix parentLocalTransformMtx = parentLocalTransforms[index].Transformation;
                    globalTransformMtx = Matrix.Multiply(globalTransformMtx, parentLocalTransformMtx);
                }

                // decompose global matrix and generate H1Transform
                Vector3 translation;
                Vector3 scale;
                Quaternion rotate;
                globalTransformMtx.Decompose(out scale, out rotate, out translation);

                H1Transform globalTransform = new H1Transform();
                globalTransform.Translation = translation;
                globalTransform.Scaling = scale;
                globalTransform.Rotation = rotate;

                globalTransforms[boneIndex] = globalTransform;
                transformParentIndices[boneIndex] = meshBoneInfo.ParentIndex;
            }

            // set bone matrices
            Int32 currGlobalBoneIndex = 0;
            foreach (H1Transform boneTransform in globalTransforms)
            {
                // @TODO - temporary scaling to match when we importing vertices in H1AssimpImporter
                //boneTransform.Scaling = boneTransform.Scaling * 0.01f; 

                // get global offset matrices
                Matrix globalOffsetMatrix = m_Skeleton.RefSkeleton.RefOffsetBases[currGlobalBoneIndex].Transformation;

                // pass multiplied global offset matrices and animated global matrices
                H1Global<H1ManagedRenderer>.Instance.SetBoneMatrix(currGlobalBoneIndex, Matrix.Multiply(globalOffsetMatrix, boneTransform.Transformation));
                currGlobalBoneIndex++;
            }

            // when debug draw is enabled
            if (bDebugDraw == true)
            {
                for (Int32 boneIndex = 0; boneIndex < globalTransforms.Count(); ++boneIndex)
                {
                    Int32 parentIndex = transformParentIndices[boneIndex];
                    if (parentIndex < 0)
                        continue;

                    H1Transform parentBoneTransform = globalTransforms[parentIndex];
                    H1Transform boneTransform = globalTransforms[boneIndex];

                    Vector3 parentLoc = parentBoneTransform.Translation * 0.005f;
                    Vector3 currLoc = boneTransform.Translation * 0.005f;

                    float length = (currLoc - parentLoc).Length();
                    if (length < 0.001f)
                        continue;

                    Vector3 zAxis = Vector3.Normalize(currLoc - parentLoc);

                    Matrix pitchMtx = Matrix.RotationYawPitchRoll(0, 0.1f, 0);
                    Vector4 rotatedZAxis = Vector3.Transform(zAxis, pitchMtx);
                    rotatedZAxis /= rotatedZAxis.W;

                    Vector3 yAxis = Vector3.Normalize(Vector3.Cross(zAxis, Vector3.Normalize(new Vector3(rotatedZAxis.X, rotatedZAxis.Y, rotatedZAxis.Z))));
                    Vector3 xAxis = Vector3.Normalize(Vector3.Cross(zAxis, yAxis));

                    //H1RenderUtils.DrawCapsule(commandList, parentLoc, xAxis, yAxis, zAxis, 0.03f, length / 2.0f, 16);
                    H1RenderUtils.DrawDashedLine(commandList, parentLoc, currLoc, 2.0f);
                    //H1RenderUtils.DrawWireStar(commandList, currLoc, 0.03f);
                }
            }            
        }

        // rendering resource created at import time
        private H1SkeletalMeshResource m_SkeletalMeshResource = new H1SkeletalMeshResource();
        // skeleton of this skeletal mesh
        private H1Skeleton m_Skeleton = new H1Skeleton();
        // FBoxSphereBounds
        // list of materials applied to this mesh
        private List<H1SkeletalMaterial> m_Materials = new List<H1SkeletalMaterial>();
        // struct containing information for each LOD level, such as materials to use and when use this LOD
        private List<H1SkeletalMeshLODInfo> m_LODInfos = new List<H1SkeletalMeshLODInfo>();
        // UBodySetup
        // UPhysicsAsset PhysicsAsset
        // UPhysicsAsset ShadowPhysicsAsset
        // TArray<UMorphTarget*>
        // FRenderCommandFence ReleaseResourceFence
        // ...
        // reference skeleton precomputed bases
        private List<Matrix> m_RefBasesInvMatrix = new List<Matrix>();
        // TArray<FClothingAssetData> ClothingAsset
        // TArray<UAssetUserData*> AssetUserData
        // the cached streaming texture factors
        private List<float> m_CachedStreamingTextureFactors = new List<float>();
        // array of named socket location locations, setup in editor and used as a shortcut instead of specifying everything explicitly to AttachComponent in the SkeletalMeshComponent
        private List<H1SkeletalMeshSocket> m_Sockets = new List<H1SkeletalMeshSocket>();
    }
}
