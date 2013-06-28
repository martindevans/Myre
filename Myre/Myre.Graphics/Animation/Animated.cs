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
        #region fields
        private ModelInstance _model;

        // Information about the currently playing animation clip.
        private readonly List<PlayingClip> _activeAnimations = new List<PlayingClip>();

        // Current animation transform matrices.
        readonly List<WeightedKeyframe> _keyframes = new List<WeightedKeyframe>();
        Matrix[] _boneTransforms;
        Matrix[] _worldTransforms;
        Matrix[] _skinTransforms;

        private SkinningData skinningData
        {
            get { return _model.Model.SkinningData; }
        }
        #endregion

        #region initialise
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
        #endregion

        public void StartClip(Clip clip, float timeFactor = 1, float weight = 1)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            var instance = PlayingClip.Create(clip, skinningData.SkeletonHierarchy.Length);

            instance.TimeFactor = timeFactor;
            instance.Weight = weight;

            _activeAnimations.Add(instance);
        }

        protected override void Update(float elapsedTime)
        {
            UpdateKeyframes(TimeSpan.FromSeconds(elapsedTime));
            UpdateBoneTransforms();
            UpdateWorldTransforms();
        }

        private void UpdateKeyframes(TimeSpan dt)
        {
            for (int i = _activeAnimations.Count - 1; i >= 0; i--)
            {
                if (_activeAnimations[i].Update(dt))
                    _activeAnimations.RemoveAt(i);
            }
        }

        private void UpdateBoneTransforms()
        {
            for (int boneIndex = 0; boneIndex < _boneTransforms.Length; boneIndex++)
            {
                float totalWeight = 0;
                int activeAnimationsForBone = 0;
                _keyframes.Clear();
                foreach (var animation in _activeAnimations)
                {
                    var k = animation[boneIndex];
                    if (k != null)
                    {
                        _keyframes.Add(new WeightedKeyframe(k, animation.Weight));
                        totalWeight += animation.Weight;
                        activeAnimationsForBone++;
                    }
                }

                //Normalize the weight if it's really small (give equal weighting to all animations)
                if (Math.Abs(totalWeight) < float.Epsilon)
                    totalWeight = activeAnimationsForBone;

                if (_keyframes.Count == 0)
                    _boneTransforms[boneIndex] = skinningData.BindPose[boneIndex];
                else if (_keyframes.Count == 1)
                    _boneTransforms[boneIndex] = _keyframes[0].Transform;
                else
                {
                    //Weight each transform separately by the normalized weight
                    for (int i = 0; i < _keyframes.Count; i++)
                    {
                        var k = _keyframes[i];
                        Matrix.Multiply(ref k.Transform, k.Weight / totalWeight, out k.Transform);
                    }

                    //Sum the transforms together
                    _boneTransforms[boneIndex] = _keyframes.Select(a => a.Transform).Aggregate(Matrix.Add);
                }
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

        public void SetRenderData(BoxedValueStore<string> metadata)
        {
            metadata.Get<Matrix[]>("bones").Value = _skinTransforms;
        }

        private struct WeightedKeyframe
        {
            public Matrix Transform;
            public readonly float Weight;

            public WeightedKeyframe(Keyframe keyframe, float weight)
            {
                Transform = keyframe.Transform;
                Weight = weight;
            }
        }
    }
}
