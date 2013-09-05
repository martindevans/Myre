using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Graphics.Animation.Clips;
using Myre.Graphics.Geometry;

namespace Myre.Graphics.Animation
{
    [DefaultManager(typeof(Manager<Animated>))]
    public class Animated
        :ProcessBehaviour, ModelInstance.IRenderDataSupplier
    {
        #region fields
        private ModelInstance _model;

        private readonly Queue<ClipPlaybackParameters> _animationQueue = new Queue<ClipPlaybackParameters>();

        // Information about the currently playing animation clip.
        private TimeSpan _crossfadeDuration;
        private TimeSpan _crossfadeElapsed;
        private float _crossfadeProgress = 0;
        private PlayingClip _fadingOut;
        private PlayingClip _fadingIn;

        // Current animation transform matrices.
        Matrix[] _boneTransformTargets;
        Matrix[] _boneTransforms;
        Matrix[] _worldTransforms;
        Matrix[] _skinTransforms;

        private SkinningData skinningData
        {
            get { return _model.Model.SkinningData; }
        }

        private int BonesCount
        {
            get { return skinningData.SkeletonHierarchy.Length; }
        }

        public Action<string> OnAnimationCompleted;

        /// <summary>
        /// The clip to play while there are no other active animations
        /// </summary>
        public ClipPlaybackParameters DefaultClip { get; set; }

        private Property<float> _animationSmoothing;
        #endregion

        #region initialise
        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _animationSmoothing = context.CreateProperty<float>("animation_smoothing", 0.5f);

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            _model = Owner.GetBehaviour<ModelInstance>();
        }

        protected override void Initialised()
        {
            base.Initialised();

            _model.ModelDataChanged += ModelChanged;
            ModelChanged(_model);
        }

        private void ModelChanged(ModelInstance model)
        {
            if (model != null && model.Model != null && model.Model.SkinningData != null)
            {
                _boneTransformTargets = new Matrix[_model.Model.SkinningData.BindPose.Length];
                Array.Copy(_model.Model.SkinningData.BindPose, _boneTransformTargets, _boneTransformTargets.Length);
                _boneTransforms = new Matrix[_model.Model.SkinningData.BindPose.Length];
                Array.Copy(_model.Model.SkinningData.BindPose, _boneTransforms, _boneTransforms.Length);

                _worldTransforms = new Matrix[_model.Model.SkinningData.BindPose.Length];
                _skinTransforms = new Matrix[_model.Model.SkinningData.BindPose.Length];
            }
            else
            {
                _boneTransforms = null;
                _worldTransforms = null;
                _skinTransforms = null;
            }
        }
        #endregion

        /// <summary>
        /// Enqueues the givem clip, to be played once the previous clip reaches it's fade out time
        /// </summary>
        /// <param name="parameters"></param>
        public void EnqueueClip(ClipPlaybackParameters parameters)
        {
            _animationQueue.Enqueue(parameters);
        }

        /// <summary>
        /// Instantly begins the specified clip
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="clearQueue"></param>
        public void InterruptClip(ClipPlaybackParameters parameters, bool clearQueue = true)
        {
            if (clearQueue)
                _animationQueue.Clear();

            if (parameters.Clip == null)
                return;

            parameters.Clip.Start();

            _fadingIn = PlayingClip.Create(parameters.Clip, BonesCount);
            _fadingIn.TimeFactor = 1;
            _fadingIn.Loop = parameters.Loop;
            _fadingIn.FadeOutTime = parameters.FadeOutTime;

            _crossfadeProgress = 0;
            var fadeOutTime = _fadingOut == null ? 0 : _fadingOut.FadeOutTime.Ticks;
            _crossfadeDuration = TimeSpan.FromTicks(Math.Max(parameters.FadeInTime.Ticks, fadeOutTime));
            _crossfadeElapsed = TimeSpan.Zero;
        }

        protected override void Update(float elapsedTime)
        {
            UpdateActiveAnimations(TimeSpan.FromSeconds(elapsedTime));
            UpdateBoneTransformTargets();
            UpdateBoneTransforms();
            UpdateWorldTransforms();
        }

