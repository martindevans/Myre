using System;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Extensions;
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

        private void Restart()
        {
            ElapsedTime = TimeSpan.Zero;

            for (int i = 0; i < _channelFrames.Length; i++)
                _channelFrames[i] = 0;
        }

        private void Play(IClip animation, int bones)
        {
            if (animation == null)
                throw new ArgumentNullException("animation");

            Animation = animation;
            _channelFrames = new int[animation.Channels.Length];

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

                Animation.Start();
                Restart();
            }

            for (int i = 0; i < Animation.Channels.Length; i++)
            {
                var channel = Animation.Channels[i];

                //Iterate up frames until we find the frame which is greater than the current time index for this channel
                while (channel[_channelFrames[i]].Time < ElapsedTime)
                    _channelFrames[i]++;
            }
        }

        public Transform Transform(int channel)
        {
            int index = _channelFrames[channel];

            //frame which is greater than or equal to the current time
            var b = Animation.Channels[channel][index];
            if (b.Time == ElapsedTime || index == 0)
                return new Transform { Translation = b.Translation, Rotation = b.Rotation, Scale = b.Scale };

            //Previous frame
            var a = Animation.Channels[channel][index - 1];

            //Interpolation factor between frames
            var t = (float)((ElapsedTime - a.Time).TotalSeconds / (b.Time - a.Time).TotalSeconds);

            //Linearly interpolate frames
            return new Transform
            {
                Translation = Vector3.Lerp(a.Translation, b.Translation, t),
                Rotation = a.Rotation.Nlerp(b.Rotation, t),
                Scale = Vector3.Lerp(a.Scale, b.Scale, t)
            };
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
