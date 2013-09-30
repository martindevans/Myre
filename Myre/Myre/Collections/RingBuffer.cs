
using System;

namespace Myre.Collections
{
    /// <summary>
    /// Stores the N most recently added items
    /// </summary>
    public class RingBuffer<T>
    {
        private readonly T[] _items;

        /// <summary>
        /// Indicates the number of items added to the collection and currently stored
        /// </summary>
        public int Count { get; private set; }

        private int _end = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _items.Length)
                    throw new ArgumentOutOfRangeException("index");
                return _items[(_end + index + (_items.Length - _end)) % _items.Length];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public RingBuffer(int size)
        {
            _items = new T[size];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            _items[_end] = item;
            _end = (_end + 1) % _items.Length;

            if (Count < _items.Length)
                Count++;
        }
    }
}
