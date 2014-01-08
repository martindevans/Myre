using System;
using Myre.Collections;
using Myre.Graphics.Animation.Clips;

namespace Myre.Graphics.Animation
{
    internal class PlayingClip
    {
        private int[] _channelFrames;
        public IClip Animation { get; private set; }

        public TimeSpan ElapsedTime { get; private set; }

        public float TimeFactor { get; set; }
        public bool Loop { get; set; }
        public TimeSpan FadeOutTime { get; set; }

        private Transform[] _transforms;
        private Transform[] _previousTransforms;

        private void Restart()
        {
            ElapsedTime = TimeSpan.Zero;

            for (int i = 0; i < _channelFrames.Length; i++)
                _channelFrames[i] = 1;
            for (int i = 0; i < _previousTransforms.Length; i++)
                _previousTransforms[i] = Animation.Channels[i][0].Transform;
            for (int i = 0; i < _transforms.Length; i++)
                _transforms[i] = Animation.Channels[i][0].Transform;
        }

        private void Play(IClip animation, int bones)
        {
            if (animation == null)
                throw new ArgumentNullException("animation");

            Animation = animation;
            _channelFrames = new int[animation.Channels.Length];
            _previousTransforms = new Transform[animation.Channels.Length];
            _transforms = new Transform[animation.Channels.Length];

            Restart();
        }

        public void Update(TimeSpan elapsedTime)
        {
            // Update the animation position.
            ElapsedTime += TimeSpan.FromSeconds(elapsedTime.TotalSeconds * TimeFactor);

            if (ElapsedTime >= Animation.Duration)
            {
                if (!Loop)
                    return;

                //We've overrun, save how much we've overrun by
                var excessTime = ElapsedTime -= Animation.Duration;

                Animation.Start();
                Restart();

                //Skip ahead to how much we overran by
                ElapsedTime = excessTime;
            }

            for (int i = 0; i < Animation.Channels.Length; i++)
            {
                var channel = Animation.Channels[i];

                //Iterate up frames until we find the frame which is greater than the current time index for this channel
                while (channel[_channelFrames[i]].Time <= ElapsedTime)
                    _channelFrames[i]++;

                //Calculate new transform for this channel
                _previousTransforms[i] = _transforms[i];
                _transforms[i] = CalculateTransform(i);
            }
        }

        private Transform CalculateTransform(int channel)
        {
            int index = _channelFrames[channel];

            //frame which is greater than or equal to the current time
            var b = Animation.Channels[channel][index];
            if (b.Time == ElapsedTime || index == 0)
                return b.Transform;

            //Previous frame
            var a = Animation.Channels[channel][index - 1];

            //Interpolation factor between frames
            var t = (float)((ElapsedTime.TotalSeconds - a.Time.TotalSeconds) / (b.Time.TotalSeconds - a.Time.TotalSeconds));

            //Linearly interpolate frames
            return a.Transform.Interpolate(b.Transform, t);
        }

        public Transform Transform(int channel)
        {
            return _transforms[channel];
        }

        public Transform Delta(int channel)
        {
            return Graphics.Animation.Transform.Difference(_previousTransforms[channel], _transforms[channel]);
        }

        private static readonly Pool<PlayingClip> _pool = new Pool<PlayingClip>();
        internal static PlayingClip Create(IClip clip, int bones)
        {
            PlayingClip instance;
            lock (_pool)
                instance = _pool.Get();

            instance.Play(clip, bones);
            return instance;
        }

        internal static void Return(PlayingClip playing)
        {
            playing.Animation = null;
            _pool.Return(playing);
        }
    }
}
