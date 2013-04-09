using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre
{
    /// <summary>
    /// Generates random numbers
    /// </summary>
    public static class StaticRandom
    {
        #region random number generation
        const uint U = 273326509 >> 19;

        /// <summary>
        /// Creates a random number from the specified seed
        /// </summary>
        /// <param name="seed">The seed value</param>
        /// <param name="upperBound">The maximum value (exclusive)</param>
        /// <returns></returns>
        public static uint Random(uint seed, uint upperBound)
        {
            uint t = (seed ^ (seed << 11));
            const uint w = 273326509;
            long i = (int)(0x7FFFFFFF & ((w ^ U) ^ (t ^ (t >> 8))));
            return (uint)(i % upperBound);
        }
        #endregion

        /// <summary>
        /// Creates a random number, using the time as a seed
        /// </summary>
        /// <param name="upperBound">The maximum value (exclusive)</param>
        /// <returns></returns>
        public static uint Random(uint upperBound = uint.MaxValue)
        {
            long ticks = DateTime.Now.Ticks;
            uint time = ((uint)(ticks & uint.MaxValue)) | ((uint)((ticks >> 32) & uint.MaxValue));

            return Random(time, upperBound);
        }
    }
}
