using System;

namespace Myre.Collections
{
    /// <summary>
    /// A weak reference to a an object of a specific type. The item may still be garbage collected even while a weak reference is held.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypedWeakReference<T> where T : class
    {
        private readonly WeakReference _reference;

        /// <summary>
        /// Gets a value indicating whether this instance is alive.
        /// </summary>
        /// <value><c>true</c> if this instance is alive; otherwise, <c>false</c>.</value>
        public bool IsAlive
        {
            get
            {
                return _reference.IsAlive;
            }
        }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>The target.</value>
        public T Target
        {
            get
            {
                return _reference.Target as T;
            }
            set
            {
                _reference.Target = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object referenced by this weak reference is tracked after finalisation.
        /// </summary>
        /// <value><c>true</c> if the object referenced by this weak reference is tracked after finalisation; otherwise, <c>false</c>.</value>
        public bool TrackResurrection
        {
            get
            {
                return _reference.TrackResurrection;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedWeakReference&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public TypedWeakReference(T target)
        {
            _reference = new WeakReference(target);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedWeakReference&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="trackResurrection">if set to <c>true</c> the object referenced by this weak reference is tracked after finalisation.</param>
        public TypedWeakReference(T target, bool trackResurrection)
        {
            _reference = new WeakReference(target, trackResurrection);
        }
    }
}
