using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
            Contract.Requires(dictionary != null);
            Contract.Requires(key != null);
            Contract.Requires(add != null);
            Contract.Requires(update != null);
            Contract.Ensures(Contract.Result<TV>() != null);

            TV added;

            if (dictionary.TryGetValue(key, out added))
                added = update(key, added);
            else
                added = add(key);

            //We can't add contracts to Func<TK, TV> so we just have to assume they're conformant (sadface)
            Contract.Assume(added != null);

            dictionary[key] = added;
            return added;
        }
    }
}
