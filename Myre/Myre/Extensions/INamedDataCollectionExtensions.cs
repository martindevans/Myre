using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Collections;

namespace Myre.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class INamedDataCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? GetMaybeValue<T>(this INamedDataCollection data, string name) where T : struct
        {
            T v;
            if (data.TryGetValue<T>(name, out v))
                return v;
            return null;
        }
    }
}
