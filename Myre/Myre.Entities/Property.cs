using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Myre.Entities
{
    public delegate void PropertyChangedDelegate<T>(Property<T> property, T oldValue, T newValue);

    /// <summary>
    /// Base class for generically typed properties
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// The name of this property
        /// </summary>
        Enum Name { get; }

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
        void Clear();
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
        public Enum Name { get; private set; }

        private T value;
        /// <summary>
        /// The value of this property
        /// </summary>
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                var oldValue = this.value;
                this.value = value;
                OnValueChanged(oldValue);
            }
        }

        /// <summary>
        /// Called after the value of this property is changed
        /// </summary>
        public event PropertyChangedDelegate<T> PropertyChanged;

        object IProperty.Value
        {
            get { return Value; }
            set { Value = (T)value; }
        }

        Type IProperty.Type
        {
            get { return typeof(T); }
        }

        void IProperty.Clear()
        {
            if (PropertyChanged != null)
                foreach (var item in PropertyChanged.GetInvocationList())
                    PropertyChanged -= (PropertyChangedDelegate<T>)item;

            value = default(T);
        }

        public Property(Enum name)
        {
            this.Name = name;
            this.value = default(T);
        }

        private void OnValueChanged(T oldValue)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, oldValue, Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}