        private void UpdateActiveAnimations(TimeSpan dt)
        {
            if (_fadingOut == null && _fadingIn == null)
                InterruptClip(NextClip(), false);

            if (_fadingOut == null && _fadingIn != null)
            {
                //DO NOT merge this into the if check above.
                //The above can change the state of _fadingOut so we do need to check it twice!
                _fadingOut = _fadingIn;
                _fadingIn = null;
            }

            if (_fadingOut != null)
            {
                _fadingOut.Update(dt);

                //Check if this animation is entering it's final phase. If so, look for another animation in the queue to start playing
                if (_fadingIn == null && _fadingOut.ElapsedTime >= _fadingOut.Animation.Duration - _fadingOut.FadeOutTime)
                    InterruptClip(NextClip(), false);
            }

            if (_fadingIn != null)
            {
                _fadingIn.Update(dt);

                _crossfadeElapsed += dt;
                _crossfadeProgress = (float)_crossfadeElapsed.TotalSeconds / (float)_crossfadeDuration.TotalSeconds;

                if (_crossfadeProgress >= 1)
                {
                    _fadingOut = _fadingIn;
                    _fadingIn = null;
                }
            }
        }

        private ClipPlaybackParameters NextClip()
        {
            if (_animationQueue.Count > 0)
                return _animationQueue.Dequeue();
            else
                return DefaultClip;
        }

        private void UpdateBoneTransformTargets()
        {
            for (int boneIndex = 0; boneIndex < _boneTransformTargets.Length; boneIndex++)
            {
                Keyframe fadeOut = _fadingOut[boneIndex];
                Keyframe fadeIn = null;
                if (_fadingIn != null)
                    fadeIn = _fadingIn[boneIndex];

                if (fadeOut != null && fadeIn == null)
                    _boneTransformTargets[boneIndex] = fadeOut.Transform;
                if (fadeOut == null && fadeIn != null)
                    _boneTransformTargets[boneIndex] = SlerpMatrix(_boneTransforms[boneIndex], fadeIn.Transform, _crossfadeProgress);
                if (fadeOut != null && fadeIn != null)
                    _boneTransformTargets[boneIndex] = SlerpMatrix(fadeOut.Transform, fadeIn.Transform, _crossfadeProgress);
            }
        }

        private void UpdateBoneTransforms()
        {
            for (int i = 0; i < _boneTransforms.Length; i++)
            {
                _boneTransforms[i] = SlerpMatrix(_boneTransforms[i], _boneTransformTargets[i], (1 - _animationSmoothing.Value));
            }
        }

        private void UpdateWorldTransforms()
        {
            // Root bone.
            _worldTransforms[0] = _boneTransforms[0];

            // Child bones.
            for (int bone = 1; bone < _worldTransforms.Length; bone++)
            {
                int parentBone = skinningData.SkeletonHierarchy[bone];

                Matrix.Multiply(ref _boneTransforms[bone], ref _worldTransforms[parentBone], out _worldTransforms[bone]);
                Matrix.Multiply(ref skinningData.InverseBindPose[bone], ref _worldTransforms[bone], out _skinTransforms[bone]);
            }
        }

        public void SetRenderData(NamedBoxCollection metadata)
        {
            metadata.Set<Matrix[]>("bones", _skinTransforms);
        }

        public struct ClipPlaybackParameters
        {
            public IClip Clip;
            public TimeSpan FadeInTime;
            public TimeSpan FadeOutTime;
            public bool Loop;
        }

        private static Matrix SlerpMatrix(Matrix start, Matrix end, float slerpAmount)
        {
            slerpAmount = MathHelper.Clamp(slerpAmount, 0, 1);
// ReSharper disable CompareOfFloatsByEqualityOperator
            if (slerpAmount == 1)
                return end;
            if (slerpAmount == 0)
                return start;
// ReSharper restore CompareOfFloatsByEqualityOperator

            Vector3 startScale;
            Quaternion startRotation;
            Vector3 startTranslation;
            start.Decompose(out startScale, out startRotation, out startTranslation);

            Vector3 endScale;
            Quaternion endRotation;
            Vector3 endTranslation;
            end.Decompose(out endScale, out endRotation, out endTranslation);

            Quaternion interpolatedRotation = Quaternion.Slerp(startRotation, endRotation, slerpAmount);
            Vector3 interpolatedScale = Vector3.SmoothStep(startScale, endScale, slerpAmount);
            Vector3 interpolatedTranslation = Vector3.SmoothStep(startTranslation, endTranslation, slerpAmount);

            return Matrix.CreateScale(interpolatedScale) * Matrix.CreateFromQuaternion(interpolatedRotation) * Matrix.CreateTranslation(interpolatedTranslation);
        }
    }
}
