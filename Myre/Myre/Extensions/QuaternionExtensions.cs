using Microsoft.Xna.Framework;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuaternionExtensions
    {
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
        /// <returns></returns>
        public static void Nlerp(this Quaternion a, ref Quaternion b, float t, out Quaternion result)
        {
            //return Quaternion.Normalize(Quaternion.Lerp(a, b, t));

            Quaternion.Lerp(ref a, ref b, t, out result);
            Quaternion.Normalize(ref result, out result);
        }
    }
}
