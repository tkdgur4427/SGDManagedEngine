using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD
{
    public class H1AnimSequenceBase : H1AnimationAsset
    {
        //private List<FAnimNotifyEvent>
        // length (in seconds) of this AnimSequence if played back with a speed of 1.0
        private float m_SequenceLength;
        // nmber for tweaking of this animation globally
        private float m_RateScale;
    }    
}
