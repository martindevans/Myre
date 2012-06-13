using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Collections
{
    /// <summary>
    /// A bloom filter. False positives are possible, false negatives are not. Removing items is possible
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BloomFilter<T>
    {
        byte[] array;
        /// <summary>
        /// The number of keys to use for this filter
        /// </summary>
        public readonly int KeyCount;

        /// <summary>
        /// A hash generation function
        /// </summary>
        public delegate int GenerateHash(T a);
        private GenerateHash hashGenerator = SystemHash;

        /// <summary>
        /// Gets the number of items which have been added to this filter
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current false positive rate.
        /// </summary>
        /// <value>The false positive rate.</value>
        public double FalsePositiveRate
        {
            get
            {
                return Math.Pow(1 - Math.Exp(-KeyCount * Count / ((float)array.Length)), KeyCount);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="size">The size in bits</param>
        /// <param name="keys">The key count</param>
        public BloomFilter(int size, int keys)
            : this(size, keys, SystemHash)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="estimatedsize">The estimated number of items to add to the filter</param>
        /// <param name="targetFalsePositiveRate">The target positive rate.</param>
        public BloomFilter(int estimatedsize, float targetFalsePositiveRate)
            : this(estimatedsize, targetFalsePositiveRate, SystemHash)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="size">The size of the filter in bytes</param>
        /// <param name="keys">The number of keys to use</param>
        /// <param name="hashgen">The hash generation function</param>
        public BloomFilter(int size, int keys, GenerateHash hashgen)
        {
            array = new byte[size];
            KeyCount = keys;
            hashGenerator = hashgen;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomFilter&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="estimatedsize">The estimated number of members of the set</param>
        /// <param name="targetFalsePositiveRate">The target false positive rate when the estimated size is attained</param>
        /// <param name="hashgen">The hash generation function</param>
        public BloomFilter(int estimatedsize, float targetFalsePositiveRate, GenerateHash hashgen)
        {
            int size = (int)(-(estimatedsize * Math.Log(targetFalsePositiveRate)) / 0.480453014f);
            int keys = (int)(0.7f * size / estimatedsize);
            array = new byte[size];
            KeyCount = keys;

            hashGenerator = hashgen;
        }

        /// <summary>
        /// Adds the specified item to the filter
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Returns true if this item was possibly already in the set</returns>
        public bool Add(T item)
        {
            Count++;

            bool b = true;
            int hash = hashGenerator.Invoke(item);

            for (int i = 0; i < KeyCount; i++)
            {
                hash++;
                int index = GetIndex(hash, array.Length);

                if (array[index] == byte.MaxValue)
                {
                    //rollback half applied operation
                    for (int r = i - 1; r >= 0; r--)
                    {
                        hash--;
                        array[GetIndex(hash, array.Length)]--;
                    }

                    throw new OverflowException("Bloom filter overflowed");
                }

                //if this value was 0 before, then this item was not in the set before
                if (array[index]++ == 0)
                    b = false;
            }
            return b;
        }

        internal static int GetIndex(int hash, int arrayLength)
        {
            uint k = Random(unchecked((uint)hash), (uint)arrayLength);
            return (int)k;
        }

        /// <summary>
        /// Adds the specified item to the filter
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Returns true if this item was already in the set</returns>
        public bool Remove(T item)
        {
            if (!Contains(item))
                throw new ArgumentException("Item is not in filter");

            Count--;

            int hash = hashGenerator.Invoke(item);
            int first = hash;
            for (int i = 0; i < KeyCount; i++)
            {
                hash++;
                int index = GetIndex(hash, array.Length);
                if (array[index] == 0)
                {
                    for (int r = i - 1; r >= 0; r--)
                    {
                        hash--;
                        array[GetIndex(hash, array.Length)]++;
                    }
                    return false;
                }
                array[index]--;
            }

            return true;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            Count = 0;
            for (int i = 0; i < array.Length; i++)
                array[i] = 0;
        }

        /// <summary>
        /// Determines whether this filter contains the specificed object, this will sometimes return false positives but never false negatives
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if the filter might contain the item; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            int hash = hashGenerator(item);
            for (int i = 0; i < KeyCount; i++)
            {
                hash++;
                if (array[GetIndex(hash, array.Length)] == 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Unions the specified filters
        /// </summary>
        /// <param name="s">The s.</param>
        public void Union(BloomFilter<T> s)
        {
            if (s.KeyCount != this.KeyCount)
                throw new ArgumentException("Cannot union two filters with different key counts");
            s.UnionOnto(array);
        }

        private void UnionOnto(byte[] other)
        {
            if (other.Length != this.array.Length)
                throw new ArgumentException("Cannot union two filters with different sizes");
            for (int i = 0; i < other.Length; i++)
            {
                other[i] += array[i];
                if (other[i] + array[i] > byte.MaxValue)
                {
                    throw new NotImplementedException("Rollback and throw an overflow exception");
                }
            }
        }

        /// <summary>
        /// Creates a random number from the specified seed
        /// </summary>
        /// <param name="seed">The seed value</param>
        /// <param name="upperBound">The maximum value (exclusive)</param>
        /// <returns></returns>
        static uint Random(uint seed, uint upperBound)
        {
            const uint U = 273326509 >> 19;

            uint t = (seed ^ (seed << 11));
            uint w = 273326509;
            long i = (int)(0x7FFFFFFF & ((w ^ U) ^ (t ^ (t >> 8))));
            return (uint)(i % upperBound);
        }

        /// <summary>
        /// Uses the system "GetHashFunction" method to hash an object
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns></returns>
        public static int SystemHash(T a)
        {
            return a.GetHashCode();
        }
    }
}
