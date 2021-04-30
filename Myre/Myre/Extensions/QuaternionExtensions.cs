using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuaternionExtensions
    {
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
                2 * (a.Y * a.Z + a.W * a.X)
            );
        }

        public static Vector3 Right(this Quaternion a)
        {
            // http://nic-gamedev.blogspot.co.uk/2011/11/quaternion-math-getting-local-axis.html

            return new Vector3(
                1 - 2 * (a.Y * a.Y + a.Z * a.Z),
                2 * (a.X * a.Y + a.W * a.Z),
                2 * (a.X * a.Z - a.W * a.Y)
            );
        }
    }
}
