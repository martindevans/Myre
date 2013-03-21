using System;
using System.Collections.Generic;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="add"></param>
        /// <param name="update"></param>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static V AddOrUpdate<K, V>(this IDictionary<K, V> dictionary, K key, Func<K, V> add, Func<K, V, V> update)
        {
            V added;

            if (dictionary.TryGetValue(key, out added))
                added = update(key, added);
            else
                added = add(key);

            dictionary[key] = added;
            return added;
        }
    }
}
