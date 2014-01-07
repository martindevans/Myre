using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Myre.Graphics.Pipeline.Models;

namespace Myre.Graphics.Pipeline.Animations
{
    [ContentProcessor(DisplayName = "Myre Embedded Animation Processor")]
    public class EmbeddedAnimationProcessor : ContentProcessor<MyreEmbeddedAnimationDefinition, ClipContent>
    {
        public const long TICKS_PER_60_FPS = TimeSpan.TicksPerSecond / 60;

        private IList<BoneContent> _bones;

        public override ClipContent Process(MyreEmbeddedAnimationDefinition input, ContentProcessorContext context)
        {
            NodeContent node = context.BuildAndLoadAsset<NodeContent, NodeContent>(new ExternalReference<NodeContent>(input.AnimationSourceFile), null);

            Dictionary<string, AnimationContent> animations = FindAnimations(node).ToDictionary(a => a.Key, a => a.Value);

            _bones = MeshHelper.FlattenSkeleton(MeshHelper.FindSkeleton(node));

            if (!animations.ContainsKey(input.SourceTakeName))
                throw new KeyNotFoundException(string.Format(@"Animation '{0}' not found, only options are {1}", input.SourceTakeName, animations.Keys.Aggregate((a, b) => a + "," + b)));

            return ProcessAnimation(input.Name, animations[input.SourceTakeName], context, input.StartFrame, input.EndFrame);
        }

        private ClipContent ProcessAnimation(string name, AnimationContent anim, ContentProcessorContext context, int startFrame = 0, int endFrame = -1)
        {
            if (anim.Duration.Ticks < TICKS_PER_60_FPS)
                throw new InvalidContentException("Source animation is shorter than 1/60 seconds");

            ClipContent animationClip = new ClipContent(name);

            var startFrameTime = ConvertFrameNumberToTimeSpan(startFrame);
            var endFrameTime = ConvertFrameNumberToTimeSpan(endFrame);

            foreach (KeyValuePair<string, AnimationChannel> channel in anim.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex = MyreModelProcessor.FindBoneIndex(channel.Key, _bones);

                var keyframes = channel
                    .Value
                    .Where(k => k.Time >= startFrameTime)
                    .Where(k => endFrame == -1 || k.Time <= endFrameTime);

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in keyframes)
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

                    animationClip.Keyframes.Add(new KeyframeContent(boneIndex, keyframe.Time, pos, scale, orientation));
                }
            }

