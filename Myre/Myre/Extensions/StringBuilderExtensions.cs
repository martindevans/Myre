using System.Text;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the System.Text.StringBuilder class.
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <param name="sb"></param>
        public static void Clear(this StringBuilder sb)
        {
            sb.Remove(0, sb.Length);
        }

        /// <summary>
        /// Appends the specified stringbuilder onto this instance.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="stringBuilder">The string builder.</param>
        public static void Append(this StringBuilder sb, StringBuilder stringBuilder)
        {
            sb.Append(stringBuilder, 0, stringBuilder.Length);
        }

        /// <summary>
        /// Appends the specified stringbuilder onto this instance.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="start">The start index at which to copying.</param>
        /// <param name="length">The number of characters to append..</param>
        public static void Append(this StringBuilder sb, StringBuilder stringBuilder, int start, int length)
        {
            var end = start + length;
            for (var i = start; i < end; i++)
                sb.Append(stringBuilder[i]);
        }
    }
}
