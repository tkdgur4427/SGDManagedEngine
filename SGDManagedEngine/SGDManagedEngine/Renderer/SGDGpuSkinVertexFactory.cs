using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1GpuSkinVertexFactory : H1VertexFactory
    {  
        public H1GpuSkinVertexFactory()
            : base (H1Global<H1ManagedRenderer>.Instance.ShaderManager.CacheVertexFactoryType(
                new H1VertexFactoryType("H1GpuSkinVertexFactory", "SGDGpuSkinVertexFactory.hlsl", H1GpuSkinVertexFactory.ModifyCompilationEnvironment, H1GpuSkinVertexFactory.ShouldCache)))
        {

        }
             
        public ShaderDataType ShaderData
        {
            get { return m_ShaderData; }
        }

        public class ShaderDataType
        {
            //public H1VertexBuffer[] BoneBuffer; // total number is 2 (0/1 new-old positions)
            public H1VertexBuffer PositionBuffer;
            public H1VertexBuffer TangentZBuffer;
            public H1VertexBuffer TangentXBuffer;
            public List<H1VertexBuffer> TexcoordBuffers = new List<H1VertexBuffer>();
            public H1VertexBuffer BoneIndices;
            public H1VertexBuffer BoneWeights;
            public H1VertexBuffer ColorBuffer;

            public UInt32 CurrentBufferIndex;
            public UInt32 PreviousFrameNumber;
            public UInt32 CurrentFrameNumber;
        }

        public H1VertexStreamComponent PositionStreamComponent
        {
            get { return m_PositionStreamComponent; }

            set
            {
                if (value.Type != H1VertexStreamSematicType.Position)
                    throw new ArgumentException("invalid argument for PositionStreamComponent, please check it!");

                m_PositionStreamComponent = value;
            }
        }

        public H1VertexStreamComponent TangentZStreamComponent
        {
            get { return m_TangentZStreamComponent; }

            set
            {
                if (value.Type != H1VertexStreamSematicType.TangentZ)
                    throw new ArgumentException("invalid argument for TangentZStreamComponent, please check it!");

                m_TangentZStreamComponent = value;
            }
        }

        public H1VertexStreamComponent TangentXStreamComponent
        {
            get { return m_TangentXStreamComponent; }

            set
            {
                if (value.Type != H1VertexStreamSematicType.TangentX)
                    throw new ArgumentException("invalid argument for TangentXStreamComponent, please check it!");

                m_TangentXStreamComponent = value;
            }
        }

        public H1VertexStreamComponent BoneIndicesStreamComponent
        {
            get { return m_BoneIndicesStreamComponent; }

            set
            {
                if (value.Type != H1VertexStreamSematicType.BoneIndices)
                    throw new ArgumentException("invalid argument for BoneIndicesStreamComponent, please check it!");

                m_BoneIndicesStreamComponent = value;
            }
        }

        public H1VertexStreamComponent BoneWeightsStreamComponent
        {
            get { return m_BoneWeightsStreamComponent; }

            set
            {
                if (value.Type != H1VertexStreamSematicType.BoneWeights)
                    throw new ArgumentException("invalid argument for BoneWeightsStreamComponent, please check it!");

                m_BoneWeightsStreamComponent = value;
            }
        }

        public H1VertexStreamComponent ColorStreamComponent
        {
            get { return m_ColorStreamComponent; }

            set
            {
                if (value.Type != H1VertexStreamSematicType.Color)
                    throw new ArgumentException("invalid argument for ColorStreamComponent, please check it!");

                m_ColorStreamComponent = value;
            }
        }

        public List<H1VertexStreamComponent> TexcoordStreamComponents
        {
            get { return m_TexcoordStreamComponents; }
        }

        public void AddTexcoordStreamComponent(H1VertexStreamComponent texcordStreamComponent)
        {
            if (texcordStreamComponent.Type != H1VertexStreamSematicType.Texcoord)
                throw new ArgumentException("invalid argument for TexcoordStreamComponent, please check it!");

            m_TexcoordStreamComponents.Add(texcordStreamComponent);
        }

        public new void GenerateVertexDeclaration()
        {
            // add vertex streams
            String baseSemanticName = "ATTRIBUTE";
            Int32 semanticIndex = 0;

            // 1. position stream component
            if (m_PositionStreamComponent != null)
            {
                H1VertexStream positionVertexStream = new H1VertexStream(m_PositionStreamComponent.Name, baseSemanticName, semanticIndex,
                    m_PositionStreamComponent.ElementType, m_PositionStreamComponent.Stride, m_PositionStreamComponent.Offset,
                    m_ShaderData.PositionBuffer);
                AddVertexStream(positionVertexStream);
                semanticIndex++;
            }

            // 2. tangentZ stream component
            if (m_TangentZStreamComponent != null)
            {
                H1VertexStream tangentZVertexStream = new H1VertexStream(m_TangentZStreamComponent.Name, baseSemanticName, semanticIndex,
                    m_TangentZStreamComponent.ElementType, m_TangentZStreamComponent.Stride, m_TangentZStreamComponent.Offset,
                    m_ShaderData.TangentZBuffer);
                AddVertexStream(tangentZVertexStream);
                semanticIndex++;
            }

            // 3. tangentX stream component
            if (m_TangentXStreamComponent != null)
            {
                H1VertexStream tangentXVertexStream = new H1VertexStream(m_TangentXStreamComponent.Name, baseSemanticName, semanticIndex,
                                    m_TangentXStreamComponent.ElementType, m_TangentXStreamComponent.Stride, m_TangentXStreamComponent.Offset,
                                    m_ShaderData.TangentXBuffer);
                AddVertexStream(tangentXVertexStream);
                semanticIndex++;
            }

            // 4. bone indices
            if (m_BoneIndicesStreamComponent != null)
            {
                H1VertexStream boneIndicesStream = new H1VertexStream(m_BoneIndicesStreamComponent.Name, baseSemanticName, semanticIndex,
                                    m_BoneIndicesStreamComponent.ElementType, m_BoneIndicesStreamComponent.Stride, m_BoneIndicesStreamComponent.Offset,
                                    m_ShaderData.BoneIndices);
                AddVertexStream(boneIndicesStream);
                semanticIndex++;
            }

            // 5. bone weights
            if (m_BoneWeightsStreamComponent != null)
            {
                H1VertexStream boneWeightsStream = new H1VertexStream(m_BoneWeightsStreamComponent.Name, baseSemanticName, semanticIndex,
                                    m_BoneWeightsStreamComponent.ElementType, m_BoneWeightsStreamComponent.Stride, m_BoneWeightsStreamComponent.Offset,
                                    m_ShaderData.BoneWeights);
                AddVertexStream(boneWeightsStream);
                semanticIndex++;
            }

            // 6. texcoords stream component
            m_TexCoord2DStreamComponentNum = 0;
            m_TexCoord3DStreamComponentNum = 0;

            for (int i = 0; i < m_TexcoordStreamComponents.Count; ++i)
            {
                H1VertexStreamComponent component = m_TexcoordStreamComponents[i];
                String name = component.Name;
                String typeName;
                switch (component.ElementType)
                {
                    case H1VertexElementType.Float2:
                        typeName = "2D";
                        m_TexCoord2DStreamComponentNum++;
                        break;
                    case H1VertexElementType.Float3:
                        typeName = "3D";
                        m_TexCoord3DStreamComponentNum++;
                        break;
                    default:
                        typeName = "";
                        break;

                }
                name += typeName;

                String number = i.ToString();
                name += number; // final semantic name

                // add texcoord 
                H1VertexStream stream = new H1VertexStream(name, baseSemanticName, semanticIndex, component.ElementType, component.Stride, component.Offset, m_ShaderData.TexcoordBuffers[i]);
                AddVertexStream(stream);
                semanticIndex++;
            }            

            // 7. color
            if (m_ColorStreamComponent != null)
            {
                H1VertexStream colorStream = new H1VertexStream(m_ColorStreamComponent.Name, baseSemanticName, semanticIndex,
                                    m_ColorStreamComponent.ElementType, m_ColorStreamComponent.Stride, m_ColorStreamComponent.Offset,
                                    m_ShaderData.ColorBuffer);
                AddVertexStream(colorStream);
                semanticIndex++;
            }

            // finally generate vertex declaration
            if (!base.GenerateVertexDeclaration())
                throw new Exception("failed to create vertex declaration please check!");
        }

        public static new void ModifyCompilationEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {
            // mark gpu skin vertex factory
            outEnvironment.SetDefine("H1_GPUVERTEXFACTORY", 1);

            // set number of texture coordinate space
            outEnvironment.SetDefine("H1_NUM_TEXCOORD2D", 1);

            if (material.DrawWithColor)
                outEnvironment.SetDefine("H1_COLOR", 1);
            else
                outEnvironment.SetDefine("H1_COLOR", 0);
        }

        // @TODO - I need to wrap command list separately!, it is just temporary method! SHOULD!! FIX!!
        public void setVertexBuffers(SharpDX.Direct3D12.GraphicsCommandList commandList)
        {
            SharpDX.Direct3D12.VertexBufferView PositionBufferView = ((Gen2Layer.H1VertexBufferView)m_ShaderData.PositionBuffer.View).View;
            SharpDX.Direct3D12.VertexBufferView TangentZBufferView = ((Gen2Layer.H1VertexBufferView)m_ShaderData.TangentZBuffer.View).View;
            SharpDX.Direct3D12.VertexBufferView TangentXBufferView = ((Gen2Layer.H1VertexBufferView)m_ShaderData.TangentXBuffer.View).View;
            SharpDX.Direct3D12.VertexBufferView BoneIndicesView = ((Gen2Layer.H1VertexBufferView)m_ShaderData.BoneIndices.View).View;
            SharpDX.Direct3D12.VertexBufferView BoneWeightsView = ((Gen2Layer.H1VertexBufferView)m_ShaderData.BoneWeights.View).View;
            SharpDX.Direct3D12.VertexBufferView Texcoord0BufferView = ((Gen2Layer.H1VertexBufferView)m_ShaderData.TexcoordBuffers[0].View).View;
            SharpDX.Direct3D12.VertexBufferView ColorBufferView = ((Gen2Layer.H1VertexBufferView)m_ShaderData.ColorBuffer.View).View;

            commandList.SetVertexBuffer(0, PositionBufferView);
            commandList.SetVertexBuffer(1, TangentZBufferView);
            commandList.SetVertexBuffer(2, TangentXBufferView);
            commandList.SetVertexBuffer(3, BoneIndicesView);
            commandList.SetVertexBuffer(4, BoneWeightsView);
            commandList.SetVertexBuffer(5, Texcoord0BufferView);
            commandList.SetVertexBuffer(6, ColorBufferView);
        }

        private H1VertexStreamComponent m_PositionStreamComponent;
        private H1VertexStreamComponent m_TangentZStreamComponent;
        private H1VertexStreamComponent m_TangentXStreamComponent;

        private List<H1VertexStreamComponent> m_TexcoordStreamComponents = new List<H1VertexStreamComponent>();
        private Int32 m_TexCoord2DStreamComponentNum;
        private Int32 m_TexCoord3DStreamComponentNum;

        private H1VertexStreamComponent m_BoneIndicesStreamComponent;
        private H1VertexStreamComponent m_BoneWeightsStreamComponent;
        private H1VertexStreamComponent m_ColorStreamComponent;

        private ShaderDataType m_ShaderData = new ShaderDataType();
    }
}
