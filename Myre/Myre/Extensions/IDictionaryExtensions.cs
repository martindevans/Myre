using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.Extensions
{
    public static class IDictionaryExtensions
    {
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
