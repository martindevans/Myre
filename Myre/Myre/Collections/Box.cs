using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Collections
{
    public interface IBox
    {
        object Value { get; set; }
    }

    /// <summary>
    /// A class which boxes a value.
    /// </summary>
    /// <typeparam name="T">The type of the value to box.</typeparam>
    public class Box<T>
        : IBox
    {
        /// <summary>
        /// The value this box contains.
        /// </summary>
        public T Value;

        /// <summary>
        /// Gets or sets the value this box contains.
        /// </summary>
        /// <value>The value this box contains.</value>
        object IBox.Value
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }

    /// <summary>
    /// A dictionary, mapping keys to boxed values.
    /// </summary>
    /// <typeparam name="Key">The type of the Key.</typeparam>
    public class BoxedValueStore<Key>
        :IEnumerable<KeyValuePair<Key, IBox>>
    {
        private Dictionary<Key, IBox> values;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxedValueStore&lt;Key&gt;"/> class.
        /// </summary>
        public BoxedValueStore()
        {
            values = new Dictionary<Key, IBox>();
        }

        /// <summary>
        /// Determines whether a value exists at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Key key)
        {
            return values.ContainsKey(key);
        }

        /// <summary>
        /// Tries the value at the sspecified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGet<T>(Key key, out Box<T> value)
        {
            IBox box;
            if (values.TryGetValue(key, out box))
            {
                value =  box as Box<T>;
                return true;
            }

            value = null;
            return false;
        }
        
        /// <summary>
        /// Gets the value at the specified key.
        /// </summary>
        /// <typeparam name="T">The type of value stored at the key.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="create">Indicates if the box should be created if it is not found.</param>
        /// <returns>The value at the specified key, or null if the existing box contains a different value type.</returns>
        public Box<T> Get<T>(Key key, T defaultValue = default(T), bool create = true)
        {
            IBox box = Get(key);
            if (box != null)
                return (Box<T>)box;
            else if (!create)
                return null;
            else
            {
                var value = new Box<T> { Value = defaultValue };

                values[key] = value;
                return value;
            }
        }

        /// <summary>
        /// Get an untyped box (if it already exists)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IBox Get(Key key)
        {
            IBox box;
            if (values.TryGetValue(key, out box))
                return box;
            return null;
        }

        /// <summary>
        /// Adds the specified value to this container.
        /// </summary>
        /// <typeparam name="T">The type of value to add.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The box containing the value at the specified key.</returns>
        public Box<T> Set<T>(Key key, T value)
        {
            var box = Get<T>(key);
            if (box == null)
                throw new InvalidOperationException("The value at key " + key + " is of type " + box.GetType().GetGenericArguments()[0]);

            box.Value = value;
            return box;
        }

        public IEnumerator<KeyValuePair<Key, IBox>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
