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
        /// Splits this string, while keeping delimiters.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="delimiters">The delimiters around which to split.</param>
        /// <returns>A list of the split string parts.</returns>
        public static IList<string> SplitKeepDelimiters(this string s, params char[] delimiters)
        {
            Contract.Requires(s != null);
            Contract.Requires(delimiters != null);
            Contract.Ensures(Contract.Result<IList<string>>() != null);

            List<string> words = new List<string>();
            int i = 0, j = 0;
            while (j < s.Length && (j = s.IndexOfAny(delimiters, i + 1)) > -1)
            {
                words.Add(s.Substring(i, j - i));
                i = j;
            }
            if (i < s.Length - 1)
                words.Add(s.Substring(i, s.Length - i));
            return words;
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
