using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class which contains extension methods for the Vector2 class.
    /// </summary>
    public static class Vector2Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.Vector2 ToXNA(this System.Numerics.Vector2 v)
        {
            return new Microsoft.Xna.Framework.Vector2(v.X, v.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Vector2 FromXNA(this Microsoft.Xna.Framework.Vector2 v)
        {
            return new System.Numerics.Vector2(v.X, v.Y);
        }


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

            return -area / 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool IsConvex(this Vector2[] v, float epsilon = float.Epsilon)
        {
            int sign = 0;
            bool first = true;
            for (int i = 0; i < v.Length; i++)
            {
                var p = v[i];
                var p1 = v[(i + 1) % v.Length];
                var p2 = v[(i + 2) % v.Length];

                var d1 = p1 - p;
                var d2 = p2 - p1;

                var crossProduct = d1.X * d2.Y - d1.Y * d2.X;
                int crossProductSign = crossProduct > epsilon ? 1 : crossProduct < -epsilon ? -1 : 0;

                if (crossProductSign == 0)
                    continue;

                if (first)
                {
                    sign = crossProductSign;
                    first = false;
                }
                else if (crossProductSign != sign)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Rearrange elements in a vector2
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 XX(this Vector2 v)
        {
            return new Vector2(v.X, v.X);
        }

        /// <summary>
        /// Rearrange elements in a vector2
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 XY(this Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Rearrange elements in a vector2
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 YX(this Vector2 v)
        {
            return new Vector2(v.Y, v.X);
        }

        /// <summary>
        /// Rearrange elements in a vector2
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 YY(this Vector2 v)
        {
            return new Vector2(v.Y, v.Y);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _XY(this Vector2 v, float x)
        {
            return new Vector3(x, v.X, v.Y);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 X_Y(this Vector2 v, float y)
        {
            return new Vector3(v.X, y, v.Y);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 XY_(this Vector2 v, float z)
        {
            return new Vector3(v.X, v.Y, z);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _XX(this Vector2 v, float x)
        {
            return new Vector3(x, v.X, v.X);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 X_X(this Vector2 v, float y)
        {
            return new Vector3(v.X, y, v.X);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 XX_(this Vector2 v, float z)
        {
            return new Vector3(v.X, v.X, z);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _YY(this Vector2 v, float x)
        {
            return new Vector3(x, v.Y, v.Y);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Y_Y(this Vector2 v, float y)
        {
            return new Vector3(v.Y, y, v.Y);
        }

        /// <summary>
        /// Create a new vector3
        /// </summary>
        /// <param name="v"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 YY_(this Vector2 v, float z)
        {
            return new Vector3(v.Y, v.Y, z);
        }
    }
}
