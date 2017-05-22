using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1PipelineStateObject
    {
        public H1RootSignature RootSignature
        {
            get { return m_RootSignature; }
            set { m_RootSignature = value; }
        }

        public H1PipelineStateObject()
        {

        }

        protected H1RootSignature m_RootSignature;
    }

    public partial class H1GraphicsPipelineStateObject : H1PipelineStateObject
    {
        public H1GraphicsPipelineStateObject()
        {
            ConstructPlatformDependentMembers();
        }
    }

    public partial class H1ComputePipelineStateObject : H1PipelineStateObject
    {
        public H1ComputePipelineStateObject()
        {

        }
    }
}
