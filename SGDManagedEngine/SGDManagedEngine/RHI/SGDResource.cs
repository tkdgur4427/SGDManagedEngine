using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public abstract class H1Resource
    {
        public H1Resource(H1GPUResourceManager resourceManager, uint sizeInBytes)            
        {
            m_ResourceManagerRef = resourceManager;
            m_SizeInBytes = sizeInBytes;
        }

        protected uint m_ResourceId;
        protected uint m_SizeInBytes;
        protected H1GPUResourceManager m_ResourceManagerRef;
    }
}
