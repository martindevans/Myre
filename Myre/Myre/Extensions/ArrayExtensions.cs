using System;

namespace Myre.Extensions
{
    /// <summary>
    /// Extensions to arrays
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// In place rotate the elements in the given array by a certain amount (wrapping around the end)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static void Rotate<T>(this T[] array, int amount)
        {
            if (amount == 0)
                return;

            bool right = amount > 0;
            amount = Math.Abs(amount);

            int offset = array.Length % amount;
            if (offset > 0)
            {
                T[] temp = new T[offset];
                if (!right)
                {
                    Array.Copy(array, temp, offset);
                    Array.Copy(array, offset, array, 0, array.Length - offset);
                    Array.Copy(temp, 0, array, array.Length - offset, temp.Length);
                }
                else
                {
                    Array.Copy(array, array.Length - offset, temp, 0, offset);
                    Array.Copy(array, 0, array, offset, array.Length - offset);
                    Array.Copy(temp, 0, array, 0, temp.Length);
                }
            }
        }

        /// <summary>
        /// Copies a segment of one array to an equal length segment of another array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyTo<T>(this ArraySegment<T> source, ArraySegment<T> destination)
        {
            if (source.Count != destination.Count)
                throw new InvalidOperationException("Copy requires that source and destination are the same size");

            for (int i = 0; i < source.Count; i++)
                destination.Array[i + destination.Offset] = source.Array[source.Offset + i];
        }
    }
}
