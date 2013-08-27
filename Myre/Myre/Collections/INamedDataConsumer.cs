
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
        Box<T> Set<T>(string key, T value);
    }
}
