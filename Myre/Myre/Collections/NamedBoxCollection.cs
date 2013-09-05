
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
        public T GetValue<T>(string name, bool useDefaultValue = true)
        {
            var box = Get<T>(name, default(T), useDefaultValue);
            return box.Value;
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            Box<T> box;
            if (TryGet<T>(name, out box))
                value = box.Value;
            else
                value = default(T);

            return box != null;
        }
    }
}
