
using System.Diagnostics.Contracts;

namespace Myre.Collections
{
    /// <summary>
    /// A collection of named data which is writeable
    /// </summary>
    [ContractClass(typeof(INamedDataConsumerContract))]
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

    [ContractClassFor(typeof(INamedDataConsumer))]
    internal abstract class INamedDataConsumerContract : INamedDataConsumer
    {
        public void Set<T>(TypedName<T> key, T value)
        {
        }
    }
}
