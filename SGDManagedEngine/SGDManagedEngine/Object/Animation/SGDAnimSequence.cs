using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1RawAnimSequenceTrack
    {
        private List<Vector3> m_PosKeys = new List<Vector3>();
        private List<Quaternion> m_RotKeys = new List<Quaternion>();
        private List<Vector3> m_ScaleKeys = new List<Vector3>();
    }

    public class H1TranslationTrack
    {
        public List<Vector3> PosKeys = new List<Vector3>();
        public List<float> Times = new List<float>();
    }

    public class H1RotationTrack
    {
        public List<Quaternion> RotKeys = new List<Quaternion>();
        public List<float> Times = new List<float>();
    }

    public class H1ScalingTrack
    {
        public List<Vector3> ScaleKeys = new List<Vector3>();
        public List<float> Times = new List<float>();
    }

    public class H1AnimSequence : H1AnimSequenceBase
    {
        public List<H1TranslationTrack> TranslationTracks
        {
            get { return m_TranslationData; }
        }

        public List<H1RotationTrack> RotationTracks
        {
            get { return m_RotationData; }
        }

        public List<H1ScalingTrack> ScaleTracks
        {
            get { return m_ScalingData; }
        }

        // number of raw frames in this sequence (not used by engine - information purpose)
        private Int32 m_NumFrames;
        // TrackToSkeletonMapTable(i) should contain track mapping data for RawAnimation(i)
        //private List<>
        // raw uncompressed keyframe data
        private List<H1RawAnimSequenceTrack> m_AnimationRawData = new List<H1RawAnimSequenceTrack>();
        private List<H1TranslationTrack> m_TranslationData = new List<H1TranslationTrack>();
        private List<H1RotationTrack> m_RotationData = new List<H1RotationTrack>();
        private List<H1ScalingTrack> m_ScalingData = new List<H1ScalingTrack>();

        // base pose to use when retargetting
        private String m_RetargetSource;
    }
}
