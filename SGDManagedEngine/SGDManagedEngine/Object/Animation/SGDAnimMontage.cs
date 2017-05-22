using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1SlotAnimationTrack
    {
        public String SlotName;
        public H1AnimTrack AnimTrackRef;
    }

    public class H1AnimMontage : H1AnimCompositeBase
    {
        public List<H1SlotAnimationTrack> SlotAnimTracks
        {
            get { return m_SlotAnimTracks; }
        }

        private float m_BlendInTime;
        private float m_BlendOutTime;
        // time from SequenceEnd to trigger blendout
        // < 0 - means using BlendOutTime, so BlendOut finishes as Montage ends
        // >= 0 - means using 'SequenceEnd - BlendOutTriggerTime' to trigger blend out
        private float m_BlendOutTriggerTime;
        // TArray<FCompositeSection> CompositeSections
        // slot data, each slot contains anim track
        private List<H1SlotAnimationTrack> m_SlotAnimTracks = new List<H1SlotAnimationTrack>();
    }
}
