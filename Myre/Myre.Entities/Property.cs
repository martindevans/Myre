using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Myre.Entities
{
    public delegate void PropertySetDelegate<T>(Property<T> property, T oldValue, T newValue);

    /// <summary>
    /// Base class for generically typed properties
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// The name of this property
        /// </summary>
        String Name { get; }

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
        public String Name { get; private set; }

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
            set { Value = (T)value; }
        }

        Type IProperty.Type
        {
            get { return typeof(T); }
        }

        void IProperty.Clear()
        {
            if (PropertySet != null)
                foreach (var item in PropertySet.GetInvocationList())
                    PropertySet -= (PropertySetDelegate<T>)item;

            value = default(T);
        }

        public Property(String name)
        {
            this.Name = name;
            this.value = default(T);
        }

        private void OnValueSet(T oldValue)
        {
            if (PropertySet != null)
                PropertySet(this, oldValue, Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}