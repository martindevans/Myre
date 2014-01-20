using System;
using Myre.Collections;
using Myre.Graphics.Animation.Clips;

namespace Myre.Graphics.Animation
{
    internal class PlayingClip
    {
        private int[] _channelFrames;
        public IClip Animation { get; private set; }

        public Animated.ClipPlaybackParameters PlaybackParameters { get; set; }

        public TimeSpan ElapsedTime { get; private set; }

        public float TimeFactor { get; set; }
        public bool Loop { get; set; }
        public TimeSpan FadeOutTime { get; set; }

        private Transform[] _transforms;
        private Transform[] _previousTransforms;

        private void Restart()
        {
            //Move back by animation duration
            ElapsedTime -= Animation.Duration;

            //Set back to start frame
            for (int i = 0; i < _channelFrames.Length; i++)
                _channelFrames[i] = 1;

            //Calculate the previous transform back into initial pose space, this is to fix deltas
            for (int i = 0; i < _transforms.Length; i++)
            {
                var a = Animation.Channels[i][0].Transform;
                var b = Animation.Channels[i][Animation.Channels[i].Length - 1].Transform;
                var delta = Graphics.Animation.Transform.Subtract(b, a);

                _transforms[i] = Graphics.Animation.Transform.Subtract(_transforms[i], delta);
            }
        }

        private void Start()
        {
            //Set time to zero
            ElapsedTime = TimeSpan.Zero;

            //Set frames to start
            for (int i = 0; i < _channelFrames.Length; i++)
                _channelFrames[i] = 1;

            //Set transforms to start positions too
            for (int i = 0; i < _transforms.Length; i++)
                _transforms[i] = Animation.Channels[i][0].Transform;
        }

        private void Play(Animated.ClipPlaybackParameters clipParameters, int bones)
        {
            PlaybackParameters = clipParameters;
            if (clipParameters.Clip == null)
                throw new ArgumentNullException("clipParameters");

            Animation = clipParameters.Clip;
            _channelFrames = new int[Animation.Channels.Length];
            _previousTransforms = new Transform[Animation.Channels.Length];
            _transforms = new Transform[Animation.Channels.Length];

            Start();
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
            return Graphics.Animation.Transform.Subtract(_transforms[channel], _previousTransforms[channel]);
        }

        private static readonly Pool<PlayingClip> _pool = new Pool<PlayingClip>();
        internal static PlayingClip Create(Animated.ClipPlaybackParameters clip, int bones)
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
