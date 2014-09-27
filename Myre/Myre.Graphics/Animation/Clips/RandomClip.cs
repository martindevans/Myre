using System;
using System.Collections.Generic;

namespace Myre.Graphics.Animation.Clips
{
    public class RandomClip
        : IClip
    {
        private readonly Random _random = new Random();
        private readonly List<IClip> _clips = new List<IClip>();

        private int _selectedIndex = 0;
        private IClip SelectedClip
        {
            get { return _clips[_selectedIndex]; }
        }

        public RandomClip(params IClip[] clips)
        {
            _clips.AddRange(clips);
        }

        public void Start()
        {
            _selectedIndex = _random.Next(_clips.Count);
            SelectedClip.Start();
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
