using System;
using System.Collections.Generic;
using System.Linq;
using Myre.Collections;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Ninject;

namespace Myre.Entities
{
    /// <summary>
    /// A class which collects together entities, behaviour managers, and services.
    /// </summary>
    public class Scene
        : IDisposableObject
    {
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
        public IReadOnlyList<Entity> Entities { get { return _entities; } }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IEnumerable<IService> Services
        {
            get { return _services; }
        }

        /// <summary>
        /// A collection of diagnostic data about service execution time
        /// </summary>
        public IReadOnlyList<KeyValuePair<IService, TimeSpan>> ServiceExecutionTimes
        {
            get { return _services.ExecutionTimes; }
        }

        /// <summary>
        /// Gets the managers.
        /// </summary>
        /// <value>The managers.</value>
        public IEnumerable<IBehaviourManager> Managers
        {
            get { return _managers; }
        }

        /// <summary>
        /// Gets the Ninject kernel used to instantiate services and behaviour managers.
        /// </summary>
        public IKernel Kernel { get; private set; }

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

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="initialisationData">Data to initialise the entity with.</param>
        public void Add(Entity entity, INamedDataProvider initialisationData = null)
        {
            if (entity.Scene != null)
                throw new InvalidOperationException("Cannot add an entity to a scene if it is in a scene already");

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
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the entity was removed; else <c>false</c>.</returns>
        internal void Remove(Entity entity)
        {
            var index = _entities.IndexOf(entity);
            if (index != -1)
            {
                foreach (var behaviour in entity.Behaviours)
                {
                    if (behaviour.CurrentManager.Handler != null)
                        behaviour.CurrentManager.Handler.Remove(behaviour);
                }

                bool removeNow = entity.Shutdown();

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
            IBehaviourManager manager;
            if (_managers.TryGet(managerType, out manager))
                return manager;

            manager = (IBehaviourManager)Kernel.Get(managerType);

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
            IService service;
            if (_services.TryGet(serviceType, out service))
                return service;
            
            if (!typeof(IService).IsAssignableFrom(serviceType))
                throw new ArgumentException("serviceType is not an IService.");

            service = (IService)Kernel.Get(serviceType);
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
            return GetService(typeof(T)) as T;
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

            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                if (_entities[i].IsDisposed)
                    Remove(_entities[i]);
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
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposeManagedResources)
            {
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

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Scene"/> is reclaimed by garbage collection.
        /// </summary>
        ~Scene()
        {
            Dispose(true);
        }
    }
}
