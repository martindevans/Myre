using System;
using Microsoft.Xna.Framework;
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

        private void Restart()
        {
            //Move back by animation duration
            while (ElapsedTime > Animation.Duration)
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

                if (ElapsedTime == TimeSpan.FromSeconds(0))
                    _transforms[i] = Animation.GetChannel(i).BoneTransform(0).Transform;
                else
                {
                    _channelFrames[i] = Animation.GetChannel(i).SeekToTimestamp(ElapsedTime, 0);

                    if (_channelFrames[i] == 0)
                        _transforms[i] = Animation.GetChannel(i).BoneTransform(0).Transform;
                    else
                    {
                        var a = Animation.GetChannel(i).BoneTransform(_channelFrames[i] - 1);
                        var b = Animation.GetChannel(i).BoneTransform(_channelFrames[i]);

                        var totalTime = ElapsedTime.TotalSeconds / (b.Time - a.Time).TotalSeconds;

                        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                        _transforms[i] = a.Transform.Interpolate(b.Transform, (float) totalTime);
                    }
                }
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
            _channelFrames = new int[Animation.ChannelCount];
            _transforms = new Transform[Animation.ChannelCount];

            Start();
        }

        public void Update(TimeSpan elapsedTime, out Transform rootDelta)
        {
            rootDelta = Transform.Identity;

            // Update the animation position.
            ElapsedTime += TimeSpan.FromSeconds(elapsedTime.TotalSeconds * TimeFactor);

            if (ElapsedTime >= Animation.Duration)
            {
                if (!Loop)
                    return;

                Animation.Start();
                Restart();
            }

            for (int i = 0; i < Animation.ChannelCount; i++)
            {
                IChannel channel = Animation.GetChannel(i);

                //Advance frame number
                _channelFrames[i] = channel.SeekToTimestamp(ElapsedTime, _channelFrames[i]);

                //save root bone transform
                if (Animation.RootBoneIndex == i)
                {
                    var oldTransform = _transforms[i];

                    //Calculate new transform for this channel
                    CalculateTransform(channel, out _transforms[i]);

                    //Calculate the delta of the root bone
                    rootDelta = Transform.Subtract(_transforms[i], oldTransform);
                }
                else
                {
                    //Calculate new transform for this channel
                    CalculateTransform(channel, out _transforms[i]);
                }
            }
        }

        private void CalculateTransform(IChannel channel, out Transform transform)
        {
            int frameIndex = _channelFrames[channel.BoneIndex];

            //frame which is greater than or equal to the current time
            Keyframe b = channel.BoneTransform(frameIndex);
            if (b.Time == ElapsedTime)
            {
                transform = b.Transform;
                return;
            }

            //Previous frame
            Keyframe a = channel.BoneTransform(frameIndex - 1);

            //Interpolation factor between frames
            var t = (float) ((ElapsedTime.TotalSeconds - a.Time.TotalSeconds) / (b.Time.TotalSeconds - a.Time.TotalSeconds));

            //Convert linear interpolation into some other easing function
            var t2 = PlaybackParameters.Interpolator(t);

            //Linearly interpolate frames
            transform = a.Transform.Interpolate(b.Transform, t2);
        }

        public Transform BoneTransform(int channel)
        {
            return _transforms[channel];
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
