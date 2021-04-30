using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extensions methods for the System.Type class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Attempts to parse the given string with the default static Parse method for the given type
        /// </summary>
        /// <param name="t"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static object Parse(this Type t, string s)
        {
            if (t == typeof(string))
                return s;

            var parseMethod = t.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
            if (parseMethod == null)
                throw new InvalidOperationException(string.Format("No default Parse method found for type '{0}'", t.Name));

            return parseMethod.Invoke(null, new object[] {s});
        }

        /// <summary>
        /// Get all types this type derives from
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetImplementedTypes(this Type t)
        {
            //A type obviously is itself
            yield return t;

            //All the interfaces this type implements
            foreach (var typeInterface in t.GetInterfaces())
                yield return typeInterface;

            //Recurse for base type
            if (t.BaseType != null)
                foreach (var implementedType in GetImplementedTypes(t.BaseType))
                    yield return implementedType;
        }
    }
}
