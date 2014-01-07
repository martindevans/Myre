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

        private bool _firstRootFrame = true;
        private Vector3 _rootPosition;
        private Vector3 _rootScale;
        private Quaternion _rootOrientation;

        public Vector3 RootPositionDelta { get; private set; }
        public Vector3 RootScaleDelta { get; private set; }
        public Quaternion RootOrientationDelta { get; private set; }

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

            bool recalculateRootDelta = false;

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
                recalculateRootDelta |= keyframe.Bone == 0;

                _currentKeyframe++;
            }

            if (recalculateRootDelta)
            {
                Vector3 pos = _keyframes[0].Position;
                Vector3 scale = _keyframes[0].Scale;
                Quaternion orientation = _keyframes[0].Orientation;

                if (!_firstRootFrame)
                {
                    _firstRootFrame = false;
                    RootPositionDelta = pos - _rootPosition;
                    RootScaleDelta = scale - _rootScale;
                    RootOrientationDelta = Quaternion.Inverse(_rootOrientation) * orientation;
                }

                _rootPosition = pos;
                _rootScale = scale;
                _rootOrientation = orientation;
            }
            else
            {
                RootOrientationDelta = Quaternion.Identity;
                RootPositionDelta = Vector3.Zero;
                RootScaleDelta = Vector3.Zero;
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
