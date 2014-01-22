using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extensions methods for the System.Type class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Searches the Type for the specified attribute, and returns the first instance it finds; else returns null.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="type">The type within which to search.</param>
        /// <returns>The first instance of the attribute found; else null.</returns>
        public static T FindAttribute<T>(this Type type) where T : Attribute
        {
            Attribute[] attrs = Attribute.GetCustomAttributes(type);

            foreach (Attribute attr in attrs)
            {
                if (attr is T)
                    return attr as T;
            }

            return null;
        }

        ///// <summary>
        ///// Determines if this Type implements the specified interface.
        ///// </summary>
        ///// <param name="type">The type to search.</param>
        ///// <param name="findInterface">The interface to find.</param>
        ///// <returns><c>true</c> if this type implements the specified interface; else <c>false</c>.</returns>
        //public static bool HasInterface(this Type type, Type findInterface)
        //{
        //    var interfaces = type.GetInterfaces();
        //    for (int i = 0; i < interfaces.Length; i++)
        //    {
        //        if (interfaces[i].AssemblyQualifiedName == findInterface.AssemblyQualifiedName)
        //            return true;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, params object[] parameters)
        {
            var parameterTypes = parameters.Select(p => p.GetType()).ToArray();
            return CreateInstance(type, parameterTypes, parameters);
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameterTypes"></param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, Type[] parameterTypes, object[] parameters)
        {
            var c = type.GetConstructor(parameterTypes);

            if (c == null)
                return null;

            return c.Invoke(parameters);
        }

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
