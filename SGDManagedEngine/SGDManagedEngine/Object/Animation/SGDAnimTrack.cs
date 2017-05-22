using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1AnimSegment
    {
        public H1AnimSequence AnimSequence
        {
            get
            {
                H1AnimSequence animSeq = m_AnimReference as H1AnimSequence;
                return animSeq;
            }
        }

        // @TODO - temporary set anim sequence : need to fix this later to find real instance storage for H1AnimSequence
        public void SetAnimSequence(H1AnimSequence animSequence)
        {
            m_AnimReference = animSequence;
        }

        // anim reference to play only allow AnimSequence and AnimComposite
        private H1AnimSequenceBase m_AnimReference;
        private float m_StartPos;
        private float m_AnimStartTime;
        private float m_AnimEndTime;
        private float m_AnimPlayRate;
        private Int32 m_LoopingCount;
    }

    public class H1AnimTrack
    {
        public List<H1AnimSegment> AnimSegments
        {
            get { return m_AnimSegments; }
        }

        private List<H1AnimSegment> m_AnimSegments = new List<H1AnimSegment>();
    }
}
