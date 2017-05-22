using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1PrimitiveSceneProxy
    {

    }

    public class H1PrimitiveComponent : H1SceneComponent
    {
        public H1PrimitiveComponent()
        {

        }

        // the minimum distance at which the primitve should be rendered, measured in worldspace units from the center of the primitive's boudning sphere to the camera position
        protected float m_MinDrawDistance;
        // the max distance (draw) exposed to LDs
        protected float m_LDMaxDrawDistance;
        // the distance to cull this primitive at
        // '0' indicates that the primitive shoudl NOT be culled by distance
        protected float m_CachedMaxDrawDistance;

        // used by the renderer, to identify a component access re-registers
        // to identify UPrimitiveComponent on the rendering thread without having to pass pointer around
        protected Int64 m_ComponentId;
        // should be thread-safe counter
        protected Int64 m_NextComponentId;
        // set of actors to ignore during component sweeps in MoveComponent
        protected List<H1Actor> m_MoveIgnoreActors = new List<H1Actor>();
        // encapsulates the data which is mirrored to render a UPrimtiveComponent parallel to the game-thread
        // private H1PrimitiveSceneProxy
        // H1RenderCommandFence m_DetachFence

        // thread-safe counter 
        // incremented by the main thread before being attached to the scene, decremented by the rendering thread after removal
        protected Int64 m_AttachmentCounter;
        // LOD parent primitive to draw instead of this one
        protected H1PrimitiveComponent m_LODParentPrimitiveRef;        
    }
}
