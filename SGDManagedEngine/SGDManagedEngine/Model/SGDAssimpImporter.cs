using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assimp;
using Assimp.Configs;

using SharpDX;

namespace SGDManagedEngine.SGD
{   
    public class H1AssimpImporter
    {      
        public H1AssimpImporter()
        {
            
        }

        public void Initialize()
        {
            Load("special_alduindeathagony.fbx");
            //Load("mitsuba-sphere.obj");           
        }

        private Matrix MatrixConversionFromAssimp(Matrix4x4 assimpMatrix4x4)
        {
            Vector3D translation;
            Vector3D scaling;
            Assimp.Quaternion rotation;
            assimpMatrix4x4.Decompose(out scaling, out rotation, out translation);

            Vector3 transformedTranslation = new Vector3(translation.X, translation.Y, translation.Z);
            Vector3 transformedScaling = new Vector3(scaling.X, scaling.Y, scaling.Z);
            SharpDX.Quaternion transformedRotation = new SharpDX.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);

            Matrix convertedMatrix = Matrix.Multiply(Matrix.Multiply(Matrix.Scaling(transformedScaling), Matrix.RotationQuaternion(transformedRotation)), Matrix.Translation(transformedTranslation));
            
            return convertedMatrix;
        }
              
        private Node FindNode(Node node, Predicate<Node> predicate)
        {
            Node currNode = node;
            if (predicate(node))
                return currNode;

            foreach (Node childNode in currNode.Children)
            {
                Node result = FindNode(childNode, predicate);
                if (result != null)
                    return result;
            }

            return null;
        }

        private Matrix GetOffsetMatrix(Scene scene, Int32 meshIndex)
        {           
            Node matchedNode = FindNode(scene.RootNode, x=>{
                foreach (Int32 index in x.MeshIndices)
                {
                    if (index == meshIndex)
                        return true;
                }
                return false;
            });

            // calculate global offset matrices
            Node currNode = matchedNode;
            List<Matrix> parentMatrices = new List<Matrix>();
            while (currNode != null)
            {
                parentMatrices.Add(MatrixConversionFromAssimp(currNode.Transform));
                currNode = currNode.Parent;
            }

            Matrix globalOffsetMatrix = Matrix.Identity;
            foreach (Matrix localOffsetMatrix in parentMatrices)
            {
                globalOffsetMatrix = Matrix.Multiply(globalOffsetMatrix, localOffsetMatrix);
            }            

            return globalOffsetMatrix;
        }

