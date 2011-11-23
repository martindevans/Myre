using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Myre.Entities
{
    public delegate void PropertyChangedDelegate(IProperty property);
    public delegate void PropertyChangedDelegate<T>(Property<T> property);

    public interface IProperty
    {
        string Name { get; }

        object Value { get; set; }

        Type Type { get; }

        event PropertyChangedDelegate PropertyChanged;

        /// <summary>
        /// Set the value to the default value and remove all events from PropertyChanged
        /// </summary>
        void Clear();
    }

    public sealed class Property<T>
        : IProperty
    {
        public string Name { get; private set; }

        private T value;
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                OnValueChanged();
            }
        }

        private event PropertyChangedDelegate propertyChanged;

        public event PropertyChangedDelegate<T> PropertyChanged;

        object IProperty.Value
        {
            get { return Value; }
            set { Value = (T)value; }
        }

        event PropertyChangedDelegate IProperty.PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        Type IProperty.Type
        {
            get { return typeof(T); }
        }

        void IProperty.Clear()
        {
            if (propertyChanged != null)
                foreach (var item in propertyChanged.GetInvocationList())
                    propertyChanged -= (PropertyChangedDelegate)item;

            if (PropertyChanged != null)
                foreach (var item in PropertyChanged.GetInvocationList())
                    PropertyChanged -= (PropertyChangedDelegate<T>)item;

            value = default(T);
        }

        public Property(string name)
        {
            this.Name = name;
            this.value = default(T);
        }

        private void OnValueChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this);

            if (propertyChanged != null)
                propertyChanged(this);
        }

        public override string ToString()
        {
            if (Value == null)
                return "null";
            return Value.ToString();
        }
    }

    /*
    /// <summary>
    /// An interface which defines an entity property.
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets or sets the value of this instance.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; set; }

        /// <summary>
        /// Gets the type of data this property contains.
        /// </summary>
        /// <value>The type.</value>
        Type Type { get; }

        /// <summary>
        /// Gets the copy behaviour.
        /// </summary>
        /// <value>The copy behaviour.</value>
        PropertyCopyBehaviour CopyBehaviour { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this propertys' value should be serialised.
        /// </summary>
        /// <value><c>true</c> if this propertys' value should be serialised; otherwise, <c>false</c>.</value>
        bool SerialiseValue { get; set; }
    }

    /// <summary>
    /// A class which contains a single piece of named data.
    /// </summary>
    /// <typeparam name="T">The type of data to store.</typeparam>
    public class Property<T>
        : IProperty
    {
        private PropertyCopyBehaviour copyBahaviour;
        private string name;
        private bool serialiseValue;
        
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this propertys' value should be serialised.
        /// </summary>
        /// <value><c>true</c> if this propertys' value should be serialised; otherwise, <c>false</c>.</value>
        public bool SerialiseValue
        {
            get { return serialiseValue; }
            set { serialiseValue = value; }
        }

        /// <summary>
        /// The value this property contains.
        /// </summary>
        public T Value;

        #region IProperty Members
        object IProperty.Value
        {
            get { return Value; }
            set { Value = (T)value; }
        }
        Type IProperty.Type
        {
            get { return typeof(T); }
        }
        PropertyCopyBehaviour IProperty.CopyBehaviour
        {
            get { return copyBahaviour; }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Property&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Property(string name)
            : this(name, default(T), PropertyCopyBehaviour.None, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="copyBehaviour">The copy behaviour.</param>
        public Property(string name, T value, PropertyCopyBehaviour copyBehaviour, bool serialise)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            
            this.name = name;
            this.Value = value;
            this.copyBahaviour = copyBehaviour;
            this.SerialiseValue = serialise;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name + ": " + ((Value == null) ? "null" : Value.ToString());
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Myre.Entities.Property&lt;T&gt;"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator T(Property<T> property)
        {
            return property.Value ;
        }
    }
 * */
}