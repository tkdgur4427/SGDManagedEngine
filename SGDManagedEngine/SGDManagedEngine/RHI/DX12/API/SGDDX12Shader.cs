using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    public enum H1ShaderStage
    {
        // graphics pipeline
        Vertex,
        Hull,
        Domain,
        Geometry,
        Pixel,

        // compute pipeline
        Compute,
        Num,
    }

    public class H1BindRanges
    {
        public H1Range this[Int32 Index]
        {
            get { return m_Ranges[Index]; }
        }

        public Int32 Num
        {
            get { return m_Ranges.Count; }
        }

        public Int32 TotalLength
        {
            get { return m_TotalLength; }
        }

        public struct H1Range
        {
            public Byte Start;  // starting index 
            public Byte Length; // length (number) of registers
            // flags using large space for padding
            public UInt16 bUsed;
            public UInt16 bShared;
            public UInt16 bUnmergable;
        }

        public void AddRange(H1Range range)
        {
            m_Ranges.Add(range);
        }

        private List<H1Range> m_Ranges = new List<H1Range>();
        private Int32 m_TotalLength;
    }

    public class H1ResourceRanges
    {
        public H1ResourceRanges()
        {

        }

        // copy constructor
        public H1ResourceRanges(H1ResourceRanges ranges)
        {
            // constant buffers
            H1BindRanges bindRanges = ranges.ConstantBuffers;            
            for (Int32 i = 0; i < bindRanges.Num; ++i)
                m_ConstantBuffers.AddRange(bindRanges[i]);            

            // input resources
            bindRanges = ranges.InputResources;
            for (Int32 i = 0; i < bindRanges.Num; ++i)
                m_InputResources.AddRange(bindRanges[i]);

            // output resources
            bindRanges = ranges.OutputResources;
            for (Int32 i = 0; i < bindRanges.Num; ++i)
                m_OutputResources.AddRange(bindRanges[i]);

            // samplers
            bindRanges = ranges.Samplers;
            for (Int32 i = 0; i < bindRanges.Num; ++i)
                m_Samplers.AddRange(bindRanges[i]);
        }

        public H1BindRanges ConstantBuffers
        {
            get { return m_ConstantBuffers; }
        }

        public H1BindRanges InputResources
        {
            get { return m_ConstantBuffers; }
        }

        public H1BindRanges OutputResources
        {
            get { return m_OutputResources; }
        }

        public H1BindRanges Samplers
        {
            get { return m_Samplers; }
        }

        private H1BindRanges m_ConstantBuffers = new H1BindRanges();   // CBV
        private H1BindRanges m_InputResources = new H1BindRanges();    // SRV
        private H1BindRanges m_OutputResources = new H1BindRanges();   // UAV
        private H1BindRanges m_Samplers = new H1BindRanges();          // SMP
    }

    public class H1Shader : IComparable
    {
        public Byte[] ShaderByteCode
        {
            get { return m_Bytecode.Buffer; }
        }

        public H1ResourceRanges ResourceRanges
        {
            get { return m_Ranges; }
        }

        public H1Shader(Byte[] shaderByteCode, H1ResourceRanges ranges)
        {
            m_Bytecode = new ShaderBytecode(shaderByteCode);
            m_Ranges = new H1ResourceRanges(ranges);
        }

        private ShaderBytecode m_Bytecode;
        private H1ResourceRanges m_Ranges;

        public int CompareTo(object obj)
        {
            // just compare instance pointer (reference)
            return Convert.ToInt32(this != obj);
        }
    }
}
