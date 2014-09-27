using System;
using System.Collections.Generic;

namespace Myre.Graphics.Animation.Clips
{
    public class SequenceClip
        : IClip
    {
        private int _index = 0;
        private readonly List<IClip> _clips = new List<IClip>();

        private IClip SelectedClip
        {
            get { return _clips[_index]; }
        }

        public SequenceClip(params IClip[] clips)
        {
            _clips.AddRange(clips);
        }

        public void Start()
        {
            SelectedClip.Start();

            _index++;
            _index %= _clips.Count;
        }

        public string Name
        {
            get { return "RandomSelection(" + SelectedClip.Name + ")"; }
        }

        public Keyframe[][] Channels
        {
            get { return SelectedClip.Channels; }
        }

        public int ChannelCount
        {
            get { return SelectedClip.ChannelCount; }
        }

        public TimeSpan Duration
        {
            get { return SelectedClip.Duration; }
        }

        public ushort RootBoneIndex
        {
            get { return SelectedClip.RootBoneIndex; }
        }

        public int FindChannelFrameIndex(int channel, int startIndex, TimeSpan elapsedTime)
        {
            return SelectedClip.FindChannelFrameIndex(channel, startIndex, elapsedTime);
        }
    }
}
