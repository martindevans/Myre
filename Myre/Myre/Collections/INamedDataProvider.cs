
namespace Myre.Collections
{
    /// <summary>
    /// 
    /// </summary>
    public interface INamedDataProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="create"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Box<T> Get<T>(string name, bool create = true);
    }
}
