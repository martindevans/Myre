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
    }
}
