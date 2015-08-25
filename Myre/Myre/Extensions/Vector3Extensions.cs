using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class which contains extension methods for the Vector2 class.
    /// </summary>
    public static class Vector3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.Vector3 ToXNA(this System.Numerics.Vector3 v)
        {
            return new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Vector3 FromXNA(this Microsoft.Xna.Framework.Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
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
            var x = Microsoft.Xna.Framework.MathHelper.CatmullRom(a.X, b.X, c.X, d.X, t);
            var y = Microsoft.Xna.Framework.MathHelper.CatmullRom(a.Y, b.Y, c.Y, d.Y, t);
            var z = Microsoft.Xna.Framework.MathHelper.CatmullRom(a.Z, b.Z, c.Z, d.Z, t);

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
                Microsoft.Xna.Framework.MathHelper.Clamp(point.X, min.X, max.X),
                Microsoft.Xna.Framework.MathHelper.Clamp(point.Y, min.Y, max.Y),
                Microsoft.Xna.Framework.MathHelper.Clamp(point.Z, min.Z, max.Z)
            );
        }
    }
}
