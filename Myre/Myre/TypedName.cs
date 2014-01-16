
namespace Myre
{
    /// <summary>
    /// A string with an associated generic type parameter
    /// </summary>
    /// <remarks>
    /// This may seem a little odd as the type parameter is not used. This type exists so that you can store a TypedName&lt;T&gt; somewhere and unambiguously refer to a box or property. The type parameter really exists for compiler inferencing
    /// </remarks>
    /// <typeparam name="T"></typeparam>
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
        /// Implicitly cast a string into a TypedName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        //public static implicit operator TypedName<T>(string name)
        //{
        //    return new TypedName<T>(name);
        //}
    }
}
