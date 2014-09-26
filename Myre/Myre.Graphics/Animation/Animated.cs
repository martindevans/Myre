using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
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

        // Current animation transform matrices.
        Matrix[] _boneTransforms;
        Matrix[] _worldTransforms;
        Matrix[] _skinTransforms;

        /// <summary>
        /// 
        /// </summary>
        public Matrix[] BoneTransforms
        {
            get { return _boneTransforms; }
        }

        public Matrix[] WorldTransforms
        {
            get { return _worldTransforms; }
        }

        public Matrix[] SkinTransforms
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
                _boneTransforms = new Matrix[_model.Model.SkinningData.BindPose.Length];
                for (int i = 0; i < _model.Model.SkinningData.BindPose.Length; i++)
                    _model.Model.SkinningData.BindPose[i].ToMatrix(out _boneTransforms[i]);

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

        protected override void Update(float elapsedTime)
        {
            UpdateWorldTransforms();
        }

        private void UpdateWorldTransforms()
        {
            // Root bone.
            _worldTransforms[0] = _boneTransforms[0];

            // Child bones.
            for (int bone = 1; bone < _worldTransforms.Length; bone++)
            {
                int parentBone = SkinningData.SkeletonHierarchy[bone];

                //Multiply by parent bone transform
                Matrix.Multiply(ref _boneTransforms[bone], ref _worldTransforms[parentBone], out _worldTransforms[bone]);

                //Multiply by bind pose
                Matrix.Multiply(ref SkinningData.InverseBindPose[bone], ref _worldTransforms[bone], out _skinTransforms[bone]);
            }
        }

        public void SetRenderData(NamedBoxCollection metadata)
        {
            metadata.Set<Matrix[]>("bones", _skinTransforms);
        }

        /// <summary>
        /// Test a ray (in model space) for intersection with individual bones.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, float>> Intersections(Ray ray)
        {
            return _model
                .Model
                .SkinningData
                .Bounds
                .Select((b, i) =>
                {
                    Matrix transform;
                    Matrix.Invert(ref _worldTransforms[i], out transform);

                    var start = Vector3.Transform(ray.Position, transform);             //Transform ray into bone space
                    var direction = Vector3.TransformNormal(ray.Direction, transform);

                    float? depth = b.Intersects(new Ray(start, direction));             //Intersect new ray in bone space
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
                    Matrix transform;
                    Matrix.Invert(ref _worldTransforms[i], out transform);

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
