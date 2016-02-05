using System;
using System.Collections.Generic;
using System.Numerics;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;
using Myre.Graphics.Animation.Clips;
using Myre.Graphics.Geometry;

namespace Myre.Graphics.Animation
{
    [DefaultManager(typeof(Manager<AnimationPlayer>))]
    public class AnimationPlayer
        : ParallelProcessBehaviour
    {
        public static readonly TypedName<ClipPlaybackParameters> DefaultClipName = new TypedName<ClipPlaybackParameters>("animation_default_clip");
        public static readonly TypedName<Transform> RootTransformName = new TypedName<Transform>("animation_root_transform");
        public static readonly TypedName<bool> RootTranslationXName = new TypedName<bool>("animation_enable_root_translation_x");
        public static readonly TypedName<bool> RootTranslationYName = new TypedName<bool>("animation_enable_root_translation_y");
        public static readonly TypedName<bool> RootTranslationZName = new TypedName<bool>("animation_enable_root_translation_z");
        public static readonly TypedName<Vector3> RootTranslationScaleName = new TypedName<Vector3>("animation_root_translation_scale");
        public static readonly TypedName<bool> EnableRootRotationName = new TypedName<bool>("animation_enable_root_rotation");
        public static readonly TypedName<bool> EnableRootScaleName = new TypedName<bool>("animation_enable_root_scale");

        private Property<ClipPlaybackParameters> _defaultClip;
        /// <summary>
        /// The clip to play while there are no other active animations
        /// </summary>
        public ClipPlaybackParameters DefaultClip
        {
            get { return _defaultClip.Value; }
            set { _defaultClip.Value = value; }
        }

        private Property<Transform> _rootBoneTransform;
        /// <summary>
        /// The transformation of the root bone
        /// </summary>
        public Transform RootBoneTransfomationDelta
        {
            get { return _rootBoneTransform.Value; }
            set { _rootBoneTransform.Value = value; }
        }

        private Property<Vector3> _rootTransformationScale;
        /// <summary>
        /// Whether or not to include the X translation part of the root bone in the transform
        /// </summary>
        public Vector3 RootBoneTranslationScale
        {
            get { return _rootTransformationScale.Value; }
            set { _rootTransformationScale.Value = value; }
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

        private Animated _animation;
        private ModelInstance _model;

        private readonly Queue<ClipPlaybackParameters> _animationQueue = new Queue<ClipPlaybackParameters>();

        // Information about the currently playing animation clip.
        private TimeSpan _crossfadeDuration;
        private TimeSpan _crossfadeElapsed;
        private float _crossfadeProgress;
        private PlayingClip _fadingOut;
        private PlayingClip _fadingIn;

        public IClip CurrentlyPlaying
        {
            get { return (_fadingIn ?? _fadingOut).Animation; }
        }

        private Matrix4x4[] _boneTransforms;

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _defaultClip = context.CreateProperty(DefaultClipName);
            _rootBoneTransform = context.CreateProperty(RootTransformName, Transform.Identity);
            _enableRootBoneTranslationX = context.CreateProperty(RootTranslationXName, false);
            _enableRootBoneTranslationY = context.CreateProperty(RootTranslationYName, false);
            _enableRootBoneTranslationZ = context.CreateProperty(RootTranslationZName, false);
            _enableRootBoneRotation = context.CreateProperty(EnableRootRotationName, true);
            _enableRootBoneScale = context.CreateProperty(EnableRootScaleName, true);
            _rootTransformationScale = context.CreateProperty(RootTranslationScaleName, Vector3.One, false);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, RootTranslationXName, _enableRootBoneTranslationX);
            initialisationData.TryCopyValue(this, RootTranslationYName, _enableRootBoneTranslationY);
            initialisationData.TryCopyValue(this, RootTranslationZName, _enableRootBoneTranslationZ);
            initialisationData.TryCopyValue(this, EnableRootRotationName, _enableRootBoneRotation);
            initialisationData.TryCopyValue(this, EnableRootScaleName, _enableRootBoneScale);

            if (initialisationData != null)
            {
                ClipPlaybackParameters defaultClip;
                if (initialisationData.TryGetValue<ClipPlaybackParameters>(DefaultClipName, out defaultClip))
                    _defaultClip.Value = defaultClip;
            }
        }

