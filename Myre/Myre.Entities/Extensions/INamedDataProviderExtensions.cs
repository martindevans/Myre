﻿using Myre.Collections;

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
            var box = dataProvider.Get<T>(name, false);
            if (box != null)
            {
                property.Value = box.Value;
                return true;
            }

            return false;
        }
    }
}