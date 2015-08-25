using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuaternionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion FromXNA(this Microsoft.Xna.Framework.Quaternion quat)
        {
            return new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.Quaternion ToXNA(this Quaternion quat)
        {
            return new Microsoft.Xna.Framework.Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }

        /// <summary>
        /// Checks if any member of the given quaternion is NaN
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsNaN(this Quaternion v)
        {
            return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z) || float.IsNaN(v.W);
        }

        /// <summary>
        /// Linearly interpolate from A to B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Quaternion Lerp(this Quaternion a, Quaternion b, float t)
        {
            return Quaternion.Lerp(a, b, t);
        }

        /// <summary>
        /// Spherical linear interpolator from a to b. Shortest path/constant velocity
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Quaternion Slerp(this Quaternion a, Quaternion b, float t)
        {
            return Quaternion.Slerp(a, b, t);
        }

        /// <summary>
        /// Normalizing lerp from a to b, shortest path/non constant velocity
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Quaternion Nlerp(this Quaternion a, Quaternion b, float t)
        {
            Quaternion q;
            Nlerp(a, ref b, t, out q);
            return q;
        }

        /// <summary>
        /// Normalizing lerp from a to b, shortest path/non constant velocity
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static void Nlerp(this Quaternion a, ref Quaternion b, float t, out Quaternion result)
        {
            result = Quaternion.Normalize(Quaternion.Lerp(a, b, t));
        }

        /// <summary>
        /// Get the forward direction vector from this quaternion
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector3 Forward(this Quaternion a)
        {
            // http://nic-gamedev.blogspot.co.uk/2011/11/quaternion-math-getting-local-axis.html

            return new Vector3(
                2 * (a.X * a.Z + a.W * a.Y),
                2 * (a.Y * a.X - a.W * a.X),
                1 - 2 * (a.X * a.X + a.Y * a.Y)
            );
        }

        /// <summary>
        /// Get the up vector from this quaternion
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector3 Up(this Quaternion a)
        {
            // http://nic-gamedev.blogspot.co.uk/2011/11/quaternion-math-getting-local-axis.html

            return new Vector3(
                2 * (a.X * a.Y - a.W * a.Z),
                1 - 2 * (a.X * a.X + a.Z * a.Z),
                2 * (a.Y * a.Z + a.W * a.W)
            );
        }
    }
}
