using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer.DirectX12
{
    public class H1CommandSignatureDX12
    {
        private Boolean m_Finalized;
        private Int32 m_NumParameters;
        private IndirectArgumentDescription[] m_ParamArray;
        private CommandSignature m_Signature;
    }
}
