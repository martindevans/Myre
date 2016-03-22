using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Executes an action for each item in the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            Contract.Requires(collection != null);
            Contract.Requires(action != null);

            foreach (var item in collection)
            {
                action(item);
            }
        }

        /// <summary>
        /// Returns each item in a collection of collections
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collection)
        {
            Contract.Requires(collection != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            foreach (var innerCollection in collection)
                foreach (var item in innerCollection)
                    yield return item;
        }

        /// <summary>
        /// Returns the first value in the collection, or the given default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T FirstOrDefaultValue<T>(this IEnumerable<T> collection, T defaultValue)
        {
            Contract.Requires(collection != null);

            foreach (var item in collection)
                return item;

            return defaultValue;
        }

        /// <summary>
        /// enumerates the start and then the end
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        [Obsolete]
        public static IEnumerable<T> Append<T>(this IEnumerable<T> start, IEnumerable<T> end)
        {
            Contract.Requires(start != null);
            Contract.Requires(end != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            foreach (var item in start)
                yield return item;

            foreach (var item in end)
                yield return item;
        }

        /// <summary>
        /// enumerates the start then the end
        /// </summary>
        /// <param name="end"></param>
        /// <param name="start"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> end, IEnumerable<T> start)
        {
            Contract.Requires(start != null);
            Contract.Requires(end != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            foreach (var item in start)
                yield return item;

            foreach (var item in end)
                yield return item;
        }

        /// <summary>
        /// Appends the given items onto this enumeration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> start, params T[] end)
        {
            Contract.Requires(start != null);
            Contract.Requires(end != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return start.Concat(end);
        }

        /// <summary>
        /// Prepends the given items onto this enumeration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> end, params T[] start)
        {
            Contract.Requires(end != null);
            Contract.Requires(start != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            return Prepend(end, start as IEnumerable<T>);
        }

        /// <summary>
        /// Returns every element in the sequence with it's index in the sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<int, T>> ZipWithIndex<T>(this IEnumerable<T> enumerable, int startIndex = 0)
        {
            Contract.Requires(enumerable != null);
            Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<int, T>>>() != null);

            var index = startIndex;
            foreach (var item in enumerable)
                yield return new KeyValuePair<int, T>(index++, item);
        }

        /// <summary>
        /// Drop the last N items from a collection
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="drop"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEnumerable<T> DropLast<T>(this IEnumerable<T> enumerable, int drop)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(drop > 0);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var buffer = new Queue<T>(drop + 1);

            foreach (var x in enumerable)
            {
                buffer.Enqueue(x);

                if (buffer.Count == drop + 1)
                    yield return buffer.Dequeue();
            }
        }
    }
}
