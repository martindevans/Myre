using System;
using Myre.Collections;

namespace Myre.Entities.Behaviours
{
    /// <summary>
    /// An abstract class which represents specific functionality which an Entity can perform.
    /// </summary>
    /// <remarks>
    /// <para>Behaviours effectively tag an entity as performing some task. They may contain private working data, and gather
    /// references to required properties. They should not contain logic, as that is handled by the behaviours' manager.</para> 
    /// <para>Each Scene has a behaviour manager for each type of behaviour. 
    /// This manager performs any updating or drawing for all behaviours of the relevant type.</para>
    /// </remarks>
    public abstract class Behaviour
    {
        internal struct ManagerBinding
        {
            public IManagerHandler Handler;
            public Type ManagedAs;
        }

        /// <summary>
        /// Gets the name of this behaviour.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the owner of this behaviour.
        /// </summary>
        public Entity Owner { get; internal set; }

        /// <summary>
        /// Gets a value indicating if this behaviour has been initialised.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Gets the manager this behaviour belongs to.
        /// </summary>
        internal ManagerBinding CurrentManager { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behaviour"/> class.
        /// </summary>
        protected Behaviour()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behaviour"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        protected Behaviour(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        /// <param name="initialisationData">
        /// Initialisation context. This object can be used to query properties and behaviours.
        /// </param>
        /// <remarks>
        /// Initialise/Shutdown may be called multiple times, as the instance is recycled.
        /// Here the behaviour should do any setup needed to put the behaviour into its' initial state, including getting optional properties from the entity which may have been created by other behaviours, and register to any services.
        /// Initialise is called before the behaviour is added to the manager.
        /// </remarks>
        public virtual void Initialise(INamedDataProvider initialisationData)
        {
            IsReady = true;
        }

        /// <summary>
        /// Indicates that this instance has all been initialised
        /// </summary>
        protected internal virtual void Initialised()
        {
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        /// <param name="context">
        /// Initialisation context. This object can be used to publish properties to the owning entity.
        /// </param>
        /// <remarks>
        /// CreatePropeties is called once when the entity is constructed.
        /// Here the behaviour should create any properties required by this behaviour to function.
        /// Create properties is called before Initialise.
        /// </remarks>
        public virtual void CreateProperties(Entity.ConstructionContext context)
        {

        }

        /// <summary>
        /// Shuts down this instance.
        /// </summary>
        /// <remarks>
        /// Initialise/Shutdown may be called multiple times, as the instance is recycled.
        /// Shutdown is called after the behaviour has been removed from the manager.
        /// </remarks>
        public virtual void Shutdown(INamedDataProvider shutdownData)
        {
            IsReady = false;
        }

        public string GetFullPropertyName(string propertyName)
        {
            return Name == null ? propertyName : string.Format("{0}_{1}", propertyName, Name);
        }
    }
}
