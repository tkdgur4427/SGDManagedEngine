using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1AnimationAsset : H1Object
    {
        // pointer to the skeleton this asset can be played on
        private H1Skeleton m_SkeletonRef;
        // if changes you need to remap info
        private Guid m_SkeletonGuid;
        //private List<H1AnimMetaData>
    }
}
