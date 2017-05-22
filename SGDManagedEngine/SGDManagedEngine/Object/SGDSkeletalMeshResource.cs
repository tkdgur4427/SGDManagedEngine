using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1SkeletalMeshResource
    {
        public H1StaticLODModel GetLODModel(Int32 index)
        {
            return m_LODModels[index];
        }

        public H1StaticLODModel AddLODModel()
        {
            Int32 newIndex = m_LODModels.Count;
            m_LODModels.Add(new H1StaticLODModel());
            return m_LODModels[newIndex];
        }

        private List<H1StaticLODModel> m_LODModels = new List<H1StaticLODModel>();        
    }
}
