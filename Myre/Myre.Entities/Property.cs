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
        Enum Name { get; }

        object Value { get; set; }

        Type Type { get; }

        event PropertyChangedDelegate PropertyChanged;

        void Clear();
    }

    public sealed class Property<T>
        : IProperty
    {
        public Enum Name { get; private set; }

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
            value = default(T);
        }

        public Property(Enum name)
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
            return Value.ToString();
        }
    }
}