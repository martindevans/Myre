using System;
using Myre.Collections;
using Myre.Entities.Behaviours;

namespace Myre.Entities.Extensions
{
    public static class NamedDataProviderExtensions
    {
        /// <summary>
        /// Try to copy a value from a dataProvider into a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="behaviour">>The behaviour performing this copy</param>
        /// <param name="dataProvider">The data provider to copy data from</param>
        /// <param name="name">The name of the data box to get</param>
        /// <param name="property">The property to copy the value in to</param>
        /// <param name="appended">Indicates if the name of the behaviour should be appeneded to the box name</param>
        /// <param name="fallback">Indicates if the non appended name should be used as a fallback if the appended name is not found</param>
        /// <returns></returns>
        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, Behaviour behaviour, TypedName<T> name, Property<T> property, bool appended = true, bool fallback = true)
        {
            return TryCopyValue<T>(dataProvider, behaviour, name, v => property.Value = v, appended, fallback);
        }

        /// <summary>
        /// Try to copy a value from a dataProvider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="behaviour">The behaviour performing this copy</param>
        /// <param name="dataProvider">The data provider to copy data from</param>
        /// <param name="name">The name of the data box to get</param>
        /// <param name="action">The copy action</param>
        /// <param name="appended">Indicates if the name of the behaviour should be appeneded to the box name</param>
        /// <param name="fallback">Indicates if the non appended name should be used as a fallback if the appended name is not found</param>
        /// <returns></returns>
        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, Behaviour behaviour, TypedName<T> name, Action<T> action, bool appended = true, bool fallback = true)
        {
            if (dataProvider == null)
                return false;

            if (!appended)
                return Try(dataProvider, name, action);
            else
            {
                if (Try(dataProvider, name + behaviour.Name, action))
                    return true;
                else if (fallback)
                    return Try(dataProvider, name, action);
                else
                    return false;
            }
        }

        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, Behaviour behaviour, TypedName<T> name, out T field, bool appended = true, bool fallback = true)
        {
            T f = default(T);
            var result = TryCopyValue(dataProvider, behaviour, name, a => f = a, appended, fallback);
            field = f;
            return result;
        }

        private static bool Try<T>(INamedDataProvider dataProvider, TypedName<T> name, Action<T> action)
        {
            T value;
            if (dataProvider.TryGetValue<T>(name, out value))
            {
                action(value);
                return true;
            }

            return false;
        }
    }
}
