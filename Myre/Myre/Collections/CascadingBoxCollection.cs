using System;

namespace Myre.Collections
{
    /// <summary>
    /// A INamedDataCollection with a parent. When getting a value the Get will search the parent if this object does not have the value.
    /// When setting the value it will be set in this instance, potentionally overriding the parent.
    /// </summary>
    public class CascadingBoxCollection
        :MarshalByRefObject, INamedDataCollection
    {
        private readonly INamedDataProvider _parent;
        private readonly NamedBoxCollection _values;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public CascadingBoxCollection(INamedDataProvider parent)
        {
            _parent = parent;
            _values = new NamedBoxCollection();
        }

        public void Set<T>(TypedName<T> key, T value)
        {
            _values.Set<T>(key, value);
        }

        public T GetValue<T>(TypedName<T> name, bool useDefaultValue = true)
        {
            T value;
            if (TryGetValue<T>(name, out value))
                return value;
            else
                return _parent.GetValue<T>(name, useDefaultValue);
        }

        public bool TryGetValue<T>(TypedName<T> name, out T value)
        {
            if (_values.TryGetValue<T>(name, out value))
                return true;

            return _parent.TryGetValue<T>(name, out value);
        }

        public Box<T> Get<T>(TypedName<T> key, T defaultValue = default(T), bool create = true)
        {
            return _values.Get<T>(key.Name, defaultValue, create);
        }
    }
}
