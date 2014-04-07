using System;
using Microsoft.Xna.Framework;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class which contains extension methods for the Vector2 class.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Determines whether this Vector2 contains any components which are not a number.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>
        /// 	<c>true</c> if either X or Y are NaN; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNaN(this Vector2 v)
        {
            return float.IsNaN(v.X) || float.IsNaN(v.Y);
        }

        /// <summary>
        /// Creates a vector perpendicular to this vector.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Perpendicular(this Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        /// <summary>
        /// Calculates the perpendicular dot product of this vector and another.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Cross(this Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Determines the length of a vector using the manhattan length function
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float ManhattanLength(this Vector2 v)
        {
            return Math.Abs(v.X) + Math.Abs(v.Y);
        }

        /// <summary>
        /// Returns the largest element in the vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float LargestElement(this Vector2 v)
        {
            return Math.Max(v.X, v.Y);
        }

        /// <summary>
        /// Calculates the area of an irregular polygon. If the polygon is anticlockwise wound the area will be negative
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Area(this Vector2[] v)
        {
            var area = 0f;

            int previous = v.Length - 1;
            for (int i = 0; i < v.Length; i++)
            {
                area += (v[i].X + v[previous].X) * (v[i].Y - v[previous].Y);
                previous = i;
            }

            return area / 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsConvex(this Vector2[] v)
        {
            int sign = 0;
            for (int i = 0; i < v.Length; i++)
            {
                var p = v[i];
                var p1 = v[(i + 1) % v.Length];
                var p2 = v[(i + 2) % v.Length];

                var d1 = p1 - p;
                var d2 = p2 - p1;

                var zcrossproduct = d1.X * d2.Y - d1.Y * d2.X;

                if (i == 0)
                    sign = Math.Sign(zcrossproduct);
                else if (Math.Sign(zcrossproduct) != sign)
                    return false;
            }

            return true;
        }
    }
}
