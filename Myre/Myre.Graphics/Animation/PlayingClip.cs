using System;
using Myre.Collections;
using Myre.Graphics.Animation.Clips;

namespace Myre.Graphics.Animation
{
    internal class PlayingClip
    {
        private int[] _channelFrames;
        public IClip Animation { get; private set; }

        private AnimationQueue.ClipPlaybackParameters _parameters;
        public AnimationQueue.ClipPlaybackParameters PlaybackParameters
        {
            get { return _parameters; }
        }

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

            SeekToStart();
        }

        private void Start()
        {
            //Set time to zero
            ElapsedTime = TimeSpan.Zero;

            SeekToStart();
        }

        private void SeekToStart()
        {
            for (int i = 0; i < _channelFrames.Length; i++)
            {
                _channelFrames[i] = 0;
                _transforms[i] = Animation.Channels[i][0].Transform;
            }
        }

        private void Play(AnimationQueue.ClipPlaybackParameters clipParameters, int bones)
        {
            _parameters = clipParameters;
            if (_parameters.Clip == null)
                throw new ArgumentNullException("clipParameters");

            if (_parameters.Interpolator == null)
                _parameters.Interpolator = Interpolation.Linear();

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
                CalculateTransform(i, out _transforms[i]);
            }
        }

        private void CalculateTransform(int channel, out Transform transform)
        {
            int index = _channelFrames[channel];

            unsafe
            {
                //frame which is greater than or equal to the current time
                fixed (Keyframe* b = &Animation.Channels[channel][index])
                {
                    if (b->Time == ElapsedTime || index == 0)
                    {
                        transform = b->Transform;
                        return;
                    }

                    //Previous frame
                    fixed (Keyframe* a = &Animation.Channels[channel][index - 1])
                    {

                        //Interpolation factor between frames
                        var t = (float) ((ElapsedTime.TotalSeconds - a->Time.TotalSeconds) / (b->Time.TotalSeconds - a->Time.TotalSeconds));

                        //Convert linear interpolation into some other easing function
                        var t2 = PlaybackParameters.Interpolator(t);

                        //Linearly interpolate frames
                        transform = a->Transform.Interpolate(b->Transform, t2);
                    }
                }
            }
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
        internal static PlayingClip Create(AnimationQueue.ClipPlaybackParameters clip, int bones)
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
