using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    // interface for mesh rendering data
    public class H1SkeletalMeshObject
    {
        public H1SkeletalMeshObject(H1SkeletalMeshResource skeletalMeshResourceRef)
        {
            m_SkeletalMeshResourceRef = skeletalMeshResourceRef;
        }

        // up-to this number of chunks, we can use on the GPU skinning cache
        // no point in using TArray as it's using Int16 elements
        const Int32 MAX_GPUSKINCACHE_CHUNKS_LOD = 32;

        // setup for rendering a specific LOD entry of the component
        public class H1SkelMeshObjectLODInfo
        {
            // material index not section index (hidden material section flags for rendering)
            public List<Boolean> HiddenMaterials = new List<Boolean>();
        }

        protected List<H1SkelMeshObjectLODInfo> m_LODInfos = new List<H1SkelMeshObjectLODInfo>();
        //private List<FCapsuleShape> ShadowCapsuleShapes;
        // this frame's min desired LOD level
        protected Int32 m_MinDesiredLODLevel;
        // high(best) distance factor that was desired for rendering this skeletal mesh last frame
        protected float m_MaxDistanceFactor;
        protected float m_WorkingMaxDistanceFactor;
        // this is set to use when we have sent out mesh data to the rendering thread
        protected Boolean m_bHasBeenUpdatedAtLeastOnce;
        // the skeletal mesh resource with which to render
        protected H1SkeletalMeshResource m_SkeletalMeshResourceRef;
        protected List<H1SkeletalMeshLODInfo> m_SkeletalMeshLODInfo = new List<H1SkeletalMeshLODInfo>();
        // GPU skin cache keys per chunk - -1 means not suing GPUSkinCache
        protected Int16[] m_GPUSkinCacheKeys = new Int16[MAX_GPUSKINCACHE_CHUNKS_LOD];
        // used to keep track of the first call to UpdateMinDesiredLODLevel each frame
        protected UInt32 m_LastFrameNumber;
    }
}
