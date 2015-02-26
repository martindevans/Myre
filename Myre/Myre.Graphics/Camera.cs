using Microsoft.Xna.Framework;
using Myre.Collections;

namespace Myre.Graphics
{
    public sealed class Camera
    {
        private Matrix _view;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Matrix _inverseView;
        private Matrix _inverseProjection;
        private Matrix _inverseViewProjection;
        private float _nearClip;
        private float _farClip;
        private BoundingFrustum _bounds;
        private bool _isDirty = true;

        private readonly Vector3[] _frustumCorners = new Vector3[8];
        private readonly Vector3[] _farFrustumCorners = new Vector3[4];

        public Matrix View
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

        public Matrix Projection
        {
            get { return _projection; }
            set 
            {
                _projection = value; 
            }
        }

        public Matrix ViewProjection
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
            Matrix.Multiply(ref _view, ref _projection, out _viewProjection);
            Matrix.Invert(ref _view, out _inverseView);
            Matrix.Invert(ref _projection, out _inverseProjection);
            Matrix.Invert(ref _viewProjection, out _inverseViewProjection);
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
            Vector3.Transform(_farFrustumCorners, ref _view, _farFrustumCorners);
            metadata.Set("farfrustumcorners", _farFrustumCorners);
        }
    }
}