        protected override void Initialised()
        {
            base.Initialised();

            _animation = Owner.GetBehaviour<Animated>(Name);

            _model = Owner.GetBehaviour<ModelInstance>(Name);
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
                _boneTransforms = new Matrix4x4[_model.Model.SkinningData.BindPose.Length];
                for (var i = 0; i < _model.Model.SkinningData.BindPose.Length; i++)
                    _boneTransforms[i] = _model.Model.SkinningData.BindPose[i];
            }
            else
            {
                _boneTransforms = null;
            }
        }

        /// <summary>
        /// Enqueues the givem clip, to be played once the previous clip reaches it's fade out time
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="clearQueue">Whether to cancel the animation queue and add this to it</param>
        public void EnqueueClip(ClipPlaybackParameters parameters, bool clearQueue = false)
        {
            if (clearQueue)
                _animationQueue.Clear();

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
                parameters = DefaultClip;

            parameters.Clip.Start();

            _fadingIn = PlayingClip.Create(parameters, _animation.BonesCount);
            _fadingIn.TimeFactor = 1;
            _fadingIn.Loop = parameters.Loop;
            _fadingIn.FadeOutTime = parameters.FadeOutTime;

            _crossfadeProgress = 0;
            var fadeOutTime = _fadingOut == null ? 0 : _fadingOut.FadeOutTime.Ticks;
            _crossfadeDuration = TimeSpan.FromTicks(Math.Max(parameters.FadeInTime.Ticks, fadeOutTime));
            _crossfadeElapsed = TimeSpan.Zero;
        }

        protected override void ParallelUpdate(float elapsedTime)
        {
            //Chose which animations are playing
            Transform oldRootFadingOut;
            Transform oldRootFadingIn;
            UpdateActiveAnimations(TimeSpan.FromSeconds(elapsedTime), out oldRootFadingIn, out oldRootFadingOut);

            //Calculate bone transforms and world transforms
            UpdateBoneTransforms();
            AnimationHelpers.CalculateWorldTransformsFromBoneTransforms(_animation.SkinningData.SkeletonHierarchy, _boneTransforms, _animation.WorldTransforms);

            //Calculate how far the root bone has transformed
            CalculateRootBoneDelta(ref oldRootFadingOut, ref oldRootFadingIn);
        }

        private void CalculateRootBoneDelta(ref Transform fadingOutRootDelta, ref Transform fadingInRootDelta)
        {
            if (_fadingOut != null && _fadingIn != null)
            {
                RootBoneTransfomationDelta = fadingOutRootDelta.Interpolate(fadingInRootDelta, _crossfadeProgress);
            }
            else if (_fadingOut != null)
            {
                RootBoneTransfomationDelta = fadingOutRootDelta;
            }
            else if (_fadingIn != null)
            {
                RootBoneTransfomationDelta = fadingInRootDelta;
            }
            else
            {
                throw new InvalidOperationException("No root bone motion found");
            }
        }

        private void UpdateActiveAnimations(TimeSpan dt, out Transform fadingInRootDelta, out Transform fadingOutRootDelta)
        {
            fadingInRootDelta = Transform.Identity;
            fadingOutRootDelta = Transform.Identity;

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
                _fadingOut.Update(dt, out fadingOutRootDelta);

                //Check if this animation is entering it's final phase. If so, look for another animation in the queue to start playing
                if (_fadingIn == null && _fadingOut.ElapsedTime >= _fadingOut.Animation.Duration - _fadingOut.FadeOutTime && (_animationQueue.Count > 0 || !_fadingOut.Loop))
                    InterruptClip(NextClip(), false);
            }

            if (_fadingIn != null)
            {
                _fadingIn.Update(dt, out fadingInRootDelta);

                _crossfadeElapsed += dt;
                _crossfadeProgress = (float)_crossfadeElapsed.TotalSeconds / (float)_crossfadeDuration.TotalSeconds;

                if (_crossfadeElapsed >= _crossfadeDuration)
                {
                    _fadingOut = _fadingIn;
                    _fadingIn = null;
                }
            }
        }

        private void UpdateBoneTransforms()
        {
            for (var boneIndex = 0; boneIndex < _boneTransforms.Length; boneIndex++)
            {
                Transform transform;
                if (_fadingOut != null && _fadingIn == null)
                    transform = _fadingOut.BoneTransform(boneIndex);
                else if (_fadingOut == null && _fadingIn != null)
                    transform = _fadingIn.BoneTransform(boneIndex);
                else if (_fadingOut != null && _fadingIn != null)
                    transform = _fadingOut.BoneTransform(boneIndex).Interpolate(_fadingIn.BoneTransform(boneIndex), _crossfadeProgress);
                else
                    throw new InvalidOperationException("No active animations");

                if (_fadingIn != null && boneIndex == _fadingIn.Animation.RootBoneIndex)
                    BuildRootBoneMatrix(ref transform, out _boneTransforms[boneIndex]);
                else if (_fadingOut != null && boneIndex == _fadingOut.Animation.RootBoneIndex)
                    BuildRootBoneMatrix(ref transform, out _boneTransforms[boneIndex]);
                else
                    transform.ToMatrix(out _boneTransforms[boneIndex]);
            }
        }

        private void BuildRootBoneMatrix(ref Transform transform, out Matrix4x4 m)
        {
            if (!EnableRootBoneTranslationX || !EnableRootBoneTranslationY || !EnableRootBoneTranslationZ || !EnableRootBoneScale || !EnableRootBoneRotation)
            {
                m = (EnableRootBoneScale ? Matrix4x4.CreateScale(transform.Scale) : Matrix4x4.Identity) *
                    (EnableRootBoneRotation ? Matrix4x4.CreateFromQuaternion(transform.Rotation) : Matrix4x4.Identity) *
                    Matrix4x4.CreateTranslation(new Vector3(
                        EnableRootBoneTranslationX ? transform.Translation.X : 0,
                        EnableRootBoneTranslationY ? transform.Translation.Y : 0,
                        EnableRootBoneTranslationZ ? transform.Translation.Z : 0
                    )
                );
            }
            else
            {
                transform.ToMatrix(out m);
            }
        }

        private ClipPlaybackParameters NextClip(bool allowLoop = true)
        {
            if (_animationQueue.Count > 0)
            {
                var followup = _animationQueue.Dequeue();   //Play followup from queue?

                if (followup.Clip == null)
                    return NextClip(false);     //If followup clip was empty, find next clip but explicitly disallow looping of currently playing animation
                else
                    return followup;            //Otherwise, play followup clip
            }
            else if (_fadingOut != null && _fadingOut.Loop && allowLoop)
                return _fadingOut.PlaybackParameters; //If the queue is empty, replay this clip (if it's looping)
            else
                return DefaultClip; //Otherwise idle
        }

        public struct ClipPlaybackParameters
        {
            public IClip Clip;
            public TimeSpan FadeInTime;
            public TimeSpan FadeOutTime;
            public bool Loop;
            public Func<float, float> Interpolator;
        }
    }
}
