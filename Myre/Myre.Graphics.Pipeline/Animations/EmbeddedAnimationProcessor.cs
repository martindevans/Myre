using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentProcessor(DisplayName = "Myre Embedded Animation Processor")]
    public class EmbeddedAnimationProcessor : ContentProcessor<MyreEmbeddedAnimationDefinition, ClipContent>
    {
        public const long TICKS_PER_60_FPS = TimeSpan.TicksPerSecond / 60;

        private IList<BoneContent> _bones;
        private Dictionary<string, ushort> _boneNames;

        public override ClipContent Process(MyreEmbeddedAnimationDefinition input, ContentProcessorContext context)
        {
            //Build the content source
            NodeContent node = context.BuildAndLoadAsset<NodeContent, NodeContent>(new ExternalReference<NodeContent>(input.AnimationSourceFile), null);

            //Find the named animation from the content source
            var animations = FindAnimations(node).ToArray();
            if (animations.Length == 0)
                throw new InvalidContentException(string.Format("No animations found in '{0}'", input.AnimationSourceFile));
            var animation = animations.Where(a => string.Equals(a.Key, input.SourceTakeName, StringComparison.InvariantCultureIgnoreCase)).Select(a => a.Value).FirstOrDefault();
            if (animation == null)
                throw new KeyNotFoundException(string.Format(@"Animation '{0}' not found, only options are {1}", input.SourceTakeName, animations.Select(a => a.Key).Aggregate((a, b) => a + "," + b)));

            //Find the skeleton
            _bones = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(node));
            _boneNames = _bones.Select((a, i) => new  { a, i = i }).ToDictionary(a => a.a.Name, a => (ushort)a.i);

            //The "Root" bone is not necessarily the actual root of the tree
            //Find which bones are before and after the notional "Root" bone
            var root = _bones[Lookup(_boneNames, input.RootBone)];
            HashSet<string> descendents = new HashSet<string>(Descendents(root).Select(b => b.Name)); //Descendents of root obviously come after root (tautology)
            HashSet<string> ancestors = new HashSet<string>(_bones.Select(b => b.Name));              //Set of all bones, minus descendents and minus the root itself must be all ancestors of root
            ancestors.ExceptWith(descendents);
            ancestors.Remove(root.Name);

            return ProcessAnimation(animation, input.StartTime, input.EndTime, 1 / input.FramesPerSecond, ancestors, root.Name, input.FixLooping, input.LinearKeyframeReduction);
        }

        private ClipContent ProcessAnimation(AnimationContent anim, float startTime, float endTime, double frameTime, ISet<string> preRootBones, string rootBone, bool fixLooping, bool linearKeyframeReduction)
        {
            if (anim.Duration.Ticks < TICKS_PER_60_FPS)
                throw new InvalidContentException("Source animation is shorter than 1/60 seconds");

            ClipContent animationClip = new ClipContent(_boneNames.Count, Lookup(_boneNames, rootBone));
            if (_boneNames.Count == 0)
                throw new InvalidOperationException("Animation must have at least 1 bone channel");

            var startFrameTime = TimeSpan.FromSeconds(startTime);
            var endFrameTime = TimeSpan.FromSeconds(endTime);
            var singleFrameTime = TimeSpan.FromSeconds(frameTime);

            Parallel.ForEach(anim.Channels, channel =>
            //foreach (KeyValuePair<string, AnimationChannel> channel in anim.Channels)
            {
                var boneIndex = Lookup(_boneNames, channel.Key);
                animationClip.Channels[boneIndex] = ProcessChannel(boneIndex, channel, startFrameTime, endFrameTime, singleFrameTime, preRootBones, rootBone, fixLooping, linearKeyframeReduction);
            }
            );

            //Some channels can be null (becuase the animation specifies nothing for that channel) fill it in with identity transforms and set the channel weight to zero
            for (ushort i = 0; i < animationClip.Channels.Length; i++)
            {
                if (animationClip.Channels[i] == null)
                {
                    animationClip.Channels[i] = new Channel(new List<KeyframeContent>() {
                        new KeyframeContent(i, startFrameTime, Vector3.Zero, Vector3.One, Quaternion.Identity),
                        new KeyframeContent(i, endFrameTime, Vector3.Zero, Vector3.One, Quaternion.Identity),
                    }, 0);
                }
            }

            //Check for empty channels, definitely an error!
            if (animationClip.Channels.Any(a => a.Keyframes.Count == 0))
                throw new InvalidContentException("Animation has no keyframes for a channel.");

            // Sort the keyframes by time.
            animationClip.SortKeyframes();

            // Move the animation so the first keyframe sits at time zero
            animationClip.SubtractKeyframeTime();

            // Ensure every bone has a keyframe at the start and end of the animation
            animationClip.InsertStartAndEndFrames();

            return animationClip;
        }

        /// <summary>
        /// Find all keyframes within the specified time range. If there is not a keyframe at the exact time specified the range will be widened up to half frameTime each way to find a keyframe
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="frameTime"></param>
        /// <returns></returns>
        private static IEnumerable<AnimationKeyframe> KeyframesInTimeRange(AnimationChannel channel, TimeSpan start, TimeSpan end, TimeSpan frameTime)
        {
            var halfFrameTime = TimeSpan.FromTicks(frameTime.Ticks / 2);

            //Find all frames within the expanded range
            var frames = channel.Where(k => k.Time >= start - halfFrameTime && k.Time <= end + halfFrameTime)
                                .ToArray();

            //Now we have a range which is expanded by the maximum allowed amount (0.5 frames)
            //We want to *narrow* the range as much as possible so that the closest keyframe is chosen to the start and end points

            //Find the first frame which is greater than or equal to the start time. The frame immediately before this (if there is one) is the last keyframe before the specified time
            //Choose between these two options - whichever is closer
            var startIndex = Array.FindIndex(frames, k => k.Time >= start);
            if (startIndex > 0)
            {
                var a = frames[startIndex - 1];
                var b = frames[startIndex];

                //Check if A (before) is closer than B (after)
                if ((a.Time - start).Duration() < (b.Time - start).Duration())
                    startIndex--;
            }

            //Same technique as above but reversed. Find the last frame less than or equal to the end then expand up if necessary and possible
            var endIndex = Array.FindLastIndex(frames, k => k.Time <= end);
            if (endIndex < frames.Length - 1)
            {
                var a = frames[endIndex];
                var b = frames[endIndex + 1];

                //Check if A (before) is further than B (after)
                if ((a.Time - start).Duration() > (b.Time - start).Duration())
                    startIndex++;
            }

            //slice is not IEnumerable<T> in .net4! :/
            //return new ArraySegment<AnimationKeyframe>(frames, startIndex, endIndex - startIndex);
            var slice = new ArraySegment<AnimationKeyframe>(frames, startIndex, endIndex - startIndex);
            for (var i = 0; i < slice.Count; i++)
                yield return slice.Array[slice.Offset + i];

        }

        private static Channel ProcessChannel(ushort boneIndex, KeyValuePair<string, AnimationChannel> channel, TimeSpan startFrameTime, TimeSpan endFrameTime, TimeSpan frameTime, ICollection<string> preRoot, string root, bool fixLooping, bool linearKeyframeReduction)
        {
            //Find keyframes for this channel
            var keyframes = KeyframesInTimeRange(channel.Value, startFrameTime, endFrameTime, frameTime);

            var animationKeyframes = new LinkedList<KeyframeContent>();

            //Discard any data about channels which come before the root
            bool discard = preRoot.Contains(channel.Key);

            // Convert the keyframe data and accumulate in a linked list
            foreach (var keyframe in keyframes.OrderBy(a => a.Time))
            {
                //Decompose into parts
                Vector3 pos, scale;
                Quaternion orientation;
                if (!discard)
                {
                    //Clean up transform
                    var transform = keyframe.Transform;
                    //transform.Right = Vector3.Normalize(transform.Right);
                    //transform.Up = Vector3.Normalize(transform.Up);
                    //transform.Backward = Vector3.Normalize(transform.Backward);

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
                FixLooping(animationKeyframes, endFrameTime);

            //Remove keyframes that can be estimated by linear interpolation
            if (linearKeyframeReduction)
                LinearKeyframeReduction(animationKeyframes);

            //Add these keyframes to the animation
            return new Channel(animationKeyframes.ToList(), 1);
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
            const float EPSILON_LENGTH   = 0.0000001f;
            const float EPSILON_COS_ANGLE = 0.9999999f;
            const float EPSILON_SCALE    = 0.0000001f;

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
                Quaternion rotation = NLerp(a.Rotation, c.Rotation, t);

                //If it's a close enough guess, run with it and drop B
                if ((translation - b.Translation).LengthSquared() < EPSILON_LENGTH && Quaternion.Dot(rotation, b.Rotation) > EPSILON_COS_ANGLE && (scale - b.Scale).LengthSquared() < EPSILON_SCALE)
                {
                    var n = node.Previous;
                    keyframes.Remove(node);
                    node = n;
                }
            }
        }

        private static Quaternion NLerp(Quaternion a, Quaternion b, float t)
        {
            return Quaternion.Normalize(Quaternion.Lerp(a, b, t));
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

        private static ushort Lookup(IDictionary<string, ushort> dict, string key)
        {
            ushort value;
            if (dict.TryGetValue(key, out value))
                return value;

            throw new KeyNotFoundException(string.Format("Failed to find bone \"{0}\", options are {1}", key, string.Join(",", dict.Keys)));
        }
    }
}
