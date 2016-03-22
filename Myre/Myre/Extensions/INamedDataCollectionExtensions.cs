using System.Diagnostics.Contracts;
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
        public static T? GetMaybeValue<T>(this INamedDataCollection data, TypedName<T> name) where T : struct
        {
            Contract.Requires(data != null);

            T v;
            if (data.TryGetValue<T>(name, out v))
                return v;
            return null;
        }
    }
}