        private Boolean ExtractMeshData(Scene scene, ref List<H1MeshContext> meshContexts)
        {
            Int32 currMeshContextIndex = 0;
            foreach (Mesh mesh in scene.Meshes) // process each mesh in the scene
            {
                H1MeshContext currMeshContext = new H1MeshContext();

                // set mesh name
                currMeshContext.Name = mesh.Name;
                // get the offset matrix                
                Matrix globalOffsetMtx = GetOffsetMatrix(scene, currMeshContextIndex);

                // store LocalToTransform for later usage (converting offset matrix space to root node space)
                Vector3 translation;
                SharpDX.Quaternion rotation;
                Vector3 scaling;
                globalOffsetMtx.Decompose(out scaling, out rotation, out translation);

                currMeshContext.LocalToGlobalTransform.Scaling = scaling;
                currMeshContext.LocalToGlobalTransform.Rotation = rotation;
                currMeshContext.LocalToGlobalTransform.Translation = translation;

                // if mesh doesn't follow the rule below, do not process mesh in Assimp
                if (mesh.HasVertices 
                    && mesh.HasNormals 
                    && mesh.HasTangentBasis 
                    && mesh.TextureCoordinateChannels[0].Count > 0
                    && mesh.HasFaces) // process vertices in the mesh
                {
                    int vertexCount = mesh.VertexCount;
                    foreach (Vector3D vertex in mesh.Vertices)
                    {
                        Vector3 convertedVertex = new Vector3(vertex.X, vertex.Y, vertex.Z);
                                              
                        // transform with the 'offsetMtx'
                        Vector4 result = Vector3.Transform(convertedVertex, globalOffsetMtx);
                        // divide by w for homogeneous vector
                        result /= result.W;
                        convertedVertex.X = result.X;
                        convertedVertex.Y = result.Y;
                        convertedVertex.Z = result.Z;

                        currMeshContext.Positions.Add(convertedVertex);
                    }

                    foreach (Vector3D normal in mesh.Normals)
                    {
                        // @TODO - need to transform space appropriately (normal transform is applied by different algorithm! - scaling!)
                        Vector3 convertedNormal = new Vector3(normal.X, normal.Y, normal.Z);
                        currMeshContext.Normals.Add(convertedNormal);
                    }


                    foreach (Vector3D biTangent in mesh.BiTangents)
                    {
                        // @TODO - need to transform this appropriately
                        Vector3 convertedBiTangent = new Vector3(biTangent.X, biTangent.Y, biTangent.Z);
                        currMeshContext.BiTangents.Add(convertedBiTangent);
                    }

                    foreach (Vector3D tangent in mesh.Tangents)
                    {
                        // @TODO - need to transform this appropriately
                        Vector3 convertedTangent = new Vector3(tangent.X, tangent.Y, tangent.Z);
                        currMeshContext.Tangents.Add(convertedTangent);
                    }


                    // only handling one texture coordinates (first one and only UV type (not UVW type)), currently
                    if (mesh.UVComponentCount[0] == 3) // if UVW type
                        throw new Exception("invalid UV component type (UVW), please check!");

                    List<Vector2> Tecoord2Ds = currMeshContext.AddTexcoord2DBufferContext();

                    foreach (Vector3D texCoord in mesh.TextureCoordinateChannels[0])
                    {
                        Vector2 convertedTexCoord = new Vector2(texCoord.X, texCoord.Y);
                        Tecoord2Ds.Add(convertedTexCoord);
                    }
                    
                    foreach (Face face in mesh.Faces)
                    {
                        if (face.IndexCount != 3)
                        {
                            // @TODO - split polygon to triangles!
                            continue;
                            //throw new Exception("invalid index count, it is over 3, please check!");
                        }

                        foreach (UInt32 index in face.Indices)
                            currMeshContext.Indices.Add(index);
                    }

                    meshContexts.Add(currMeshContext);
                }

                currMeshContextIndex++;
            }

            return true;
        }

        private void ExtractBone(Bone bone, ref H1SkeletalContext.Joint jointData)
        {
            Vector3D translation;
            Vector3D scaling;
            Assimp.Quaternion quaternion;
            bone.OffsetMatrix.Decompose(out scaling, out quaternion, out translation);

            jointData.OffsetTransformation.Translation = new Vector3(translation.X, translation.Y, translation.Z);
            jointData.OffsetTransformation.Scaling = new Vector3(scaling.X, scaling.Y, scaling.Z);
            jointData.OffsetTransformation.Rotation = new SharpDX.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

            if (bone.HasVertexWeights)
            {
                foreach (VertexWeight vertexWeight in bone.VertexWeights)
                {
                    H1SkeletalContext.WeightedVertex outVertexWeight = new H1SkeletalContext.WeightedVertex();
                    outVertexWeight.VertexIndex = vertexWeight.VertexID;
                    outVertexWeight.Weight = vertexWeight.Weight;

                    jointData.WeightedVertices.Add(outVertexWeight);
                }
            }
        }

