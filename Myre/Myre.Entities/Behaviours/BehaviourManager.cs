using System;
using System.Collections.Generic;
using System.Linq;

namespace Myre.Entities.Behaviours
{
    /// <summary>
    /// An interface which defines a manger for entity behaviours.
    /// </summary>
    /// <remarks>
    /// <para>Behaviour managers perform all the logic required to implement a specific type of behaviour.
    /// Each scene contains an manager instance for each behaviour type, and these managers batch process all behaviours in the scene.</para>
    /// <para>Managers can utilise the services contained in the scene to register for updates or events.</para>
    /// </remarks>
    public interface IBehaviourManager
        : IDisposableObject
    {
        void Initialise(Scene scene);
    }

    public static class BehaviourManagerExtensions
    {
        //private static Dictionary<Type, Dictionary<Type, MethodInfo>> managerMethods = new Dictionary<Type, Dictionary<Type, MethodInfo>>();
        private static readonly Dictionary<Type, Type[]> _managedTypes = new();

        public static IEnumerable<Type> GetManagedTypes(this IBehaviourManager manager)
        {
            var managerType = manager.GetType();

            if (!_managedTypes.TryGetValue(managerType, out var behaviourTypes))
            {
                var types = from i in managerType.GetInterfaces()
                            where i.FullName.StartsWith(typeof(IBehaviourManager<>).FullName)
                            select i.GetGenericArguments().First();

                behaviourTypes = types.ToArray();
                _managedTypes[managerType] = behaviourTypes;
            }

            return behaviourTypes;
        }
    }

    /// <summary>
    /// An interface which defines a manger for entity behaviours.
    /// </summary>
    /// <remarks>
    /// <para>Behaviour managers perform all the logic required to implement a specific type of behaviour.
    /// Each scene contains an manager instance for each behaviour type, and these managers batch process all behaviours in the scene.</para>
    /// <para>Managers can utilise the services contained in the scene to register for updates or events.</para>
    /// </remarks>
    public interface IBehaviourManager<in T>
        : IBehaviourManager
        where T : Behaviour
    {
        /// <summary>
        /// Adds a behaviour to this manager.
        /// </summary>
        /// <param name="behaviour">The behaviour the manager should begin to manage.</param>
        void Add(T behaviour);

        /// <summary>
        /// Removes a behaviour from this manager.
        /// </summary>
        /// <param name="behaviour">The behaviour this manager should stop managing.</param>
        /// <returns><c>true</c> if the behaviour was removed; else <c>false</c>.</returns>
        bool Remove(T behaviour);
    }

    /// <summary>
    /// An abstract base implementation of a Behaviour Manager.
    /// </summary>
    /// <typeparam name="T">The type of behaviour to be managed.</typeparam>
    public abstract class BehaviourManager<T>
        : IBehaviourManager<T>
        where T : Behaviour
    {
        /// <summary>
        /// Gets a list of behaviours being managed by this instance.
        /// </summary>
        /// <value>The behaviours.</value>
        protected List<T> Behaviours { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the type this manager manages.
        /// </summary>
        /// <value></value>
        public Type BehaviourType => typeof(T);

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviourManager&lt;T&gt;"/> class.
        /// </summary>
        protected BehaviourManager()
        {
            Behaviours = new List<T>();
        }

        public virtual void Initialise(Scene scene)
        {
        }

        /// <summary>
        /// Adds a behaviour to this manager.
        /// </summary>
        /// <param name="behaviour">The behaviour the manager should begin to manage.</param>
        public virtual void Add(T behaviour)
        {
            Behaviours.Add(behaviour);
        }

        /// <summary>
        /// Removes a behaviour from this manager.
        /// </summary>
        /// <param name="behaviour">The behaviour this manager should stop managing.</param>
        /// <returns>
        /// 	<c>true</c> if the behaviour was removed; else <c>false</c>.
        /// </returns>
        public virtual bool Remove(T behaviour)
        {
            return Behaviours.Remove(behaviour);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposeManagedResources"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
// ReSharper disable VirtualMemberNeverOverriden.Global
        protected virtual void Dispose(bool disposeManagedResources)
// ReSharper restore VirtualMemberNeverOverriden.Global
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposeManagedResources)
            {
                var behaviours = Behaviours;
                for (var i = behaviours.Count - 1; i >= 0; i--)
                {
                    var b = behaviours[i];
                    Remove(b);
                }
            }
        }
    }
}
