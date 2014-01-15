
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

        public bool TryGetValue<T>(TypedName<T> name, out T value)
        {
            Box<T> box;
            if (TryGet<T>(name.Name, out box))
                value = box.Value;
            else
                value = default(T);

            return box != null;
        }

        public void Set<T>(TypedName<T> key, T value)
        {
            Set<T>(key.Name, value);
        }
    }
}
