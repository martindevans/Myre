using System.Numerics;
using Myre.Collections;

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
            _bounds = new BoundingFrustum(_viewProjection);
            _isDirty = false;
        }

        public void SetMetadata(NamedBoxCollection metadata)
        {
            metadata.Set("camera", this);
            metadata.Set("view", View);
            metadata.Set("projection", Projection);
            metadata.Set("viewprojection", ViewProjection);
            metadata.Set("inverseview", _inverseView);
            metadata.Set("inverseprojection", _inverseProjection);
            metadata.Set("inverseviewprojection", _inverseViewProjection);
            metadata.Set("viewfrustum", Bounds);
            metadata.Set("nearclip", NearClip);
            metadata.Set("farclip", FarClip);
            metadata.Set("cameraposition", Position);

            _bounds.GetCorners(_frustumCorners);
            for (int i = 0; i < 4; i++)
                _farFrustumCorners[i] = _frustumCorners[i + 4];

            for (int i = 0; i < _farFrustumCorners.Length; i++)
                _farFrustumCorners[i] = Vector3.Transform(_farFrustumCorners[i], _view);

            metadata.Set("farfrustumcorners", _farFrustumCorners);
        }
    }
}
