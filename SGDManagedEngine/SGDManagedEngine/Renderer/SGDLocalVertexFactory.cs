using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public enum H1VertexStreamSematicType
    {
        Position,
        Normal,
        Texcoord,
        Color,
        TangentZ,
        TangentX,
        BoneIndices,
        BoneWeights,
    }

    public class H1VertexStreamComponent
    {
        public String Name
        {
            get { return m_VertexStreamComponentType.ToString(); }
        }

        public H1VertexStreamSematicType Type
        {
            get { return m_VertexStreamComponentType; }
        }

        public H1VertexElementType ElementType
        {
            get { return m_VertexElementType; }
        }

        public int Stride
        {
            get { return m_Stride; }
        }

        public int Offset
        {
            get { return m_Offset; }
        }

        public H1VertexStreamComponent(H1VertexStreamSematicType type, H1VertexElementType elementType, int offset)
        {
            m_VertexStreamComponentType = type;
            m_VertexElementType = elementType;
            m_Stride = Convert.ToInt32(H1RHIDefinitionHelper.ElementTypeToSize(elementType));
            m_Offset = offset;
        }

        // this enum will be converted into corresponding semantic name when adding vertex stream in H1VertexFactory
        private readonly H1VertexStreamSematicType m_VertexStreamComponentType;
        private readonly H1VertexElementType m_VertexElementType;
        private readonly int m_Stride;
        private readonly int m_Offset;
    }

    public class H1LocalVertexFactory : H1VertexFactory
    {
        public H1LocalVertexFactory()
            : base (H1Global<H1ManagedRenderer>.Instance.ShaderManager.CacheVertexFactoryType(
                new H1VertexFactoryType("H1LocalVertexFactory", "SGDLocalVertexFactory.hlsl", H1LocalVertexFactory.ModifyCompilationEnvironment, H1LocalVertexFactory.ShouldCache)))
        {
            
        }

        public H1VertexStreamComponent PositionStreamComponent
        {
            set
            {
                if (value.Type != H1VertexStreamSematicType.Position)
                    throw new ArgumentException("invalid argument for PositionStreamComponent, please check it!");

                m_PositionStreamComponent = value;
            }

            get { return m_PositionStreamComponent; }
        }

        public H1VertexStreamComponent NormalStreamComponent
        {
            set
            {
                if (value.Type != H1VertexStreamSematicType.Normal)
                    throw new ArgumentException("invalid argument for NormalStreamComponent, please check it!");

                m_NormalStreamComponent = value;
            }

            get { return m_NormalStreamComponent;  }
        }

        public H1VertexStreamComponent ColorStreamComponent
        {
            set
            {
                if (value.Type != H1VertexStreamSematicType.Color)
                    throw new ArgumentException("invalid argument for ColorStreamComponent, please check it!");

                m_ColorStreamComponent = value;
            }

            get { return m_ColorStreamComponent; }
        }

        public H1VertexBuffer PositionVertexBuffer
        {
            get { return m_PositionVertexBuffer; }
            set { m_PositionVertexBuffer = value; }
        }

        public H1VertexBuffer NormalVertexBuffer
        {
            get { return m_NormalVertexBuffer; }
            set { m_NormalVertexBuffer = value; }
        }

        public H1VertexBuffer ColorVertexBuffer
        {
            get { return m_ColorVertexBuffer; }
            set { m_ColorVertexBuffer = value; }
        }

        public List<H1VertexBuffer> TexcoordVertexBuffers
        {
            get { return m_TexcoordVertexBuffers; }
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
            int semanticIndex = 0;

            // 1. position stream component
            if (m_PositionStreamComponent != null)
            {                
                H1VertexStream positionVertexStream = new H1VertexStream(m_PositionStreamComponent.Name, baseSemanticName, semanticIndex,
                                m_PositionStreamComponent.ElementType, m_PositionStreamComponent.Stride, m_PositionStreamComponent.Offset,
                                m_PositionVertexBuffer);
                AddVertexStream(positionVertexStream);
                semanticIndex++;
            }            

            // 2. normal stream component
            if (m_NormalStreamComponent != null)
            {
                H1VertexStream normalVertexStream = new H1VertexStream(m_NormalStreamComponent.Name, baseSemanticName, semanticIndex,
                m_NormalStreamComponent.ElementType, m_NormalStreamComponent.Stride, m_NormalStreamComponent.Offset,
                m_NormalVertexBuffer);
                AddVertexStream(normalVertexStream);
                semanticIndex++;
            }                                 

            // 3. texture coordinate stream components
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
                H1VertexStream stream = new H1VertexStream(name, baseSemanticName, semanticIndex, component.ElementType, component.Stride, component.Offset, m_TexcoordVertexBuffers[i]);
                AddVertexStream(stream);
                semanticIndex++;
            }

            // 4. color stream component
            if (m_ColorStreamComponent != null)
            {
                // @TODO - need to fix, temporary set as '4'
                semanticIndex = 4;

                H1VertexStream colorVertexStream = new H1VertexStream(m_ColorStreamComponent.Name, baseSemanticName, semanticIndex,
                m_ColorStreamComponent.ElementType, m_ColorStreamComponent.Stride, m_ColorStreamComponent.Offset,
                m_ColorVertexBuffer);
                AddVertexStream(colorVertexStream);                
            }

            // finally generate vertex declaration
            if (!base.GenerateVertexDeclaration())
                throw new Exception("failed to create vertex declaration please check!");
        }

        public static new void ModifyCompilationEnvironment(H1MaterialResource material, H1ShaderCompileHelper.H1ShaderCompilerEnvironment outEnvironment)
        {          
            // set number of texture coordinate space
            outEnvironment.SetDefine("H1_NUM_TEXCOORD2D", 1);
            outEnvironment.SetDefine("H1_NUM_TEXCOORD3D", 0); // currently disable TEXCOORD3D

            if (material.DrawWithColor) // depending on material type enabling drawing with color
                outEnvironment.SetDefine("H1_COLOR", 1);
            else
                outEnvironment.SetDefine("H1_COLOR", 0);
        }

        // @TODO - I need to wrap command list separately!, it is just temporary method! SHOULD!! FIX!!
        public void setVertexBuffers(SharpDX.Direct3D12.GraphicsCommandList commandList)
        {
            SharpDX.Direct3D12.VertexBufferView positionVertexBufferView = ((Gen2Layer.H1VertexBufferView)m_PositionVertexBuffer.View).View;
            SharpDX.Direct3D12.VertexBufferView normalVertexBufferView = ((Gen2Layer.H1VertexBufferView)m_NormalVertexBuffer.View).View;
            SharpDX.Direct3D12.VertexBufferView texcoordVertexBufferView = ((Gen2Layer.H1VertexBufferView)m_TexcoordVertexBuffers[0].View).View;
            SharpDX.Direct3D12.VertexBufferView colorVertexBufferView = ((Gen2Layer.H1VertexBufferView)m_ColorVertexBuffer.View).View;

            commandList.SetVertexBuffer(0, positionVertexBufferView);
            commandList.SetVertexBuffer(1, normalVertexBufferView);
            commandList.SetVertexBuffer(2, texcoordVertexBufferView);
            commandList.SetVertexBuffer(3, colorVertexBufferView);            
        }

        // vertex buffer object's references which is not real holder for vertex buffer objects!
        private H1VertexBuffer m_PositionVertexBuffer;
        private H1VertexBuffer m_NormalVertexBuffer;
        private H1VertexBuffer m_ColorVertexBuffer;
        private readonly List<H1VertexBuffer> m_TexcoordVertexBuffers = new List<H1VertexBuffer>();

        private H1VertexStreamComponent m_PositionStreamComponent;
        private H1VertexStreamComponent m_NormalStreamComponent;
        private H1VertexStreamComponent m_ColorStreamComponent;
        private readonly List<H1VertexStreamComponent> m_TexcoordStreamComponents = new List<H1VertexStreamComponent>();       

        private int m_TexCoord2DStreamComponentNum = -1;
        private int m_TexCoord3DStreamComponentNum = -1;
    }
}
