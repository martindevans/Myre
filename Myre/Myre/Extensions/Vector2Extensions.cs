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
    }
}
