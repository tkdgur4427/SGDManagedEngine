using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    // base mesh drawing policy
    // subclasses are used to draw meshes with type-specific context variables
    public class H1MeshDrawingPolicy
    {
        public class ElementDataType { }

        private H1VertexFactory m_VertexFactory;
        //private H1MaterialRenderProxy* m_MaterialRenderProxy;
        private H1MaterialResource m_MaterialResource;
        //...
    }
}
