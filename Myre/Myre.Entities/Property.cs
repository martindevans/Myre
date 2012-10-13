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
    public abstract class Property
    {
        /// <summary>
        /// The name of this property
        /// </summary>
        public abstract String Name { get; internal set; }

        internal abstract void SetBoxedValue(object box);

        /// <summary>
        /// The type this property contains
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Set this property to default values
        /// </summary>
        /// <summary>
        /// Set the value to the default value and remove all events from PropertyChanged
        /// </summary>
        public abstract void Clear();
    }

    public abstract class BaseProperty<T> : Property
    {
    }

    /// <summary>
    /// A generically typed property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Property<T>
        : BaseProperty<T>
    {
        /// <summary>
        /// The name of this instance
        /// </summary>
        public override String Name { get; internal set; }

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

        internal override void SetBoxedValue(object box)
        {
            Value = (T)box;
        }

        /// <summary>
        /// Called after the value of this property is changed
        /// </summary>
        public event PropertySetDelegate<T> PropertySet;

        public override Type Type
        {
            get { return typeof(T); }
        }

        public override void Clear()
        {
            if (PropertySet != null)
                foreach (var item in PropertySet.GetInvocationList())
                    PropertySet -= (PropertySetDelegate<T>)item;

            value = default(T);
        }

        public Property()
        {
            this.value = default(T);
        }

        private void OnValueSet(T oldValue)
        {
            if (PropertySet != null)
                PropertySet(this, oldValue, Value);
        }

        public override string ToString()
        {
            if (Value == null)
                return "null";
            return Value.ToString();
        }
    }
}