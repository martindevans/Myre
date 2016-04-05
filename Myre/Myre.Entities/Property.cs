using System;
using System.Diagnostics.Contracts;

namespace Myre.Entities
{
    public delegate void PropertySetDelegate<T>(Property<T> property, T oldValue, T newValue);

    public delegate void PropertySetDelegate(IProperty property, object oldValue, object newValue);

    /// <summary>
    /// Base class for generically typed properties
    /// </summary>
    [ContractClass(typeof(IPropertyContract))]
    public interface IProperty
    {
        /// <summary>
        /// The name of this property
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The current value of this property
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// The type this property contains
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Set this property to default values
        /// </summary>
        /// <summary>
        /// Set the value to the default value and remove all events from PropertyChanged
        /// </summary>
        void Clear();

        /// <summary>
        /// Event triggered whenever the property value changes
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global (Justification: Public API)
        event PropertySetDelegate PropertySet;
    }

    [ContractClassFor(typeof(IProperty))]
    internal abstract class IPropertyContract : IProperty
    {
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                return null;
            }
        }

        public object Value
        {
            get { return null; }
            set { }
        }

        public Type Type
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                return null;
            }
        }

        public void Clear()
        {
        }

        public event PropertySetDelegate PropertySet;
    }

    /// <summary>
    /// A generically typed property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Property<T>
        : IProperty
    {
        private readonly string _name;
        /// <summary>
        /// The name of this instance
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return _name;
            }
        }

        public TypedName<T> TypedName
        {
            get { return new TypedName<T>(Name); }
        }

        private T _value;

        /// <summary>
        /// The value of this property
        /// </summary>
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                var oldValue = _value;
                _value = value;
                OnValueSet(oldValue);
            }
        }

        /// <summary>
        /// Called after the value of this property is changed
        /// </summary>
        public event PropertySetDelegate<T> PropertySet;

        object IProperty.Value
        {
            get { return Value; }
            set
            {
                if (value == null)
                    Value = default(T);
                else
                    Value = (T)value;
            }
        }

        Type IProperty.Type
        {
            get { return typeof(T); }
        }

        void IProperty.Clear()
        {
            PropertySet = null;
            _value = default(T);
        }

        public Property(string name)
        {
            Contract.Requires(name != null);

            _name = name;
            _value = default(T);
        }

        private void OnValueSet(T oldValue)
        {
            if (PropertySet != null)
                PropertySet(this, oldValue, Value);
            if (_propertySet != null)
                _propertySet(this, oldValue, Value);
        }

        public override string ToString()
        {
            return Value == null ? "null" : Value.ToString();
        }

        private event PropertySetDelegate _propertySet;
        event PropertySetDelegate IProperty.PropertySet
        {
            add { _propertySet += value; }
            remove { _propertySet -= value; }
        }
    }
}