
namespace Myre.Entities.Services
{
    /// <summary>
    /// An interface which defines a process.
    /// A process is updated every frame by a ProcessService, until it is complete.
    /// </summary>
    public interface IProcess
    {
        /// <summary>
        /// Gets a value indicating whether this instance is complete.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        bool IsComplete { get; }

        /// <summary>
        /// Updates the process for a single frame.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds elapsed since the last frame.</param>
        void Update(float elapsedTime);
    }
}
