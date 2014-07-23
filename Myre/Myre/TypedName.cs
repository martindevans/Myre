
using System;

namespace Myre
{
    /// <summary>
    /// A string with an associated generic type parameter
    /// </summary>
    /// <remarks>
    /// This may seem a little odd as the type parameter is not used. This type exists so that you can store a TypedName&lt;T&gt; somewhere and unambiguously refer to a box or property. The type parameter really exists for compiler inferencing
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct TypedName<T>
    {
        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Construct a new TypedName
        /// </summary>
        /// <param name="name"></param>
        public TypedName(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Append a string to the name of a TypedName
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b">the string to append, if this is null or empty nothing will happen</param>
        /// <returns></returns>
        public static TypedName<T> operator +(TypedName<T> a, string b)
        {
            if (string.IsNullOrEmpty(b))
                return a;

            return new TypedName<T>(string.Format("{0}_{1}", a.Name, b));
        }

        /// <summary>
        /// Explicitly cast this name into a string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static explicit operator string(TypedName<T> name)
        {
            return name.Name;
        }

        /// <summary>
        /// Explicitly cast a string into a typed name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static explicit operator TypedName<T>(string name)
        {
            return new TypedName<T>(name);
        }
    }
}
