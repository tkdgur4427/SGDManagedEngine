using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1VisualDebugger
    {
        public List<H1RenderUtils.H1DynamicMeshBuilder> DebugDynamicMeshBuilders
        {
            get { return m_DebugDynamicMeshBuilders; }
        }

        public H1VisualDebugger()
        {

        }

        public H1RenderUtils.H1DynamicMeshBuilder GetNewDynamicMeshBuilder()
        {
            Int32 availableIndex = m_DebugDynamicMeshBuilders.FindIndex(x => x == null);
            if (availableIndex != -1)
            {
                m_DebugDynamicMeshBuilders[availableIndex] = new H1RenderUtils.H1DynamicMeshBuilder();
            }
            else
            {
                availableIndex = m_DebugDynamicMeshBuilders.Count;
                m_DebugDynamicMeshBuilders.Add(new H1RenderUtils.H1DynamicMeshBuilder());
            }
            
            return m_DebugDynamicMeshBuilders[availableIndex];
        }

        public void ClearAllMeshBuilders()
        {
            for (Int32 index = 0; index < m_DebugDynamicMeshBuilders.Count; index++)
            {
                // dispose unmanaged resource and nullify for next use
                m_DebugDynamicMeshBuilders[index].Dispose();
                m_DebugDynamicMeshBuilders[index] = null;
            }            
        }

        private List<H1RenderUtils.H1DynamicMeshBuilder> m_DebugDynamicMeshBuilders = new List<H1RenderUtils.H1DynamicMeshBuilder>();
    }
}
