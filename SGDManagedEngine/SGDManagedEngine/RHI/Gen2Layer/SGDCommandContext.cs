using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1CommandContext
    {
        public H1DynamicDescriptorHeap DynamicDescriptorHeap
        {
            get { return m_DynamicDescriptorHeap; }
            set { m_DynamicDescriptorHeap = value; }
        }

        public H1CommandListManager CommandListManager
        {
            get { return m_CommandListManagerRef; }
        }

        public H1CommandListType CommandListType
        {
            get { return m_Type; }
        }

        public H1GpuResLinearAllocator CpuLinearAllocator
        {
            get { return m_CpuLinearAllocator; }
        }

        public H1GpuResLinearAllocator GpuLinearAllocator
        {
            get { return m_GpuLinearAllocator; }
        }

        public H1CommandContext(H1CommandListManager commandListManagerRef, H1CommandListType type)
        {
            m_Type = type;
            m_CommandListManagerRef = commandListManagerRef;
        }

        public virtual Boolean Initialize()
        {
            Boolean bInitialized = false;
            InitializePlatformDependent(ref bInitialized);
            return bInitialized;
        }

        public virtual void Destroy()
        {
            DestroyPlatformDependent();
        }

        protected H1CommandListType m_Type;
        protected H1CommandListManager m_CommandListManagerRef;
        protected H1RootSignature m_CurrGraphicsRootSignatureRef;
        protected H1RootSignature m_CurrComputeRootSignatureRef;
        protected H1PipelineStateObject m_CurrGraphicsPSORef;
        protected H1PipelineStateObject m_CurrComputePSORef;
        protected H1GpuResLinearAllocator m_CpuLinearAllocator = new H1GpuResLinearAllocator(H1LinearAllocationType.CpuWritable);
        protected H1GpuResLinearAllocator m_GpuLinearAllocator = new H1GpuResLinearAllocator(H1LinearAllocationType.GpuExclusive);
        protected H1DynamicDescriptorHeap m_DynamicDescriptorHeap;
    }

    public partial class H1GraphicsCommandContext : H1CommandContext
    {
        public H1GraphicsCommandContext(H1CommandListManager commandListManagerRef, H1CommandListType type)
            : base (commandListManagerRef, type)
        {
            
        }

        public override Boolean Initialize()
        {
            return base.Initialize();
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }

    public partial class H1ComputeCommandContext : H1CommandContext
    {
        public H1ComputeCommandContext(H1CommandListManager commandListManagerRef, H1CommandListType type)
            : base (commandListManagerRef, type)
        {
           
        }

        public override bool Initialize()
        {
            return base.Initialize();
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
