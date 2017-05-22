using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1SceneComponent : H1ActorComponent
    {
        public H1SceneComponent()
        {

        }

        protected H1SceneComponent m_AttachParentRef;
        protected List<H1SceneComponent> m_AttachChildren = new List<H1SceneComponent>();
        // optional socket name on AttachParaent that we are attached to
        protected String m_AttachSocketName;
        // private APhysicsVolume
        // private FSphereBounds
        protected H1Transform m_ComponentToWorld = new H1Transform();
        // FRotationConversionCache WorldRotationCache
        protected Vector3 m_RelativeLocation;
        protected Vector3 m_RelativeRotation;
    }
}
