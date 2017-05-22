using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1ShaderId : IEquatable<H1ShaderId>
    {
        public Guid Guid
        {
            get { return m_Guid; }
        }

        public H1ShaderId()
        {
            CreateNewGuid();
        }

        public H1ShaderId(Guid newGuid)
        {
            m_Guid = newGuid;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as H1ShaderId);
        }

        public override int GetHashCode()
        {
            return m_Guid.GetHashCode();
        }

        public bool Equals(H1ShaderId obj)
        {
            return m_Guid == obj.m_Guid;
        }

        public Guid CreateNewGuid()
        {
            return m_Guid = Guid.NewGuid();
        }

        // shader id is indicated by the guid
        private Guid m_Guid;
    }

    public enum H1ShaderFrequency
    {
        VertexShader = 0, // vertex shader
        PixelShader,      // pixel shader
    }

    public enum H1ShaderPlatform
    {
        SM5_1 = 0,
    }

    public class H1ShaderTarget
    {
        public H1ShaderFrequency Freqency { get; set; }
        public H1ShaderPlatform Platform { get; set; }
        public String ToFormat
        {
            get { return ShaderFrequencyFormatter[Convert.ToInt32(Freqency)] + "_" + ShaderPlatformFormatter[Convert.ToInt32(Platform)]; }
        }        

        private static readonly String[] ShaderFrequencyFormatter =
        {
            "vs",
            "ps",
        };

        private static readonly String[] ShaderPlatformFormatter =
        {
            "5_1",
        };
    }

    public enum H1ParameterType
    {
        Variable,
        ConstantBuffer,
        ShaderResourceView,
        UnorderedAccessView,
        Sampler,
    }

    public class H1ParameterAllocation
    {
        public H1ParameterAllocation(H1ParameterType type, int bufferIndex, int offset, int size)
        {
            m_Type = type; m_BufferIndex = bufferIndex; m_BaseIndex = offset; m_Size = size;
        }

        public int BufferIndex
        {
            get { return m_BufferIndex; }
        }

        public int Offset
        {
            get { return m_BaseIndex; }
        }

        public int Size
        {
            get { return m_Size; }
        }

        public H1ParameterType Type
        {
            get { return m_Type; }
        }

        public bool IsVariable
        {
            // if the buffer index and base index are -1, it is not variable type!
            get { return m_Type == H1ParameterType.Variable; }
        }

        private int m_BufferIndex = -1; // buffer index that the parameter reside in
        private int m_BaseIndex = -1;   // base index (offset) in the buffer
        private int m_Size = -1;        // size
        // additional data
        private H1ParameterType m_Type;
    }

    public class H1ShaderParameterMap
    {
        public Dictionary<String, H1ParameterAllocation> ParameterMap
        {
            get { return m_ParameterMap; }
        }

        private readonly Dictionary<String, H1ParameterAllocation> m_ParameterMap = new Dictionary<string, H1ParameterAllocation>();
    }

    // general shader parameter structure
    class H1ShaderParameter
    {
        private UInt16 m_BufferIndex = 0xFFFF;
        private UInt16 m_BaseIndex = 0xFFFF;
        private UInt16 m_Size = 0xFFFF;
    }
}
