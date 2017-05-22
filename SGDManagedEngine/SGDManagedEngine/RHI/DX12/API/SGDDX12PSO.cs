using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Direct3D12
{
    class H1PSO
    {
        private Int32 m_Hash;
        private PipelineState m_DX12PipelineState;
    }

    class H1GraphicsPSO : H1PSO
    {
        private GraphicsPipelineStateDescription m_Desc;
    }

    class H1ComputePSO : H1PSO
    {
        private ComputePipelineStateDescription m_Desc;
    }

    class H1PSOCache
    {
        private H1DX12Device m_DeviceRef;
        private Dictionary<Int32, H1PSO> m_PSOMap;
    }
}
