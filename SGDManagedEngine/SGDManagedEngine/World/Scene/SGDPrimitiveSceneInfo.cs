using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    // the information used to render a primitive
    public class H1PrimitiveSceneInfo
    {
        private H1PrimitiveSceneProxy m_PrimitiveSceneProxy;
        private H1PrimitiveComponent m_PrimitiveComponentRef;
        private H1Actor m_ActorRef;
        private List<H1StaticMeshBatch> m_StaticMeshes = new List<H1StaticMeshBatch>();
    }
}
