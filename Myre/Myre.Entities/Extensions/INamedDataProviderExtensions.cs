using System;
using Myre.Collections;

namespace Myre.Entities.Extensions
{
    public static class NamedDataProviderExtensions
    {
        public static bool TryCopyValue<T>(this INamedDataProvider? dataProvider, string nameSpace, TypedName<T> name, Property<T> property, bool appended = true, bool fallback = true)
        {
            return TryCopyValue(dataProvider, nameSpace, name, v => property.Value = v, appended, fallback);
        }

        /// <summary>
        /// Try to copy a value from a dataProvider into a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataProvider">The data provider to copy data from</param>
        /// <param name="nameSpace"></param>
        /// <param name="name">The name of the data box to get</param>
        /// <param name="action"></param>
        /// <param name="appended">Indicates if the name of the behaviour should be appeneded to the box name</param>
        /// <param name="fallback">Indicates if the non appended name should be used as a fallback if the appended name is not found</param>
        /// <returns></returns>
        public static bool TryCopyValue<T>(this INamedDataProvider? dataProvider, string nameSpace, TypedName<T> name, Action<T> action, bool appended = true, bool fallback = true)
        {
            if (dataProvider == null)
                return false;

            if (!appended)
                return Try(dataProvider, name, action);
            else
            {
                if (Try(dataProvider, name + nameSpace, action))
                    return true;
                else if (fallback)
                    return Try(dataProvider, name, action);
                else
                    return false;
            }
        }

        public static bool TryCopyValue<T>(this INamedDataProvider? dataProvider, string nameSpace, TypedName<T> name, out T? field, bool appended = true, bool fallback = true)
        {
            var f = default(T);
            var result = TryCopyValue(dataProvider, nameSpace, name, a => f = a, appended, fallback);
            field = f;
            return result;
        }

        private static bool Try<T>(INamedDataProvider dataProvider, TypedName<T> name, Action<T> action)
        {
            if (dataProvider.TryGetValue(name, out var value))
            {
                action(value!);
                return true;
            }

            return false;
        }
    }
}
