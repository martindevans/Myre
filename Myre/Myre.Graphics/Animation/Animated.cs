using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities.Behaviours;
using Myre.Graphics.Geometry;

namespace Myre.Graphics.Animation
{
    [DefaultManager(typeof(Manager<Animated>))]
    public class Animated
        :ProcessBehaviour, ModelInstance.IRenderDataSupplier
    {
        private ModelInstance _model;

        public Dictionary<string, Clip> Clips
        {
            get
            {
                if (_model == null || _model.Model == null || _model.Model.SkinningData == null)
                    return null;
                return _model.Model.SkinningData.AnimationClips;
            }
        }

        // Information about the currently playing animation clip.
        Clip _currentClip;
        TimeSpan _currentTimeValue;
        int _currentKeyframe;


        // Current animation transform matrices.
        Matrix[] _boneTransforms;
        Matrix[] _worldTransforms;
        Matrix[] _skinTransforms;

        private SkinningData skinningData
        {
            get { return _model.Model.SkinningData; }
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
                _boneTransforms = new Matrix[_model.Model.SkinningData.BindPose.Length];
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

        public void StartClip(Clip clip)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            _currentClip = clip;
            _currentTimeValue = TimeSpan.Zero;
            _currentKeyframe = 0;

            // Initialize bone transforms to the bind pose.
            skinningData.BindPose.CopyTo(_boneTransforms, 0);
        }

        protected override void Update(float elapsedTime)
        {
            UpdateBoneTransforms(TimeSpan.FromSeconds(elapsedTime));
            UpdateWorldTransforms();
            UpdateSkinTransforms();
        }

        private void UpdateBoneTransforms(TimeSpan time)
        {
            if (_currentClip == null)
                return;

            // Update the animation position.
            time += _currentTimeValue;

            // If we reached the end, loop back to the start.
            bool loopback = false;
            while (time >= _currentClip.Duration)
            {
                loopback = true;
                time -= _currentClip.Duration;
            }

            if (loopback)
            {
                _currentKeyframe = 0;
                skinningData.BindPose.CopyTo(_boneTransforms, 0);
            }

            if ((time < TimeSpan.Zero) || (time >= _currentClip.Duration))
                throw new ArgumentOutOfRangeException("time");

            _currentTimeValue = time;

            // Read keyframe matrices.
            Keyframe[] keyframes = _currentClip.Keyframes;

            while (_currentKeyframe < keyframes.Length)
            {
                Keyframe keyframe = keyframes[_currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > _currentTimeValue)
                    break;

                // Use this keyframe.
                _boneTransforms[keyframe.Bone] = keyframe.Transform;

                _currentKeyframe++;
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

                _worldTransforms[bone] = _boneTransforms[bone] * _worldTransforms[parentBone];
            }
        }

        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < _skinTransforms.Length; bone++)
            {
                _skinTransforms[bone] = skinningData.InverseBindPose[bone] * _worldTransforms[bone];
            }
        }

        public void SetRenderData(BoxedValueStore<string> metadata)
        {
            metadata.Get<Matrix[]>("bones").Value = _skinTransforms;
        }
    }
}
