using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Myre.Extensions;
using Myre.Graphics.Pipeline.Models;

namespace Myre.Graphics.Pipeline.Animations
{
    public abstract class BaseAnimationProcessor<TInput>
        : ContentProcessor<TInput, ClipContent>
    {
        protected ClipContent Process(NodeContent node, AnimationContent animation, string rootBoneName, float ignoreFramesBefore, float ignoreFramesAfter, bool fixLooping, bool linearReduction, IList<BoneContent> restPose)
        {
            //Find the skeleton
            var bones = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(node));
            var boneNames = bones.Select((b, i) => new { b, i = i }).ToDictionary(a => a.b.Name, a => (ushort)a.i);
            rootBoneName = string.IsNullOrWhiteSpace(rootBoneName) ? bones[0].Name : rootBoneName;

            //The "Root" bone is not necessarily the actual root of the tree
            //Find which bones are before and after the notional "Root" bone
            var root = bones[LookupBone(boneNames, rootBoneName)];
            HashSet<string> descendents = new HashSet<string>(DescendentBones(root).Select(b => b.Name)); //Descendents of root obviously come after root (tautology)
            HashSet<string> preRootBones = new HashSet<string>(bones.Select(b => b.Name));
            preRootBones.ExceptWith(descendents);
            preRootBones.Remove(root.Name);

            //Retarget the rest pose
            Matrix[] retargetMatrices = new Matrix[bones.Count];
            if (restPose != null)
                retargetMatrices = ModelHelpers.Retarget(restPose, bones);
            else
                for (int i = 0; i < retargetMatrices.Length; i++)
                    retargetMatrices[i] = Matrix.Identity;

            return ProcessAnimation(animation, ignoreFramesBefore, ignoreFramesAfter, boneNames, preRootBones, rootBoneName, fixLooping, linearReduction, retargetMatrices);
        }

        /// <summary>
        /// Find all AnimationContent which are a child of the given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected static IEnumerable<KeyValuePair<string, AnimationContent>> FindAnimations(NodeContent node)
        {
            foreach (KeyValuePair<string, AnimationContent> k in node.Animations)
                yield return k;

            foreach (NodeContent child in node.Children)
                foreach (var childAnimation in FindAnimations(child))
                    yield return childAnimation;
        }

        private static ushort LookupBone(IDictionary<string, ushort> dict, string key)
        {
            ushort value;
            if (dict.TryGetValue(key, out value))
                return value;

            throw new KeyNotFoundException(string.Format("Failed to find bone \"{0}\", options are {1}", key, string.Join(",", dict.Keys)));
        }

        private static IEnumerable<BoneContent> DescendentBones(NodeContent bone)
        {
            foreach (var boneContent in bone.Children.OfType<BoneContent>())
            {
                yield return boneContent;
                foreach (var descendent in DescendentBones(boneContent))
                    yield return descendent;
            }
        }

        public const long TICKS_PER_60_FPS = TimeSpan.TicksPerSecond / 60;

        private static ClipContent ProcessAnimation(AnimationContent anim, float startTime, float endTime, IDictionary<string, ushort> boneNames, ICollection<string> preRootBones, string rootBone, bool fixLooping, bool linearKeyframeReduction, Matrix[] retarget)
        {
            if (anim.Duration.Ticks < TICKS_PER_60_FPS)
                throw new InvalidContentException("Source animation is shorter than 1/60 seconds");

            ClipContent animationClip = new ClipContent(boneNames.Count, LookupBone(boneNames, rootBone), boneNames.OrderBy(a => a.Value).Select(a => a.Key).ToArray());
            if (boneNames.Count == 0)
                throw new InvalidOperationException("Animation must have at least 1 bone channel");

            //Parallel.ForEach(anim.Channels, channel =>
            foreach (KeyValuePair<string, AnimationChannel> channel in anim.Channels)
            {
                ushort boneIndex = LookupBone(boneNames, channel.Key);
                animationClip.Channels[boneIndex] = ProcessChannel(boneIndex, channel, startTime, endTime, preRootBones, rootBone, fixLooping, linearKeyframeReduction, retarget[boneIndex]).ToList();
            }
            //);

            for (int i = 0; i < animationClip.Channels.Length; i++)
            {
                if (animationClip.Channels[i] == null)
                    animationClip.Channels[i] = new List<KeyframeContent> {
                        new KeyframeContent((ushort)i, TimeSpan.FromSeconds(0), Vector3.Zero, Vector3.One, Quaternion.Identity)
                    };
            }

            // Sort the keyframes by time.
            animationClip.SortKeyframes();

            // Move the animation so the first keyframe sits at time zero
            animationClip.SubtractKeyframeTime();

            // Ensure every bone has a keyframe at the start and end of the animation
            animationClip.InsertStartAndEndFrames();

            return animationClip;
        }