        private void ExtractNodeSpace(Node node, ref H1SkeletalContext.JointNode jointNode)
        {
            jointNode.JointName = node.Name;

            Vector3D translation;
            Vector3D scaling;
            Assimp.Quaternion quaternion;
            node.Transform.Decompose(out scaling, out quaternion, out translation);

            jointNode.NodeLocalTransform.Translation = new Vector3(translation.X, translation.Y, translation.Z);
            jointNode.NodeLocalTransform.Scaling = new Vector3(scaling.X, scaling.Y, scaling.Z);
            jointNode.NodeLocalTransform.Rotation = new SharpDX.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        // @TODO - enable these algorithms correctly in the near future (may be not~)
        // I found extracting skeletal mesh data with bone tree can be problem when there are multiple root bones (like head, pelvis ... etc)
        // so I disable these methods and change whole algorithms for extracting skeletal mesh data
        #region Disabled Extracting skeletal mesh data methods
        private Boolean ExtractSkeletalMeshDataRecursive(Node node, List<Bone> bones, ref H1SkeletalContext.JointNode outNode)
        {
            if (node == null) // reach to the leaf node
                return true;

            // create joint data ans set it
            //outNode.JointData = new H1SkeletalContext.Joint();
            Bone bone = bones.Find(x => (x.Name == node.Name));
            if (bone == null) // it is leaf node
                return false;

            //ExtractBone(bone, ref outNode.JointData);

            // create children
            foreach (Node child in node.Children)
            {
                H1SkeletalContext.JointNode childNode = new H1SkeletalContext.JointNode();
                ExtractSkeletalMeshDataRecursive(child, bones, ref childNode);             
                    
                // set parent
                childNode.Parent = outNode;
                outNode.Children.Add(childNode);
            }

            return true;     
        }        

        public Boolean ExtractSkeletalMeshData(Node rootBoneNode, List<Bone> bones, ref H1SkeletalContext newContext)
        {
            H1SkeletalContext.JointNode rootNode = new H1SkeletalContext.JointNode();
            ExtractSkeletalMeshDataRecursive(rootBoneNode, bones, ref rootNode);
            newContext.Root = rootNode;

            return true;            
        }

        public Node FindRootBone(List<Bone> bones, Node currNode)
        {
            Bone bone = bones.Find(x => (x.Name == currNode.Name));
            if (bone != null)
                return currNode;

            Node result = null;
            foreach (Node child in currNode.Children)
            {
                result = FindRootBone(bones, child);
                if (result != null)
                    return result;
            }

            return result;
        }
        #endregion

        public bool Load(String fileName)
        {
            // @TODO - change the file path for asset folder in the future currently, set as executable path
            String path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets\\");
            m_FileName = path + fileName;

            // setting assimp context
            AssimpContext importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));

            // loading scene
            Scene scene = null;
            try
            {
                scene = importer.ImportFile(m_FileName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.FlipWindingOrder);
            }
            catch (Exception e) // handling assimp exception
            {
                if (e.Source != null)
                    Console.WriteLine("Error: {0}", e.Message);
            }

            asset = new H1AssetContext();
            H1ModelContext convertedModel = asset.AddModel();

            // extract the meshes
            if (scene.HasMeshes)
            {
                Int32 meshCount = scene.Meshes.Count;
                List<H1MeshContext> meshContexts = new List<H1MeshContext>();           

                ExtractMeshData(scene, ref meshContexts);

                foreach(H1MeshContext meshContext in meshContexts)
                    convertedModel.Meshes.Add(meshContext);
            }

            // extract skeletal data

            // what I learned from this failed algorithm
            // 1. root bone nodes can be multiple
            // 2. for example, blabla_Hub(containing null meshes, only space infos) have null information for weighted vertices, but it should not be considered as leaf node!
            #region Disabled Extracting skeletal mesh data methods            
            /*if (scene.HasMeshes)
            {
                // collect all bones from meshes
                List<Bone> bones = new List<Bone>();
                foreach (Mesh mesh in scene.Meshes)
                {
                    if (mesh.HasBones)
                    {
                        bones.AddRange(mesh.Bones);
                    }
                }

                // find root node for all meshes
                Node boneRootNode = FindRootBone(bones, scene.RootNode);

                H1SkeletalContext skeletalContext = null;
                skeletalContext = new H1SkeletalContext();
                //@TODO - need to optimize for deep copies
                ExtractSkeletalMeshData(boneRootNode, bones, ref skeletalContext);

                convertedModel.SkeletalContexts.Add(skeletalContext);
            }*/
            #endregion

