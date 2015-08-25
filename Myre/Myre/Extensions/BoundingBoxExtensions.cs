using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.BoundingBox struct.
    /// </summary>
    public static class BoundingBoxExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.BoundingBox ToXNA(this BoundingBox box)
        {
            return box.XnaBox;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundingBox FromXNA(this Microsoft.Xna.Framework.BoundingBox box)
        {
            return new BoundingBox(box);
        }

        /// <summary>
        /// Transformes the <see cref="BoundingBox"/> with a specified <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="box">The rectangle to transform.</param>
        /// <param name="m">The matrix with which to do the transformation.</param>
        /// <returns>The transformed <see cref="BoundingBox"/>.</returns>
        public static BoundingBox Transform(this BoundingBox box, ref Matrix4x4 m)
        {
            // get corners
            Vector3 v1 = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
            Vector3 v2 = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
            Vector3 v3 = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
            Vector3 v4 = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
            Vector3 v5 = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
            Vector3 v6 = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
            Vector3 v7 = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);
            Vector3 v8 = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);

            // tranfrom corners
            var n1 = Vector3.Transform(v1, m);
            var n2 = Vector3.Transform(v2, m);
            var n3 = Vector3.Transform(v3, m);
            var n4 = Vector3.Transform(v4, m);
            var n5 = Vector3.Transform(v5, m);
            var n6 = Vector3.Transform(v6, m);
            var n7 = Vector3.Transform(v7, m);
            var n8 = Vector3.Transform(v8, m);

            // find new extents
            Vector3 min = new Vector3(
                Math.Min(n1.X, Math.Min(n2.X, Math.Min(n3.X, Math.Min(n4.X, Math.Min(n5.X, Math.Min(n6.X, Math.Min(n7.X, n8.X))))))), // extreme nesting o0
                Math.Min(n1.Y, Math.Min(n2.Y, Math.Min(n3.Y, Math.Min(n4.Y, Math.Min(n5.Y, Math.Min(n6.Y, Math.Min(n7.Y, n8.Y))))))),
                Math.Min(n1.Z, Math.Min(n2.Z, Math.Min(n3.Z, Math.Min(n4.Z, Math.Min(n5.Z, Math.Min(n6.Z, Math.Min(n7.Z, n8.Z))))))));
            Vector3 max = new Vector3(
                Math.Max(n1.X, Math.Max(n2.X, Math.Max(n3.X, Math.Max(n4.X, Math.Max(n5.X, Math.Max(n6.X, Math.Max(n7.X, n8.X))))))),
                Math.Max(n1.Y, Math.Max(n2.Y, Math.Max(n3.Y, Math.Max(n4.Y, Math.Max(n5.Y, Math.Max(n6.Y, Math.Max(n7.Y, n8.Y))))))),
                Math.Max(n1.Z, Math.Max(n2.Z, Math.Max(n3.Z, Math.Max(n4.Z, Math.Max(n5.Z, Math.Max(n6.Z, Math.Max(n7.Z, n8.Z))))))));

            // create and return rectangle
            return new BoundingBox(min, max);
        }

        /// <summary>
        /// Finds the closest point on a bounding box from another point
        /// </summary>
        /// <param name="box"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(this BoundingBox box, Vector3 point)
        {
            return point.Clamp(box.Min, box.Max);
        }
    }
}
