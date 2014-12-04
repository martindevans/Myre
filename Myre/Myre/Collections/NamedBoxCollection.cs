
namespace Myre.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public class NamedBoxCollection
        :BoxedValueStore<string>, INamedDataCollection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="useDefaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>(TypedName<T> name, bool useDefaultValue = true)
        {
            var box = Get<T>(name.Name, default(T), useDefaultValue);
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
            if (TryGet<T>(name.Name, out box))
                value = box.Value;
            else
                value = default(T);

            return box != null;
        }

        /// <summary>
        /// Set the value with the given name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(TypedName<T> key, T value)
        {
            Set<T>(key.Name, value);
        }

        /// <summary>
        /// Calls Set (key, value). This method allows you to use a collection initializer to initialize a NamedBoxCollection
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void Add<T>(TypedName<T> key, T value)
        {
            Set(key, value);
        }
    }
}
