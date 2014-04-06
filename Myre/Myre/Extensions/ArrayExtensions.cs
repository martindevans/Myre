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
        /// <param name="offset"></param>
        /// <returns></returns>
        public static void Rotate<T>(this T[] array, int offset)
        {
            if (offset == 0 || array.Length <= 1)
                return;

            offset = offset % array.Length;

            if (offset == 0)
                return;

            if (offset < 0)
                offset = array.Length + offset;

            T[] temp = new T[offset];
            Array.Copy(array, array.Length - offset, temp, 0, offset);
            Array.Copy(array, 0, array, offset, array.Length - offset);
            Array.Copy(temp, 0, array, 0, temp.Length);
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
