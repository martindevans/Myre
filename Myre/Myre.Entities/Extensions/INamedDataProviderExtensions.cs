﻿using System;
using Myre.Collections;

namespace Myre.Entities.Extensions
{
    public static class NamedDataProviderExtensions
    {
        /// <summary>
        /// Attempts to copy the value from a data provider box to a property
        /// </summary>
        /// <typeparam name="T">Type of the data being handled</typeparam>
        /// <param name="dataProvider">The data provider to try to get a boxed value from</param>
        /// <param name="name">The name of the data to get</param>
        /// <param name="property">The property to copy the data into if the box exists on the data provider</param>
        /// <returns>true, if the copy happened, otherwise false</returns>
        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, string name, Property<T> property)
        {
            return TryCopyValue<T>(dataProvider, new TypedName<T>(name), v => property.Value = v);
        }

        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, TypedName<T> name, Property<T> property)
        {
            return TryCopyValue<T>(dataProvider, name, v => property.Value = v);
        }

        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, Property<T> property)
        {
            return dataProvider.TryCopyValue<T>(property.Name, property);
        }

        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, TypedName<T> name, ref T field)
        {
            T f = default(T);
            if (dataProvider.TryCopyValue(name, a => f = a))
            {
                field = f;
                return true;
            }

            return false;
        }

        public static bool TryCopyValue<T>(this INamedDataProvider dataProvider, TypedName<T> name, Action<T> action)
        {
            if (dataProvider == null)
                return false;

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
