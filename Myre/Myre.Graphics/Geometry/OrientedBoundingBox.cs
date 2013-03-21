using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.Graphics.Geometry
{
    public class OrientedBoundingBox
    {
        private BoundingBox _bounds;
        private Matrix _transform;
        private BoundingBox _axisAligned;
        private bool _dirty;

        public BoundingBox LocalBounds
        {
            get { return _bounds; }
            set
            {
                if (_bounds != value)
                {
                    _bounds = value;
                    _dirty = true;
                }
            }
        }

        public Matrix Transform
        {
            get { return _transform; }
            set
            {
                if (_transform != value)
                {
                    _transform = value;
                    _dirty = true;
                }
            }
        }

        public BoundingBox AxisAlignedBounds
        {
            get
            {
                if (_dirty) Update();
                return _axisAligned;
            }
        }

        private void Update()
        {
            _axisAligned = _bounds.Transform(ref _transform);
            _dirty = false;
        }
    }
}
