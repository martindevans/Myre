using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Myre
{
    public static class Assert
    {
        [Conditional("DEBUG")]
        public static void IsTrue(bool value, string message)
        {
            if (!value)
                throw new Exception(message);
        }

        [Conditional("DEBUG")]
        public static void ArgumentNotNull(string name, object value)
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        [Conditional("DEBUG")]
        public static void ArgumentInRange<T>(string name, T value, T min, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(name, string.Format("Must be between {0} and {1}", min, max));
        }

        [Conditional("DEBUG")]
        public static void ArgumentGreaterThan<T>(string name, T value, T min)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                throw new ArgumentOutOfRangeException(name, string.Format("Must be greater than {0}", min));
        }

        [Conditional("DEBUG")]
        public static void ArgumentLessThan<T>(string name, T value, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(name, string.Format("Must be less than {0}", max));
        }
    }
}
