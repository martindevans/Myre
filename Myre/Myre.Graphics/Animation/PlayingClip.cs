using System;
using System.Collections.Generic;
using Myre.Collections;

namespace Myre.Graphics.Animation
{
    internal class PlayingClip
    {
        private readonly List<Keyframe> _keyframes = new List<Keyframe>();

        Clip _currentClip;
        TimeSpan _currentTimeValue;
        int _currentKeyframe;

        public Keyframe this[int boneIndex]
        {
            get { return _keyframes[boneIndex]; }
        }

        public float Weight { get; set; }

        private void ClearKeyframes(int size)
        {
            _keyframes.Capacity = size;
            _keyframes.Clear();
            for (int i = 0; i < size; i++)
                _keyframes.Add(null);
        }

        private void Play(Clip animation, int bones)
        {
            if (animation == null)
                throw new ArgumentNullException("animation");

            ClearKeyframes(bones);
            _currentClip = animation;
            _currentTimeValue = TimeSpan.Zero;
            _currentKeyframe = 0;
        }

        public bool Update(TimeSpan elapsedTime)
        {
            // Update the animation position.
            elapsedTime += _currentTimeValue;

            // If we reached the end, loop back to the start.
            bool loopback = false;
            while (elapsedTime >= _currentClip.Duration)
            {
                loopback = true;
                elapsedTime -= _currentClip.Duration;
            }

            if (loopback)
            {
                _currentKeyframe = 0;
                ClearKeyframes(_keyframes.Count);
            }

            if ((elapsedTime < TimeSpan.Zero) || (elapsedTime >= _currentClip.Duration))
                throw new ArgumentOutOfRangeException("elapsedTime");

            _currentTimeValue = elapsedTime;

            // Read keyframe matrices.
            Keyframe[] keyframes = _currentClip.Keyframes;

            while (_currentKeyframe < keyframes.Length)
            {
                Keyframe keyframe = keyframes[_currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > _currentTimeValue)
                    break;

                // Use this keyframe.
                _keyframes[keyframe.Bone] = keyframe;

                _currentKeyframe++;
            }

            return false;   //TODO: Return when animation ends
        }

        private static readonly Pool<PlayingClip> _pool = new Pool<PlayingClip>();
        internal static PlayingClip Create(Clip clip, int bones)
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
            playing._currentClip = null;
            _pool.Return(playing);
        }
    }
}
