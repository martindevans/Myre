using System;

namespace Myre.Entities
{
    public delegate void PropertySetDelegate<T>(Property<T> property, T oldValue, T newValue);

    /// <summary>
    /// Base class for generically typed properties. Do not directly implement this class, instead use Property&lt;TData&gt;
    /// </summary>
    public abstract class BaseUntypedProperty
    {
        /// <summary>
        /// The name of this property
        /// </summary>
        public abstract String Name { get; internal set; }

        /// <summary>
        /// Convert a boxed object into a value for this property and assign it. Throw a cast exception if box cannot be converted
        /// </summary>
        /// <param name="box"></param>
        internal abstract void SetBoxedValue(object box);

        /// <summary>
        /// The type this property contains
        /// </summary>
        public abstract Type DataType { get; }

        /// <summary>
        /// Set this property to default values
        /// </summary>
        /// <summary>
        /// Set the value to the default value and remove all events from PropertyChanged
        /// </summary>
        public abstract void Clear();
    }

    /// <summary>
    /// Base class for all properties
    /// </summary>
    /// <typeparam name="TData">The type of the data contained within this property</typeparam>
    public abstract class Property<TData> : BaseUntypedProperty
    {
        public abstract TData Value { get; set; }

        internal override void SetBoxedValue(object box)
        {
            Value = (TData)box;
        }

        /// <summary>
        /// The type of the data contained within this property
        /// </summary>
        public override Type DataType
        {
            get { return typeof(TData); }
        }

        /// <summary>
        /// Called after the value of this property is changed
        /// </summary>
        public event PropertySetDelegate<TData> PropertySet;

        /// <summary>
        /// Trigger the property set event
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected void TriggerPropertySet(TData oldValue, TData newValue)
        {
            if (PropertySet != null)
                PropertySet(this, oldValue, newValue);
        }

        public override void Clear()
        {
            if (PropertySet != null)
                foreach (var item in PropertySet.GetInvocationList())
                    PropertySet -= (PropertySetDelegate<TData>)item;

            Value = default(TData);
        }
    }

    /// <summary>
    /// A generically typed property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultProperty<T>
        : Property<T>
    {
        /// <summary>
        /// The name of this instance
        /// </summary>
        public override String Name { get; internal set; }

        private T _value;
        /// <summary>
        /// The value of this property
        /// </summary>
        public override T Value
        {
            get
            {
                return _value;
            }
            set
            {
                var oldValue = _value;
                _value = value;
                TriggerPropertySet(oldValue, value);
            }
        }

        public DefaultProperty()
        {
            _value = default(T);
        }

        public override string ToString()
        {
// ReSharper disable CompareNonConstrainedGenericWithNull
            if (Value == null)
// ReSharper restore CompareNonConstrainedGenericWithNull
                return "null";
            return Value.ToString();
        }
    }
}