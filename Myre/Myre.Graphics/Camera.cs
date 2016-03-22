using System.Numerics;
using Myre.Collections;
using SwizzleMyVectors.Geometry;

namespace Myre.Graphics
{
    public sealed class Camera
    {
        private Matrix4x4 _view;
        private Matrix4x4 _projection;
        private Matrix4x4 _viewProjection;
        private Matrix4x4 _inverseView;
        private Matrix4x4 _inverseProjection;
        private Matrix4x4 _inverseViewProjection;
        private float _nearClip;
        private float _farClip;
        private BoundingFrustum _bounds;
        private bool _isDirty = true;

        private readonly Vector3[] _frustumCorners = new Vector3[8];
        private readonly Vector3[] _farFrustumCorners = new Vector3[4];

        public Matrix4x4 View
        {
            get { return _view; }
            set
            {
                if (_view != value)
                {
                    _view = value;
                    _isDirty = true;
                }
            }
        }

        public Matrix4x4 Projection
        {
            get { return _projection; }
            set 
            {
                _projection = value; 
            }
        }

        public Matrix4x4 ViewProjection
        {
            get 
            {
                if (_isDirty) Update();
                return _viewProjection; 
            }
        }

        public float NearClip
        {
            get { return _nearClip; }
            set
            {
// ReSharper disable CompareOfFloatsByEqualityOperator
                if (_nearClip != value)
// ReSharper restore CompareOfFloatsByEqualityOperator
                {
                    _nearClip = value;
                    _isDirty = true;
                }
            }
        }

        public float FarClip
        {
            get { return _farClip; }
            set
            {
// ReSharper disable CompareOfFloatsByEqualityOperator
                if (_farClip != value)
// ReSharper restore CompareOfFloatsByEqualityOperator
                {
                    _farClip = value;
                    _isDirty = true;
                }
            }
        }

        public BoundingFrustum Bounds
        {
            get 
            {
                if (_isDirty) Update();
                return _bounds; 
            }
        }

        public Vector3 Position
        {
            get
            {
                return _inverseView.Translation;
            }
        }

        private void Update()
        {
            _viewProjection = Matrix4x4.Multiply(_view, _projection);
            Matrix4x4.Invert(_view, out _inverseView);
            Matrix4x4.Invert(_projection, out _inverseProjection);
            Matrix4x4.Invert(_viewProjection, out _inverseViewProjection);
            _bounds.Matrix = _viewProjection;
            _isDirty = false;
        }

        public void SetMetadata(NamedBoxCollection metadata)
        {
            metadata.Set(Names.View.Camera, this);
            metadata.Set(Names.Matrix.View, View);
            metadata.Set(Names.Matrix.Projection, Projection);
            metadata.Set(Names.Matrix.ViewProjection, ViewProjection);
            metadata.Set(Names.Matrix.InverseView, _inverseView);
            metadata.Set(Names.Matrix.InverseProjection, _inverseProjection);
            metadata.Set(Names.Matrix.InverseViewProjection, _inverseViewProjection);
            metadata.Set(Names.View.ViewFrustum, Bounds);
            metadata.Set(Names.View.NearClip, NearClip);
            metadata.Set(Names.View.FarClip, FarClip);
            metadata.Set(Names.View.CameraPosition, Position);

            _bounds.GetCorners(_frustumCorners);
            for (int i = 0; i < 4; i++)
                _farFrustumCorners[i] = _frustumCorners[i + 4];

            for (int i = 0; i < _farFrustumCorners.Length; i++)
                _farFrustumCorners[i] = Vector3.Transform(_farFrustumCorners[i], _view);

            metadata.Set(Names.View.FarFrustumCorners, _farFrustumCorners);
        }
    }
}
