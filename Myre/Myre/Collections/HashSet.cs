using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Myre.Collections
{
    #if WINDOWS
    public class HashSet<T>
        : System.Collections.Generic.HashSet<T>
    {
        #region Constructors
        public HashSet()
            : base()
        {
        }

        public HashSet(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public HashSet(IEqualityComparer<T> comparer)
            : base(comparer)
        {
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : base(collection, comparer)
        {
        }
        #endregion
    }
    #else
    //todo: Make the compact framework hashset a proper hashset instead of a list in disguise
    public class HashSet<T>
         : ICollection<T>, IEnumerable<T>
    {
        List<T> set;

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        // Summary:
        //     Gets the System.Collections.Generic.IEqualityComparer<T> object that is used
        //     to determine equality for the values in the set.
        //
        // Returns:
        //     The System.Collections.Generic.IEqualityComparer<T> object that is used to
        //     determine equality for the values in the set.
        public IEqualityComparer<T> Comparer { get; private set; }

        //
        // Summary:
        //     Gets the number of elements that are contained in a set.
        //
        // Returns:
        //     The number of elements that are contained in the set.
        public int Count { get { return set.Count; } }

        #region constructors
        public HashSet()
        {
            set = new List<T>();
            Comparer = (IEqualityComparer<T>)EqualityComparer<T>.Default;
        }

        public HashSet(IEnumerable<T> collection)
        {
            set = new List<T>(collection);
            Comparer = (IEqualityComparer<T>)EqualityComparer<T>.Default;
        }

        public HashSet(IEqualityComparer<T> comparer)
        {
            set = new List<T>();
            Comparer = comparer;
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            set = new List<T>(collection);
            Comparer = comparer;
        }
        #endregion

        // Summary:
        //     Adds the specified element to a set.
        //
        // Parameters:
        //   item:
        //     The element to add to the set.
        //
        // Returns:
        //     true if the element is added to the System.Collections.Generic.HashSet<T>
        //     object; false if the element is already present.
        public void Add(T item)
        {
            if (!Contains(item))
            {
                set.Add(item);
            }

        }

        //
        // Summary:
        //     Removes all elements from a System.Collections.Generic.HashSet<T> object.
        public void Clear()
        {
            set.Clear();
        }

        //
        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object contains
        //     the specified element.
        //
        // Parameters:
        //   item:
        //     The element to locate in the System.Collections.Generic.HashSet<T> object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object contains the specified
        //     element; otherwise, false.
        public bool Contains(T item)
        {
            return set.Contains(item, Comparer);
        }

        //
        // Summary:
        //     Copies the elements of a System.Collections.Generic.HashSet<T> object to
        //     an array.
        //
        // Parameters:
        //   array:
        //     The one-dimensional array that is the destination of the elements copied
        //     from the System.Collections.Generic.HashSet<T> object. The array must have
        //     zero-based indexing.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        public void CopyTo(T[] array)
        {
            set.CopyTo(array);
        }

        //
        // Summary:
        //     Copies the elements of a System.Collections.Generic.HashSet<T> object to
        //     an array, starting at the specified array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional array that is the destination of the elements copied
        //     from the System.Collections.Generic.HashSet<T> object. The array must have
        //     zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.
        //
        //   System.ArgumentException:
        //     arrayIndex is greater than the length of the destination array.-or-count
        //     is larger than the size of the destination array.
        public void CopyTo(T[] array, int arrayIndex)
        {
            set.CopyTo(array, arrayIndex);
        }

        //
        // Summary:
        //     Copies the specified number of elements of a System.Collections.Generic.HashSet<T>
        //     object to an array, starting at the specified array index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional array that is the destination of the elements copied
        //     from the System.Collections.Generic.HashSet<T> object. The array must have
        //     zero-based indexing.
        //
        //   arrayIndex:
        //     The zero-based index in array at which copying begins.
        //
        //   count:
        //     The number of elements to copy to array.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is null.
        //
        //   System.ArgumentOutOfRangeException:
        //     arrayIndex is less than 0.-or-count is less than 0.
        //
        //   System.ArgumentException:
        //     arrayIndex is greater than the length of the destination array.-or-count
        //     is greater than the available space from the index to the end of the destination
        //     array.
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            set.CopyTo(0, array, arrayIndex, count);
        }

        //
        // Summary:
        //     Returns an System.Collections.IEqualityComparer object that can be used for
        //     equality testing of a System.Collections.Generic.HashSet<T> object.
        //
        // Returns:
        //     An System.Collections.IEqualityComparer object that can be used for deep
        //     equality testing of the System.Collections.Generic.HashSet<T> object.
        public static IEqualityComparer<HashSet<T>> CreateSetComparer()
        {
            throw new NotImplementedException();
        }

        //
        // Summary:
        //     Removes all elements in the specified collection from the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Parameters:
        //   other:
        //     The collection of items to remove from the System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
                Remove(item);
        }

        //
        // Summary:
        //     Modifies the current System.Collections.Generic.HashSet<T> object to contain
        //     only elements that are present in that object and in the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public void IntersectWith(IEnumerable<T> other)
        {
            for (int i = set.Count - 1; i >= 0; i--)
            {
                if (!other.Contains(set[i]))
                    set.RemoveAt(i);
            }
        }

        //
        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a proper
        //     subset of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a proper subset
        //     of other; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        //
        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a proper
        //     superset of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a proper superset
        //     of other; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        //
        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a subset
        //     of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a subset of other;
        //     otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        //
        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object is a superset
        //     of the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is a superset of
        //     other; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        //
        // Summary:
        //     Determines whether the current System.Collections.Generic.HashSet<T> object
        //     and a specified collection share common elements.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object and other share
        //     at least one common element; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool Overlaps(IEnumerable<T> other)
        {
            foreach (var item in other)
                if (Contains(item))
                    return true;

            return false;
        }

        //
        // Summary:
        //     Removes the specified element from a System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Parameters:
        //   item:
        //     The element to remove.
        //
        // Returns:
        //     true if the element is successfully found and removed; otherwise, false.
        //     This method returns false if item is not found in the System.Collections.Generic.HashSet<T>
        //     object.
        public bool Remove(T item)
        {
            return set.Remove(item);
        }

        //
        // Summary:
        //     Removes all elements that match the conditions defined by the specified predicate
        //     from a System.Collections.Generic.HashSet<T> collection.
        //
        // Parameters:
        //   match:
        //     The System.Predicate<T> delegate that defines the conditions of the elements
        //     to remove.
        //
        // Returns:
        //     The number of elements that were removed from the System.Collections.Generic.HashSet<T>
        //     collection.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     match is null.
        public int RemoveWhere(Predicate<T> match)
        {
            int count = 0;

            for (int i = set.Count - 1; i >= 0; i--)
            {
                if (match(set[i]))
                {
                    set.RemoveAt(i);
                    count++;
                }
            }

            return count;
        }

        //
        // Summary:
        //     Determines whether a System.Collections.Generic.HashSet<T> object and the
        //     specified collection contain the same elements.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Returns:
        //     true if the System.Collections.Generic.HashSet<T> object is equal to other;
        //     otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        //
        // Summary:
        //     Modifies the current System.Collections.Generic.HashSet<T> object to contain
        //     only elements that are present either in that object or in the specified
        //     collection, but not both.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                if (!Remove(item))
                    set.Add(item);
            }
        }

        //
        // Summary:
        //     Sets the capacity of a System.Collections.Generic.HashSet<T> object to the
        //     actual number of elements it contains, rounded up to a nearby, implementation-specific
        //     value.
        public void TrimExcess()
        {
            set.TrimExcess();
        }

        //
        // Summary:
        //     Modifies the current System.Collections.Generic.HashSet<T> object to contain
        //     all elements that are present in both itself and in the specified collection.
        //
        // Parameters:
        //   other:
        //     The collection to compare to the current System.Collections.Generic.HashSet<T>
        //     object.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     other is null.
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other)
            {
                Add(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return set.GetEnumerator();
        }
    }
    #endif
}