            // what I learned from this failed algorithm
            // 1. the counts of bones and bone nodes (joint nodes) can be different
            // 2. bone node can be used for just transforming spaces
            // 3. bone node could have empty meshes (just includes space information)
            // 4. bones are subsidiary for bone nodes
            // 5. don't consider aiBone as bone node in hierarchy rather regards node(aiNode) as bone node in hierarchy
            // 6. consider aiBone as node bone data containing all necessary data
            #region Failed Algorithm
            // new algorithms to handle multiple root node (like pelvis and head) more efficient way
            /*if (scene.HasMeshes)
            {
                // 1. extracted all bones for all meshes in the scene
                // NOTICE - multiple meshes in file are considered in same body but chunked several parts
                Dictionary<String, Bone> extractedBones = new Dictionary<String, Bone>();
                Dictionary<String, H1SkeletalContext.JointNode> jointNodes = new Dictionary<string, H1SkeletalContext.JointNode>();
                foreach (Mesh mesh in scene.Meshes)
                {
                    if (mesh.HasBones)
                    {
                        foreach (Bone bone in mesh.Bones)
                        {
                            String boneName = bone.Name;
                            if (!extractedBones.ContainsKey(boneName))
                            {
                                extractedBones.Add(boneName, bone);

                                H1SkeletalContext.JointNode newJointNode = new H1SkeletalContext.JointNode();
                                newJointNode.JointData.JointName = boneName;
                                jointNodes.Add(boneName, newJointNode);
                            }                               
                        }
                    }
                }

                // 2. process extracted bones
                // @TODO - naive algorithm need to optimize by searching tree and set the bones parent
                foreach (KeyValuePair<String, Bone> extractedBone in extractedBones)
                {
                    Node node = scene.RootNode.FindNode(extractedBone.Key);
                    H1SkeletalContext.JointNode jointNode = jointNodes[node.Name];
                    H1SkeletalContext.JointNode parentJointNode = jointNodes[node.Parent.Name];

                    // set proper properties
                    jointNode.Parent = parentJointNode;
                    foreach (Node child in node.Children)
                    {
                        H1SkeletalContext.JointNode childJointNode = null;
                        if (jointNodes.TryGetValue(child.Name, out childJointNode))
                        {
                            jointNode.Children.Add(childJointNode);
                        }
                    }
                    ExtractBone(extractedBone.Value, ref jointNode.JointData);
                }

                // 3. find root node(s) and set them at the front
            }*/
            #endregion

