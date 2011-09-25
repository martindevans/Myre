using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Services;
using Ninject;
using Ninject.Parameters;
using Myre.Entities.Behaviours;
using Myre;
using System.Collections.ObjectModel;
using Myre.Collections;

namespace Myre.Entities
{
    /// <summary>
    /// A class which collects together entities, behaviour managers, and services.
    /// </summary>
    public class Scene
        : IDisposableObject
    {
        private static Dictionary<Type, Type> defaultManagers = new Dictionary<Type, Type>();
        
        private ServiceContainer services;
        private BehaviourManagerContainer managers;
        private List<Entity> entities;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a read only collection of the entities contained in this scene.
        /// </summary>
        /// <value>The entities.</value>
        public ReadOnlyCollection<Entity> Entities { get; private set; }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public IEnumerable<IService> Services { get { return services; } }

        /// <summary>
        /// Gets the managers.
        /// </summary>
        /// <value>The managers.</value>
        public IEnumerable<IBehaviourManager> Managers { get { return managers; } }

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
            this.services = new ServiceContainer();
            this.managers = new BehaviourManagerContainer();
            this.entities = new List<Entity>();
            this.Kernel = kernel ?? NinjectKernel.Instance; //new ChildKernel(kernel ?? NinjectKernel.Instance);
            this.Entities = new ReadOnlyCollection<Entity>(entities);

            //this.Kernel.Bind<Scene>().ToConstant(this);
        }

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Add(Entity entity, INamedDataProvider initialisationData = null)
        {
            if (entity.Scene != null)
                throw new InvalidOperationException("Cannot add an entity to a scene if it is in a scene already");

            entity.Scene = this;
            entity.Initialise(initialisationData);

            foreach (var behaviour in entity.Behaviours)
            {
                var managerType = SearchForDefaultManager(behaviour.GetType());
                var manager = (managerType != null) ? GetManager(managerType) : null;
                var handler = managers.Find(behaviour.GetType(), manager);

                if (handler != null)
                    handler.Add(behaviour);
            }

            entities.Add(entity);
        }

        private Type SearchForDefaultManager(Type behaviourType)
        {
            Type managerType = null;
            if (defaultManagers.TryGetValue(behaviourType, out managerType))
                return managerType;

            var attributes = behaviourType.GetCustomAttributes(typeof(DefaultManagerAttribute), false);
            if (attributes.Length > 0)
            {
                var attribute = attributes[0] as DefaultManagerAttribute;
                managerType = attribute.Manager;

                defaultManagers.Add(behaviourType, managerType);
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
        public bool Remove(Entity entity)
        {
            var removed = entities.Remove(entity);

            if (removed)
            {
                foreach (var behaviour in entity.Behaviours)
                {
                    if (behaviour.CurrentManager.Handler != null)
                        behaviour.CurrentManager.Handler.Remove(behaviour);
                }

                entity.Scene = null;
                entity.Shutdown();
            }

            return removed;
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
            if (managers.TryGet(managerType, out manager))
                return manager;

            manager = Kernel.Get(managerType) as IBehaviourManager;

            var behaviourTypes = manager.GetManagedTypes();
            foreach (var type in behaviourTypes)
            {
                if (managers.ContainsForBehaviour(type))
                    throw new InvalidOperationException(string.Format("A manager for {0} already exists.", type));
            }

            managers.Add(manager);
            AddBehavioursToManager(behaviourTypes);

            manager.Initialise(this);

            return manager;
        }

        private void AddBehavioursToManager(IEnumerable<Type> behaviourTypes)
        {
            var behavioursToBeAdded = 
                from behaviourType in behaviourTypes
                let handler = managers.GetByBehaviour(behaviourType)
                from entity in entities
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
            IService service = null;
            if (services.TryGet(serviceType, out service))
                return service;
            
            if (!typeof(IService).IsAssignableFrom(serviceType))
                throw new ArgumentException("serviceType is not an IService.");

            service = Kernel.Get(serviceType) as IService;
            services.Add(service);

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
        public ReadOnlyCollection<T> FindManagers<T>()
        {
            return managers.FindByType<T>();
        }

        /// <summary>
        /// Updates the scene for a single frame.
        /// </summary>
        /// <param name="elapsedTime">The number of seconds which have elapsed since the previous frame.</param>
        public void Update(float elapsedTime)
        {
            services.Update(elapsedTime);

            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (entities[i].IsDisposed)
                    Remove(entities[i]);
            }
        }

        /// <summary>
        /// Draws the scene.
        /// </summary>
        public void Draw()
        {
            services.Draw();
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
                entities.Clear();

                foreach (var manager in managers)
                    manager.Dispose();

                managers.Clear();

                foreach (var service in services)
                    service.Dispose();

                services.Clear();
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
