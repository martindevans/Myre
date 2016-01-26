using Myre.Collections;
using Myre.Entities.Behaviours;
using Myre.Graphics.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SwizzleMyVectors.Geometry;

namespace Myre.Graphics.Animation
{
    /// <summary>
    /// Sets the appropriate bone/skinning data into the renderer (given a set of calculated world transforms)
    /// </summary>
    [DefaultManager(typeof(Manager<Animated>))]
    public class Animated
        : ParallelProcessBehaviour, ModelInstance.IRenderDataSupplier
    {
        #region fields

        private ModelInstance _model;

        // Current animation transform matrices
        private Matrix4x4[] _worldTransforms;
        private Matrix4x4[] _skinTransforms;

        public Matrix4x4[] WorldTransforms
        {
            get { return _worldTransforms; }
        }

        public Matrix4x4[] SkinTransforms
        {
            get { return _skinTransforms; }
        }

        public SkinningData SkinningData
        {
            get { return _model.Model.SkinningData; }
        }

        public int BonesCount
        {
            get { return SkinningData.SkeletonHierarchy.Length; }
        }

        public Action<string> OnAnimationCompleted;

        #endregion

        #region initialise
        public override void Initialise(INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            _model = Owner.GetBehaviour<ModelInstance>(Name);
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
                _worldTransforms = new Matrix4x4[_model.Model.SkinningData.BindPose.Length];
                _skinTransforms = new Matrix4x4[_model.Model.SkinningData.BindPose.Length];

                var skinning = _model.Model.SkinningData;
                AnimationHelpers.CalculateWorldTransformsFromBoneTransforms(skinning.SkeletonHierarchy, skinning.BindPose, _worldTransforms);
            }
            else
            {
                _worldTransforms = null;
                _skinTransforms = null;
            }
        }
        #endregion

        protected override void ParallelUpdate(float elapsedTime)
        {
            UpdateSkinTransforms();
        }

        private void UpdateSkinTransforms()
        {
            for (var bone = 0; bone < _worldTransforms.Length; bone++)
                _skinTransforms[bone] = Matrix4x4.Multiply(SkinningData.InverseBindPose[bone], _worldTransforms[bone]);
        }

        void ModelInstance.IRenderDataSupplier.SetRenderData(NamedBoxCollection metadata)
        {
            metadata.Set<Matrix4x4[]>("bones", _skinTransforms);
        }

        /// <summary>
        /// Test a ray (in model space) for intersection with individual bones.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, float>> Intersections(Ray3 ray)
        {
            return _model
                .Model
                .SkinningData
                .Bounds
                .Select((b, i) =>
                {
                    Matrix4x4 transform;
                    Matrix4x4.Invert(_worldTransforms[i], out transform);

                    var start = Vector3.Transform(ray.Position, transform);             //Transform ray into bone space
                    var direction = Vector3.TransformNormal(ray.Direction, transform);

                    float? depth = b.Intersects(new Ray3(start, direction));             //Intersect new ray in bone space
                    var name = _model.Model.SkinningData.Names[i];

                    return new KeyValuePair<string, float?>(name, depth);

                })
                .Where(a => a.Value.HasValue)                                           //Only pass values which intersect
// ReSharper disable PossibleInvalidOperationException
                .Select(a => new KeyValuePair<string, float>(a.Key, a.Value.Value))     //Select float (now we know it's not null)
// ReSharper restore PossibleInvalidOperationException
                .OrderBy(a => a.Value)                                                  //Order by distance along ray
                .ToArray();
        }

        public IEnumerable<string> Intersections(BoundingSphere sphere)
        {
            return _model
                .Model
                .SkinningData
                .Bounds
                .Select((b, i) =>
                {
                    Matrix4x4 transform;
                    Matrix4x4.Invert(_worldTransforms[i], out transform);

                    var center = Vector3.Transform(sphere.Center, transform);                   //Transform sphere center into bone space

                    var intersects = b.Intersects(new BoundingSphere(center, sphere.Radius));   //Intersect new sphere in bone space
                    var name = _model.Model.SkinningData.Names[i];

                    return new KeyValuePair<bool, string>(intersects, name);

                })
                .Where(a => a.Key)
                .Select(a => a.Value);
        }
    }
}
