using System;
using System.Collections.Generic;
using System.Linq;

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
            get { return typeof(T); }
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

    /// <summary>
    /// A dictionary, mapping keys to boxed values.
    /// </summary>
    /// <typeparam name="Key">The type of the Key.</typeparam>
    public class BoxedValueStore<Key>
        : IEnumerable<KeyValuePair<Key, IBox>>
    {
        /// <summary>
        /// 
        /// </summary>
        public struct TypedKey
        {
            /// <summary>
            /// 
            /// </summary>
            public readonly Key Key;

            /// <summary>
            /// 
            /// </summary>
            // This struct acts as a key to a dictionary, so the presence of this field is important
            // ReSharper disable once NotAccessedField.Global
            public readonly Type Type;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="key"></param>
            /// <param name="type"></param>
            public TypedKey(Key key, Type type)
            {
                Key = key;
                Type = type;
            }
        }

        private readonly Dictionary<TypedKey, IBox> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxedValueStore&lt;Key&gt;"/> class.
        /// </summary>
// ReSharper disable MemberCanBeProtected.Global
        public BoxedValueStore()
// ReSharper restore MemberCanBeProtected.Global
        {
            _values = new Dictionary<TypedKey, IBox>();
        }

        /// <summary>
        /// Determines whether a value exists at the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified key]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(Key key, Type type)
        {
            return _values.ContainsKey(new TypedKey(key, type));
        }

        /// <summary>
        /// Tries the value at the sspecified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGet<T>(Key key, out Box<T> value)
        {
            IBox box;
            if (_values.TryGetValue(new TypedKey(key, typeof(T)), out box))
            {
                value =  box as Box<T>;
                return value != null;
            }

            value = null;
            return false;
        }
        
        /// <summary>
        /// Gets the value at the specified key.
        /// </summary>
        /// <typeparam name="T">The type of value stored at the key.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="create">Indicates if the box should be created if it is not found.</param>
        /// <returns>The value at the specified key, or null if the existing box contains a different value type.</returns>
        public Box<T> Get<T>(Key key, T defaultValue = default(T), bool create = true)
        {
            IBox box = Get(key, typeof(T));
            if (box as Box<T> != null)
                return (Box<T>)box;
            
            if (!create)
                return null;

            var value = new Box<T> { Value = defaultValue };
            _values[new TypedKey(key, typeof(T))] = value;
            return value;
        }

        /// <summary>
        /// Get an untyped box (if it already exists)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IBox Get(Key key, Type type)
        {
            IBox box;
            if (_values.TryGetValue(new TypedKey(key, type), out box))
                return box;
            return null;
        }

        /// <summary>
        /// Adds the specified value to this container.
        /// </summary>
        /// <typeparam name="T">c</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The box containing the value at the specified key.</returns>
        public void Set<T>(Key key, T value)
        {
            var box = Get<T>(key);
            if (box == null)
                throw new InvalidOperationException("The value at key " + key + " is of the wrong type");

            box.Value = value;
        }

        /// <summary>
        /// Adds the specified value to this container.
        /// </summary>
        /// <param name="key">The value.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type of value to add.</param>
        public void Set(Key key, object value, Type type)
        {
            var box = Get(key, type);
            if (box != null && !box.Type.IsAssignableFrom(type))
                throw new InvalidOperationException("The value at key " + key + " is of the wrong type");

            if (box == null)
            {
                box = (IBox)Activator.CreateInstance(typeof(Box<>).MakeGenericType(type));
                _values.Add(new TypedKey(key, type), box);
            }

            box.Value = value;
        }

        /// <summary>
        /// Enumerate all the values in this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<Key, IBox>> GetEnumerator()
        {
            return _values.Select(a => new KeyValuePair<Key, IBox>(a.Key.Key, a.Value)).GetEnumerator();
        }

        /// <summary>
        /// Enumerate this collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
