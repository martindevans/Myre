using Myre.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using PlaneIntersectionType = Microsoft.Xna.Framework.PlaneIntersectionType;

namespace Myre
{
    public struct BoundingSphere
    {
        internal readonly Microsoft.Xna.Framework.BoundingSphere XnaSphere;

        public BoundingSphere(Vector3 center, float radius)
        {
            XnaSphere = new Microsoft.Xna.Framework.BoundingSphere(center.ToXNA(), radius);
        }

        private BoundingSphere(Microsoft.Xna.Framework.BoundingSphere sphere)
        {
            XnaSphere = sphere;
        }

        public Vector3 Center
        {
            get { return XnaSphere.Center.FromXNA(); }
        }

        public float Radius
        {
            get { return XnaSphere.Radius; }
        }

        public bool Intersects(BoundingFrustum frustum)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaSphere.Intersects(frustum.XnaFrustum);
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaSphere.Intersects(plane.ToXNA());
        }

        public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
        {
            var s = Microsoft.Xna.Framework.BoundingSphere.CreateFromPoints(points.Select(a => a.ToXNA()));

            return new BoundingSphere(s);
        }
    }
}
