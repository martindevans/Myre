using System;
using System.Diagnostics.Contracts;

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
        private readonly string _name;
        /// <summary>
        /// The name
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _name;
            }
        }

        /// <summary>
        /// Construct a new TypedName
        /// </summary>
        /// <param name="name"></param>
        public TypedName(string name)
        {
            Contract.Requires(name != null);

            _name = name;
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

            return new TypedName<T>(string.Format("{0}_{1}", a._name, b));
        }

        /// <summary>
        /// Explicitly cast this name into a string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static explicit operator string(TypedName<T> name)
        {
            Contract.Ensures(Contract.Result<string>() != null);

            return name.Name;
        }

        /// <summary>
        /// Explicitly cast a string into a typed name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static explicit operator TypedName<T>(string name)
        {
            Contract.Requires(name != null);

            return new TypedName<T>(name);
        }

        /// <summary>
        /// Explicitly cast this generically typed name into an non generically typed one
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static explicit operator NameWithType(TypedName<T> name)
        {
            return new NameWithType(name.Name, typeof(T));
        }
    }

    /// <summary>
    /// A string with an associated Type (non generic)
    /// </summary>
    [Serializable]
    public struct NameWithType
        : IEquatable<NameWithType>
    {
        private readonly string _name;
        /// <summary>
        /// The name
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _name;
            }
        }

        private readonly Type _type;
        /// <summary>
        /// The type
        /// </summary>
        public Type Type
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                return _type;
            }
        }

        /// <summary>
        /// Construct a new NameWithType
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public NameWithType(string name, Type type)
        {
            Contract.Requires(name != null);
            Contract.Requires(type != null);

            _name = name;
            _type = type;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_name != null);
            Contract.Invariant(_type != null);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is NameWithType))
                return false;

            return Equals((NameWithType)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode() * 397)
                    + (Type.GetHashCode() * 587);
            }
        }

        public bool Equals(NameWithType other)
        {
            return other._type == _type
                && other._name == _name;
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}>", _name, _type.Name);
        }
    }
}
