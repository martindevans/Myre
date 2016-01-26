using System;

namespace Myre.Graphics.Animation.Clips
{
    /// <summary>
    /// A clip which throws a NotSupportedException if interacted with in any way
    /// </summary>
    public class ErrorClip
        : IClip
    {
        public string Name
        {
            get { return "ErrorClip"; }
        }

        public void Start()
        {
            throw new NotSupportedException("Attempted to Start Playback of ErrorClip");
        }

        public TimeSpan Duration
        {
            get { throw new NotSupportedException("Attempted to get duration of ErrorClip"); }
        }

        public ushort RootBoneIndex { get { throw new NotSupportedException("Attempted to get root bone index of ErrorClip"); } }

        public int ChannelCount { get { throw new NotSupportedException("Attempted to get channel count of ErrorClip"); } }

        public IChannel GetChannel(int index)
        {
            throw new NotSupportedException(string.Format("Attempted to get channel at index {0} from ErrorClip", index));
        }

        public void SeekToTimestamp(TimeSpan time, ref int[] channelFrames)
        {
            throw new NotSupportedException("Attempted to seek ErrorClip");
        }
    }
}
