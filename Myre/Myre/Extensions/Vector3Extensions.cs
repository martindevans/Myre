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

        #region conver to v2
        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 XX(this Vector3 point)
        {
            return new Vector2(point.X, point.X);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 XY(this Vector3 point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 XZ(this Vector3 point)
        {
            return new Vector2(point.X, point.Z);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 YX(this Vector3 point)
        {
            return new Vector2(point.Y, point.X);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 YY(this Vector3 point)
        {
            return new Vector2(point.Y, point.Y);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 YZ(this Vector3 point)
        {
            return new Vector2(point.Y, point.Z);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 ZX(this Vector3 point)
        {
            return new Vector2(point.Z, point.X);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 ZY(this Vector3 point)
        {
            return new Vector2(point.Z, point.Y);
        }

        /// <summary>
        /// Get a vector 2 with the specified elements
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector2 ZZ(this Vector3 point)
        {
            return new Vector2(point.Z, point.Z);
        }
        #endregion

        #region reorder v3
        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XXX(this Vector3 point)
        {
            return new Vector3(point.X, point.X, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XXY(this Vector3 point)
        {
            return new Vector3(point.X, point.X, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XXZ(this Vector3 point)
        {
            return new Vector3(point.X, point.X, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XYX(this Vector3 point)
        {
            return new Vector3(point.X, point.Y, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XYY(this Vector3 point)
        {
            return new Vector3(point.X, point.Y, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XYZ(this Vector3 point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XZX(this Vector3 point)
        {
            return new Vector3(point.X, point.Z, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XZY(this Vector3 point)
        {
            return new Vector3(point.X, point.Z, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 XZZ(this Vector3 point)
        {
            return new Vector3(point.X, point.Z, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YXX(this Vector3 point)
        {
            return new Vector3(point.Y, point.X, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YXY(this Vector3 point)
        {
            return new Vector3(point.Y, point.X, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YXZ(this Vector3 point)
        {
            return new Vector3(point.Y, point.X, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YYX(this Vector3 point)
        {
            return new Vector3(point.Y, point.Y, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YYY(this Vector3 point)
        {
            return new Vector3(point.Y, point.Y, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YYZ(this Vector3 point)
        {
            return new Vector3(point.Y, point.Y, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YZX(this Vector3 point)
        {
            return new Vector3(point.Y, point.Z, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YZY(this Vector3 point)
        {
            return new Vector3(point.Y, point.Z, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 YZZ(this Vector3 point)
        {
            return new Vector3(point.Y, point.Z, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZXX(this Vector3 point)
        {
            return new Vector3(point.Z, point.X, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZXY(this Vector3 point)
        {
            return new Vector3(point.Z, point.X, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZXZ(this Vector3 point)
        {
            return new Vector3(point.Z, point.X, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZYX(this Vector3 point)
        {
            return new Vector3(point.Z, point.Y, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZYY(this Vector3 point)
        {
            return new Vector3(point.Z, point.Y, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZYZ(this Vector3 point)
        {
            return new Vector3(point.Z, point.Y, point.Z);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZZX(this Vector3 point)
        {
            return new Vector3(point.Z, point.Z, point.X);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZZY(this Vector3 point)
        {
            return new Vector3(point.Z, point.Z, point.Y);
        }

        /// <summary>
        /// Create a new vector 3 with the same elements in a different order
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 ZZZ(this Vector3 point)
        {
            return new Vector3(point.Z, point.Z, point.Z);
        }
        #endregion

        #region reorder v3 substitute z
        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 XX_(this Vector3 point, float z)
        {
            return new Vector3(point.X, point.X, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 XY_(this Vector3 point, float z)
        {
            return new Vector3(point.X, point.Y, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 XZ_(this Vector3 point, float z)
        {
            return new Vector3(point.X, point.Z, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 YX_(this Vector3 point, float z)
        {
            return new Vector3(point.Y, point.X, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 YY_(this Vector3 point, float z)
        {
            return new Vector3(point.Y, point.Y, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 YZ_(this Vector3 point, float z)
        {
            return new Vector3(point.Y, point.Z, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 ZX_(this Vector3 point, float z)
        {
            return new Vector3(point.Z, point.X, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 ZY_(this Vector3 point, float z)
        {
            return new Vector3(point.Z, point.Y, z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 ZZ_(this Vector3 point, float z)
        {
            return new Vector3(point.Z, point.Z, z);
        }
        #endregion

        #region reorder v3 substitute y
        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 X_X(this Vector3 point, float z)
        {
            return new Vector3(point.X, z, point.X);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 X_Y(this Vector3 point, float y)
        {
            return new Vector3(point.X, y, point.Y);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 X_Z(this Vector3 point, float y)
        {
            return new Vector3(point.X, y, point.Z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Y_X(this Vector3 point, float y)
        {
            return new Vector3(point.Y, y, point.X);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Y_Y(this Vector3 point, float y)
        {
            return new Vector3(point.Y, y, point.Y);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Y_Z(this Vector3 point, float y)
        {
            return new Vector3(point.Y, y, point.Z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Z_X(this Vector3 point, float y)
        {
            return new Vector3(point.Z, y, point.X);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Z_Y(this Vector3 point, float y)
        {
            return new Vector3(point.Z, y, point.Y);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Z_Z(this Vector3 point, float y)
        {
            return new Vector3(point.Z, y, point.Z);
        }
        #endregion

        #region reorder v3 substitute y
        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _XX(this Vector3 point, float x)
        {
            return new Vector3(x, point.X, point.X);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _XY(this Vector3 point, float x)
        {
            return new Vector3(x, point.X, point.Y);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _XZ(this Vector3 point, float x)
        {
            return new Vector3(x, point.X, point.Z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _YX(this Vector3 point, float x)
        {
            return new Vector3(x, point.Y, point.X);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _YY(this Vector3 point, float x)
        {
            return new Vector3(x, point.Y, point.Y);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _YZ(this Vector3 point, float x)
        {
            return new Vector3(x, point.Y, point.Z);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _ZX(this Vector3 point, float x)
        {
            return new Vector3(x, point.Z, point.X);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _ZY(this Vector3 point, float x)
        {
            return new Vector3(x, point.Z, point.Y);
        }

        /// <summary>
        /// Create a vector 3 with 2 elements from this vector, and a third floating point value
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Vector3 _ZZ(this Vector3 point, float x)
        {
            return new Vector3(x, point.Z, point.Z);
        }
        #endregion
    }
}
