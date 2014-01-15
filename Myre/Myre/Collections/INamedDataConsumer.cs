
namespace Myre.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public interface INamedDataConsumer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        void Set<T>(TypedName<T> key, T value);
    }
}
