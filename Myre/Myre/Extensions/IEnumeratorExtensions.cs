using System.Collections.Generic;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the IEnumberable interface.
    /// </summary>
    public static class IEnumeratorExtensions
    {
        /// <summary>
        /// Converts this IEnumberator into an IEnumberable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator">The enumerator.</param>
        /// <returns>An IEnumerable which iterates over this IEnumerator.</returns>
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }
}
