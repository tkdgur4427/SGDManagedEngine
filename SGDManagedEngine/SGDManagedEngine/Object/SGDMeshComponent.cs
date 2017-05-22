using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1MeshComponent : H1PrimitiveComponent
    {
        // per-component material overrides
        protected List<H1MaterialInterface> m_OverrideMaterials = new List<H1MaterialInterface>();
    }
}
