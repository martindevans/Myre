using System;
using System.Collections.Generic;

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
#if XBOX
            try
            {
                result = byte.Parse(value);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
#else
            return byte.TryParse(value, out result);
#endif
        }

        /// <summary>
        /// Tries to convert this string into an int.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The parsed int.</param>
        /// <returns><c>true</c> if the parse succeeded; else <c>false</c>.</returns>
        public static bool TryToInt(this string value, out int result)
        {
#if XBOX
            try
            {
                result = int.Parse(value);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
#else
            return int.TryParse(value, out result);
#endif
        }

        /// <summary>
        /// Tries to convert this string into a bool.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The parsed bool.</param>
        /// <returns><c>true</c> if the parse succeeded; else <c>false</c>.</returns>
        public static bool TryToBool(this string value, out bool result)
        {
#if XBOX
            try
            {
                result = bool.Parse(value);
                return true;
            }
            catch (Exception)
            {
                result = false;
                return false;
            }
#else
            return bool.TryParse(value, out result);
#endif
        }

        /// <summary>
        /// Tries to convert this string into a float.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="result">The parsed float.</param>
        /// <returns><c>true</c> if the parse succeeded; else <c>false</c>.</returns>
        public static bool TryToFloat(this string value, out float result)
        {
#if XBOX
            try
            {
                result = float.Parse(value);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
#else
            return float.TryParse(value, out result);
#endif
        }

        /// <summary>
        /// Splits this string, while keeping delimiters.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="delimiters">The delimiters around which to split.</param>
        /// <returns>A list of the split string parts.</returns>
        public static IList<string> SplitKeepDelimiters(this string s, params char[] delimiters)
        {
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
            if (String.IsNullOrEmpty(s))
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
            if (String.IsNullOrEmpty(s))
                return false;
            return s[0] == c;
        }
    }
}
