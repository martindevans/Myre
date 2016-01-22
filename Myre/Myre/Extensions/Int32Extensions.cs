
namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extensions for the System.Int32 struct.
    /// </summary>
    public static class Int32Extensions
    {
        /// <summary>
        /// Determines if this Int32 has all the bits set to true as there are in
        /// the specified Int32.
        /// </summary>
        public static bool ContainsBits(this int n, int flags)
        {
            return (n & flags) == flags;
        }

        /// <summary>
        /// Determines if the specified bit is set to true in this Int32.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="bit">The index of the bit, zero indicates the least significant bit</param>
        /// <returns></returns>
        public static bool IsBitSet(this int n, int bit)
        {
            var mask = 1 << bit;
            return (n & mask) == mask;
        }
    }
}
