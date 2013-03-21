using System;

namespace Myre.Entities.Behaviours
{
    /// <summary>
    /// An attribute which allows a behaviour to specify a default manager, which a scene will
    /// automatically create if it does not already have a manager for this behaviour type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DefaultManagerAttribute : Attribute
    {
        /// <summary>
        /// Gets the manager type.
        /// </summary>
        /// <value>The manager.</value>
        public Type Manager { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultManagerAttribute"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public DefaultManagerAttribute(Type manager)
        {
            if (!typeof(IBehaviourManager).IsAssignableFrom(manager))
                throw new ArgumentException("The type must implement IBehaviourManager", "manager");

            this.Manager = manager;
        }
    }
}