        private static IEnumerable<KeyframeContent> ProcessChannel(ushort boneIndex, KeyValuePair<string, AnimationChannel> channel, float startTime, float endTime, ICollection<string> preRoot, string root, bool fixLooping, bool linearKeyframeReduction, Matrix retarget)
        {
            //Find keyframes for this channel
            var keyframes = channel
                .Value
                .Where(k => k.Time.TotalSeconds >= startTime)
                .Where(k => k.Time.TotalSeconds < endTime);

            LinkedList<KeyframeContent> animationKeyframes = new LinkedList<KeyframeContent>();

            //Discard any data about channels which come before the root
            bool discard = preRoot.Contains(channel.Key);

            // Convert the keyframe data and accumulate in a linked list
            foreach (AnimationKeyframe keyframe in keyframes.OrderBy(a => a.Time))
            {
                //Decompose into parts
                Vector3 pos, scale;
                Quaternion orientation;
                if (!discard)
                {
                    //Clean up transform
                    var transform = retarget * keyframe.Transform;
                    transform.Right = Vector3.Normalize(transform.Right);
                    transform.Up = Vector3.Normalize(transform.Up);
                    transform.Backward = Vector3.Normalize(transform.Backward);

                    transform.Decompose(out scale, out orientation, out pos);
                }
                else
                {
                    pos = Vector3.Zero;
                    scale = Vector3.One;
                    orientation = Quaternion.Identity;
                }

                animationKeyframes.AddLast(new KeyframeContent(boneIndex, keyframe.Time, pos, scale, orientation));
            }

            //If necessary copy the first keyframe into the data of the last keyframe
            if (fixLooping && !discard && !channel.Key.Equals(root, StringComparison.InvariantCulture))
                FixLooping(animationKeyframes, endTime);

            //Remove keyframes that can be estimated by linear interpolation
            if (linearKeyframeReduction)
                LinearKeyframeReduction(animationKeyframes);

            //Add these keyframes to the animation
            return animationKeyframes;
        }

        /// <summary>
        /// Remove keyframes from the linked list which can be well estimated using linear interpolation
        /// </summary>
        /// <param name="keyframes"></param>
        private static void LinearKeyframeReduction(LinkedList<KeyframeContent> keyframes)
        {
            const float EPSILON_LENGTH = 0.0000001f;
            const float EPSILON_COS_ANGLE = 0.9999999f;
            const float EPSILON_SCALE = 0.0000001f;

            if (keyframes.First == null || keyframes.First.Next == null || keyframes.First.Next.Next == null)
                return;

            for (LinkedListNode<KeyframeContent> node = keyframes.First.Next; node != null && node.Next != null && node.Previous != null; node = node.Next)
            {
                // Determine nodes before and after the current node.
                KeyframeContent a = node.Previous.Value;
                KeyframeContent b = node.Value;
                KeyframeContent c = node.Next.Value;

                //Determine how far between "A" and "C" "B" is
                float t = (float)((b.Time.TotalSeconds - a.Time.TotalSeconds) / (c.Time.TotalSeconds - a.Time.TotalSeconds));

                //Estimate where B *should* be using purely LERP
                Vector3 translation = Vector3.Lerp(a.Translation, c.Translation, t);
                Vector3 scale = Vector3.Lerp(a.Scale, c.Scale, t);
                Quaternion rotation = a.Rotation.Nlerp(c.Rotation, t);

                //If it's a close enough guess, run with it and drop B
                if ((translation - b.Translation).LengthSquared() < EPSILON_LENGTH && Quaternion.Dot(rotation, b.Rotation) > EPSILON_COS_ANGLE && (scale - b.Scale).LengthSquared() < EPSILON_SCALE)
                {
                    var n = node.Previous;
                    keyframes.Remove(node);
                    node = n;
                }
            }
        }

        /// <summary>
        /// Make sure that the frame at the end of this channel has the same data as the frame at the start of this channel
        /// </summary>
        /// <param name="animationKeyframes"></param>
        /// <param name="endTime"></param>
        private static void FixLooping(LinkedList<KeyframeContent> animationKeyframes, float endTime)
        {
            TimeSpan endFrameTime = TimeSpan.FromSeconds(endTime);

            if (Math.Abs(animationKeyframes.Last.Value.Time.TotalSeconds - endTime) < (1 / 120f))
            {
                animationKeyframes.Last.Value.Translation = animationKeyframes.First.Value.Translation;
                animationKeyframes.Last.Value.Scale = animationKeyframes.First.Value.Scale;
                animationKeyframes.Last.Value.Rotation = animationKeyframes.First.Value.Rotation;
            }
            else if (animationKeyframes.Last.Value.Time > endFrameTime)
                throw new ArgumentException("Last frame comes after the end of the animation", "endTime");
            else
                animationKeyframes.AddLast(new KeyframeContent(animationKeyframes.Last.Value.Bone, endFrameTime, animationKeyframes.First.Value.Translation, animationKeyframes.First.Value.Scale, animationKeyframes.First.Value.Rotation));
        }
    }
}
