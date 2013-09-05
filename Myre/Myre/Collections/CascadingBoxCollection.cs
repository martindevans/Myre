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
        private readonly INamedDataCollection _parent;
        private readonly NamedBoxCollection _values;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public CascadingBoxCollection(INamedDataCollection parent)
        {
            _parent = parent;
            _values = new NamedBoxCollection();
        }

        public void Set<T>(string key, T value)
        {
            _values.Set<T>(key, value);
        }

        public T GetValue<T>(string name, bool useDefaultValue = true)
        {
            T value;
            if (TryGetValue<T>(name, out value))
                return value;
            else
                return _parent.GetValue<T>(name, useDefaultValue);
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            return _values.TryGetValue<T>(name, out value);
        }
    }
}
