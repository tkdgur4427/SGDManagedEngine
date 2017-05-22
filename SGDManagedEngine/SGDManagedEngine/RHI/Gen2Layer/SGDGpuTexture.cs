using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    public partial class H1GpuTexture
    {
        public H1GpuResource Resource
        {
            get { return m_Resource; }
        }

        public H1GpuTexture()
        {
            m_Resource = null;
            ConstructPlatformDependentMembers();
        }

        public virtual void Destroy()
        {
            DestructPlatformDependentMembers();
        }

        protected H1GpuResource m_Resource;        
    }

    public partial class H1GpuTexture1D : H1GpuTexture
    {
        
    }

    public partial class H1GpuTexture2D : H1GpuTexture
    {
        public static H1GpuTexture2D Create(Vector4 clearValue, H1Texture2D.Description desc, H1SubresourceData initialData)
        {
            H1GpuTexture2D newTex2D = new H1GpuTexture2D();
            CreatePlatformDependent(clearValue, desc, initialData, ref newTex2D);

            if (newTex2D.Resource == null)
            {
                newTex2D.Destroy();
                return null;
            }

            return newTex2D;
        }
    }
}
