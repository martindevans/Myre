using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Myre.Collections
{
    public class NamedBoxCollection
        : INamedDataCollection, IEnumerable<KeyValuePair<string, IBox>>
    {
        #region fields
        private readonly Dictionary<NameWithType, IBox> _values = new Dictionary<NameWithType, IBox>();

        private static readonly Type _boxType = typeof(Box<>);
        #endregion

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_values != null);
            Contract.Invariant(_boxType != null);
        }

        #region queries
        /// <summary>
        /// Determines whether a value exists at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="type"></param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(string key, Type type)
        {
            Contract.Requires(key != null);
            Contract.Requires(type != null);

            return _values.ContainsKey(new NameWithType(key, type));
        }

        public bool Contains<T>(TypedName<T> name)
        {
            return _values.ContainsKey(new NameWithType(name.Name, typeof(T)));
        }
        #endregion

        #region get (box)
        /// <summary>
        /// Gets the box with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of value stored at the key.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value at the specified key, or null if the existing box contains a different value type.</returns>
        public Box<T> GetOrCreate<T>(TypedName<T> key, T defaultValue = default(T))
        {
            Contract.Ensures(Contract.Result<Box<T>>() != null);

            var box = Get(key.Name, typeof(T));

            var box1 = box as Box<T>;
            if (box1 != null)
                return box1;

            var value = new Box<T> { Value = defaultValue };
            _values[new NameWithType(key.Name, typeof(T))] = value;
            return value;
        }

        /// <summary>
        /// Tries the box with the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGet<T>(TypedName<T> key, out Box<T> value)
        {
            IBox box;
            if (_values.TryGetValue(new NameWithType(key.Name, typeof(T)), out box))
            {
                value = (Box<T>)box;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Get an untyped box (if it already exists)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IBox Get(string key, Type type)
        {
            Contract.Requires(key != null);
            Contract.Requires(type != null);

            IBox box;
            if (_values.TryGetValue(new NameWithType(key, type), out box))
                return box;
            return null;
        }
        #endregion

        #region get (value)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="useDefaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>(TypedName<T> name, bool useDefaultValue = true)
        {
            //If using the default is allowed this is simple, get or create a new box and return the contents
            if (useDefaultValue)
                return GetOrCreate<T>(name).Value;

            //Ok no defaults is a little more complex!
            //Try to get the existing box
            Box<T> box;
            if (!TryGet<T>(name, out box))
                throw new KeyNotFoundException(string.Format("Failed to find a value with key '{0}'", name.Name));

            //Found a box, yay!
            return box.Value;
        }

        /// <summary>
        /// Try to get the value with the given name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue<T>(TypedName<T> name, out T value)
        {
            Box<T> box;
            if (TryGet<T>(name, out box))
                value = box.Value;
            else
                value = default(T);

            return box != null;
        }
        #endregion

        #region set (value)
        /// <summary>
        /// Adds the specified value to this container.
        /// </summary>
        /// <typeparam name="T">c</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set<T>(TypedName<T> key, T value)
        {
            GetOrCreate<T>(key).Value = value;
        }

        /// <summary>
        /// Adds the specified value to this container.
        /// </summary>
        /// <param name="key">The value.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type of value to add.</param>
        public void Set(string key, object value, Type type)
        {
            Contract.Requires(key != null);
            Contract.Requires(type != null);

            var box = Get(key, type);
            if (box != null && !box.Type.IsAssignableFrom(type))
                throw new InvalidOperationException("The value at key " + key + " is of the wrong type");

            if (box == null)
            {
                var genericType = _boxType.MakeGenericType(type);
                Contract.Assert(genericType != null);
                box = (IBox)Activator.CreateInstance(genericType);
                _values.Add(new NameWithType(key, type), box);
            }

            box.Value = value;
        }

        /// <summary>
        /// Calls Set (key, value). This method allows you to use a collection initializer to initialize a NamedBoxCollection
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void Add<T>(TypedName<T> key, T value)
        {
            //This method allows you to use a collection initializer to initialize a NamedBoxCollection!
            Set(key, value);
        }
        #endregion

        #region enumeration
        /// <summary>
        /// Enumerate all the values in this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, IBox>> GetEnumerator()
        {
            return _values.Select(a => new KeyValuePair<string, IBox>(a.Key.Name, a.Value)).GetEnumerator();
        }

        /// <summary>
        /// Enumerate this collection
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
