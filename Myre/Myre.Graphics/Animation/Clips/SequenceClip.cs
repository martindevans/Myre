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

        public TimeSpan Duration
        {
            get { return SelectedClip.Duration; }
        }
    }
}
