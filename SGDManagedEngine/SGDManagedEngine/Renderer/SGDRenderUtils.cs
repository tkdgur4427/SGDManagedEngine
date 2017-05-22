using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public static class H1RenderUtils
    {
        public class H1DynamicMeshVertex
        {
            public Vector4 Position;
            public Vector2 Texcoord;
            public Vector4 TangentX;
            public Vector4 TangentY;
            public Vector4 TangentZ;
            public Vector4 Color;
        }

        public class H1DynamicMeshVertexFactory : H1LocalVertexFactory
        {
            public H1DynamicMeshVertexFactory()
                : base()
            {

            } 
        }

        public class H1DynamicMeshBuilder : IDisposable
        {
            public void Dispose()
            {
                // release all vertex buffers and index buffer
                m_PositionVertexBuffer.Resource.Dispose();
                m_NormalVertexBuffer.Resource.Dispose();
                m_Texcoord2DVertexBuffer.Resource.Dispose();
                m_Texcoord2DVertexBuffer.Resource.Dispose();
                m_IndexBuffer.Resource.Dispose();
            }

            public H1DynamicMeshVertexFactory VertexFactory
            {
                get { return m_VertexFactory; }
            }

            public H1IndexBuffer IndexBuffer
            {
                get { return m_IndexBuffer; }
            }

            public Int32 AddVertex(H1DynamicMeshVertex vertex)
            {
                Int32 newIndex = m_Vertices.Count;
                m_Vertices.Add(vertex);
                return newIndex;
            }

            public void AddVertices(IEnumerable<H1DynamicMeshVertex> vertices)
            {
                m_Vertices.AddRange(vertices);
            }

            public void AddTriangle(Int32 v0, Int32 v1, Int32 v2)
            {
                m_Indices.Add(v0);
                m_Indices.Add(v1);
                m_Indices.Add(v2);
            }

            public void AddTriangles(IEnumerable<Int32> indices)
            {
                m_Indices.AddRange(indices);
            }

            // @TODO - need to remove this method (it is just temporary)
            public void AddLine(Vector4 start, Vector4 end, Vector4 color)
            {
                H1DynamicMeshVertex vert0 = new H1DynamicMeshVertex();
                vert0.Position = start;
                vert0.Color = color;

                H1DynamicMeshVertex vert1 = new H1DynamicMeshVertex();
                vert1.Position = end;
                vert1.Color = color;

                Int32 vertIndex = m_Vertices.Count;
                m_Vertices.Add(vert0);
                m_Vertices.Add(vert1);

                m_Indices.Add(vertIndex);
                m_Indices.Add(vertIndex + 1);
            }

            public void GenerateVertexIndexBuffersAndVertexDeclaration()
            {
                // create vertex buffers
                List<Vector3> positions = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<Vector2> texcoords = new List<Vector2>();
                List<Vector4> colors = new List<Vector4>();

                foreach (H1DynamicMeshVertex dynamicMeshVertex in m_Vertices)
                {
                    positions.Add(new Vector3(dynamicMeshVertex.Position.X, dynamicMeshVertex.Position.Y, dynamicMeshVertex.Position.Z));
                    normals.Add(new Vector3(dynamicMeshVertex.TangentZ.X, dynamicMeshVertex.TangentZ.Y, dynamicMeshVertex.TangentZ.Z));
                    texcoords.Add(dynamicMeshVertex.Texcoord);
                    colors.Add(dynamicMeshVertex.Color);
                }

                // create vertex buffers
                m_PositionVertexBuffer = H1VertexBuffer.ProcessVertexBuffer(positions.ToArray());
                m_NormalVertexBuffer = H1VertexBuffer.ProcessVertexBuffer(normals.ToArray());
                m_Texcoord2DVertexBuffer = H1VertexBuffer.ProcessVertexBuffer(texcoords.ToArray());
                m_ColorBuffer = H1VertexBuffer.ProcessVertexBuffer(colors.ToArray());
                
                m_VertexFactory.PositionVertexBuffer = m_PositionVertexBuffer;
                m_VertexFactory.NormalVertexBuffer = m_NormalVertexBuffer;
                m_VertexFactory.TexcoordVertexBuffers.Add(m_Texcoord2DVertexBuffer);
                m_VertexFactory.ColorVertexBuffer = m_ColorBuffer;

                // set vertex stream components
                // offset tracking
                Int32 currOffset = 0;
                // 1. position
                m_VertexFactory.PositionStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Position, H1VertexElementType.Float3, currOffset++);
                // 2. normal
                m_VertexFactory.NormalStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Normal, H1VertexElementType.Float3, currOffset++);
                // 3. texcoord
                m_VertexFactory.AddTexcoordStreamComponent(new H1VertexStreamComponent(H1VertexStreamSematicType.Texcoord, H1VertexElementType.Float2, currOffset++));
                // 4. color
                m_VertexFactory.ColorStreamComponent = new H1VertexStreamComponent(H1VertexStreamSematicType.Color, H1VertexElementType.Float4, 4);

                // generate vertex declaration
                m_VertexFactory.GenerateVertexDeclaration();

                // create index buffers
                List<UInt32> convertedIndices = new List<UInt32>();
                foreach (Int32 index in m_Indices)
                    convertedIndices.Add(Convert.ToUInt32(index));

                m_IndexBuffer = H1IndexBuffer.ProcessIndexBuffer(convertedIndices.ToArray());
            }

            public void Draw()
            {

            }

            private List<H1DynamicMeshVertex> m_Vertices = new List<H1DynamicMeshVertex>();
            private List<Int32> m_Indices = new List<Int32>();

            private H1DynamicMeshVertexFactory m_VertexFactory = new H1DynamicMeshVertexFactory();

            // real buffer object holders
            private H1VertexBuffer m_PositionVertexBuffer;
            private H1VertexBuffer m_NormalVertexBuffer;
            private H1VertexBuffer m_Texcoord2DVertexBuffer;
            private H1VertexBuffer m_ColorBuffer;

            private H1IndexBuffer m_IndexBuffer;
        }

        static public void DrawPlane10x10(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector2 UVMin, Vector2 UVMax)
        {
            // TileCount * TileCount * 2 triangles
            const UInt32 tileCount = 10;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();

            float step = 2.0f / tileCount;

            for (Int32 z = 0; z < tileCount; ++z)
            {
                // implement this way to avoid cracks, could be optimized
                float z0 = z * step - 1.0f;
                float z1 = (z + 1) * step - 1.0f;

                float v0 = MathUtil.Lerp(UVMin.Y, UVMax.Y, z0 * 0.5f + 0.5f);
                float v1 = MathUtil.Lerp(UVMin.Y, UVMax.Y, z1 * 0.5f + 0.5f);

                for(Int32 x = 0; x < tileCount; ++x)
                {
                    // implemented this way to avoid cracks, could be optimized
                    float x0 = Convert.ToSingle(x) * step - 1.0f;
                    float x1 = Convert.ToSingle(x + 1) * step - 1.0f;

                    float u0 = MathUtil.Lerp(UVMin.X, UVMax.X, x0 * 0.5f + 0.5f);
                    float u1 = MathUtil.Lerp(UVMin.X, UVMax.X, x1 * 0.5f + 0.5f);

                    meshBuilder.AddVertex(new H1DynamicMeshVertex()
                    {
                        Position = new Vector4(x0, 0, z0, 1),
                        Texcoord = new Vector2(u0, v0),
                        TangentX = new Vector4(1, 0, 0, 0),
                        TangentY = new Vector4(0, 1, 0, 0),
                        TangentZ = new Vector4(0, 0, 1, 0),
                        Color = new Vector4(1, 1, 1, 1),
                    });
                    meshBuilder.AddVertex(new H1DynamicMeshVertex()
                    {
                        Position = new Vector4(x0, 0, z1, 1),
                        Texcoord = new Vector2(u0, v1),
                        TangentX = new Vector4(1, 0, 0, 0),
                        TangentY = new Vector4(0, 1, 0, 0),
                        TangentZ = new Vector4(0, 0, 1, 0),
                        Color = new Vector4(1, 1, 1, 1),
                    });
                    meshBuilder.AddVertex(new H1DynamicMeshVertex()
                    {
                        Position = new Vector4(x1, 0, z1, 1),
                        Texcoord = new Vector2(u1, v1),
                        TangentX = new Vector4(1, 0, 0, 0),
                        TangentY = new Vector4(0, 1, 0, 0),
                        TangentZ = new Vector4(0, 0, 1, 0),
                        Color = new Vector4(1, 1, 1, 1),
                    });
                    meshBuilder.AddVertex(new H1DynamicMeshVertex()
                    {
                        Position = new Vector4(x1, 0, z0, 1),
                        Texcoord = new Vector2(u1, v0),
                        TangentX = new Vector4(1, 0, 0, 0),
                        TangentY = new Vector4(0, 1, 0, 0),
                        TangentZ = new Vector4(0, 0, 1, 0),
                        Color = new Vector4(1, 1, 1, 1),
                    });

                    Int32 index = Convert.ToInt32(x + z * tileCount) * 4;
                    meshBuilder.AddTriangle(index + 0, index + 1, index + 2);
                    meshBuilder.AddTriangle(index + 0, index + 2, index + 3);
                }
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        // @TODO - I need to wrap command list properly!
        static public void DrawBox(SharpDX.Direct3D12.GraphicsCommandList commandList, H1Transform BoxToWorld)
        {
            List<Vector3> positions = new List<Vector3>();
            positions.Add(new Vector3(-1, +1, -1));
            positions.Add(new Vector3(-1, +1, +1));
            positions.Add(new Vector3(+1, +1, +1));
            positions.Add(new Vector3(+1, +1, -1));

            List<Vector2> texcoords = new List<Vector2>();
            texcoords.Add(new Vector2(0, 0));
            texcoords.Add(new Vector2(1, 0));
            texcoords.Add(new Vector2(1, 1));
            texcoords.Add(new Vector2(0, 1));

            // rotate each face 6 times
            Matrix[] faceRotations = new Matrix[6];
            AngleSingle angle180 = new AngleSingle(180, AngleType.Degree);
            AngleSingle angle90 = new AngleSingle(90, AngleType.Degree);
            faceRotations[0] = Matrix.RotationYawPitchRoll(0, 0, 0);
            faceRotations[1] = Matrix.RotationYawPitchRoll(0, angle90.Radians, 0);
            faceRotations[2] = Matrix.RotationYawPitchRoll(0, -angle90.Radians, 0);
            faceRotations[3] = Matrix.RotationYawPitchRoll(0, 0, angle90.Radians);
            faceRotations[4] = Matrix.RotationYawPitchRoll(0, 0, -angle90.Radians);
            faceRotations[5] = Matrix.RotationYawPitchRoll(0, angle180.Radians, 0);

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();

            for (Int32 f = 0; f < 6; ++f)
            {
                Matrix faceTransform = faceRotations[f];

                Int32[] VertexIndices = new Int32[4];
                for (Int32 vertexIndex = 0; vertexIndex < VertexIndices.Count(); ++vertexIndex)
                {
                    VertexIndices[vertexIndex] = meshBuilder.AddVertex(
                        new H1DynamicMeshVertex()
                        {
                            Position = Vector3.Transform(positions[vertexIndex], faceTransform),
                            Texcoord = texcoords[vertexIndex],
                            TangentX = new Vector4(Vector3.TransformNormal(new Vector3(1, 0, 0), faceTransform), 0),
                            TangentY = new Vector4(Vector3.TransformNormal(new Vector3(0, 1, 0), faceTransform), 0),
                            TangentZ = new Vector4(Vector3.TransformNormal(new Vector3(0, 0, 1), faceTransform), 0),
                            Color = new Vector4(1, 1, 1, 1), // white color
                        });
                }

                meshBuilder.AddTriangle(VertexIndices[0], VertexIndices[1], VertexIndices[2]);
                meshBuilder.AddTriangle(VertexIndices[0], VertexIndices[2], VertexIndices[3]);
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawSphere(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 center, Vector3 radii, Int32 numSides, Int32 numRings)
        {
            // use a mesh builder to draw the sphere
            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();

            // the first/last arc are on top of each other
            Int32 numVerts = (numSides + 1) * (numRings + 1);

            // calculate verts for one arc
            List<H1DynamicMeshVertex> arcVerts = new List<H1DynamicMeshVertex>();

            for (Int32 i = 0; i < numRings + 1; ++i)
            {
                H1DynamicMeshVertex arcVert = new H1DynamicMeshVertex();
                float angle = ((float)i / numRings) * MathUtil.Pi;

                // note - unit sphere, so position always has mag of one we can just use it for normal!
                arcVert.Position.X = 0.0f;
                arcVert.Position.Y = Convert.ToSingle(Math.Sin(Convert.ToDouble(angle)));
                arcVert.Position.Z = Convert.ToSingle(Math.Cos(Convert.ToDouble(angle)));

                arcVert.TangentX = new Vector4(1, 0, 0, 0);
                arcVert.TangentY = new Vector4(0, -arcVert.Position.Z, arcVert.Position.Y, 0);
                arcVert.TangentZ = new Vector4(arcVert.Position.X, arcVert.Position.Y, arcVert.Position.Z, 0);

                arcVert.Texcoord.X = 0.0f;
                arcVert.Texcoord.Y = Convert.ToSingle(i) / numRings;

                arcVerts.Add(arcVert);
            }

            // then rotate this arc numSides+1 times
            List<H1DynamicMeshVertex> Verts = new List<H1DynamicMeshVertex>();
            for (Int32 i = 0; i < numVerts; ++i)
                Verts.Add(new H1DynamicMeshVertex());

            for (Int32 s = 0; s < numSides + 1; ++s)
            {
                AngleSingle angle = new AngleSingle(360.0f * Convert.ToSingle(s) / numSides, AngleType.Degree);
                Matrix arcRotation = Matrix.RotationYawPitchRoll(0, angle.Radians, 0);
                float texcoordX = (Convert.ToSingle(s) / numSides);

                for(Int32 v = 0; v < numRings+1; v++)
                {
                    Int32 VIx = (numRings + 1) * s + v;
                    Verts[VIx].Position = Vector4.Transform(arcVerts[v].Position, arcRotation);
                    Verts[VIx].TangentX = Vector4.Transform(arcVerts[v].TangentX, arcRotation);
                    Verts[VIx].TangentY = Vector4.Transform(arcVerts[v].TangentY, arcRotation);
                    Verts[VIx].TangentZ = Vector4.Transform(arcVerts[v].TangentZ, arcRotation);
                    Verts[VIx].Texcoord.X = texcoordX;
                    Verts[VIx].Texcoord.Y = arcVerts[v].Texcoord.Y;
                }
            }

            // add all of the vertices we generated to the mesh builder
            for (Int32 vertIdx = 0; vertIdx < numVerts; vertIdx++)
            {
                meshBuilder.AddVertex(Verts[vertIdx]);
            }

            // add all of the triangles we generated to the mesh builder
            for (Int32 s = 0; s < numSides; s++)
            {
                Int32 a0start = (s + 0) * (numRings + 1);
                Int32 a1start = (s + 1) * (numRings + 1);

                for (Int32 r = 0; r < numRings; r++)
                {
                    meshBuilder.AddTriangle(a0start + r + 0, a1start + r + 0, a0start + r + 1);
                    meshBuilder.AddTriangle(a1start + r + 0, a1start + r + 1, a0start + r + 1);
                }
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static private Vector3 CalcConeVert(float angle1, float angle2, float azimuthAngle)
        {
            float ang1 = MathUtil.Clamp(angle1, 0.001f, (float)MathUtil.Pi - 0.01f);
            float ang2 = MathUtil.Clamp(angle2, 0.001f, (float)MathUtil.Pi - 0.01f);

            float sinX_2 = Convert.ToSingle(Math.Sin(0.5f * ang1));
            float sinY_2 = Convert.ToSingle(Math.Sin(0.5f * ang2));

            float sinSqX_2 = sinX_2 * sinX_2;
            float sinSqY_2 = sinY_2 * sinY_2;

            float tanX_2 = Convert.ToSingle(Math.Tan(0.5f * ang1));
            float tanY_2 = Convert.ToSingle(Math.Tan(0.5f * ang2));

            float phi = Convert.ToSingle(Math.Atan2(Math.Sin(azimuthAngle) * sinY_2, Math.Cos(azimuthAngle) * sinX_2));
            float sinPhi = Convert.ToSingle(Math.Sin(phi));
            float cosPhi = Convert.ToSingle(Math.Cos(phi));
            float sinSqPhi = sinPhi * sinPhi;
            float cosSqPhi = cosPhi * cosPhi;

            float rSq, r, Sqr, alpha, beta;

            rSq = sinSqX_2 * sinSqY_2 / (sinSqX_2 * sinSqPhi + sinSqY_2 * cosSqPhi);
            r = Convert.ToSingle(Math.Sqrt(rSq));
            Sqr = Convert.ToSingle(Math.Sqrt(1 - rSq));
            alpha = r * cosPhi;
            beta = r * sinPhi;

            Vector3 coneVert = new Vector3((1 - 2 * rSq), 2 * Sqr * alpha, 2 * Sqr * beta);
            return coneVert;
        }

        static private void BuildConeVerts(float angle1, float angle2, float scale, float xOffset, Int32 numSides, ref List<H1DynamicMeshVertex> outVerts, ref List<Int32> outIndices)
        {
            List<Vector3> coneVerts = new List<Vector3>();
            for (Int32 i = 0; i < numSides; ++i)
            {
                float fraction = Convert.ToSingle(i) / numSides;
                float azi = 2.0f * MathUtil.Pi * fraction;
                coneVerts.Add(CalcConeVert(angle1, angle2, azi) * scale + new Vector3(xOffset, 0, 0));
            }

            for (Int32 i = 0; i < numSides; ++i)
            {
                // normal of the current face
                Vector3 triTangentZ = Vector3.Cross(coneVerts[(i + 1) % numSides], coneVerts[i]); // aka triangle normal
                Vector3 triTangentY = coneVerts[i];
                Vector3 triTangentX = Vector3.Cross(triTangentZ, triTangentY);

                H1DynamicMeshVertex v0 = new H1DynamicMeshVertex();
                H1DynamicMeshVertex v1 = new H1DynamicMeshVertex();
                H1DynamicMeshVertex v2 = new H1DynamicMeshVertex();

                v0.Position = new Vector4(new Vector3(0) + new Vector3(xOffset, 0, 0), 1);
                v0.Texcoord.X = 0.0f;
                v0.Texcoord.Y = Convert.ToSingle(i) / numSides;
                v0.TangentX = new Vector4(triTangentX, 0);
                v0.TangentY = new Vector4(triTangentY, 0);
                v0.TangentZ = new Vector4(new Vector3(-1, 0, 0), 0);

                Int32 i0 = outVerts.Count;
                outVerts.Add(v0);

                v1.Position = new Vector4(coneVerts[i], 1);
                v1.Texcoord.X = 1.0f;
                v1.Texcoord.Y = Convert.ToSingle(i) / numSides;
                Vector3 triTangentZPrev = Vector3.Cross(coneVerts[i], coneVerts[i == 0 ? numSides - 1 : i - 1]); // normal of the previous face connected to this face
                v1.TangentX = new Vector4(triTangentX, 0);
                v1.TangentY = new Vector4(triTangentY, 0);
                Vector3 TangentZv3 = triTangentZPrev + triTangentZ;
                TangentZv3.Normalize();
;               v1.TangentZ = new Vector4(TangentZv3, 0);

                Int32 i1 = outVerts.Count;
                outVerts.Add(v1);

                v2.Position = new Vector4(coneVerts[(i + 1) % numSides], 1);
                v2.Texcoord.X = 1.0f;
                v2.Texcoord.Y = Convert.ToSingle((i + 1) % numSides) / numSides;
                Vector3 triTangentNext = Vector3.Cross(coneVerts[(i + 2) % numSides], coneVerts[(i + 1) % numSides]);
                v2.TangentX = new Vector4(triTangentX, 0);
                v2.TangentY = new Vector4(triTangentY, 0);
                TangentZv3 = triTangentNext + triTangentZ;
                TangentZv3.Normalize();
                v2.TangentZ = new Vector4(TangentZv3, 0);

                Int32 i2 = outVerts.Count;
                outVerts.Add(v2);

                // flip winding for negative scale
                if (scale > 0.0f)
                {
                    outIndices.Add(i0);
                    outIndices.Add(i1);
                    outIndices.Add(i2);
                }
                else
                {
                    outIndices.Add(i0);
                    outIndices.Add(i2);
                    outIndices.Add(i1);
                }
            }
        }

        static public void DrawCone(SharpDX.Direct3D12.GraphicsCommandList commandList, float angle1, float angle2, Int32 numSides, Boolean bDrawSideLines, Vector4 sideLinearColor)
        {
            List<H1DynamicMeshVertex> meshVerts = new List<H1DynamicMeshVertex>();
            List<Int32> meshIndices = new List<Int32>();
            BuildConeVerts(angle1, angle2, 1.0f, 0.0f, numSides, ref meshVerts, ref meshIndices);

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            meshBuilder.AddVertices(meshVerts);
            meshBuilder.AddTriangles(meshIndices);

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static private void BuildCylinderVerts(Vector3 basePos, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, float radius, float halfHeight, Int32 sides, ref List<H1DynamicMeshVertex> outVerts, ref List<Int32> outIndices)
        {
            float angleDelta = 2.0f * MathUtil.Pi / sides;
            Vector3 lastVertex = basePos + xAxis * radius;

            Vector2 texcoord = new Vector2(0, 0);
            float tcStep = 1.0f / sides;

            Vector3 topOffset = halfHeight * zAxis;
            Int32 baseVertIndex = outVerts.Count;

            // compute vertices for base circle
            for (Int32 sideIndex = 0; sideIndex < sides; ++sideIndex)
            {
                Vector3 vertex = basePos + (xAxis * Convert.ToSingle(Math.Cos(angleDelta * (sideIndex + 1))) + (yAxis * Convert.ToSingle(Math.Sin(angleDelta * (sideIndex + 1))))) * radius;
                Vector3 normal = vertex - basePos;
                normal.Normalize();

                H1DynamicMeshVertex meshVertex = new H1DynamicMeshVertex();
                meshVertex.Position = new Vector4(vertex - topOffset, 0);
                meshVertex.Texcoord = texcoord;

                meshVertex.TangentX = new Vector4(-zAxis, 0);
                meshVertex.TangentY = new Vector4(Vector3.Cross(-zAxis, normal), 0);
                meshVertex.TangentZ = new Vector4(normal, 0);

                outVerts.Add(meshVertex);

                lastVertex = vertex;
                texcoord.X += tcStep;
            }

            lastVertex = basePos + xAxis * radius;
            texcoord = new Vector2(0, 1);

            // compute vertices for the top circle
            for (Int32 sideIndex = 0; sideIndex < sides; ++sideIndex)
            {
                Vector3 vertex = basePos + (xAxis * Convert.ToSingle(Math.Cos(angleDelta * (sideIndex + 1))) + yAxis * Convert.ToSingle(Math.Sin(angleDelta * (sideIndex + 1)))) * radius;
                Vector3 normal = vertex - basePos;
                normal.Normalize();

                H1DynamicMeshVertex meshVertex = new H1DynamicMeshVertex();

                meshVertex.Position = new Vector4(vertex + topOffset, 0);
                meshVertex.Texcoord = texcoord;

                meshVertex.TangentX = new Vector4(-zAxis, 0);
                meshVertex.TangentY = new Vector4(Vector3.Cross(-zAxis, normal), 0);
                meshVertex.TangentZ = new Vector4(normal, 0);

                outVerts.Add(meshVertex);

                lastVertex = vertex;
                texcoord.X += tcStep;
            }

            // add top/bottom triangles, in the style of a fan
            // note if we wanted nice rendering of the caps then we need to duplicate the vertices and modify texture/tangent coordinates
            for (Int32 sideIndex = 1; sideIndex < sides; sideIndex++)
            {
                Int32 v0 = baseVertIndex;
                Int32 v1 = baseVertIndex + sideIndex;
                Int32 v2 = baseVertIndex + ((sideIndex - 1) % sides);

                // bottom
                outIndices.Add(v0);
                outIndices.Add(v1);
                outIndices.Add(v2);

                // top
                outIndices.Add(sides + v2);
                outIndices.Add(sides + v1);
                outIndices.Add(sides + v0);
            }

            // all sides
            for (Int32 sideIndex = 0; sideIndex < sides; sideIndex++)
            {
                Int32 v0 = baseVertIndex + sideIndex;
                Int32 v1 = baseVertIndex + ((sideIndex + 1) % sides);
                Int32 v2 = v0 + sides;
                Int32 v3 = v1 + sides;

                outIndices.Add(v0);
                outIndices.Add(v2);
                outIndices.Add(v1);

                outIndices.Add(v2);
                outIndices.Add(v3);
                outIndices.Add(v1);
            }
        }

        static public void DrawCylinder(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 basePos, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, float radius, float halfHeight, Int32 sides)
        {
            List<H1DynamicMeshVertex> meshVerts = new List<H1DynamicMeshVertex>();
            List<Int32> meshIndices = new List<Int32>();

            BuildCylinderVerts(basePos, xAxis, yAxis, zAxis, radius, halfHeight, sides, ref meshVerts, ref meshIndices);

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            meshBuilder.AddVertices(meshVerts);
            meshBuilder.AddTriangles(meshIndices);

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static private void BuildHalfSphereVerts(Vector3 center, Vector3 radii, Int32 numSides, Int32 numRings, float startAngle, float endAngle, ref List<H1DynamicMeshVertex> outVerts, ref List<Int32> outIndices)
        {
            // the first/last arc are on top of each other
            Int32 numVerts = (numSides + 1) * (numRings + 1);
            // mark the start vertex index
            Int32 lastVertex = outVerts.Count;
            // resize outVerts in advance with numVerts
            List<H1DynamicMeshVertex> verts = outVerts;
            for (Int32 i = 0; i < numVerts; ++i)
                verts.Add(new H1DynamicMeshVertex());

            // calculate verts for one arc
            List<H1DynamicMeshVertex> arcVerts = new List<H1DynamicMeshVertex>();

            for (Int32 i = 0; i < numRings + 1; ++i)
            {
                H1DynamicMeshVertex arcVert = new H1DynamicMeshVertex();
                float angle = startAngle + (Convert.ToSingle(i) / numRings) * (endAngle - startAngle);

                // note - unit sphere, so position always has mag of one. We can just use if for normal
                arcVert.Position.X = 0.0f;
                arcVert.Position.Y = Convert.ToSingle(Math.Sin(angle));
                arcVert.Position.Z = Convert.ToSingle(Math.Cos(angle));
                arcVert.Position.W = 1.0f;

                arcVert.TangentX = new Vector4(1, 0, 0, 0);
                arcVert.TangentY = new Vector4(0, -arcVert.Position.Z, arcVert.Position.Y, 0);
                arcVert.TangentZ = new Vector4(arcVert.Position.X, arcVert.Position.Y, arcVert.Position.Z, arcVert.Position.W);

                arcVert.Texcoord.X = 0.0f;
                arcVert.Texcoord.Y = Convert.ToSingle(i) / numRings;

                arcVerts.Add(arcVert);
            }

            // then rotate this arc numsides + 1 times
            for (Int32 s = 0; s < numSides + 1; ++s)
            {
                AngleSingle angle = new AngleSingle(360.0f, AngleType.Degree);
                Matrix arcRot = Matrix.RotationYawPitchRoll(0, 0, angle.Radians * Convert.ToSingle(s) / numSides);
                float xTexcoord = Convert.ToSingle(s) / numSides;

                for (Int32 v = 0; v < numRings + 1; ++v)
                {
                    Int32 VIx = lastVertex + (numRings + 1) * s + v;
                    verts[VIx].Position = Vector4.Transform(arcVerts[v].Position, arcRot);
                    verts[VIx].TangentX = Vector4.Transform(arcVerts[v].TangentX, arcRot);
                    verts[VIx].TangentY = Vector4.Transform(arcVerts[v].TangentY, arcRot);
                    verts[VIx].TangentZ = Vector4.Transform(arcVerts[v].TangentZ, arcRot);

                    verts[VIx].Texcoord.X = xTexcoord;
                    verts[VIx].Texcoord.Y = arcVerts[v].Texcoord.Y;
                }
            }

            // add all of the triangles we generated to the mesh builder
            for (Int32 s = 0; s < numSides; ++s)
            {
                Int32 a0start = lastVertex + (s + 0) * (numRings + 1);
                Int32 a1start = lastVertex + (s + 1) * (numRings + 1);

                for (Int32 r = 0; r < numRings; r++)
                {
                    outIndices.Add(a0start + r + 0);
                    outIndices.Add(a1start + r + 0);
                    outIndices.Add(a0start + r + 1);

                    outIndices.Add(a1start + r + 0);
                    outIndices.Add(a1start + r + 1);
                    outIndices.Add(a0start + r + 1);
                }
            }

            // transform vertices properly
            Matrix scaleMtx = Matrix.Scaling(radii);
            Matrix translateMtx = Matrix.Translation(center);
            Matrix finalMtx = Matrix.Multiply(scaleMtx, translateMtx);
            for (Int32 i = lastVertex; i < outVerts.Count; ++i)
            {
                H1DynamicMeshVertex vert = outVerts[i];
                vert.Position = Vector4.Transform(vert.Position, finalMtx);
            }          
        }

        static public void DrawCapsule(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 origin, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, float radius, float halfHeight, Int32 numSides)
        {
            float halfAxis = Math.Max(halfHeight - radius, 1.0f);
            Vector3 bottomEnd = origin + radius * zAxis;
            Vector3 topEnd = bottomEnd + (2 * halfAxis) * zAxis;
            float cylinderHalfHeight = (topEnd - bottomEnd).Length() * 0.5f;
            Vector3 cylinderLocation = bottomEnd + cylinderHalfHeight * zAxis;

            List<H1DynamicMeshVertex> meshVerts = new List<H1DynamicMeshVertex>();
            List<Int32> meshIndices = new List<Int32>();

            BuildHalfSphereVerts(topEnd, new Vector3(radius), numSides, 8, 0, MathUtil.Pi / 2.0f, ref meshVerts, ref meshIndices);
            BuildCylinderVerts(cylinderLocation, xAxis, yAxis, zAxis, radius, cylinderHalfHeight, numSides, ref meshVerts, ref meshIndices);
            BuildHalfSphereVerts(bottomEnd, new Vector3(radius), numSides, 8, MathUtil.Pi / 2.0f, MathUtil.Pi, ref meshVerts, ref meshIndices);

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            meshBuilder.AddVertices(meshVerts);
            meshBuilder.AddTriangles(meshIndices);

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawDisc(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 xAxis, Vector3 yAxis, float radius, Int32 numSides)
        {
            float angleDelta = 2.0f * MathUtil.Pi / numSides;

            Vector2 texcoord = new Vector2(0, 0);
            float tcStep = 1.0f / numSides;

            Vector3 zAxis = Vector3.Cross(xAxis, yAxis);

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();

            // compute vertices for base circle
            for (Int32 sideIndex = 0; sideIndex < numSides; ++sideIndex)
            {
                Vector3 vertex = baseLoc + (xAxis * Convert.ToSingle(Math.Cos(angleDelta * (sideIndex))) + yAxis * Convert.ToSingle(Math.Sin(angleDelta * (sideIndex)))) * radius;
                Vector3 normal = vertex - baseLoc;

                H1DynamicMeshVertex vert = new H1DynamicMeshVertex();
                vert.Position = new Vector4(vertex, 1);
                vert.Color = new Vector4(1);
                vert.Texcoord = texcoord;
                vert.Texcoord.X += tcStep * sideIndex;

                vert.TangentX = new Vector4(-zAxis, 0);
                vert.TangentY = new Vector4(Vector3.Cross(-zAxis, normal), 0);
                vert.TangentZ = new Vector4(normal, 0);

                meshBuilder.AddVertex(vert); // add bottom vertex
            }

            // add top/bottom triangle, in the style of a fan
            for (Int32 sideIndex = 0; sideIndex < numSides - 1; sideIndex++)
            {
                Int32 v0 = 0;
                Int32 v1 = sideIndex;
                Int32 v2 = sideIndex + 1;

                meshBuilder.AddTriangle(v0, v1, v2);
                meshBuilder.AddTriangle(v0, v2, v1);
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawFlatArrow(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 xAxis, Vector3 yAxis, float length, Int32 width, float thickness)
        {
            float distanceFromBaseToHead = length / 3.0f;
            float distanceFromBaseToTip = distanceFromBaseToHead * 2.0f;
            float widthOfBase = width;
            float widthOfHead = 2.0f * width;

            Vector3[] arrowPoints = new Vector3[7];
            // base points
            arrowPoints[0] = baseLoc - yAxis * (widthOfBase * 0.5f);
            arrowPoints[1] = baseLoc + yAxis * (widthOfBase * 0.5f);
            // inner head
            arrowPoints[2] = arrowPoints[0] + xAxis * distanceFromBaseToHead;
            arrowPoints[3] = arrowPoints[1] + xAxis * distanceFromBaseToHead;
            // outer head
            arrowPoints[4] = arrowPoints[2] - yAxis * (widthOfBase * 0.5f);
            arrowPoints[5] = arrowPoints[3] + yAxis * (widthOfBase * 0.5f);
            // tip
            arrowPoints[6] = baseLoc + xAxis * length;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();

            // compute vertices for base circle
            for (Int32 i = 0; i < 7; ++i)
            {
                H1DynamicMeshVertex meshVertex = new H1DynamicMeshVertex();
                meshVertex.Position = new Vector4(arrowPoints[i], 1);
                meshVertex.Color = new Vector4(1);
                meshVertex.Texcoord = new Vector2(0);
                meshVertex.TangentX = new Vector4(Vector3.Cross(xAxis, yAxis), 0);
                meshVertex.TangentY = new Vector4(yAxis, 0);
                meshVertex.TangentZ = new Vector4(xAxis, 0);
                meshBuilder.AddVertex(meshVertex);
            }

            // add triangles / double sided
            {
                // base
                meshBuilder.AddTriangle(0, 2, 1);
                meshBuilder.AddTriangle(0, 1, 2);
                meshBuilder.AddTriangle(1, 2, 3);
                meshBuilder.AddTriangle(1, 3, 2);
                // head
                meshBuilder.AddTriangle(4, 5, 6);
                meshBuilder.AddTriangle(4, 6, 5);
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        // line drawing utility functions
        // @TODO - need to implement this correctly after completing FPrimitiveDrawInterface with FMeshBatch
        static public void DrawWireBox(SharpDX.Direct3D12.GraphicsCommandList commandList, Matrix mtx, Vector3 min, Vector3 max, float thickness, float depthBias, float bScreenSpace)
        {
            Vector3[] b = new Vector3[2];
            b[0] = min;
            b[1] = max;

            // @TODO - need to fix this properly
            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();

            for (Int32 i = 0; i < 2; ++i)
            {
                for (Int32 j = 0; j < 2; ++j)
                {
                    Vector3 p = new Vector3();
                    Vector3 q = new Vector3();

                    p.X = b[i].X; q.X = b[i].X;
                    p.Y = b[j].Y; q.Y = b[j].Y;
                    p.Z = b[0].Z; q.Z = b[1].Z;
                    meshBuilder.AddLine(Vector3.Transform(p, mtx), Vector3.Transform(q, mtx), new Vector4(1));

                    p.Y = b[i].Y; q.Y = b[i].Y;
                    p.Z = b[j].Z; q.Z = b[j].Y;
                    p.X = b[0].X; q.X = b[1].X;
                    meshBuilder.AddLine(Vector3.Transform(p, mtx), Vector3.Transform(q, mtx), new Vector4(1));

                    p.Z = b[i].Z; q.Z = b[i].Z;
                    p.X = b[j].X; q.X = b[j].X;
                    p.Y = b[0].Y; q.Y = b[1].Y;
                    meshBuilder.AddLine(Vector3.Transform(p, mtx), Vector3.Transform(q, mtx), new Vector4(1));
                }
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);

            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawCircle(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 x, Vector3 y, float radius, Int32 numSides)
        {
            float angleDelta = 2.0f * MathUtil.Pi / numSides;
            Vector3 lastVertex = baseLoc + x * radius;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();

            for (Int32 sideIndex = 0; sideIndex < numSides; ++sideIndex)
            {
                Vector3 vertex = baseLoc + (x * Convert.ToSingle(Math.Cos(angleDelta * (sideIndex + 1))) + y * Convert.ToSingle(Math.Sin(angleDelta * (sideIndex + 1)))) * radius;
                meshBuilder.AddLine(new Vector4(lastVertex, 1), new Vector4(vertex, 1), new Vector4(1));
                lastVertex = vertex;
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawWireSphere(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, float radius, Int32 numSides)
        {
            DrawCircle(commandList, baseLoc, new Vector3(1, 0, 0), new Vector3(0, 1, 0), radius, numSides);
            DrawCircle(commandList, baseLoc, new Vector3(1, 0, 0), new Vector3(0, 0, 1), radius, numSides);
            DrawCircle(commandList, baseLoc, new Vector3(0, 1, 0), new Vector3(0, 0, 1), radius, numSides);
        }

        static public void DrawArc(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 x, Vector3 y, float minAngle, float maxAngle, float radius, Int32 sections)
        {
            float angleStep = (maxAngle - minAngle) / sections;
            float currentAngle = minAngle;

            Vector3 lastVertex = baseLoc + radius * (Convert.ToSingle(Math.Cos(currentAngle * (MathUtil.Pi / 180.0f))) * x + Convert.ToSingle(Math.Sin(currentAngle * (MathUtil.Pi / 180.0f))) * y);
            currentAngle += angleStep;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            for (Int32 i = 0; i < sections; ++i)
            {
                Vector3 vertex = baseLoc + radius * (Convert.ToSingle(Math.Cos(currentAngle * (MathUtil.Pi / 180.0f))) * x + Convert.ToSingle(Math.Sin(currentAngle * (MathUtil.Pi / 180.0f))) * y);
                meshBuilder.AddLine(new Vector4(lastVertex, 1), new Vector4(vertex, 1), new Vector4(1));
                lastVertex = vertex;
                currentAngle += angleStep;
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawWireSphereAutoSides(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, float radius)
        {
            Int32 numSides = MathUtil.Clamp(Convert.ToInt32(radius / 4.0f), 16, 64);
            DrawWireSphere(commandList, baseLoc, radius, numSides);
        }

        static public void DrawWireCylinder(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 x, Vector3 y, Vector3 z, float radius, float halfHeight, Int32 numSides)
        {
            float angleDelta = 2.0f * MathUtil.Pi / numSides;
            Vector3 lastVertex = baseLoc + x * radius;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            for (Int32 sideIndex = 0; sideIndex < numSides; ++sideIndex)
            {
                Vector3 vertex = baseLoc + (x * Convert.ToSingle(Math.Cos(angleDelta * (sideIndex + 1))) + y * Convert.ToSingle(Math.Sin(angleDelta * (sideIndex + 1)))) * radius;
                meshBuilder.AddLine(new Vector4(lastVertex - z * halfHeight, 1), new Vector4(vertex - z * halfHeight, 1), new Vector4(1));
                meshBuilder.AddLine(new Vector4(lastVertex + z * halfHeight, 1), new Vector4(vertex + z * halfHeight, 1), new Vector4(1));
                meshBuilder.AddLine(new Vector4(lastVertex - z * halfHeight, 1), new Vector4(lastVertex + z * halfHeight, 1), new Vector4(1));

                lastVertex = vertex;
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawHalfCircle(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 x, Vector3 y, float radius, Int32 numSides)
        {
            float angleDelta = MathUtil.Pi / numSides;
            Vector3 lastVertex = baseLoc + x * radius;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            for (Int32 sideIndex = 0; sideIndex < numSides; ++sideIndex)
            {
                Vector3 vertex = baseLoc + (x * Convert.ToSingle(Math.Cos(angleDelta * (sideIndex + 1))) + y * Convert.ToSingle(Math.Sin(angleDelta * (sideIndex + 1)))) * radius;
                meshBuilder.AddLine(new Vector4(lastVertex, 1), new Vector4(vertex, 1), new Vector4(1));
                lastVertex = vertex;
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawWireCapsule(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 x, Vector3 y, Vector3 z, float radius, float halfHeight, Int32 numSides)
        {
            Vector3 origin = baseLoc;
            Vector3 xAxis = Vector3.Normalize(x);
            Vector3 yAxis = Vector3.Normalize(y);
            Vector3 zAxis = Vector3.Normalize(z);

            float xScale = x.Length();
            float yScale = y.Length();
            float zScale = z.Length();
            float capsuleRadius = radius * Math.Max(xScale, yScale);
            halfHeight *= zScale;
            capsuleRadius = MathUtil.Clamp(capsuleRadius, 0.0f, halfHeight); // cap radius based on total height
            halfHeight -= capsuleRadius;
            halfHeight = Math.Max(0.0f, halfHeight);

            // draw top and bottom circles
            Vector3 topEnd = origin + (halfHeight * zAxis);
            Vector3 bottomEnd = origin - (halfHeight * zAxis);

            DrawCircle(commandList, topEnd, xAxis, yAxis, capsuleRadius, numSides);
            DrawCircle(commandList, bottomEnd, xAxis, yAxis, capsuleRadius, numSides);

            // draw domed caps
            DrawHalfCircle(commandList, topEnd, yAxis, zAxis, capsuleRadius, numSides / 2);
            DrawHalfCircle(commandList, topEnd, xAxis, zAxis, capsuleRadius, numSides / 2);

            Vector3 negZAxis = -zAxis;
            DrawHalfCircle(commandList, bottomEnd, yAxis, negZAxis, capsuleRadius, numSides / 2);
            DrawHalfCircle(commandList, bottomEnd, xAxis, negZAxis, capsuleRadius, numSides / 2);

            // we set num sides to 4 as it makes a nicer looking capsule as we only draw 2 half circles above
            const Int32 numCylinderLines = 4;

            // draw lines for the cylinder portion
            const float angleDelta = 2.0f * MathUtil.Pi / numCylinderLines;
            Vector3 lastVertex = baseLoc + xAxis * capsuleRadius;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            for (Int32 sideIndex = 0; sideIndex < numCylinderLines; sideIndex++)
            {
                Vector3 vertex = baseLoc + (xAxis * Convert.ToSingle(Math.Cos(angleDelta * (sideIndex + 1))) + yAxis * Convert.ToSingle(Math.Sin(angleDelta * (sideIndex + 1)))) * capsuleRadius;
                meshBuilder.AddLine(new Vector4(lastVertex - zAxis * halfHeight, 1), new Vector4(lastVertex + zAxis * halfHeight, 1), new Vector4(1));
                lastVertex = vertex;
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawWireCone(SharpDX.Direct3D12.GraphicsCommandList commandList, ref List<Vector3> verts, Matrix transformMtx, float coneRadius, float coneAngle, Int32 coneSides)
        {
            const float twoPI = 2.0f * MathUtil.Pi;
            const float toRads = MathUtil.Pi / 180.0f;
            const float maxAngle = 89.0f * toRads + 0.001f;
            float clampedConeAngle = MathUtil.Clamp(coneAngle * toRads, 0.001f, maxAngle);
            float sinClampedConeAngle = Convert.ToSingle(Math.Sin(clampedConeAngle));
            float cosClampedConeAngle = Convert.ToSingle(Math.Cos(clampedConeAngle));
            Vector3 coneDirection = new Vector3(1, 0, 0);
            Vector3 coneUpVector = new Vector3(0, 1, 0);
            Vector3 coneLeftVector = new Vector3(0, 0, 1);

            for (Int32 i = 0; i < coneSides; ++i)
            {
                float theta = twoPI * i / coneSides;
                verts.Add(
                    (coneDirection * (coneRadius * cosClampedConeAngle)) +
                    ((sinClampedConeAngle * coneRadius * Convert.ToSingle(Math.Cos(theta))) * coneUpVector) +
                    ((sinClampedConeAngle * coneRadius * Convert.ToSingle(Math.Sin(theta))) * coneLeftVector)
                    );
            }

            // transform to world space
            for (Int32 i = 0; i < verts.Count; ++i)
            {
                Vector4 transformedVert = Vector3.Transform(verts[i], transformMtx);
                verts[i] = new Vector3(transformedVert.X, transformedVert.Y, transformedVert.Z);
            }

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            Vector3 translation = new Vector3();
            Vector3 scale = new Vector3();
            Quaternion rotation = new Quaternion();
            transformMtx.Decompose(out scale, out rotation, out translation);

            // draw spokes
            for (Int32 i = 0; i < verts.Count; ++i)
            {
                meshBuilder.AddLine(new Vector4(translation, 1), new Vector4(verts[i], 1), new Vector4(1));
            }

            // draw rim
            for (Int32 i = 0; i < verts.Count - 1; ++i)
            {
                meshBuilder.AddLine(new Vector4(verts[i], 1), new Vector4(verts[i + 1], 1), new Vector4(1));
            }
            meshBuilder.AddLine(new Vector4(verts[verts.Count - 1], 1), new Vector4(verts[0], 1), new Vector4(1));

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawWireSphereCappedCone(SharpDX.Direct3D12.GraphicsCommandList commandList, Matrix transformMtx, float coneRadius, float coneAngle, Int32 coneSides, Int32 arcFrequency, Int32 capSegments)
        {
            List<Vector3> verts = new List<Vector3>();
            DrawWireCone(commandList, ref verts, transformMtx, coneRadius, coneAngle, coneSides);

            Vector3 translation = new Vector3();
            Vector3 scale = new Vector3();
            Quaternion rotation = new Quaternion();
            transformMtx.Decompose(out scale, out rotation, out translation);

            // draw arcs
            Int32 arcCount = verts.Count / 2;
            for (Int32 i = 0; i < arcCount; i += arcFrequency)
            {
                Vector4 x = Vector4.Normalize(Vector4.Transform(new Vector4(1, 0, 0, 0), transformMtx));
                Vector4 y = Vector4.Normalize(new Vector4(verts[i] - verts[arcCount + i], 0));

                DrawArc(commandList, translation, new Vector3(x.X, x.Y, x.Z), new Vector3(y.X, y.Y, y.Z), -coneAngle, coneAngle, coneRadius, capSegments);
            }
        }

        static public void DrawOrientedWireBox(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 baseLoc, Vector3 x, Vector3 y, Vector3 z, Vector3 extent)
        {
            Vector3[] b = new Vector3[2];
            Vector3 p = new Vector3();
            Vector3 q = new Vector3();

            Matrix mtx = new Matrix(x.X, x.Y, x.Z, 0, y.X, y.Y, y.Z, 0, z.X, z.Y, z.Z, 0, baseLoc.X, baseLoc.Y, baseLoc.Z, 0);
            b[0] = -extent;
            b[1] = extent;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            for (Int32 i = 0; i < 2; ++i)
            {
                for (Int32 j = 0; j < 2; ++j)
                {
                    p.X = b[i].X; q.X = b[i].X;
                    p.Y = b[j].Y; q.Y = b[j].Y;
                    p.Z = b[0].Z; q.Z = b[1].Z;
                    meshBuilder.AddLine(Vector3.Transform(p, mtx), Vector3.Transform(q, mtx), new Vector4(1));

                    p.Y = b[i].Y; q.Y = b[i].Y;
                    p.Z = b[j].Z; q.Z = b[j].Y;
                    p.X = b[0].X; q.X = b[1].X;
                    meshBuilder.AddLine(Vector3.Transform(p, mtx), Vector3.Transform(q, mtx), new Vector4(1));

                    p.Z = b[i].Z; q.Z = b[i].Z;
                    p.X = b[j].X; q.X = b[j].X;
                    p.Y = b[0].Y; q.Y = b[1].Y;
                    meshBuilder.AddLine(Vector3.Transform(p, mtx), Vector3.Transform(q, mtx), new Vector4(1));
                }
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawCoordinateSystem(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 axisLoc, Matrix axisRot, float scale)
        {
            Matrix scaleMtx = Matrix.Scaling(scale);
            Matrix transformMtx = Matrix.Multiply(axisRot, scaleMtx);

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            meshBuilder.AddLine(new Vector4(axisLoc, 1), Vector3.Transform(new Vector3(1, 0, 0), transformMtx), new Vector4(1, 0, 0, 1));
            meshBuilder.AddLine(new Vector4(axisLoc, 1), Vector3.Transform(new Vector3(0, 1, 0), transformMtx), new Vector4(0, 1, 0, 1));
            meshBuilder.AddLine(new Vector4(axisLoc, 1), Vector3.Transform(new Vector3(0, 0, 1), transformMtx), new Vector4(0, 0, 1, 1));

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawDirectionalArrow(SharpDX.Direct3D12.GraphicsCommandList commandList, Matrix arrowToWorld, float length, float arrowSize)
        {
            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            meshBuilder.AddLine(Vector3.Transform(new Vector3(length, 0, 0), arrowToWorld), Vector3.Transform(new Vector3(0), arrowToWorld), new Vector4(1));
            meshBuilder.AddLine(Vector3.Transform(new Vector3(length, 0, 0), arrowToWorld), Vector3.Transform(new Vector3(length - arrowSize, +arrowSize, +arrowSize), arrowToWorld), new Vector4(1));
            meshBuilder.AddLine(Vector3.Transform(new Vector3(length, 0, 0), arrowToWorld), Vector3.Transform(new Vector3(length - arrowSize, +arrowSize, -arrowSize), arrowToWorld), new Vector4(1));
            meshBuilder.AddLine(Vector3.Transform(new Vector3(length, 0, 0), arrowToWorld), Vector3.Transform(new Vector3(length - arrowSize, -arrowSize, +arrowSize), arrowToWorld), new Vector4(1));
            meshBuilder.AddLine(Vector3.Transform(new Vector3(length, 0, 0), arrowToWorld), Vector3.Transform(new Vector3(length - arrowSize, -arrowSize, -arrowSize), arrowToWorld), new Vector4(1));

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawWireStar(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 position, float size)
        {
            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            {
                meshBuilder.AddLine(new Vector4(position + size * new Vector3(1, 0, 0), 1), new Vector4(position - size * new Vector3(1, 0, 0), 1), new Vector4(1));
                meshBuilder.AddLine(new Vector4(position + size * new Vector3(0, 1, 0), 1), new Vector4(position - size * new Vector3(0, 1, 0), 1), new Vector4(1));
                meshBuilder.AddLine(new Vector4(position + size * new Vector3(0, 0, 1), 1), new Vector4(position - size * new Vector3(0, 0, 1), 1), new Vector4(1));
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawDashedLine(SharpDX.Direct3D12.GraphicsCommandList commandList, Vector3 start, Vector3 end, float dashSize)
        {
            Vector3 lineDir = end - start;
            float lineLeft = (end - start).Length();
            if (lineLeft > 0)
            {
                lineDir /= lineLeft;
            }

            Int32 nLines = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(lineLeft) / (dashSize * 2)));
            Vector3 dash = (dashSize * lineDir);
            Vector3 drawStart = start;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            while (lineLeft > dashSize)
            {
                Vector3 drawEnd = drawStart + dash;
                meshBuilder.AddLine(new Vector4(drawStart, 1), new Vector4(drawEnd, 1), new Vector4(1));

                lineLeft -= 2 * dashSize;
                drawStart = drawEnd + dash;
            }

            if (lineLeft > 0.0f)
            {
                Vector3 drawEnd = end;
                meshBuilder.AddLine(new Vector4(drawStart, 1), new Vector4(drawEnd, 1), new Vector4(1));
            }

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }

        static public void DrawWireDiamond(SharpDX.Direct3D12.GraphicsCommandList commandList, Matrix diamondMatrix, float size)
        {
            Vector4 transformed_p = Vector3.Transform(new Vector3(0, 0, 1) * size, diamondMatrix);
            transformed_p /= transformed_p.W;
            Vector3 topPoint = new Vector3(transformed_p.X, transformed_p.Y, transformed_p.Z);

            transformed_p = Vector3.Transform(new Vector3(0, 0, -1) * size, diamondMatrix);
            transformed_p /= transformed_p.W;
            Vector3 bottomPoint = new Vector3(transformed_p.X, transformed_p.Y, transformed_p.Z);

            float oneOverRootTwo = Convert.ToSingle(Math.Sqrt(0.5f));

            Vector4[] squarePoints = new Vector4[4];
            squarePoints[0] = Vector3.Transform(new Vector3(1, 1, 0) * size * oneOverRootTwo, diamondMatrix);
            squarePoints[1] = Vector3.Transform(new Vector3(1, -1, 0) * size * oneOverRootTwo, diamondMatrix);
            squarePoints[2] = Vector3.Transform(new Vector3(-1, -1, 0) * size * oneOverRootTwo, diamondMatrix);
            squarePoints[3] = Vector3.Transform(new Vector3(-1, 1, 0) * size * oneOverRootTwo, diamondMatrix);
            squarePoints[0] /= squarePoints[0].W;
            squarePoints[1] /= squarePoints[1].W;
            squarePoints[2] /= squarePoints[2].W;
            squarePoints[3] /= squarePoints[3].W;

            H1DynamicMeshBuilder meshBuilder = H1Global<H1VisualDebugger>.Instance.GetNewDynamicMeshBuilder();
            meshBuilder.AddLine(new Vector4(topPoint, 1), squarePoints[0], new Vector4(1));
            meshBuilder.AddLine(new Vector4(topPoint, 1), squarePoints[1], new Vector4(1));
            meshBuilder.AddLine(new Vector4(topPoint, 1), squarePoints[2], new Vector4(1));
            meshBuilder.AddLine(new Vector4(topPoint, 1), squarePoints[3], new Vector4(1));

            meshBuilder.AddLine(new Vector4(bottomPoint, 1), squarePoints[0], new Vector4(1));
            meshBuilder.AddLine(new Vector4(bottomPoint, 1), squarePoints[1], new Vector4(1));
            meshBuilder.AddLine(new Vector4(bottomPoint, 1), squarePoints[2], new Vector4(1));
            meshBuilder.AddLine(new Vector4(bottomPoint, 1), squarePoints[3], new Vector4(1));

            meshBuilder.AddLine(squarePoints[0], squarePoints[1], new Vector4(1));
            meshBuilder.AddLine(squarePoints[1], squarePoints[2], new Vector4(1));
            meshBuilder.AddLine(squarePoints[2], squarePoints[3], new Vector4(1));
            meshBuilder.AddLine(squarePoints[3], squarePoints[0], new Vector4(1));

            // generate vertex & index buffers and vertex declaration
            meshBuilder.GenerateVertexIndexBuffersAndVertexDeclaration();

            commandList.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
            meshBuilder.VertexFactory.setVertexBuffers(commandList);
            SharpDX.Direct3D12.IndexBufferView rawIBV = ((Gen2Layer.H1IndexBufferView)meshBuilder.IndexBuffer.View).View;
            commandList.SetIndexBuffer(rawIBV);

            Int32 instanceCount = Convert.ToInt32(meshBuilder.IndexBuffer.Count);
            commandList.DrawIndexedInstanced(instanceCount, 1, 0, 0, 0);
        }
    }
}
