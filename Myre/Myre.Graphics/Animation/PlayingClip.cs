using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Graphics.Animation.Clips;

namespace Myre.Graphics.Animation
{
    internal class PlayingClip
    {
        private readonly List<Keyframe> _keyframes = new List<Keyframe>();

        public IClip Animation { get; private set; }

        public TimeSpan ElapsedTime { get; private set; }
        int _currentKeyframe;

        public Keyframe this[int boneIndex]
        {
            get { return _keyframes[boneIndex]; }
        }

        public float TimeFactor { get; set; }
        public bool Loop { get; set; }
        public TimeSpan FadeOutTime { get; set; }

        private void ClearKeyframes(int size)
        {
            _keyframes.Capacity = size;
            _keyframes.Clear();
            for (int i = 0; i < size; i++)
                _keyframes.Add(null);
        }

        private void Play(IClip animation, int bones)
        {
            if (animation == null)
                throw new ArgumentNullException("animation");

            ClearKeyframes(bones);
            Animation = animation;
            ElapsedTime = TimeSpan.Zero;
            _currentKeyframe = 0;
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
                _currentKeyframe = 0;
                ElapsedTime = TimeSpan.Zero;
            }

            // Read keyframe matrices.
            Keyframe[] keyframes = Animation.Keyframes;
            while (_currentKeyframe < keyframes.Length)
            {
                Keyframe keyframe = keyframes[_currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > ElapsedTime)
                    break;

                // Use this keyframe.
                _keyframes[keyframe.Bone] = keyframe;

                _currentKeyframe++;
            }
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
            playing._keyframes.Clear();
            playing.Animation = null;
            _pool.Return(playing);
        }
    }
}
