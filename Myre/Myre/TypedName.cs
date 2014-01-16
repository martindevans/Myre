
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
        /// <param name="b"></param>
        /// <returns></returns>
        public static TypedName<T> operator +(TypedName<T> a, string b)
        {
            return new TypedName<T>(string.Format("{0}_{1}", a.Name, b));
        }
    }
}