            if (scene.HasMeshes)
            {
                H1SkeletalContext skeletalContext = null;
                skeletalContext = new H1SkeletalContext();

                // prepare data for BFS search
                List<H1SkeletalContext.JointNode> jointNodes = skeletalContext.JointNodes;
                Dictionary<String, Int32> jointNameToJointNodeIndex = new Dictionary<String, Int32>();

                // extractedBones could have same bone name and multiple bone data
                Dictionary<String, List<Bone>> extractedBones = new Dictionary<String, List<Bone>>();
                // boneToMeshContexIndex should be mirrored same as extractedBones (index of list should be same)
                Dictionary<String, List<Int32>> boneToMeshContextIndex = new Dictionary<String, List<Int32>>();

                #region Debug Validation Weight Vertex Counts
#if DEBUG
                List<Boolean> taggedVertexIndex = new List<Boolean>();      // verification to tag all needed vertices 
                List<float> taggedVertexWeights = new List<float>();        // verification to tag all vertex weight for sum of each vertex
                List<Int32> taggedVertexIndexOffset = new List<Int32>();

                // pre-test for validation
                foreach (Mesh mesh in scene.Meshes)
                {
                    if (mesh.HasVertices)
                    {
                        foreach (Vector3D vertex in mesh.Vertices)
                        {
                            taggedVertexIndex.Add(false);                            
                        }
                    }

                    if (mesh.HasBones)
                    {
                        foreach (Bone bone in mesh.Bones)
                        {
                            if (bone.HasVertexWeights)
                            {
                                foreach (VertexWeight vertexWeight in bone.VertexWeights)
                                    taggedVertexIndex[vertexWeight.VertexID] = true;
                            }
                        }
                    }

                    List<Int32> notTaggedVertexIndex0 = new List<Int32>();
                    Int32 currVertexIndex0 = 0;
                    foreach (Boolean isTagged in taggedVertexIndex)
                    {
                        if (isTagged == false)
                            notTaggedVertexIndex0.Add(currVertexIndex0);
                        currVertexIndex0++;
                    }

                    if (notTaggedVertexIndex0.Count > 0)
                    {
                        return false;
                    }

                    taggedVertexIndex.Clear();
                }
#endif
                #endregion

                Int32 validMeshContextIndex = 0;
                foreach (Mesh mesh in scene.Meshes)
                {
                    if (mesh.HasVertices
                    && mesh.HasNormals
                    && mesh.HasTangentBasis
                    && mesh.TextureCoordinateChannels[0].Count > 0
                    && mesh.HasFaces) // process vertices in the mesh
                    {
                        if (mesh.HasBones)
                        {
                            foreach (Bone bone in mesh.Bones)
                            {
                                String boneName = bone.Name;
                                if (!extractedBones.ContainsKey(boneName))
                                {
                                    // add new list of bone list
                                    extractedBones.Add(boneName, new List<Bone>());
                                    extractedBones[boneName].Add(bone);
                                    // add new list of mesh context index
                                    boneToMeshContextIndex.Add(boneName, new List<Int32>());
                                    boneToMeshContextIndex[boneName].Add(validMeshContextIndex);
                                }
                                else // same bone name, but different bone data exists
                                {
                                    // no need to create list of data, just add new item
                                    // bone data could have different set of weighted vertices, but with same bone name
                                    extractedBones[boneName].Add(bone);
                                    boneToMeshContextIndex[boneName].Add(validMeshContextIndex);
                                }
                            }

                            validMeshContextIndex++;

                            #region Debug Validation Weight Vertex Count
#if DEBUG
                            taggedVertexIndexOffset.Add(taggedVertexIndex.Count);
                            for (Int32 i = 0; i < mesh.VertexCount; ++i)
                            {
                                taggedVertexIndex.Add(false);
                                taggedVertexWeights.Add(0.0f);
                            }
#endif
                            #endregion
                        }
                    }
                }

                // BFS search to construct bone data
                Stack<Node> nodes = new Stack<Node>();
                nodes.Push(scene.RootNode);

                while (nodes.Count != 0)
                {
                    Node currNode = nodes.Pop();
                    H1SkeletalContext.JointNode jointNode = new H1SkeletalContext.JointNode();

                    H1SkeletalContext.JointNode parentJointNode = jointNodes.Find(x => (x.JointName == currNode.Parent.Name));
                    jointNode.Parent = parentJointNode;
                    ExtractNodeSpace(currNode, ref jointNode);

                    List<Bone> boneDataList = null;
                    List<Int32> meshContextIndexList = null;
                    if (extractedBones.TryGetValue(jointNode.JointName, out boneDataList))
                    {
                        meshContextIndexList = boneToMeshContextIndex[jointNode.JointName];
                        
                        // looping bone data list, extract bone data
                        Int32 currBoneIndex = 0;
                        foreach (Bone bone in boneDataList)
                        {
                            H1SkeletalContext.Joint newJointData = new H1SkeletalContext.Joint();
                            ExtractBone(bone, ref newJointData);

                            newJointData.MeshContextIndex = meshContextIndexList[currBoneIndex];
                            currBoneIndex++;

                            // store mesh context local-to-global H1Transform for later transformation of offsetMatrix for animation
                            newJointData.MeshContextLocalToGlobal = convertedModel.Meshes[newJointData.MeshContextIndex].LocalToGlobalTransform;

                            // add new joint data
                            jointNode.JointDataList.Add(newJointData);
                        }

                        // mark this node is bone-space
                        jointNode.MarkedAsBoneSpace = true;

                        // tag its parent until it reaches the state that 'MarkedAsBoneSpace' is true
                        H1SkeletalContext.JointNode markNode = jointNode.Parent;
                        while (markNode != null && markNode.MarkedAsBoneSpace != true)
                        {
                            markNode.MarkedAsBoneSpace = true;
                            markNode = markNode.Parent;
                        }
                    }
                    else
                    {
                        // for debugging
                    }

                    foreach (Node child in currNode.Children)
                    {
                        nodes.Push(child);
                        // tag child names to process child nodes after BFS search
                        jointNode.ChildNodeNames.Add(child.Name);
                    }

                    // add new joint node
                    Int32 newJointNodeIndex = jointNodes.Count;
                    jointNodes.Add(jointNode);
                    jointNameToJointNodeIndex.Add(jointNode.JointName, newJointNodeIndex);
                }

                // process tagged child nodes
                foreach (H1SkeletalContext.JointNode node in jointNodes)
                    foreach (String childName in node.ChildNodeNames)
                        node.Children.Add(jointNodes[jointNameToJointNodeIndex[childName]]);

                #region Debug Validation for total weight value of weighted vertices & vertex counts
#if DEBUG
                foreach (H1SkeletalContext.JointNode node in jointNodes)
                {
                    foreach (H1SkeletalContext.Joint jointData in node.JointDataList)
                    {
                        foreach (H1SkeletalContext.WeightedVertex weightedVertex in jointData.WeightedVertices)
                        {
                            // confirm all vertices in Mesh are tagged by weighted vertices
                            taggedVertexIndex[taggedVertexIndexOffset[jointData.MeshContextIndex] + weightedVertex.VertexIndex] = true;
                            // add vertex weight to verify
                            taggedVertexWeights[taggedVertexIndexOffset[jointData.MeshContextIndex] + weightedVertex.VertexIndex] += weightedVertex.Weight;
                        }
                    }
                }

                // verification code to extract not tagged vertex index
                List<Int32> notTaggedVertexIndex = new List<Int32>();
                Int32 currVertexIndex = 0;
                foreach (Boolean isTagged in taggedVertexIndex)
                {
                    if (isTagged == false)
                        notTaggedVertexIndex.Add(currVertexIndex);
                    currVertexIndex++;
                }
                if (notTaggedVertexIndex.Count > 0)
                {
                    return false;
                }

                // verification code to extract vertex which has invalid vertex weight value
                List<Int32> invalidVertexWeightVertexIndex = new List<Int32>();
                currVertexIndex = 0;
                foreach (float currVertexWeights in taggedVertexWeights)
                {
                    if (currVertexWeights < 0.99f)
                        invalidVertexWeightVertexIndex.Add(currVertexIndex);
                }
                if (invalidVertexWeightVertexIndex.Count > 0)
                {
                    return false;
                }
#endif
                #endregion

                convertedModel.SkeletalContexts.Add(skeletalContext);
            }
            
