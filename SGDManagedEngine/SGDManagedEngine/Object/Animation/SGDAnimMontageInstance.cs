using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1AnimMontageInstance
    {
        public H1AnimMontage MontageRef
        {
            set { m_MontageRef = value; }
            get { return m_MontageRef; }
        }

        private H1AnimMontage m_MontageRef;
        private float m_DesiredWeight;
        private float m_BlendTime;
        private float m_Weight;
        // blend time multipler to allow extending and narrowing blend times
        private float m_DefaultBlendTimeMultipler;
        // list of next sections per section
        private List<Int32>[] m_NextSections;
        private Boolean m_bPlaying;
        // reference to AnimInstance
        private H1AnimInstance m_AnimInstance;
        private float m_Position;
        private float m_PlayRate;
        // followers this montage will synchonize
        private List<H1AnimMontageInstance> m_MontageSyncFollowers = new List<H1AnimMontageInstance>();
        // frame counter to sync montage once per frame
        private UInt32 m_MontageSyncUpdateFrameCounter;
    }
}
