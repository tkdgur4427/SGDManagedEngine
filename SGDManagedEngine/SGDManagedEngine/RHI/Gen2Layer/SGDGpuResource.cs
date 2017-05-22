using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1GpuResource
    {
        public long GpuVirtualAddress
        {
            get { return m_GpuVirtualAddress; }
        }

        public H1ResourceStates UsageStates
        {
            get { return m_UsageStates; }
            set { m_UsageStates = value; }
        }

        public H1ResourceStates TransitioningStates
        {
            get { return m_TransitioningStates; }
            set { m_TransitioningStates = value; }
        }

        // static constructor to call static methods for H1GpuResource
        static H1GpuResource()
        {
            // static member variable initialization
            InitializeMappers();
        }

        //@TODO - temporary set default constructor, but need to erase!
        protected H1GpuResource()
        {
            // defaultly set it as 'Single'
            m_AllocPolicyType = Memory.H1GpuResAllocPolicyType.Single;
        }

        protected H1GpuResource(Memory.H1GpuResAllocPolicyType policyType)
        {
            // set alloc policy type
            m_AllocPolicyType = policyType;
        }

        protected Boolean CreateResource(H1HeapType heapType, H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {           
            Int32 totalSizeInBytes = CreateResourcePlatformDependent(heapType, resourceDesc, defaultUsage);
            if (m_GpuResource == null)
                return false;

            // based on created platform dependent resource information, create alloc policy
            RegisterAllocPolicy(totalSizeInBytes);

            return true;
        }

        protected void DestroyResource()
        {
            DestroyResourcePlatformDependent();
        }

        protected IntPtr Map(Int32 subResource = 0)
        {
            IntPtr outPtr = new IntPtr();
            MapPlatformDependent(ref outPtr, subResource);

            return outPtr;
        }

        protected void Unmap(Int32 subResource = 0)
        {
            UnmapPlatformDependent(subResource);
        }

        protected void RegisterAllocPolicy(Int64 totalSizeInBytes)
        {
            switch (m_AllocPolicyType)
            {
                case Memory.H1GpuResAllocPolicyType.Single:
                    m_AllocPolicy = new Memory.H1GpuResAllocPolicySingle(totalSizeInBytes);
                    break;
                case Memory.H1GpuResAllocPolicyType.Segmented:
                    m_AllocPolicy = new Memory.H1GpuResAllocPolicySegmented(totalSizeInBytes);
                    break;
            }
        }

        public static Int32[] ResourceStateMapper = new Int32[Convert.ToInt32(H1ResourceStates.TotalNum)];
        public static Int32[] DimensionMapper = new Int32[Convert.ToInt32(H1Dimension.TotalNum)];
        public static Int32[] TextureLayoutMapper = new Int32[Convert.ToInt32(H1TextureLayout.TotalNum)];

        // NOTE - resource flags mapper can have combination of flags (so watch out!) 
        //      - don't use this directly, use 'H1RHIDefinitionHelper class'
        public static Int32[] ResourceFlagsMapper = new Int32[Convert.ToInt32(H1ResourceFlags.TotalNum)];

        private H1ResourceStates m_UsageStates = new H1ResourceStates();
        private H1ResourceStates m_TransitioningStates = new H1ResourceStates();
        private long m_GpuVirtualAddress;

        // alloc policy
        protected Memory.H1GpuResAllocPolicyType m_AllocPolicyType = Memory.H1GpuResAllocPolicyType.Invalid;
        protected Memory.H1GpuResAllocPolicy m_AllocPolicy = null; // deferred creation after the gpu resource is created
    }

    public class H1GpuResourceSingle : H1GpuResource
    {
        protected H1GpuResourceSingle()
            : base (Memory.H1GpuResAllocPolicyType.Single)
        {
            
        }

        public static H1GpuResourceSingle CreateGpuResource(H1HeapType heapType, H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {
            H1GpuResourceSingle newRes = new H1GpuResourceSingle();
            if (!newRes.CreateResource(heapType, resourceDesc, defaultUsage))
            {
                newRes = null; // nullify newly created resource
            }
            return newRes;
        }
                
        public static H1GpuResourceSingle CreateEmptyGpuResource()
        {
            H1GpuResourceSingle newEmptyRes = new H1GpuResourceSingle();
            return newEmptyRes;
        }
    }

    public class H1GpuResourceSegmented : H1GpuResource
    {
        protected H1GpuResourceSegmented()
            : base (Memory.H1GpuResAllocPolicyType.Segmented)
        {
           
        }

        public static H1GpuResourceSegmented CreateGpuResource(H1HeapType heapType, H1GpuResourceDesc resourceDesc, H1ResourceStates defaultUsage)
        {
            H1GpuResourceSegmented newRes = new H1GpuResourceSegmented();
            if (!newRes.CreateResource(heapType, resourceDesc, defaultUsage))
            {
                newRes = null;
            }
            return newRes;
        }

        public static H1GpuResourceSegmented CreateEmptyGpuResource()
        {
            H1GpuResourceSegmented newEmptyRes = new H1GpuResourceSegmented();
            return newEmptyRes;
        }
    }    
}
