using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using SharpDX;

namespace SGDManagedEngine.SGD
{
    public class H1AnimTickRecord
    {
        private H1AnimationAsset m_SourceAssetRef;
        private float[] m_TimeAccumulator;
        private Vector3 m_BlendSpacePosition;
        // FBlendFilter
        //private List<FBlendSampleData>
        private float m_PlayRateMultipler;
        private float m_EffectiveBlendWeight;
        private Boolean m_bLooping;
    }

    public class H1AnimGroupInstance
    {
        private List<H1AnimTickRecord> m_ActivePlayers = new List<H1AnimTickRecord>();
        private Int32 m_GroupLeaderIndex;
    }

    public class H1MontageEvalutionState
    {
        private H1AnimMontage m_MontageRef;
        private float m_MontageWeight;
        private float m_MontagePosition;
    }

    public class H1AnimInstance : H1Object
    {
        // @TODO - temporary processing animation context, need to think about where to put this functionality
        public Boolean ProcessAnimationContext(H1AnimationContext animContext)
        {
            H1AnimMontage montage = new H1AnimMontage();

            foreach (H1AnimationContext.AnimSequence animSeq in animContext.AnimSequences)
            {
                String animSeqName = animSeq.AnimSeqName;
                float duration = Convert.ToSingle(animSeq.Duration);
                float tickInSec = Convert.ToSingle(animSeq.TicksPerSecond);

                // @TODO - temporary insert duration variable
                Duration = duration;
                TickInSec = tickInSec;

                foreach (H1AnimationContext.JointAnimation jointAnimation in animSeq.BoneAnimations)
                {
                    String boneName = jointAnimation.BoneName;
                    H1SlotAnimationTrack slotAnimationTrack = new H1SlotAnimationTrack();
                    slotAnimationTrack.SlotName = boneName;
                    slotAnimationTrack.AnimTrackRef = new H1AnimTrack();

                    H1AnimSegment animSegment = new H1AnimSegment();
                    H1AnimSequence animSequence = new H1AnimSequence();

                    // 1. process position keys
                    animSequence.TranslationTracks.Add(new H1TranslationTrack());
                    foreach (H1AnimationContext.PositionKey positionKey in jointAnimation.PosKeys)
                    {
                        animSequence.TranslationTracks[0].PosKeys.Add(positionKey.Value);
                        animSequence.TranslationTracks[0].Times.Add(Convert.ToSingle(positionKey.Time));
                    }

                    // 2. process rotation keys
                    animSequence.RotationTracks.Add(new H1RotationTrack());
                    foreach (H1AnimationContext.QuaternionKey quaternionKey in jointAnimation.RotKeys)
                    {
                        animSequence.RotationTracks[0].RotKeys.Add(quaternionKey.Value);
                        animSequence.RotationTracks[0].Times.Add(Convert.ToSingle(quaternionKey.Time));
                    }

                    // 3. process scale keys
                    animSequence.ScaleTracks.Add(new H1ScalingTrack());
                    foreach (H1AnimationContext.ScalingKey scaleKey in jointAnimation.ScaleKeys)
                    {
                        animSequence.ScaleTracks[0].ScaleKeys.Add(scaleKey.Value);
                        animSequence.ScaleTracks[0].Times.Add(Convert.ToSingle(scaleKey.Time));
                    }

                    animSegment.SetAnimSequence(animSequence);
                    slotAnimationTrack.AnimTrackRef.AnimSegments.Add(animSegment);
                    montage.SlotAnimTracks.Add(slotAnimationTrack);
                }                
            }

            H1AnimMontageInstance montageInstance = new H1AnimMontageInstance();
            montageInstance.MontageRef = montage;
            m_MontageInstances.Add(montageInstance);

            return true;
        }

