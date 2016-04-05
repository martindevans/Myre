using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the System.String class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Tries to convert this string into a byte.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The parsed byte.</param>
        /// <returns><c>true</c> if the parse succeeded; else <c>false</c>.</returns>
        public static bool TryToByte(this string value, out byte result)
        {
            return byte.TryParse(value, out result);
        }

        /// <summary>
        /// Tries to convert this string into an int.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The parsed int.</param>
        /// <returns><c>true</c> if the parse succeeded; else <c>false</c>.</returns>
        public static bool TryToInt(this string value, out int result)
        {
            return int.TryParse(value, out result);
        }

        /// <summary>
        /// Tries to convert this string into a bool.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The parsed bool.</param>
        /// <returns><c>true</c> if the parse succeeded; else <c>false</c>.</returns>
        public static bool TryToBool(this string value, out bool result)
        {
            return bool.TryParse(value, out result);
        }

        /// <summary>
        /// Tries to convert this string into a float.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The parsed float.</param>
        /// <returns><c>true</c> if the parse succeeded; else <c>false</c>.</returns>
        public static bool TryToFloat(this string value, out float result)
        {
            return float.TryParse(value, out result);
        }

        /// <summary>
        /// Checks if the given string ends with the given character
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool EndsWith(this string s, char c)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            return s[s.Length - 1] == c;
        }

        /// <summary>
        /// Checks if the given string ends with the given character
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool StartsWith(this string s, char c)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            return s[0] == c;
        }
    }
}
