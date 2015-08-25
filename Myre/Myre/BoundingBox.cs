using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Myre.Extensions;

using ContainmentType = Microsoft.Xna.Framework.ContainmentType;
using Vector3 = System.Numerics.Vector3;

namespace Myre
{
    public struct BoundingBox
    {
        internal readonly Microsoft.Xna.Framework.BoundingBox XnaBox;

        public Vector3 Min
        {
            get { return XnaBox.Min.FromXNA(); }
        }

        public Vector3 Max
        {
            get { return XnaBox.Max.FromXNA(); }
        }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            XnaBox = new Microsoft.Xna.Framework.BoundingBox(min.ToXNA(), max.ToXNA());
        }

        internal BoundingBox(Microsoft.Xna.Framework.BoundingBox box)
        {
            XnaBox = box;
        }

        [Pure]
        public ContainmentType Contains(Vector3 point)
        {
            // This is pure!
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaBox.Contains(point.ToXNA());
        }

        [Pure]
        public bool Intersects(BoundingBox bounds)
        {
            // This is pure!
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaBox.Intersects(bounds.XnaBox);
        }

        [Pure]
        public bool Intersects(BoundingSphere bounds)
        {
            // This is pure!
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaBox.Intersects(bounds.XnaSphere);
        }

        private static SpinLock _cornersLock = new SpinLock();
        private static readonly Microsoft.Xna.Framework.Vector3[] _xnaCorners = new Microsoft.Xna.Framework.Vector3[8];

        public void GetCorners(Vector3[] corners)
        {
            bool lockTaken = false;
            try
            {
                _cornersLock.Enter(ref lockTaken);

                // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                XnaBox.GetCorners(_xnaCorners);

                for (int i = 0; i < _xnaCorners.Length; i++)
                    corners[i] = _xnaCorners[i].FromXNA();
            }
            finally
            {
                if (lockTaken)
                    _cornersLock.Exit();
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is BoundingBox && Equals((BoundingBox)obj);
        }

        public bool Equals(BoundingBox other)
        {
            return XnaBox.Equals(other.XnaBox);
        }

        public override int GetHashCode()
        {
            return XnaBox.GetHashCode();
        }

        public static bool operator ==(BoundingBox a, BoundingBox b)
        {
            return a.XnaBox == b.XnaBox;
        }

        public static bool operator !=(BoundingBox a, BoundingBox b)
        {
            return a.XnaBox != b.XnaBox;
        }

        public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
        {
            var b = Microsoft.Xna.Framework.BoundingBox.CreateFromPoints(points.Select(a => a.ToXNA()));
            return new BoundingBox(b);
        }

        public float? Intersects(Ray ray)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaBox.Intersects(ray.XnaRay);
        }

        public ContainmentType Contains(BoundingSphere boundingSphere)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaBox.Contains(boundingSphere.XnaSphere);
        }
    }
}
