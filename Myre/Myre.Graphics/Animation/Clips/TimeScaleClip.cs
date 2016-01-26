using System;
using System.Linq;

namespace Myre.Graphics.Animation.Clips
{
    public class TimeScaleClip
        : IClip
    {
        #region fields and properties
        private readonly IClip _inner;
        private readonly float _scale;

        private readonly TimeSpan _duration;
        private IChannel[] _channels;

        public string Name
        {
            get { return _inner.Name; }
        }

        public ushort RootBoneIndex
        {
            get { return _inner.RootBoneIndex; }
        }

        public int ChannelCount
        {
            get { return _inner.ChannelCount; }
        }
        #endregion

        #region constructor
        public TimeScaleClip(IClip inner, float scale)
        {
            _inner = inner;
            _scale = scale;

            _duration = TimeSpan.FromTicks((long)(_inner.Duration.Ticks / _scale));
            _channels = Enumerable.Range(0, _inner.ChannelCount).Select(i => new TimeScaleChannel(_inner.GetChannel(i), _scale)).ToArray<IChannel>();
        }
        #endregion

        public void Start()
        {
            _inner.Start();
        }

        public TimeSpan Duration
        {
            get { return _duration; }
        }

        public IChannel GetChannel(int index)
        {
            return _channels[index];
        }

        private class TimeScaleChannel
            : IChannel
        {
            private readonly IChannel _inner;
            private readonly float _scale;

            public TimeScaleChannel(IChannel inner, float scale)
            {
                _inner = inner;
                _scale = scale;
            }

            public ushort BoneIndex
            {
                get { return _inner.BoneIndex; }
            }

            public int SeekToTimestamp(TimeSpan time, int startIndex = 0)
            {
                return _inner.SeekToTimestamp(TimeSpan.FromTicks((long)(time.Ticks * _scale)), startIndex);
            }

            public Keyframe BoneTransform(int index)
            {
                var kf = _inner.BoneTransform(index);

                return new Keyframe(
                    BoneIndex,
                    TimeSpan.FromTicks((long)(kf.Time.Ticks / _scale)),
                    kf.Transform
                );
            }

            public float TransformWeight(int index)
            {
                return _inner.TransformWeight(index);
            }
        }
    }
}
