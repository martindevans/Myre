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
        /// The duration of this animation
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Get the index of the root bone of this animation
        /// </summary>
        ushort RootBoneIndex { get; }

        /// <summary>
        /// Get the number of channels in this animation
        /// </summary>
        int ChannelCount { get; }

        /// <summary>
        /// Get the channel at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IChannel GetChannel(int index);
    }

    public interface IChannel
    {
        /// <summary>
        /// The index of the bone this channel controls
        /// </summary>
        ushort BoneIndex { get; }

        /// <summary>
        /// Find the index of the frame for the given timestamp
        /// </summary>
        /// <param name="time"></param>
        /// <param name="startIndex"></param>
        int SeekToTimestamp(TimeSpan time, int startIndex = 0);

        /// <summary>
        /// Get the frame at the given index
        /// </summary>
        /// <param name="index"></param>
        Keyframe BoneTransform(int index);
    }
}
