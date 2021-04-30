﻿using System;

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
        public string Name { get; }

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
        /// <summary>
        /// The name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Construct a new NameWithType
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public NameWithType(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public override bool Equals(object obj)
        {
            if (obj is not NameWithType type)
                return false;

            return Equals(type);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397)
                    + (Type.GetHashCode() * 587);
            }
        }

        public bool Equals(NameWithType other)
        {
            return other.Type == Type
                && other.Name == Name;
        }

        public override string ToString()
        {
            return $"{Name}<{Type.Name}>";
        }
    }
}
