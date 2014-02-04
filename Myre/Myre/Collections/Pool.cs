using System.Collections.Generic;

namespace Myre.Collections
{
    /// <summary>
    /// Maintains pool of class instances.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object to store. It must define a parameterless constructor, 
    /// and may implement <see cref="IRecycleable"/>.</typeparam>
    public class Pool<T> where T : class, new()
    {
        private static readonly Pool<T> _instance = new Pool<T>();
        /// <summary>
        /// Gets the static instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Pool<T> Instance
        {
            get
            {
                return _instance;
            }
        }

        readonly Stack<T> _items;
        readonly bool _isResetableType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool&lt;T&gt;"/> class.
        /// </summary>
        public Pool()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="initialCapacity">The initial number of elements contained within the <see cref="Pool&lt;T&gt;"/>.</param>
        public Pool(int initialCapacity)
        {
            _isResetableType = typeof(IRecycleable).IsAssignableFrom(typeof(T));
            _items = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
                _items.Push(new T());
        }

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> from the <see cref="Pool&lt;T&gt;"/>
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public T Get()
        {
            if (_items.Count > 0)
            {
                T item = _items.Pop();
                if (_isResetableType)
                {
                    var recycleable = item as IRecycleable;
                    if (recycleable != null)
                    {
                        recycleable.Recycle();
                    }
                }
                return item;
            }

            return new T();
        }

        /// <summary>
        /// Returns the specified item to the <see cref="Pool&lt;T&gt;"/>.
        /// </summary>
        /// <param name="item">The item to be returned.</param>
        public void Return(T item)
        {
            _items.Push(item);
        }
    }
}
