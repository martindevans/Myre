using System.Collections.Concurrent;

namespace Myre.Collections
{
    /// <summary>
    /// Maintains pool of class instances. Completely threadsafe.
    /// If a request is made for an item when the pool is empty a new item will be constructed
    /// </summary>
    /// <typeparam name="T"> The type of object to store. It must define a parameterless constructor</typeparam>
    public class Pool<T> where T : class, new()
    {
        #region fields and properties
        /// <summary>
        /// Gets the static instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Pool<T> Instance { get; } = new();

        /// <summary>
        /// Maximum number of items to keep in this pool (0 or less will be interpreted as infinite capacity)
        /// </summary>
        public int Capacity { get; set; }

        private readonly ConcurrentStack<T> _items;
        private readonly bool _recycleable;
        #endregion

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
            // ReSharper disable once IntroduceOptionalParameters.Global (Justification: this would be a breaking change)
            : this(initialCapacity, -1)
        {   
        }

        public Pool(int initialCapacity, int maxCapacity)
        {
            _items = new ConcurrentStack<T>();
            for (var i = 0; i < initialCapacity; i++)
                _items.Push(new T());

            Capacity = maxCapacity;
            _recycleable = typeof(IRecycleable).IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> from the <see cref="Pool&lt;T&gt;"/>
        /// </summary>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public T Get()
        {
            if (!_items.TryPop(out T item))
            {
                item = new T();
            }
            else if (_recycleable)
            {
                if (item is IRecycleable recycleable)
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
            if (Capacity < 1 || _items.Count < Capacity)
                _items.Push(item);
        }
    }
}