            // extract the animations
            if (scene.HasAnimations)
            {
                H1AnimationContext newAnimContext = new H1AnimationContext();
                for (Int32 animIndex = 0; animIndex < scene.AnimationCount; ++animIndex)
                {
                    Animation currAnimation = scene.Animations[animIndex];                                        
                    if (currAnimation.HasNodeAnimations) // we only handle node animations (not vertex animation)
                    {
                        // create new animation sequence
                        H1AnimationContext.AnimSequence newAnimSeq = new H1AnimationContext.AnimSequence();
                        newAnimSeq.AnimSeqName = currAnimation.Name;
                        newAnimSeq.Duration = currAnimation.DurationInTicks;
                        newAnimSeq.TicksPerSecond = currAnimation.TicksPerSecond;

                        foreach (NodeAnimationChannel animChannel in currAnimation.NodeAnimationChannels)
                        {
                            H1AnimationContext.JointAnimation newJointAnim = new H1AnimationContext.JointAnimation();
                            newJointAnim.BoneName = animChannel.NodeName;

                            // calculate maximum number of keys to fill up the special case (only holding one key)
                            Int32 maxNumKeys = Math.Max(Math.Max(animChannel.ScalingKeyCount, animChannel.RotationKeyCount), animChannel.PositionKeyCount);

                            if (animChannel.HasPositionKeys)
                            {
                                if (animChannel.PositionKeyCount == 1) // special handling (only holding one key)
                                {
                                    VectorKey positionKey = animChannel.PositionKeys[0];
                                    for (Int32 numKey = 0; numKey < maxNumKeys; ++numKey)
                                    {
                                        H1AnimationContext.PositionKey newPosKey = new H1AnimationContext.PositionKey();
                                        newPosKey.Time = positionKey.Time;
                                        newPosKey.Value = new Vector3(positionKey.Value.X, positionKey.Value.Y, positionKey.Value.Z);

                                        newJointAnim.PosKeys.Add(newPosKey);
                                    }
                                }

                                else
                                {
                                    foreach (VectorKey positionKey in animChannel.PositionKeys)
                                    {
                                        H1AnimationContext.PositionKey newPosKey = new H1AnimationContext.PositionKey();
                                        newPosKey.Time = positionKey.Time;
                                        newPosKey.Value = new Vector3(positionKey.Value.X, positionKey.Value.Y, positionKey.Value.Z);

                                        newJointAnim.PosKeys.Add(newPosKey);
                                    }
                                }
                            }

                            if (animChannel.HasRotationKeys)
                            {
                                if (animChannel.RotationKeyCount == 1) // special handling (only holding one key)
                                {
                                    QuaternionKey quatKey = animChannel.RotationKeys[0];
                                    for (Int32 numKey = 0; numKey < maxNumKeys; ++numKey)
                                    {
                                        H1AnimationContext.QuaternionKey newQuatKey = new H1AnimationContext.QuaternionKey();
                                        newQuatKey.Time = quatKey.Time;
                                        newQuatKey.Value = new SharpDX.Quaternion(quatKey.Value.X, quatKey.Value.Y, quatKey.Value.Z, quatKey.Value.W);

                                        newJointAnim.RotKeys.Add(newQuatKey);
                                    }
                                }

                                else
                                {
                                    foreach (QuaternionKey quatKey in animChannel.RotationKeys)
                                    {
                                        H1AnimationContext.QuaternionKey newQuatKey = new H1AnimationContext.QuaternionKey();
                                        newQuatKey.Time = quatKey.Time;
                                        newQuatKey.Value = new SharpDX.Quaternion(quatKey.Value.X, quatKey.Value.Y, quatKey.Value.Z, quatKey.Value.W);

                                        newJointAnim.RotKeys.Add(newQuatKey);
                                    }
                                }                                
                            }

                            if (animChannel.HasScalingKeys)
                            {
                                if (animChannel.ScalingKeyCount == 1) // special handling (only holding one key)
                                {
                                    VectorKey scalingKey = animChannel.ScalingKeys[0];
                                    for (Int32 numKey = 0; numKey < maxNumKeys; ++numKey)
                                    {
                                        H1AnimationContext.ScalingKey newScalingKey = new H1AnimationContext.ScalingKey();
                                        newScalingKey.Time = scalingKey.Time;
                                        newScalingKey.Value = new Vector3(scalingKey.Value.X, scalingKey.Value.Y, scalingKey.Value.Z);

                                        newJointAnim.ScaleKeys.Add(newScalingKey);
                                    }
                                }

                                else
                                {
                                    foreach (VectorKey scalingKey in animChannel.ScalingKeys)
                                    {
                                        H1AnimationContext.ScalingKey newScalingKey = new H1AnimationContext.ScalingKey();
                                        newScalingKey.Time = scalingKey.Time;
                                        newScalingKey.Value = new Vector3(scalingKey.Value.X, scalingKey.Value.Y, scalingKey.Value.Z);

                                        newJointAnim.ScaleKeys.Add(newScalingKey);
                                    }
                                }                                
                            }

                            newAnimSeq.BoneAnimations.Add(newJointAnim);
                        }

                        newAnimContext.AnimSequences.Add(newAnimSeq);
                    }
                }

                // set the animation context
                convertedModel.AnimationContext = newAnimContext;
            }

            return true;
        }

        //@TODO - temporary
        public H1AssetContext asset;
        private String m_FileName;         
    }
}
