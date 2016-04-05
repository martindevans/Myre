using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Myre.Collections;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Ninject;
using Ninject.Syntax;

namespace Myre.Entities
{
    /// <summary>
    /// A class which collects together entities, behaviour managers, and services.
    /// </summary>
    public class Scene
        : IDisposableObject
    {
        #region fields and properties
        private static readonly Dictionary<Type, Type> _defaultManagers = new Dictionary<Type, Type>();

        private readonly ServiceContainer _services;
        private readonly BehaviourManagerContainer _managers;
        private readonly List<Entity> _entities;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a read only collection of the entities contained in this scene.
        /// </summary>
        /// <value>The entities.</value>
        public IReadOnlyList<Entity> Entities
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<Entity>>() != null);
                return _entities;
            }
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IEnumerable<IService> Services
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<IService>>() != null);
                return _services;
            }
        }

        /// <summary>
        /// A collection of diagnostic data about service execution time
        /// </summary>
        public IReadOnlyList<KeyValuePair<IService, TimeSpan>> ServiceExecutionTimes
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyList<KeyValuePair<IService, TimeSpan>>>() != null);
                return _services.ExecutionTimes;
            }
        }

        /// <summary>
        /// Gets the managers.
        /// </summary>
        /// <value>The managers.</value>
        public IEnumerable<IBehaviourManager> Managers
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<IBehaviourManager>>() != null);
                return _managers;
            }
        }

        /// <summary>
        /// Gets the Ninject kernel used to instantiate services and behaviour managers.
        /// </summary>
        public IKernel Kernel { get; private set; }
        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        /// <param name="kernel">The kernel used to instantiate services and behaviours. <c>null</c> for NinjectKernel.Instance.</param>
        public Scene(IKernel kernel = null)
        {
            _services = new ServiceContainer();
            _managers = new BehaviourManagerContainer();
            _entities = new List<Entity>();
            Kernel = kernel ?? NinjectKernel.Instance; //new ChildKernel(kernel ?? NinjectKernel.Instance);

            Kernel.Bind<Scene>().ToConstant(this);
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_services != null);
            Contract.Invariant(_managers != null);
            Contract.Invariant(_entities != null);
        }
        #endregion

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="initialisationData">Data to initialise the entity with.</param>
        public void Add(Entity entity, INamedDataProvider initialisationData = null)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Scene == null);

            entity.Scene = this;
            entity.Initialise(initialisationData);

            foreach (var behaviour in entity.Behaviours)
            {
                var managerType = SearchForDefaultManager(behaviour.GetType());
                var manager = managerType != null ? GetManager(managerType) : null;
                var handler = _managers.Find(behaviour.GetType(), manager);

                if (handler != null)
                    handler.Add(behaviour);
            }

            entity.Initialised();

            _entities.Add(entity);
        }

        private static Type SearchForDefaultManager(Type behaviourType)
        {
            Contract.Requires(behaviourType != null);

            Type managerType;
            lock (_defaultManagers)
            {
                if (_defaultManagers.TryGetValue(behaviourType, out managerType))
                    return managerType;
            }

            var attributes = behaviourType.GetCustomAttributes(typeof(DefaultManagerAttribute), false);
            if (attributes.Length > 0)
            {
                var attribute = (DefaultManagerAttribute)attributes[0];
                managerType = attribute.Manager;

                lock (_defaultManagers)
                {
                    _defaultManagers.Add(behaviourType, managerType);
                }
                return managerType;
            }

            if (behaviourType.BaseType != null)
                return SearchForDefaultManager(behaviourType.BaseType);

            return null;
        }

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="index">The index of entity to remove</param>
        /// <returns><c>true</c> if the entity was removed; else <c>false</c>.</returns>
        private void RemoveAt(int index)
        {
            if (index >= _entities.Count)
                throw new ArgumentOutOfRangeException("index", string.Format("Cannot remove entity at index {0}, there are only {1} entities in the scene", index, _entities.Count));

            if (index != -1)
            {
                var entity = _entities[index];

                foreach (var behaviour in entity.Behaviours)
                {
                    if (behaviour.CurrentManager.Handler != null)
                        behaviour.CurrentManager.Handler.Remove(behaviour);
                }

                var removeNow = entity.Shutdown();
                if (removeNow)
                {
                    entity.Scene = null;
                    _entities.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Gets the manager of the specified type from this sene, or creates one
        /// if one does not already exist.
        /// </summary>
        /// <param name="managerType">Type of the manager.</param>
        /// <returns></returns>
        public IBehaviourManager GetManager(Type managerType)
        {
            Contract.Requires(managerType != null);
            Contract.Ensures(Contract.Result<IBehaviourManager>() != null);

            IBehaviourManager manager;
            if (_managers.TryGet(managerType, out manager))
                return manager;

            manager = (IBehaviourManager)Kernel.Get(managerType);
            Contract.Assume(manager != null);

            var behaviourTypes = manager.GetManagedTypes();
            foreach (var type in behaviourTypes)
            {
                if (_managers.ContainsForBehaviour(type))
                    throw new InvalidOperationException(string.Format("A manager for {0} already exists.", type));
            }

            _managers.Add(manager);
            AddBehavioursToManager(behaviourTypes);

            manager.Initialise(this);

            return manager;
        }

        private void AddBehavioursToManager(IEnumerable<Type> behaviourTypes)
        {
            Contract.Requires(behaviourTypes != null);

            var behavioursToBeAdded = 
                from behaviourType in behaviourTypes
                let handler = _managers.GetByBehaviour(behaviourType)
                from entity in _entities
                from behaviour in entity.Behaviours
                let type = behaviour.GetType()
                where
                    // this manager can manage the behaviour
                    behaviourType.IsAssignableFrom(type)
                    // and either there is no current manager, or (this manager is more derived than the current one, and there is no default manager).
                    && (behaviour.CurrentManager.Handler == null || (!behaviourType.IsAssignableFrom(behaviour.CurrentManager.ManagedAs) && SearchForDefaultManager(type) == null))
                select new { Handler = handler, Behaviour = behaviour };

            foreach (var item in behavioursToBeAdded)
                item.Handler.Add(item.Behaviour);
        }

        /// <summary>
        /// Gets the manager of the specified type from this sene, or creates one
        /// if one does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of the manager.</typeparam>
        /// <returns></returns>
        public T GetManager<T>()
            where T : class, IBehaviourManager
        {
            return GetManager(typeof(T)) as T;
        }

        /// <summary>
        /// Gets the service of the specified type from this scene, or creates one
        /// if one does not already exist.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public IService GetService(Type serviceType)
        {
            Contract.Requires(serviceType != null);
            Contract.Ensures(Contract.Result<IService>() != null);

            IService service;
            if (_services.TryGet(serviceType, out service))
                return service;
            
            if (!typeof(IService).IsAssignableFrom(serviceType))
                throw new ArgumentException("serviceType is not an IService.");

            service = (IService)Kernel.Get(serviceType);
            Contract.Assume(service != null);
            _services.Add(service);

            service.Initialise(this);

            return service;
        }

        /// <summary>
        /// Gets the service of the specified type from this sene, or creates one
        /// if one does not already exist.
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <returns></returns>
        public T GetService<T>()
            where T : class, IService
        {
            Contract.Ensures(Contract.Result<T>() != null);

            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// Gets a collection of managers which derive from type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of manager to search for.</typeparam>
        /// <returns></returns>
        public IReadOnlyList<T> FindManagers<T>()
        {
            return _managers.FindByType<T>();
        }

        /// <summary>
        /// Updates the scene for a single frame.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds which have elapsed since the previous frame.</param>
        public void Update(float elapsedTime)
        {
            _services.Update(elapsedTime);

            for (var i = _entities.Count - 1; i >= 0; i--)
            {
                var e = _entities[i];
                Contract.Assume(e != null);
                if (e.IsDisposed)
                    RemoveAt(i);
            }
        }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        public void Draw()
        {
            _services.Draw();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            Kernel.Unbind<Scene>();

            _entities.Clear();

            foreach (var manager in _managers)
                manager.Dispose();
            _managers.Clear();

            foreach (var service in _services)
                service.Dispose();
            _services.Clear();
        }
    }
}
