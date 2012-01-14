using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Collections
{
    /// <summary>
    /// A heap which allows O(1) extraction of both minimum and maximum items, and O(logn) insertion/deletion
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MinMaxHeap<T>
        : ICollection<T>
    {
        #region fields and properties
        private List<T> heap;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</returns>
        public int Count
        {
            get
            {
                return heap.Count;
            }
        }

        private IComparer<T> comparer = Comparer<T>.Default;
        /// <summary>
        /// The comparer to use for items in this collection. Changing this comparer will trigger a heapify operation
        /// </summary>
        public IComparer<T> Comparer
        {
            get
            {
                return comparer;
            }
            set
            {
                comparer = value;
                Heapify();
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxHeap&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        public MinMaxHeap(int capacity)
            : this(Comparer<T>.Default, capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxHeap&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="initialItems">The initial items.</param>
        public MinMaxHeap(IEnumerable<T> initialItems)
            : this(Comparer<T>.Default, initialItems)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxHeap&lt;T&gt;"/> class.
        /// </summary>
        public MinMaxHeap()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxHeap&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer to use</param>
        public MinMaxHeap(Comparer<T> comparer)
            : this(comparer, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxHeap&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer to use</param>
        /// <param name="capacity">The initial capacity of the heap</param>
        public MinMaxHeap(Comparer<T> comparer, int capacity)
        {
            this.comparer = comparer;
            heap = new List<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxHeap&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer to use</param>
        /// <param name="initialItems">The initial items to put into the heap</param>
        public MinMaxHeap(Comparer<T> comparer, IEnumerable<T> initialItems)
            : this(comparer)
        {
            AddMany(initialItems);
        }
        #endregion

        #region add
        /// <summary>
        /// Adds the specified item to the heap
        /// </summary>
        /// <param name="a">item to add to the heap</param>
        public void Add(T a)
        {
            heap.Add(a);
            int myPos = heap.Count - 1;

            BubbleUp(myPos);
        }

        /// <summary>
        /// adds all these Items to the heap and heapifies the heap (more efficient than adding each of the Items individually)
        /// </summary>
        /// <param name="a"></param>
        public void AddMany(IEnumerable<T> a)
        {
            heap.AddRange(a);
            Heapify();
        }

        /// <summary>
        /// Adds the items to the heap and heapifies
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="start">The start index to take items from</param>
        /// <param name="length">The number of items to take</param>
        public void AddMany(IEnumerable<T> a, int start, int length)
        {
            heap.AddRange(a.Skip(start).Take(length)); //Linq <3
            Heapify();
        }
        #endregion

        #region deletion
        /// <summary>
        /// Clears this heap.
        /// </summary>
        public void Clear()
        {
            heap.Clear();
        }

        /// <summary>
        /// Deletes the item with the largest key in the heap
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the heap is empty</exception>
        public T RemoveMax()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Heap is empty!");

            T value;

            if (heap.Count == 1)
            {
                value = heap[0];
                Clear();
            }
            else if (heap.Count == 2)
            {
                value = heap[1];
                heap.RemoveAt(1);
            }
            else
            {
                int maxPos = MaxIndex();
                value = heap[maxPos];

                int lastPos = heap.Count - 1;
                if (maxPos == lastPos)
                    heap.RemoveAt(lastPos);
                else
                {
                    heap[maxPos] = heap[lastPos];
                    heap.RemoveAt(lastPos);
                    TrickleDown(maxPos);
                }
            }

            return value;
        }

        /// <summary>
        /// Deletes the item with the smallest key in the heap
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the heap is empty</exception>
        public T RemoveMin()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Heap is empty!");

            T value = heap[0];

            if (heap.Count == 1)
            {
                Clear();
            }
            else if (heap.Count == 2)
            {
                heap.RemoveAt(0);
            }
            else
            {
                heap[0] = heap[heap.Count - 1];
                heap.RemoveAt(heap.Count - 1);
                TrickleDown(0);
            }

            return value;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;

            int lastPos = heap.Count - 1;
            if (lastPos == index)
                heap.RemoveAt(lastPos);
            else
            {
                heap[index] = heap[lastPos];
                heap.RemoveAt(lastPos);
                TrickleDown(BubbleUp(index));
            }
            return true;
        }

        /// <summary>
        /// Removes several nodes from the maximum end of the heap
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public void RemoveMax(int count)
        {
            if (count > Count)
                throw new InvalidOperationException("Not enough items in heap to remove " + count + " objects");
            if (count == Count)
                Clear();
            else
                for (int i = 0; i < count; i++)
                    RemoveMax();
        }

        /// <summary>
        /// Removes several nodes from the minimum end of the heap
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public void RemoveMin(int count)
        {
            if (count > Count)
                throw new InvalidOperationException("Not enough items in heap to remove " + count + " objects");
            if (count == Count)
                Clear();
            else
                for (int i = 0; i < count; i++)
                    RemoveMin();
        }
        #endregion

        #region peeking
        /// <summary>
        /// finds the largest item in the heap
        /// </summary>
        /// <returns>value with maximal key</returns>
        public T Maximum
        {
            get
            {
                int i = MaxIndex();
                if (i < 0)
                    throw new InvalidOperationException("Heap is empty");
                return heap[i];
            }
        }

        /// <summary>
        /// Finds the index of the max element
        /// </summary>
        /// <returns></returns>
        private int MaxIndex()
        {
            if (heap.Count == 0)
                return -1;
            if (heap.Count == 1)
                return 0;
            if (heap.Count == 2)
                return 1;
            return (Comparer.Compare(heap[1], heap[2]) > 0 ? 1 : 2);
        }

        /// <summary>
        /// finds the smallest item in the heap
        /// </summary>
        /// <returns>value with minimal key</returns>
        public T Minimum
        {
            get
            {
                return heap[0];
            }
        }

        /// <summary>
        /// Determines whether the heap contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the heap.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the heap; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }
        #endregion

        #region heapify
        /// <summary>
        /// Reorder the heap
        /// </summary>
        public void Heapify()
        {
            for (int i = heap.Count / 2 - 1; i >= 0; i--)
                TrickleDown(i);
        }
        #endregion

        #region trickledown
        private void TrickleDown(int index)
        {
            if (IsMinLevel(index))
                TrickleDownMin(index);
            else
                TrickleDownMax(index);
        }

        private void TrickleDownMin(int index)
        {
            int m = IndexMinChildGrandchild(index);
            if (m <= -1)
                return;
            if (IsLessThan(heap[m], heap[index]))
            {
                if (m > (index + 1) * 2) //check if this is a grandchild
                {
                    //m is a grandchild
                    Swap(m, index);
                    if (IsGreaterThan(heap[m], heap[Parent(m)]))
                        Swap(m, Parent(m));
                    TrickleDownMin(m);
                }
                else
                {
                    //m is a child
                    Swap(m, index);
                    TrickleDownMin(index);
                }
            }
        }

        private void TrickleDownMax(int index)
        {
            int m = IndexMaxChildGrandchild(index);
            if (m <= -1)
                return;
            if (IsGreaterThan(heap[m], heap[index]))
            {
                if (m > (index + 1) * 2)
                {
                    //m is a grandchild
                    Swap(m, index);
                    if (IsLessThan(heap[m], heap[Parent(m)]))
                        Swap(m, Parent(m));
                    TrickleDownMax(m);
                }
                else
                {
                    //m is a child
                    Swap(m, index);
                    TrickleDownMax(index);
                }
            }
        }
        #endregion

        #region bubble up
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns>the final resting index of the item</returns>
        private int BubbleUp(int index)
        {
            int parent = Parent(index);
            if (parent < 0)
                return index;
            if (IsMinLevel(index))
            {
                if (IsGreaterThan(heap[index], heap[parent]))
                {
                    Swap(index, parent);
                    return BubbleUpMax(parent);
                }
                else
                    return BubbleUpMin(index);
            }
            else
            {
                if (IsLessThan(heap[index], heap[parent]))
                {
                    Swap(index, parent);
                    return BubbleUpMin(parent);
                }
                else
                    return BubbleUpMax(index);
            }
        }

        private int BubbleUpMax(int index)
        {
            int grandparent = Parent(Parent(index));
            if (grandparent < 0)
                return index;
            if (IsGreaterThan(heap[index], heap[grandparent]))
            {
                Swap(index, grandparent);
                return BubbleUpMax(grandparent);
            }
            return index;
        }

        private int BubbleUpMin(int index)
        {
            int grandparent = Parent(Parent(index));
            if (grandparent < 0)
                return index;
            if (IsLessThan(heap[index], heap[grandparent]))
            {
                Swap(index, grandparent);
                return BubbleUpMin(grandparent);
            }
            return index;
        }
        #endregion

        #region helpers
        private bool IsGreaterThanOrEqualTo(T a, T b)
        {
            return Comparer.Compare(a, b) >= 0;
        }

        private bool IsLessThanOrEqualTo(T a, T b)
        {
            return Comparer.Compare(a, b) <= 0;
        }

        /// <summary>
        /// Finds the index of the given item in the hash tree
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private int IndexOf(T item)
        {
            //return heap.IndexOf(item);
            return IndexOf(item, 0);
        }

        private int IndexOf(T item, int rootIndex)
        {
            if (rootIndex < 0 || rootIndex >= heap.Count)
                return -1;

            if (heap[rootIndex].Equals(item))
                return rootIndex;

            bool isMin = IsMinLevel(rootIndex);
            if ((isMin && IsGreaterThanOrEqualTo(item, heap[rootIndex])) || (!isMin && IsLessThanOrEqualTo(item, heap[rootIndex])))
            {
                int i = IndexOf(item, IndexLeftChild(rootIndex));
                if (i != -1)
                    return i;

                i = IndexOf(item, IndexRightChild(rootIndex));
                if (i != -1)
                    return i;
            }

            return -1;
        }

        private static bool IsMinLevel(int index)
        {
            int level = (int)Math.Floor(Math.Log(index + 1, 2.0));
            return level % 2 == 0;
        }

        private int Parent(int m)
        {
            if (m <= 0)
                return -1;
            return (int)((m - 1) / 2);
        }

        private int IndexRightChild(int index)
        {
            return (index + 1) * 2;
        }

        private int IndexLeftChild(int index)
        {
            return ((index + 1) * 2) - 1;
        }

        private int IndexMinChildGrandchild(int index)
        {
            int indexMin = -1;
            int a = IndexLeftChild(index);
            int b = IndexRightChild(index);
            int c = ((a + 1) * 2) - 1;
            int d = ((a + 1) * 2);
            int e = ((b + 1) * 2) - 1;
            int f = ((b + 1) * 2);

            if (a < heap.Count)
                indexMin = a;
            if (b < heap.Count && IsLessThan(heap[b], heap[indexMin]))
                indexMin = b;
            if (c < heap.Count && IsLessThan(heap[c], heap[indexMin]))
                indexMin = c;
            if (d < heap.Count && IsLessThan(heap[d], heap[indexMin]))
                indexMin = d;
            if (e < heap.Count && IsLessThan(heap[e], heap[indexMin]))
                indexMin = e;
            if (f < heap.Count && IsLessThan(heap[f], heap[indexMin]))
                indexMin = f;

            return indexMin;
        }

        private int IndexMaxChildGrandchild(int index)
        {
            int indexMax = -1;
            int a = index * 2 + 1;
            int b = index * 2 + 2;
            int c = a * 2 + 1;
            int d = a * 2 + 2;
            int e = b * 2 + 1;
            int f = b * 2 + 2;

            if (a < heap.Count)
                indexMax = a;
            if (b < heap.Count && IsGreaterThan(heap[b], heap[indexMax]))
                indexMax = b;
            if (c < heap.Count && IsGreaterThan(heap[c], heap[indexMax]))
                indexMax = c;
            if (d < heap.Count && IsGreaterThan(heap[d], heap[indexMax]))
                indexMax = d;
            if (e < heap.Count && IsGreaterThan(heap[e], heap[indexMax]))
                indexMax = e;
            if (f < heap.Count && IsGreaterThan(heap[f], heap[indexMax]))
                indexMax = f;

            return indexMax;
        }

        private bool IsLessThan(T a, T b)
        {
            return Comparer.Compare(a, b) < 0;
        }

        private bool IsGreaterThan(T a, T b)
        {
            return Comparer.Compare(a, b) > 0;
        }

        private void Swap(int a, int b)
        {
            T parent = heap[a];
            heap[a] = heap[b];
            heap[b] = parent;
        }

        private void PrintHeap()
        {
            Console.Write("Heap:");
            for (int i = 0; i < heap.Count; i++)
            {
                Console.Write(heap[i] + " ");
            }
            Console.Write("\n");
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Count = " + Count;
        }

        #region ICollection<T> Members
        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="array"/> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// 	<paramref name="array"/> is multidimensional.-or-<paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type T cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("Array supplied to CopyTo is null");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("Array index < 0");
            if (array.Rank != 1)
                throw new ArgumentException("Array is multidimensional!");
            if (arrayIndex >= array.Length)
                throw new ArgumentException("index > length of array");
            if (array.Length - arrayIndex < heap.Count)
                throw new ArgumentException("Not enough space in given array");
            int upperIndex = heap.Count + arrayIndex;
            for (int i = arrayIndex; i < upperIndex; i++)
            {
                array[i] = heap[i - arrayIndex];
            }
        }

        /// <summary>
        /// always false
        /// </summary>
        /// <value></value>
        /// <returns>false.</returns>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }
        #endregion

        #region IEnumerable<T> Members
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.heap.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.heap.GetEnumerator();
        }
        #endregion
    }
}