        public Boolean ExtractInterpolatedLocalTransform(float deltaTime, ref List<InterpolatedLocalTransform> localTransforms)
        {
            StopWatch.Stop();
            deltaTime = StopWatch.Elapsed.Milliseconds / 1000.0f;

            //AccumulatedAnimationTime = 0.0f; //deltaTime * 2.0f;
            AccumulatedAnimationTime += 1.0f;

            foreach (InterpolatedLocalTransform interpolatedLocalTransform in localTransforms)
            {
                // find the matched bone name                
                H1SlotAnimationTrack slotAnimTrack = m_MontageInstances[0].MontageRef.SlotAnimTracks.Find(
                x => {
                    return interpolatedLocalTransform.BoneName == x.SlotName;
                });
                
                if (slotAnimTrack == null)
                {
                    // skip the current local transform (no modification exists)
                    continue;
                }

                float accumulatedAnimationTimeInMS = AccumulatedAnimationTime;
                H1AnimSequence animSeq = slotAnimTrack.AnimTrackRef.AnimSegments[0].AnimSequence;

                // invalidate for count mismatched in advance
                if (animSeq.TranslationTracks[0].PosKeys.Count != animSeq.RotationTracks[0].RotKeys.Count
                    || animSeq.TranslationTracks[0].PosKeys.Count != animSeq.ScaleTracks[0].ScaleKeys.Count)
                {
                    return false;
                }
                
                for (Int32 timeSlot = 0; timeSlot < animSeq.TranslationTracks[0].Times.Count; ++timeSlot)
                {
                    float trackTime = animSeq.TranslationTracks[0].Times[timeSlot];
                    if (AccumulatedAnimationTime < trackTime)
                    {
                        Vector3 position = animSeq.TranslationTracks[0].PosKeys[timeSlot];
                        Quaternion rotation = animSeq.RotationTracks[0].RotKeys[timeSlot];
                        Vector3 scale = animSeq.ScaleTracks[0].ScaleKeys[timeSlot];

                        interpolatedLocalTransform.LocalTransform = new H1Transform()
                        {
                            Translation = position,
                            Rotation = rotation,
                            Scaling = scale,
                        };                        

                        break;
                    }
                }
            }

            if (AccumulatedAnimationTime > Duration)
                AccumulatedAnimationTime -= Duration;

            StopWatch.Start();

            return true;
        }

        #region temporary animation testing variables
        public float AccumulatedAnimationTime = 0.0f;
        public float Duration = 0.0f;
        public float TickInSec = 0.0f;

        public class InterpolatedLocalTransform
        {
            public String BoneName;
            public H1Transform LocalTransform = new H1Transform();
        }

        public Stopwatch StopWatch = new Stopwatch();
        #endregion

        // used to extract animation, if mesh exists, this will be overwritten by mesh->Skeleton
        private H1Skeleton m_CurrentSkeletonRef;
        // the list of animation assets which are going to be evaluated this frame and need to be ticked(ungrouped)
        private List<H1AnimTickRecord> m_UngroupedActivePlayers = new List<H1AnimTickRecord>();
        // the set of tick group for this anim instance
        private List<H1AnimGroupInstance> m_SyncGroups = new List<H1AnimGroupInstance>();
        // ERootMotionMode
        // AnimMontage instances that are running currently - only one is primarily active per group, and the other ones are blending out
        private List<H1AnimMontageInstance> m_MontageInstances = new List<H1AnimMontageInstance>();
        // cached data for montage evalution, save us from having to access MontageInstances from slot nodes as that isn't thread-safe
        private List<H1MontageEvalutionState> m_MontageEvaluationData = new List<H1MontageEvalutionState>();
        private Dictionary<H1AnimMontage, H1AnimMontageInstance> m_ActiveMontageMap = new Dictionary<H1AnimMontage, H1AnimMontageInstance>();
        // temporary array of bone indices required this frame - should be subset of skeleton and meshes RequiredBones
        // FBoneContainer RequiredBones
        // anim notification that has been triggered in the lastest tick
        // List<FAnimNotifyEvent>

        H1AnimNodeBase m_RootNode;
    }
}
