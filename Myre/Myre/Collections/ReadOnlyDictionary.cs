using System;
using System.Collections.Generic;

namespace Myre.Collections
{
    /// <summary>
    /// a readonly wrapper around an IDictionary object
    /// </summary>
    /// <typeparam name="Key">The type of the key.</typeparam>
    /// <typeparam name="Data">The type of the data.</typeparam>
    public class ReadOnlyDictionary<Key, Data>
        : IEnumerable<KeyValuePair<Key, Data>>
    {
        readonly IDictionary<Key, Data> _source;

        /// <summary>
        /// Gets the data assosciated with the specified key.
        /// </summary>
        /// <value></value>
        public Data this[Key key]
        {
            get { return _source[key]; }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get { return _source.Count; }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public IEnumerable<Key> Keys
        {
            get { return _source.Keys; }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<Data> Values
        {
            get { return _source.Values; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionary&lt;Key, Data&gt;"/> class.
        /// </summary>
        /// <param name="source">The backing collection to query</param>
        public ReadOnlyDictionary(IDictionary<Key, Data> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _source = source;
        }

        /// <summary>
        /// Determines whether the specified key is within the backing collection
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if the specified key is contained; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Key key)
        {
            return _source.ContainsKey(key);
        }

        #region IEnumerable<KeyValuePair<Key,Data>> Members
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Key, Data>> GetEnumerator()
        {
            return _source.GetEnumerator();
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
            return _source.GetEnumerator();
        }
        #endregion
    }
}
