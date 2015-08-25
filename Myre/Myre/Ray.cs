using Microsoft.Xna.Framework.Audio;
using Myre.Extensions;
using System.Numerics;

namespace Myre
{
    public struct Ray
    {
        internal readonly Microsoft.Xna.Framework.Ray XnaRay;

        public Ray(Vector3 position, Vector3 direction)
        {
            XnaRay = new Microsoft.Xna.Framework.Ray(position.ToXNA(), direction.ToXNA());
        }

        public Vector3 Position
        {
            get { return XnaRay.Position.FromXNA(); }
        }

        public Vector3 Direction
        {
            get { return XnaRay.Direction.FromXNA(); }
        }

        public float? Intersects(BoundingSphere sphere)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaRay.Intersects(sphere.XnaSphere);
        }

        public float? Intersects(BoundingBox box)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaRay.Intersects(box.XnaBox);
        }

        public float? Intersects(BoundingFrustum frustum)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaRay.Intersects(frustum.XnaFrustum);
        }

        public float? Intersects(Plane plane)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return XnaRay.Intersects(new Microsoft.Xna.Framework.Plane(plane.Normal.ToXNA(), plane.D));
        }
    }
}
