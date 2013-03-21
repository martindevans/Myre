
namespace Myre.StateManagement
{
    /// <summary>
    /// The type of a transition.
    /// </summary>
    public enum TransitionType
    {
        /// <summary>
        /// The new state transitions on at the same time that the
        /// previous state is transitioned off.
        /// </summary>
        CrossFade,

        /// <summary>
        /// The new state transitions on after the previous state
        /// has transitioned off.
        /// </summary>
        Sequential
    }
}