            // Sort the merged by time.
            animationClip.Keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));

            if (animationClip.Keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            //Move the animation back so it starts at time zero
            var startTime = animationClip.Keyframes[0].Time;
            foreach (var keyframe in animationClip.Keyframes)
                keyframe.Time -= startTime;

            //Ensure every bone has a keyframe at the start of the animation
            for (int i = 0; i < _bones.Count; i++)
            {
                var firstKeyframe = animationClip.Keyframes.FirstOrDefault(a => a.Bone == i);
                if (firstKeyframe == null || firstKeyframe.Time.Ticks == 0)
                    continue;

                animationClip.Keyframes.Add(new KeyframeContent(i, new TimeSpan(0), firstKeyframe.Position, firstKeyframe.Scale, firstKeyframe.Orientation));
            }

            // Sort the keyframes by time.
            animationClip.Keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));

            if (animationClip.Keyframes.Last().Time.Ticks <= TICKS_PER_60_FPS)
                throw new InvalidContentException("Animation has < 1/60th second duration");

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
        /// Interpolates an AnimationContent object to 60 fps.
        /// </summary>
        /// <param name="input">The AnimationContent to interpolate.</param>
        /// <returns>The interpolated AnimationContent.</returns>
        private static AnimationContent Interpolate(AnimationContent input)
        {
            AnimationContent output = new AnimationContent();

            // default XNA importers, due to floating point errors or TimeSpan
            // estimation, sometimes  have channels with a duration slightly longer than
            // the animation duration.  So, set the animation duration to its true
            // value
            long animationDuration = Math.Max(input.Channels.Select(c => c.Value.Last().Time.Ticks).Max(), input.Duration.Ticks);

            foreach (KeyValuePair<string, AnimationChannel> c in input.Channels)
            {
                long time = 0;
                string channelName = c.Key;
                AnimationChannel channel = c.Value;
                AnimationChannel outChannel = new AnimationChannel();
                int currentFrame = 0;

                // Step through time until the time passes the animation duration
                while (time <= animationDuration)
                {
                    AnimationKeyframe keyframe;
                    // Clamp the time to the duration of the animation and make this 
                    // keyframe equal to the last animation frame.
                    if (time >= animationDuration)
                    {
                        time = animationDuration;
                        keyframe = new AnimationKeyframe(new TimeSpan(time),
                            channel[channel.Count - 1].Transform);
                    }
                    else
                    {
                        // If the channel only has one keyframe, set the transform for the current time
                        // to that keyframes transform
                        if (channel.Count == 1 || time < channel[0].Time.Ticks)
                        {
                            keyframe = new AnimationKeyframe(new TimeSpan(time), channel[0].Transform);
                        }
                        // If the current track duration is less than the animation duration,
                        // use the last transform in the track once the time surpasses the duration
                        else if (channel[channel.Count - 1].Time.Ticks <= time)
                        {
                            keyframe = new AnimationKeyframe(new TimeSpan(time), channel[channel.Count - 1].Transform);
                        }
                        else // proceed as normal
                        {
                            // Go to the next frame that is less than the current time
                            while (channel[currentFrame + 1].Time.Ticks < time)
                                currentFrame++;

                            // Numerator of the interpolation factor
                            double interpNumerator = time - channel[currentFrame].Time.Ticks;
                            // Denominator of the interpolation factor
                            double interpDenom = channel[currentFrame + 1].Time.Ticks - channel[currentFrame].Time.Ticks;
                            // The interpolation factor, or amount to interpolate between the current
                            // and next frame
                            double interpAmount = interpNumerator / interpDenom;

                            // If the frames are roughly 60 frames per second apart, use linear interpolation
                            // else if the transforms between the current frame and the next aren't identical
                            // decompose the matrix and interpolate the rotation separately
                            if (channel[currentFrame + 1].Time.Ticks - channel[currentFrame].Time.Ticks <= TICKS_PER_60_FPS * 1.05)
                            {
                                keyframe = new AnimationKeyframe(new TimeSpan(time),
                                    Matrix.Lerp(
                                    channel[currentFrame].Transform,
                                    channel[currentFrame + 1].Transform,
                                    (float)interpAmount));
                            }
                            else if (channel[currentFrame].Transform != channel[currentFrame + 1].Transform)
                            {
                                keyframe = new AnimationKeyframe(new TimeSpan(time),
                                    SlerpMatrix(
                                    channel[currentFrame].Transform,
                                    channel[currentFrame + 1].Transform,
                                    (float)interpAmount));
                            }
                            else // Else the adjacent frames have identical transforms and we can use
                            // the current frames transform for the current keyframe.
                            {
                                keyframe = new AnimationKeyframe(new TimeSpan(time),
                                    channel[currentFrame].Transform);
                            }

                        }
                    }
                    // Add the interpolated keyframe to the new channel.
                    outChannel.Add(keyframe);
                    // Step the time forward by 1/60th of a second
                    time += TICKS_PER_60_FPS;
                }

                // Compensate for the time error,(animation duration % TICKS_PER_60FPS),
                // caused by the interpolation by setting the last keyframe in the
                // channel to the animation duration.
                if (outChannel[outChannel.Count - 1].Time.Ticks < animationDuration)
                {
                    outChannel.Add(new AnimationKeyframe(
                        TimeSpan.FromTicks(animationDuration),
                        channel[channel.Count - 1].Transform));
                }

                outChannel.Add(new AnimationKeyframe(input.Duration,
                    channel[channel.Count - 1].Transform));
                // Add the interpolated channel to the animation
                output.Channels.Add(channelName, outChannel);
            }
            // Set the interpolated duration to equal the inputs duration for consistency
            output.Duration = TimeSpan.FromTicks(animationDuration);
            return output;
        }

        /// <summary>
        /// Roughly decomposes two matrices and performs spherical linear interpolation
        /// </summary>
        /// <param name="start">Source matrix for interpolation</param>
        /// <param name="end">Destination matrix for interpolation</param>
        /// <param name="slerpAmount">Ratio of interpolation</param>
        /// <returns>The interpolated matrix</returns>
        private static Matrix SlerpMatrix(Matrix start, Matrix end, float slerpAmount)
        {
            Quaternion qStart, qEnd, qResult;
            Vector3 curTrans, nextTrans, lerpedTrans;
            Vector3 curScale, nextScale, lerpedScale;
            Matrix startRotation, endRotation;
            Matrix returnMatrix;

            // Get rotation components and interpolate (not completely accurate but I don't want 
            // to get into polar decomposition and this seems smooth enough)
            Quaternion.CreateFromRotationMatrix(ref start, out qStart);
            Quaternion.CreateFromRotationMatrix(ref end, out qEnd);
            Quaternion.Lerp(ref qStart, ref qEnd, slerpAmount, out qResult);

            // Get final translation components
            curTrans.X = start.M41;
            curTrans.Y = start.M42;
            curTrans.Z = start.M43;
            nextTrans.X = end.M41;
            nextTrans.Y = end.M42;
            nextTrans.Z = end.M43;
            Vector3.Lerp(ref curTrans, ref nextTrans, slerpAmount, out lerpedTrans);

            // Get final scale component
            Matrix.CreateFromQuaternion(ref qStart, out startRotation);
            Matrix.CreateFromQuaternion(ref qEnd, out endRotation);
            curScale.X = start.M11 - startRotation.M11;
            curScale.Y = start.M22 - startRotation.M22;
            curScale.Z = start.M33 - startRotation.M33;
            nextScale.X = end.M11 - endRotation.M11;
            nextScale.Y = end.M22 - endRotation.M22;
            nextScale.Z = end.M33 - endRotation.M33;
            Vector3.Lerp(ref curScale, ref nextScale, slerpAmount, out lerpedScale);

            // Create the rotation matrix from the slerped quaternions
            Matrix.CreateFromQuaternion(ref qResult, out returnMatrix);

            // Set the translation
            returnMatrix.M41 = lerpedTrans.X;
            returnMatrix.M42 = lerpedTrans.Y;
            returnMatrix.M43 = lerpedTrans.Z;

            // And the lerped scale component
            returnMatrix.M11 += lerpedScale.X;
            returnMatrix.M22 += lerpedScale.Y;
            returnMatrix.M33 += lerpedScale.Z;
            return returnMatrix;
        }
    }
}
