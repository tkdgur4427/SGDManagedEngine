using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD
{
    public class H1VertexDeclaration
    {
        public H1VertexDeclaration(H1VertexStream[] streams)
        {
            // create input layout based on the result
            m_InputLayoutDesc = new Gen2Layer.H1InputLayout(streams);
        }

        public Gen2Layer.H1InputLayout InputLayout
        {
            get { return m_InputLayoutDesc; }
        }

        private Gen2Layer.H1InputLayout m_InputLayoutDesc;        
    }
}
