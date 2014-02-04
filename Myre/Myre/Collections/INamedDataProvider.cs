
using System;
using System.Collections.Generic;

namespace Myre.Collections
{
    /// <summary>
    /// 
    /// </summary>
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
        T GetValue<T>(TypedName<T> name, bool useDefaultValue = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryGetValue<T>(TypedName<T> name, out T value);
    }
}
