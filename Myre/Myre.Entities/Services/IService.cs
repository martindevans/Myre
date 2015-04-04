using System;

namespace Myre.Entities.Services
{
    /// <summary>
    /// An interface defining a service.
    /// </summary>
    /// <remarks>
    /// Services exist within a scene, and provide functionality for behaviour managers to use.
    /// For example, the even service can be used to communicate between managers.
    /// </remarks>
    public interface IService
        : IDisposableObject
    {
        /// <summary>
        /// Gets a key on which services are sorted to determine update order.
        /// </summary>
        int UpdateOrder { get; }

        /// <summary>
        /// Gets a key on which services are sorted to determine draw order.
        /// </summary>
        int DrawOrder { get; }

        /// <summary>
        /// Initialises the service
        /// </summary>
        /// <param name="scene">The scene to which this service belongs to.</param>
        void Initialise(Scene scene);

        /// <summary>
        /// Updates the service for a single frame.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds which have elapsed since the previous frame.</param>
        void Update(float elapsedTime);

        /// <summary>
        /// Draws the service.
        /// </summary>
        void Draw();
    }

    /// <summary>
    /// An abstract class implementation of IService.
    /// </summary>
    public abstract class Service
        : IService
    {
        /// <summary>
        /// Gets or sets a key on which services are sorted to determine update order.
        /// </summary>
        /// <value></value>
        public int UpdateOrder { get; set; }

        /// <summary>
        /// Gets or sets a key on which services are sorted to determine draw order.
        /// </summary>
        /// <value></value>
        public int DrawOrder { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initialises the service
        /// </summary>
        /// <param name="scene">The scene to which this service belongs to.</param>
        public virtual void Initialise(Scene scene)
        {
        }

        /// <summary>
        /// Updates the service for a single frame.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds which have elapsed since the previous frame.</param>
        public virtual void Update(float elapsedTime)
        {
        }

        /// <summary>
        /// Draws the service.
        /// </summary>
        public virtual void Draw()
        {
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
        protected virtual void Dispose(bool disposeManagedResources)
        {
            IsDisposed = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Service"/> is reclaimed by garbage collection.
        /// </summary>
        ~Service()
        {
            Dispose(false);
        }
    }
}
