using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1StaticMeshDrawListBase
    {

    }

    // a set of static meshes, each associated with a mesh drawing policy of a particular type
    public class H1StaticMeshDrawList<DrawingPolicyType> where DrawingPolicyType : H1MeshDrawingPolicy
    {
        public class H1ElementHandle : H1StaticMeshBatch.H1DrawListElementLink
        {
            // @TODO - need to override two virtual methods

            public H1StaticMeshDrawList<DrawingPolicyType> StaticMeshDrawList;
            public Int32 ElementIndex;
        }

        public class H1Element
        {
            // @TODO - need to think about this
            //public DrawingPolicyType.ElementDataType PolicyData;
            public H1StaticMeshBatch MeshRef;
        }

        public class H1DrawingPolicyLink
        {
            public List<H1Element> Elements = new List<H1Element>();
            public DrawingPolicyType DrawingPolicyRef;
            H1StaticMeshDrawList<DrawingPolicyType> DrawListRef;
        }

        // all drawing policies in the draw list, in rendering order
        private List<H1DrawingPolicyLink> OrderedDrawingPolicies = new List<H1DrawingPolicyLink>();
    }
}
