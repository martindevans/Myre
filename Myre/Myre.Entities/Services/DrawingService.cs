using System;
using System.Collections.Generic;

namespace Myre.Entities.Services
{
    /// <summary>
    /// An interface which defines objects which can draw themselves.
    /// </summary>
    public interface IDrawableObject
    {
        /// <summary>
        /// Gets the draw order.
        /// </summary>
        /// <value>The draw order.</value>
        int DrawOrder { get; }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        void Draw();
    }

    /// <summary>
    /// A service which draws IDrawable objects.
    /// </summary>
    public class DrawingService
        : List<IDrawableObject>, IService
    {
        private readonly Comparison<IDrawableObject> _comparison;

        /// <summary>
        /// Gets a key on which services are sorted to determine update order.
        /// </summary>
        /// <value></value>
        public int UpdateOrder { get; set; }

        /// <summary>
        /// Gets a key on which services are sorted to determine draw order.
        /// </summary>
        /// <value></value>
        public int DrawOrder {get; set;}

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingService"/> class.
        /// </summary>
        public DrawingService()
        {
            _comparison = (a, b) => a.DrawOrder.CompareTo(b.DrawOrder);
        }

        /// <summary>
        /// Initialises the service
        /// </summary>
        /// <param name="scene">The scene to which this service belongs to.</param>
        public void Initialise(Scene scene)
        {
        }

        /// <summary>
        /// Updates the service for a single frame.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds which have elapsed since the previous frame.</param>
        public void Update(float elapsedTime)
        {
        }

        /// <summary>
        /// Draws the service.
        /// </summary>
        public void Draw()
        {
            Sort(_comparison);
            foreach (var item in this)
                item.Draw();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposeManagedResources"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
// ReSharper disable UnusedParameter.Global
        protected virtual void Dispose(bool disposeManagedResources)
// ReSharper restore UnusedParameter.Global
        {
            IsDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Scene"/> is reclaimed by garbage collection.
        /// </summary>
        ~DrawingService()
        {
            Dispose(true);
        }
    }
}
