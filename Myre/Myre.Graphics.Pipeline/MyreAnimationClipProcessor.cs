using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Myre.Graphics.Pipeline
{
    [ContentProcessor(DisplayName = "Myre Animation Processor")]
    public class MyreAnimationClipProcessor : ContentProcessor<NodeContent, MyreClipContent>
    {
        public const long TICKS_PER_60_FPS = TimeSpan.TicksPerSecond / 60;

        private IList<BoneContent> _bones;

        public override MyreClipContent Process(NodeContent input, ContentProcessorContext context)
        {
            //Setup bone indices
            _bones = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(input));

            //Process animation clip
            Dictionary<string, AnimationContent> animations = new Dictionary<string, AnimationContent>();
            FindAnimations(input, animations);

            if (animations.Count() > 1)
                throw new InvalidContentException("Animation file cannot contain more than 1 animation");

            return ProcessAnimation(animations.First().Value, context);
        }

        private MyreClipContent ProcessAnimation(AnimationContent anim, ContentProcessorContext context)
        {
            if (anim.Duration.Ticks < TICKS_PER_60_FPS)
                throw new InvalidContentException("Animation is shorter than 1/60 seconds");

            //TODO:: Resample the animation to some set FPS?

            return ProcessAnimationClip(anim);
        }

        private MyreClipContent ProcessAnimationClip(AnimationContent anim)
        {
            MyreClipContent animationClip = new MyreClipContent(anim.Name);

            foreach (KeyValuePair<string, AnimationChannel> channel in anim.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex = MyreModelProcessor.FindBoneIndex(channel.Key, _bones);

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                    animationClip.Keyframes.Add(new MyreKeyframeContent(boneIndex, keyframe.Time, keyframe.Transform));
            }

            // Sort the merged keyframes by time.
            animationClip.Keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));

            if (animationClip.Keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (anim.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return animationClip;
        }

        private static void FindAnimations(NodeContent node, Dictionary<string, AnimationContent> animations)
        {
            foreach (KeyValuePair<string, AnimationContent> k in node.Animations)
            {
                if (animations.ContainsKey(k.Key))
                {
                    foreach (KeyValuePair<string, AnimationChannel> c in k.Value.Channels)
                    {
                        animations[k.Key].Channels.Add(c.Key, c.Value);
                    }
                }
                else
                {
                    animations.Add(k.Key, k.Value);
                }
            }

            foreach (NodeContent child in node.Children)
                FindAnimations(child, animations);
        }
    }
}
