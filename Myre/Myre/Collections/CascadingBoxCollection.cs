using Myre.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Myre.Collections
{
    /// <summary>
    /// A INamedDataCollection with a parent. When getting a value the Get will search the parent if this object does not have the value.
    /// When setting the value it will be set in this instance, potentionally overriding the parent.
    /// </summary>
    public class CascadingBoxCollection
        : INamedDataCollection
    {
        private readonly INamedDataProvider _parent;
        private readonly NamedBoxCollection _values;
        private readonly IEnumerable<KeyValuePair<string, IBox>> _enumerable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public CascadingBoxCollection(INamedDataProvider parent)
        {
            _parent = parent;
            _values = new NamedBoxCollection();

            _enumerable = _parent.Where(a => _values.Contains(a.Key, a.Value.Type)).Append(_values);
        }

        /// <summary>
        /// Set the value with the given name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(TypedName<T> key, T value)
        {
            _values.Set<T>(key, value);
        }

        /// <summary>
        /// Get the value with the given name
        /// </summary>
        /// <typeparam name="T">Type of the value (inferred from the TypedName)</typeparam>
        /// <param name="name">The name and type of this value</param>
        /// <param name="useDefaultValue">Indicates if the default value should be used if no value can be found</param>
        /// <returns></returns>
        public T GetValue<T>(TypedName<T> name, bool useDefaultValue = true)
        {
            T value;
            if (TryGetValue<T>(name, out value))
                return value;

            //DIdn't manage to get it from parent or self, get it from self using default value flag
            return _values.GetValue<T>(name, useDefaultValue);
        }

        /// <summary>
        /// Try to get a value with the given name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue<T>(TypedName<T> name, out T value)
        {
            if (_values.TryGetValue<T>(name, out value))
                return true;

            return _parent.TryGetValue<T>(name, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="create"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Box<T> Get<T>(TypedName<T> key, T defaultValue = default(T), bool create = true)
        {
            return _values.Get<T>(key.Name, defaultValue, create);
        }

        /// <summary>
        /// Enumerate all values in this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, IBox>> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        /// <summary>
        /// Enumerate this collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
