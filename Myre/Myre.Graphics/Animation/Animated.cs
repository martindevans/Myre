using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Extensions;
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

        public Vector3 RootPositionDelta
        {
            get
            {
                Vector3 a = Vector3.Zero;
                if (_fadingIn != null)
                    a += _fadingIn.RootPositionDelta;
                if (_fadingOut != null)
                    a += _fadingOut.RootPositionDelta;
                return a;
            }
        }


        public Action<string> OnAnimationCompleted;

        private Property<Matrix> _rootBoneTransform;
        /// <summary>
        /// The transformation of the root bone
        /// </summary>
        public Matrix RootBoneTransfomation
        {
            get { return _rootBoneTransform.Value; }
            set { _rootBoneTransform.Value = value; }
        }

        private Property<bool> _enableRootBoneTranslationX;
        /// <summary>
        /// Whether or not to include the X translation part of the root bone in the transform
        /// </summary>
        public bool EnableRootBoneTranslationX
        {
            get { return _enableRootBoneTranslationX.Value; }
            set { _enableRootBoneTranslationX.Value = value; }
        }

        private Property<bool> _enableRootBoneTranslationY;
        /// <summary>
        /// Whether or not to include the Y translation part of the root bone in the transform
        /// </summary>
        public bool EnableRootBoneTranslationY
        {
            get { return _enableRootBoneTranslationY.Value; }
            set { _enableRootBoneTranslationY.Value = value; }
        }

        private Property<bool> _enableRootBoneTranslationZ;
        /// <summary>
        /// Whether or not to include the Z translation part of the root bone in the transform
        /// </summary>
        public bool EnableRootBoneTranslationZ
        {
            get { return _enableRootBoneTranslationZ.Value; }
            set { _enableRootBoneTranslationZ.Value = value; }
        }

        private Property<bool> _enableRootBoneRotation;
        /// <summary>
        /// Whether or not to include the rotation part of the root bone in the transform
        /// </summary>
        public bool EnableRootBoneRotation
        {
            get { return _enableRootBoneRotation.Value; }
            set { _enableRootBoneRotation.Value = value; }
        }

        private Property<bool> _enableRootBoneScale;
        /// <summary>
        /// Whether or not to include the rotation part of the root bone in the transform
        /// </summary>
        public bool EnableRootBoneScale
        {
            get { return _enableRootBoneScale.Value; }
            set { _enableRootBoneScale.Value = value; }
        }

        /// <summary>
        /// The clip to play while there are no other active animations
        /// </summary>
        public ClipPlaybackParameters DefaultClip
        {
            get { return _defaultClip.Value; }
            set { _defaultClip.Value = value; }
        }

        private Property<ClipPlaybackParameters> _defaultClip;
        #endregion

        #region initialise
        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _defaultClip = context.CreateProperty<ClipPlaybackParameters>("animation_default_clip");
            _rootBoneTransform = context.CreateProperty<Matrix>("animation_root_transform", Matrix.Identity);
            _enableRootBoneTranslationX = context.CreateProperty<bool>("animation_enable_root_translation_x", false);
            _enableRootBoneTranslationY = context.CreateProperty<bool>("animation_enable_root_translation_y", false);
            _enableRootBoneTranslationZ = context.CreateProperty<bool>("animation_enable_root_translation_z", false);
            _enableRootBoneRotation = context.CreateProperty<bool>("animation_enable_root_rotation", true);
            _enableRootBoneScale = context.CreateProperty<bool>("animation_enable_root_scale", true);

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

        public override void Shutdown(INamedDataProvider shutdownData)
        {
            _model.ModelDataChanged -= ModelChanged;

            base.Shutdown(shutdownData);
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
                    _boneTransformTargets[boneIndex] = BuildMatrix(fadeOut.Position, fadeOut.Scale, fadeOut.Orientation);
                if (fadeOut == null && fadeIn != null)
                {
                    throw new NotImplementedException();
                    //_boneTransformTargets[boneIndex] = BuildInterpolatedMatrix(_boneTransforms[boneIndex].Position, _boneTransforms[boneIndex].Scale, _boneTransforms[boneIndex].Orientation, fadeIn.Position, fadeIn.Scale, fadeIn.Orientation, _crossfadeProgress);
                }
                if (fadeOut != null && fadeIn != null)
                    _boneTransformTargets[boneIndex] = BuildInterpolatedMatrix(fadeOut.Position, fadeOut.Scale, fadeOut.Orientation, fadeIn.Position, fadeIn.Scale, fadeIn.Orientation, _crossfadeProgress);
            }
        }

        private void UpdateBoneTransforms()
        {
            for (int i = 0; i < _boneTransforms.Length; i++)
                _boneTransforms[i] = _boneTransformTargets[i];

            _rootBoneTransform.Value = _boneTransforms[0];
            if (!EnableRootBoneTranslationX || !EnableRootBoneTranslationY || !EnableRootBoneTranslationZ || !EnableRootBoneScale || !EnableRootBoneRotation)
            {
                Vector3 translation, scale;
                Quaternion rotation;
                _boneTransforms[0].Decompose(out scale, out rotation, out translation);

                _boneTransforms[0] =
                    (EnableRootBoneScale ? Matrix.CreateScale(scale) : Matrix.Identity) *
                    (EnableRootBoneRotation ? Matrix.CreateFromQuaternion(rotation) : Matrix.Identity) *
                    Matrix.CreateTranslation(new Vector3(
                        EnableRootBoneTranslationX ? translation.X : 0,
                        EnableRootBoneTranslationY ? translation.Y : 0,
                        EnableRootBoneTranslationZ ? translation.Z : 0
                    )
                );
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

        private static Matrix BuildMatrix(Vector3 position, Vector3 scale, Quaternion orientation)
        {
            return Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(orientation) * Matrix.CreateScale(scale);
        }

        private Matrix BuildInterpolatedMatrix(Vector3 pos1, Vector3 scale1, Quaternion orientation1, Vector3 pos2, Vector3 scale2, Quaternion orientation2, float amount)
        {
            Quaternion interpolatedRotation = orientation1.Nlerp(orientation2, amount);
            Vector3 interpolatedScale = Vector3.SmoothStep(scale1, scale2, amount);
            Vector3 interpolatedTranslation = Vector3.SmoothStep(pos1, pos2, amount);

            return BuildMatrix(interpolatedTranslation, interpolatedScale, interpolatedRotation);
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

            Quaternion interpolatedRotation = startRotation.Nlerp(endRotation, slerpAmount);
            Vector3 interpolatedScale = Vector3.SmoothStep(startScale, endScale, slerpAmount);
            Vector3 interpolatedTranslation = Vector3.SmoothStep(startTranslation, endTranslation, slerpAmount);

            return Matrix.CreateScale(interpolatedScale) * Matrix.CreateFromQuaternion(interpolatedRotation) * Matrix.CreateTranslation(interpolatedTranslation);
        }
    }
}
