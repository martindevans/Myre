
namespace Myre.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public class NamedBoxCollection
        :BoxedValueStore<string>, INamedDataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="create"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Box<T> Get<T>(string name, bool create = true)
        {
            return Get<T>(name, default(T), create);
        }
    }
}
