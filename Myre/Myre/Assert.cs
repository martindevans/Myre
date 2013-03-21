using System;
using System.Diagnostics;

namespace Myre
{
    /// <summary>
    /// Helpers for asserting requirements
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"></exception>
        [Conditional("DEBUG")]
        public static void IsTrue(bool value, string message)
        {
            if (!value)
                throw new Exception(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        [Conditional("DEBUG")]
        public static void ArgumentNotNull(string name, object value)
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [Conditional("DEBUG")]
        public static void ArgumentInRange<T>(string name, T value, T min, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(name, string.Format("Must be between {0} and {1}", min, max));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [Conditional("DEBUG")]
        public static void ArgumentGreaterThan<T>(string name, T value, T min)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                throw new ArgumentOutOfRangeException(name, string.Format("Must be greater than {0}", min));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [Conditional("DEBUG")]
        public static void ArgumentLessThan<T>(string name, T value, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(name, string.Format("Must be less than {0}", max));
        }
    }
}
