using System;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static Vector3 RandomNormalVector(this Random random)
        {
            Contract.Requires(random != null);

            float randomA = (float)random.NextDouble() * 2 - 1;
            float randomB = (float)random.NextDouble() * 2 - 1;
            float randomC = (float)random.NextDouble() * 2 - 1;
            var randomVector = Vector3.Normalize(new Vector3(randomA, randomB, randomC));
            return randomVector;
        }
    }
}
