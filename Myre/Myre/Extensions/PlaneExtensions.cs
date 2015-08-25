
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    /// <summary>
    /// Extensions to the XNA plane structure
    /// </summary>
    public static class PlaneExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane FromXNA(this Microsoft.Xna.Framework.Plane plane)
        {
            return new Plane(plane.Normal.FromXNA(), plane.D);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.Plane ToXNA(this Plane plane)
        {
            return new Microsoft.Xna.Framework.Plane(plane.Normal.ToXNA(), plane.D);
        }

        /// <summary>
        /// Projects the given point onto the plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(this Plane plane, Vector3 point)
        {
            var d = plane.Distance(point);

            return point - plane.Normal * d;
        }

        /// <summary>
        /// Gets the distance from the plane to the point.
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float Distance(this Plane plane, Vector3 point)
        {
            //http://stackoverflow.com/questions/4436160/distance-to-a-plane

            float dot = Vector3.Dot(plane.Normal, point);
            float value = dot + plane.D;
            return value;
        }

        public static Plane CreateFromPoints(Vector3 a, Vector3 b, Vector3 c)
        {
            Microsoft.Xna.Framework.Plane p = new Microsoft.Xna.Framework.Plane(a.ToXNA(), b.ToXNA(), c.ToXNA());
            return new Plane(p.Normal.FromXNA(), p.D);
        }
    }
}
