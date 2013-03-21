using System;

namespace Myre
{
    /// <summary>
    /// An object that can report whether or not it is disposed.
    /// </summary>
    public interface IDisposableObject : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        bool IsDisposed { get; }
    }
}
