using System;
using System.Diagnostics.Contracts;

namespace Myre.Collections
{
    /// <summary>
    /// A reference to a value
    /// </summary>
    public interface IBox
    {
        /// <summary>
        /// The value of this box
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// The type of the value of this box
        /// </summary>
        Type Type { get; }
    }

    /// <summary>
    /// An object which contains a value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseBox<T>
        : IBox
    {
        /// <summary>
        /// The value this box contains.
        /// </summary>
        public abstract T Value { get; set; }

        /// <summary>
        /// Gets or sets the value this box contains.
        /// </summary>
        /// <value>The value this box contains.</value>
        object IBox.Value
        {
            get { return Value; }
            set
            {
                var old = Value;

                if (value == null)
                    Value = default(T);
                else
                    Value = (T)value;

                if (BoxChanged != null)
                    BoxChanged(this, old, Value);
            }
        }

        /// <summary>
        /// The type of the value in this box
        /// </summary>
        public Type Type
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                return typeof(T);
            }
        }

        /// <summary>
        /// An event which is triggered whenever the value in this box changes. Args are The box, the old value, and the new value.
        /// </summary>
        public event Action<BaseBox<T>, T, T> BoxChanged;
    }

    /// <summary>
    /// A class which boxes a value.
    /// </summary>
    /// <typeparam name="T">The type of the value to box.</typeparam>
    public class Box<T>
        : BaseBox<T>
    {
        /// <summary>
        /// The value this box contains.
        /// </summary>
        public override T Value { get; set; }
    }
}
