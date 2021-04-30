
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Myre.Collections
{
    /// <summary>
    /// A collection of named data which is readable
    /// </summary>
    [ContractClass(typeof(INamedDataProviderContract))]
    public interface INamedDataProvider
        :IEnumerable<KeyValuePair<string, IBox>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="useDefaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T? GetValue<T>(TypedName<T> name, bool useDefaultValue = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryGetValue<T>(TypedName<T> name, out T? value);
    }

    [ContractClassFor(typeof(INamedDataProvider))]
    internal abstract class INamedDataProviderContract : INamedDataProvider
    {
        public T GetValue<T>(TypedName<T> name, bool useDefaultValue = true)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue<T>(TypedName<T> name, out T value)
        {
            throw new NotSupportedException();
        }

        public abstract IEnumerator<KeyValuePair<string, IBox>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
