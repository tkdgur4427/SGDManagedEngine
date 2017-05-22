using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1Scene : H1SceneInterface
    {
        // world reference
        private H1World m_WorldRef;
        // depth priority groups
        // @TODO - can exist multiple DPG like forward/editor...
        H1DepthPriorityGroup m_DPG = new H1DepthPriorityGroup();
        // primitives
        private List<H1PrimitiveSceneInfo> m_Primitives = new List<H1PrimitiveSceneInfo>();
        // lights
        private List<H1LightSceneInfo> m_Lights = new List<H1LightSceneInfo>();        
    }
}
