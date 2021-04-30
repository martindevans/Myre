﻿using System;
using System.Collections.Generic;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the System.Collections.IList interface.
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// Sorts the list using insertion sort. This is usually slower than List.Sort and Array.Sort, but is stable.
        /// Worst case O(n^2).
        /// Best case O(n) (already sorted list).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="comparison">The comparison.</param>
        public static void InsertionSort<T>(this IList<T> list, Comparison<T> comparison)
        {
            var count = list.Count;
            for (var j = 1; j < count; j++)
            {
                var key = list[j];

                var i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }

        /// <summary>
        /// Sorts the list using insertion sort. This is usually slower than List.Sort and Array.Sort, but is stable.
        /// Worst case O(n^2).
        /// Best case O(n) (already sorted list).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="comparer">The comparer.</param>
        public static void InsertionSort<T>(this IList<T> list, IComparer<T> comparer)
        {
            var count = list.Count;
            for (var j = 1; j < count; j++)
            {
                var key = list[j];

                var i = j - 1;
                for (; i >= 0 && comparer.Compare(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }

        /// <summary>
        /// Remove all elements in the list which match the given predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        public static void RemoveAll<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (predicate.Invoke(list[i]))
                    list.RemoveAt(i);
            }
        }
    }
}
