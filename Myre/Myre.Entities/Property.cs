using System;

namespace Myre.Entities
{
    public delegate void PropertySetDelegate<T>(Property<T> property, T? oldValue, T? newValue);

    public delegate void PropertySetDelegate(IProperty property, object? oldValue, object? newValue);

    /// <summary>
    /// Base class for generically typed properties
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// The name of this property
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The current value of this property
        /// </summary>
        object? Value { get; set; }

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

    /// <summary>
    /// A generically typed property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Property<T>
        : IProperty
    {
        /// <summary>
        /// The name of this instance
        /// </summary>
        public string Name { get; }

        public TypedName<T> TypedName => new(Name);

        private T? _value;

        /// <summary>
        /// The value of this property
        /// </summary>
        public T? Value
        {
            get => _value;
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
        public event PropertySetDelegate<T>? PropertySet;

        object? IProperty.Value
        {
            get => Value;
            set
            {
                if (value == null)
                    Value = default;
                else
                    Value = (T)value;
            }
        }

        Type IProperty.Type => typeof(T);

        void IProperty.Clear()
        {
            PropertySet = null;
            _value = default;
        }

        public Property(string name)
        {
            Name = name;
            _value = default;
        }

        private void OnValueSet(T? oldValue)
        {
            PropertySet?.Invoke(this, oldValue, Value);
            _propertySet?.Invoke(this, oldValue, Value);
        }

        public override string ToString()
        {
            return Value is null ? "null" : Value.ToString();
        }

        // ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006 // Naming Styles
        private event PropertySetDelegate? _propertySet;
#pragma warning restore IDE1006 // Naming Styles
        event PropertySetDelegate IProperty.PropertySet
        {
            add => _propertySet += value;
            remove => _propertySet -= value;
        }
    }
}