using System;
using System.Diagnostics.Contracts;

namespace Myre
{
    /// <summary>
    /// Generates random numbers
    /// </summary>
    public static class StaticRandom
    {
        #region random number generation
        private const uint U = 273326509 >> 19;

        /// <summary>
        /// Creates a random number from the specified seed
        /// </summary>
        /// <param name="seed">The seed value</param>
        /// <param name="upperBound">The maximum value (exclusive)</param>
        /// <returns></returns>
        public static uint Random(uint seed, uint upperBound)
        {
            Contract.Requires(upperBound > 0);

            uint t = (seed ^ (seed << 11));
            const uint W = 273326509;
            long i = (int)(0x7FFFFFFF & ((W ^ U) ^ (t ^ (t >> 8))));
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
            Contract.Requires(upperBound > 0);

            var u = new LongUInt2Union { LongValue = DateTime.Now.Ticks };
            var time = u.IntValue1 | u.IntValue2;

            return Random(time, upperBound);
        }
    }
}
