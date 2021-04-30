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
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <returns></returns>
        public static TV AddOrUpdate<TK, TV>(this IDictionary<TK, TV> dictionary, TK key, Func<TK, TV> add, Func<TK, TV, TV> update)
        {
            if (dictionary.TryGetValue(key, out var added))
                added = update(key, added);
            else
                added = add(key);

            dictionary[key] = added;
            return added;
        }
    }
}
