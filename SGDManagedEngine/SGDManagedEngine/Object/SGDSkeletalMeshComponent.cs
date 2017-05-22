using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1SkeletalMeshComponent : H1SkinnedMeshComponent
    {
        public H1SkeletalMeshComponent()
            : base()
        {
            H1ActorComponentRegistrator.RegisterActorComponent<H1SkeletalMeshComponent>();
        }

        public H1AnimInstance AnimScriptInstance
        {
            get { return m_AnimScriptInstance; }
        }

        // whether to use Animation Blueprint or play single animation Asset
        // EAnimationMode AnimationMode

        // the AnimBlueprint class to use
        // private AnimInstanceClass m_AnimClass
        // the active animation graph program instance
        protected H1AnimInstance m_AnimScriptInstance = new H1AnimInstance();
        // FSingleAnimationPlayData
        // temporary array of local-space (relative parent) rotation/translation for each bone
        protected List<H1Transform> m_LocalAtoms = new List<H1Transform>();

        // update rate
        // cached LocalAtoms for update rate optimization
        protected List<H1Transform> m_CachedLocalAtoms = new List<H1Transform>();
        // cached SpaceBones
        protected List<H1Transform> m_CachedSpaceBases = new List<H1Transform>();
        // FBlendCurve
        // FBlendCurve CachedBlendCurve
        protected float m_GlobalAnimRateScale;
        protected Vector3 m_RootBoneTranslation;

        // TickAnimation(float deltaTime);
    }
}
