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
    }
}
