using System;
using System.Collections.Generic;
using System.Linq;
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
            NodeContent node = context.BuildAndLoadAsset<NodeContent, NodeContent>(new ExternalReference<NodeContent>(input.AnimationSourceFile), null);

            Dictionary<string, AnimationContent> animations = FindAnimations(node).ToDictionary(a => a.Key, a => a.Value);

            _bones = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(node));
            _boneNames = _bones.Select((a, i) => new  { a, i }).ToDictionary(a => a.a.Name, a => a.i);

            if (!animations.ContainsKey(input.SourceTakeName))
                throw new KeyNotFoundException(string.Format(@"Animation '{0}' not found, only options are {1}", input.SourceTakeName, animations.Keys.Aggregate((a, b) => a + "," + b)));

            return ProcessAnimation(input.Name, animations[input.SourceTakeName], context, input.StartFrame, input.EndFrame);
        }

        private ClipContent ProcessAnimation(string name, AnimationContent anim, ContentProcessorContext context, int startFrame = 0, int endFrame = -1)
        {
            if (anim.Duration.Ticks < TICKS_PER_60_FPS)
                throw new InvalidContentException("Source animation is shorter than 1/60 seconds");

            ClipContent animationClip = new ClipContent(name, _boneNames.Count);

            var startFrameTime = ConvertFrameNumberToTimeSpan(startFrame);
            var endFrameTime = ConvertFrameNumberToTimeSpan(endFrame);

            foreach (KeyValuePair<string, AnimationChannel> channel in anim.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex = _boneNames[channel.Key];

                //Find keyframes for this channel
                var keyframes = channel
                    .Value
                    .Where(k => k.Time >= startFrameTime)
                    .Where(k => endFrame == -1 || k.Time <= endFrameTime);

                LinkedList<KeyframeContent> animationKeyframes = new LinkedList<KeyframeContent>();

                // Convert the keyframe data and accumulate in a linked list
                foreach (AnimationKeyframe keyframe in keyframes.OrderBy(a => a.Time))
                {
                    //Clean up transform
                    var transform = keyframe.Transform;
                    transform.Right = Vector3.Normalize(transform.Right);
                    transform.Up = Vector3.Normalize(transform.Up);
                    transform.Backward = Vector3.Normalize(transform.Backward);

                    //Decompose into parts
                    Vector3 pos, scale;
                    Quaternion orientation;
                    keyframe.Transform.Decompose(out scale, out orientation, out pos);

                    animationKeyframes.AddLast(new KeyframeContent(boneIndex, keyframe.Time, pos, scale, orientation));
                }

                LinearKeyframeReduction(animationKeyframes);

                //Add these keyframes to the animation
                animationClip.Channels[boneIndex].AddRange(animationKeyframes);
            }

            if (animationClip.Channels.Any(a => a.Count == 0))
                throw new InvalidContentException("Animation has no keyframes.");

            // Sort the keyframes by time.
            animationClip.SortKeyframes();

            // Move the animation so the first keyframe sits at time zero
            animationClip.SubtractKeyframeTime();

            // Ensure every bone has a keyframe at the start of the animation
            animationClip.InsertStartFrames();

            return animationClip;
        }

        private static TimeSpan ConvertFrameNumberToTimeSpan(int frameNumber)
        {
            const float frameTime = 1000f / 30f;
            return new TimeSpan(0, 0, 0, 0, (int)(frameNumber * frameTime));
        }

        private static IEnumerable<KeyValuePair<string, AnimationContent>> FindAnimations(NodeContent node)
        {
            foreach (KeyValuePair<string, AnimationContent> k in node.Animations)
            {
                yield return k;

                // Why not interpolate here?
                // The way animations are done is with a single take, with all the animations back to back, and EmbeddedAnimationDefinition which selects a range of frames
                // If we interpolate the animation to 60 fps we might end up with frame *across* two of the embedded animations
                // That would be bad (tm)
                //yield return new KeyValuePair<string, AnimationContent>(k.Key, Interpolate(k.Value));
            }

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
    }
}
