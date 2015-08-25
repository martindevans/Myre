using System.Threading;
using Myre.Extensions;
using System.Numerics;

namespace Myre
{
    public struct BoundingFrustum
    {
        internal readonly Microsoft.Xna.Framework.BoundingFrustum XnaFrustum;

        public Plane Near
        {
            get { return XnaFrustum.Near.FromXNA(); }
        }

        public Plane Far
        {
            get { return XnaFrustum.Far.FromXNA(); }
        }

        public Plane Left
        {
            get { return XnaFrustum.Left.FromXNA(); }
        }

        public Plane Right
        {
            get { return XnaFrustum.Right.FromXNA(); }
        }

        public Plane Top
        {
            get { return XnaFrustum.Top.FromXNA(); }
        }

        public Plane Bottom
        {
            get { return XnaFrustum.Bottom.FromXNA(); }
        }

        public BoundingFrustum(Matrix4x4 matrix)
        {
            XnaFrustum = new Microsoft.Xna.Framework.BoundingFrustum(matrix.ToXNA());
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
                XnaFrustum.GetCorners(_xnaCorners);

                for (int i = 0; i < _xnaCorners.Length; i++)
                    corners[i] = _xnaCorners[i].FromXNA();
            }
            finally
            {
                if (lockTaken)
                    _cornersLock.Exit();
            }
        }

        public bool Intersects(BoundingSphere boundingSphere)
        {
            return XnaFrustum.Intersects(boundingSphere.XnaSphere);
        }
    }
}
