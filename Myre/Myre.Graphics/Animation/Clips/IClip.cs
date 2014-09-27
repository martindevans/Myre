using System;

namespace Myre.Graphics.Animation.Clips
{
    public interface IClip
    {
        /// <summary>
        /// The name of this animation
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This animation is about to start playing
        /// if this animation is set to loop, this will be called every time a iteration loop starts
        /// </summary>
        void Start();

        /// <summary>
        /// The keyframes of this animation in time order, split by channel
        /// </summary>
        Keyframe[][] Channels { get; }

        /// <summary>
        /// Get the number of channels in this animation
        /// </summary>
        int ChannelCount { get; }

        /// <summary>
        /// The duration of this animation
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Get the index of the root bone of this animation
        /// </summary>
        ushort RootBoneIndex { get; }

        /// <summary>
        /// Find the index of the next frame in the given channel which is greater than the given time
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="startIndex"></param>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        int FindChannelFrameIndex(int channel, int startIndex, TimeSpan elapsedTime);
    }
}
