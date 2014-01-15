using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Myre.Extensions;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentProcessor(DisplayName = "Myre Embedded Animation Processor")]
    public class EmbeddedAnimationProcessor : ContentProcessor<MyreEmbeddedAnimationDefinition, ClipContent>
    {
        public const long TICKS_PER_60_FPS = TimeSpan.TicksPerSecond / 60;

        private IList<BoneContent> _bones;
        private Dictionary<string, int> _boneNames;

        public override ClipContent Process(MyreEmbeddedAnimationDefinition input, ContentProcessorContext context)
        {
            //Build the content source
            NodeContent node = context.BuildAndLoadAsset<NodeContent, NodeContent>(new ExternalReference<NodeContent>(input.AnimationSourceFile), null);

            //Find the named animation from the content source
            var animation = FindAnimations(node).Where(a => a.Key == input.SourceTakeName).Select(a => a.Value).FirstOrDefault();
            if (animation == null)
                throw new KeyNotFoundException(string.Format(@"Animation '{0}' not found, only options are {1}", input.SourceTakeName, FindAnimations(node).Select(a => a.Key).Aggregate((a, b) => a + "," + b)));

            //Find the skeleton
            _bones = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(node));
            _boneNames = _bones.Select((a, i) => new  { a, i }).ToDictionary(a => a.a.Name, a => a.i);

            //The "Root" bone is not necessarily the actual root of the tree
            //Find which bones are before and after the notional "Root" bone
            var root = _bones[_boneNames[input.RootBone]];
            HashSet<string> descendents = new HashSet<string>(Descendents(root).Select(b => b.Name)); //Descendents of root obviously come after root (tautology)
            HashSet<string> ancestors = new HashSet<string>(_bones.Select(b => b.Name));              //Set of all bones, minus descendents and minus the root itself must be all ancestors of root
            ancestors.ExceptWith(descendents);
            ancestors.Remove(root.Name);

            return ProcessAnimation(animation, input.StartTime, input.EndTime, ancestors, root.Name, input.FixLooping);
        }

        private ClipContent ProcessAnimation(AnimationContent anim, float startTime, float endTime, ISet<string> preRootBones, string rootBone, bool fixLooping)
        {
            if (anim.Duration.Ticks < TICKS_PER_60_FPS)
                throw new InvalidContentException("Source animation is shorter than 1/60 seconds");

            ClipContent animationClip = new ClipContent(_boneNames.Count, _boneNames[rootBone]);

            var startFrameTime = TimeSpan.FromSeconds(startTime);
            var endFrameTime = TimeSpan.FromSeconds(endTime);

            Parallel.ForEach(anim.Channels, channel =>
            //foreach (KeyValuePair<string, AnimationChannel> channel in anim.Channels)
                {
                    int boneIndex = _boneNames[channel.Key];
                    animationClip.Channels[boneIndex] = ProcessChannel(boneIndex, channel, startFrameTime, endFrameTime, preRootBones, rootBone, fixLooping).ToList();
                }
            );

            if (animationClip.Channels.Any(a => a.Count == 0))
                throw new InvalidContentException("Animation has no keyframes for a channel.");

            // Sort the keyframes by time.
            animationClip.SortKeyframes();

            // Move the animation so the first keyframe sits at time zero
            animationClip.SubtractKeyframeTime();

            // Ensure every bone has a keyframe at the start and end of the animation
            animationClip.InsertStartAndEndFrames();

            return animationClip;
        }

        private static IEnumerable<KeyframeContent> ProcessChannel(int boneIndex, KeyValuePair<string, AnimationChannel> channel, TimeSpan startFrameTime, TimeSpan endFrameTime, ICollection<string> preRoot, string root, bool fixLooping)
        {
            //Find keyframes for this channel
            var keyframes = channel
                .Value
                .Where(k => k.Time >= startFrameTime)
                .Where(k => k.Time <= endFrameTime);

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
                    var transform = keyframe.Transform;
                    transform.Right = Vector3.Normalize(transform.Right);
                    transform.Up = Vector3.Normalize(transform.Up);
                    transform.Backward = Vector3.Normalize(transform.Backward);

                    keyframe.Transform.Decompose(out scale, out orientation, out pos);
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
                FixLooping(animationKeyframes, endFrameTime);

            //Remove keyframes that can be estimated by linear interpolation
            LinearKeyframeReduction(animationKeyframes);

            //Add these keyframes to the animation
            return animationKeyframes;
        }

        private IEnumerable<BoneContent> Descendents(BoneContent bone)
        {
            foreach (var boneContent in bone.Children.OfType<BoneContent>())
            {
                yield return boneContent;
                foreach (var descendent in Descendents(boneContent))
                    yield return descendent;
            }
        }

        /// <summary>
        /// Find all AnimationContent which are a child of the given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<string, AnimationContent>> FindAnimations(NodeContent node)
        {
            foreach (KeyValuePair<string, AnimationContent> k in node.Animations)
                yield return k;

            foreach (NodeContent child in node.Children)
                foreach (var childAnimation in FindAnimations(child))
                    yield return childAnimation;
        }

        /// <summary>
        /// Remove keyframes from the linked list which can be well estimated using linear interpolation
        /// </summary>
        /// <param name="keyframes"></param>
        private static void LinearKeyframeReduction(LinkedList<KeyframeContent> keyframes)
        {
            const float epsilonLength   = 0.0000001f;
            const float epsilonCosAngle = 0.9999999f;
            const float epsilonScale    = 0.0000001f;

            if (keyframes.First == null || keyframes.First.Next == null || keyframes.First.Next.Next == null)
                return;

            for (LinkedListNode<KeyframeContent> node = keyframes.First.Next; node != null && node.Next != null && node.Previous != null; node = node.Next)
            {
                // Determine nodes before and after the current node.
                KeyframeContent a = node.Previous.Value;
                KeyframeContent b = node.Value;
                KeyframeContent c = node.Next.Value;

                //Determine how far between "A" and "C" "B" is
                float t = (float) ((b.Time.TotalSeconds - a.Time.TotalSeconds) / (c.Time.TotalSeconds - a.Time.TotalSeconds));

                //Estimate where B *should* be using purely LERP
                Vector3 translation = Vector3.Lerp(a.Translation, c.Translation, t);
                Vector3 scale = Vector3.Lerp(a.Scale, c.Scale, t);
                Quaternion rotation = a.Rotation.Nlerp(c.Rotation, t);

                //If it's a close enough guess, run with it and drop B
                if ((translation - b.Translation).LengthSquared() < epsilonLength && Quaternion.Dot(rotation, b.Rotation) > epsilonCosAngle && (scale - b.Scale).LengthSquared() < epsilonScale)
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
        /// <param name="endFrameTime"></param>
        private static void FixLooping(LinkedList<KeyframeContent> animationKeyframes, TimeSpan endFrameTime)
        {
            if (animationKeyframes.Last.Value.Time == endFrameTime)
            {
                animationKeyframes.Last.Value.Translation = animationKeyframes.First.Value.Translation;
                animationKeyframes.Last.Value.Scale = animationKeyframes.First.Value.Scale;
                animationKeyframes.Last.Value.Rotation = animationKeyframes.First.Value.Rotation;
            }
            else if (animationKeyframes.Last.Value.Time > endFrameTime)
                throw new ArgumentException("Last frame comes after the end of the animation", "endFrameTime");
            else
                animationKeyframes.AddLast(new KeyframeContent(animationKeyframes.Last.Value.Bone, endFrameTime, animationKeyframes.First.Value.Translation, animationKeyframes.First.Value.Scale, animationKeyframes.First.Value.Rotation));
        }
    }
}
