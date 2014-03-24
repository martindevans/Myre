using System;
using Microsoft.Xna.Framework;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class which contains extension methods for the Vector2 class.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Determines whether this Vector3 contains any components which are not a number.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>
        /// 	<c>true</c> if either X or Y or Z are NaN; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNaN(this Vector3 v)
        {
            return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
        }

        /// <summary>
        /// Determines the length of a vector using the manhattan length function
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float ManhattanLength(this Vector3 v)
        {
            return Math.Abs(v.X) + Math.Abs(v.Y) + Math.Abs(v.Z);
        }

        /// <summary>
        /// Returns the largest element in the vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float LargestElement(this Vector3 v)
        {
            return Math.Max(Math.Max(v.X, v.Y), v.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Int3 ToInt3(this Vector3 v)
        {
            return new Int3((int) v.X, (int) v.Y, (int) v.Z);
        }

        /// <summary>
        /// Performs CatmullRom spline interpolation between vectors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            var x = MathHelper.CatmullRom(a.X, b.X, c.X, d.X, t);
            var y = MathHelper.CatmullRom(a.Y, b.Y, c.Y, d.Y, t);
            var z = MathHelper.CatmullRom(a.Z, b.Z, c.Z, d.Z, t);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Clamps each element within the ranges of equivalwnt elements from min and max
        /// </summary>
        /// <param name="point"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector3 Clamp(this Vector3 point, Vector3 min, Vector3 max)
        {
            return new Vector3(
                MathHelper.Clamp(point.X, min.X, max.X),
                MathHelper.Clamp(point.Y, min.Y, max.Y),
                MathHelper.Clamp(point.Z, min.Z, max.Z)
            );
        }
    }
}
