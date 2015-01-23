using System.Collections.Concurrent;

namespace Myre.Collections
{
    /// <summary>
    /// Maintains pool of class instances.
    /// </summary>
    /// <typeparam name="T"> The type of object to store. It must define a parameterless constructor</typeparam>
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

        private readonly ConcurrentStack<T> _items;
        private readonly bool _recycleable;

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
            _items = new ConcurrentStack<T>();
            for (int i = 0; i < initialCapacity; i++)
                _items.Push(new T());

            _recycleable = typeof(IRecycleable).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> from the <see cref="Pool&lt;T&gt;"/>
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public T Get()
        {
            T item;
            if (!_items.TryPop(out item))
            {
                item = new T();
            }
            else if (_recycleable)
            {
                var recycleable = item as IRecycleable;
                if (recycleable != null)
                    recycleable.Recycle();
            }

            return item;
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
