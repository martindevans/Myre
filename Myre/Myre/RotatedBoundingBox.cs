
using System.Numerics;

namespace Myre
{
    /// <summary>
    /// A bounding box which is not necessarily axis aligned
    /// </summary>
    public struct RotatedBoundingBox
    {
        /// <summary>
        /// The bounds of this box
        /// </summary>
        public BoundingBox Bounds;

        private Quaternion _rotation;
        private Quaternion _inverseRotation;

        /// <summary>
        /// The rotation from world into box local space
        /// </summary>
        public Quaternion Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _inverseRotation = Quaternion.Inverse(value);
            }
        }

        /// <summary>
        /// The rotation from box local space to world space
        /// </summary>
        public Quaternion InverseRotation
        {
            get { return _inverseRotation; }
            set
            {
                _rotation = Quaternion.Inverse(value);
                _inverseRotation = value;
            }
        }
    }
}
