using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Myre.Collections;
using Myre.Entities.Behaviours;

namespace Myre.Entities
{
    public struct EntityVersion
    {
        public static readonly EntityVersion None = new EntityVersion(null, 0);

        public EntityDescription Creator { get; private set; }
        public uint Version { get; private set; }

        public EntityVersion(EntityDescription creator, uint version)
            : this()
        {
            Creator = creator;
            Version = version;
        }
    }

    /// <summary>
    /// A class which represents a collection of related properties and behaviours.
    /// </summary>
    public class Entity
        : IDisposableObject, IRecycleable
    {
        public sealed class ConstructionContext
        {
            private readonly Entity _entity;
            internal bool Frozen;

            internal ConstructionContext(Entity entity)
            {
                _entity = entity;
            }

            public Property<T> CreateProperty<T>(String name, T value = default(T))
            {
                CheckFrozen();

                var property = _entity.GetProperty<T>(name);
                if (property == null)
                {
                    property = new Property<T>(name) {Value = value};
                    _entity.AddProperty(property);
                }

                return property;
            }

            private void CheckFrozen()
            {
                if (Frozen)
                    throw new InvalidOperationException("Entity initialisation contexts can only be used during initialisation.");
            }
        }

        private readonly Dictionary<String, IProperty> _properties;
        private readonly Dictionary<Type, Behaviour[]> _behaviours;

        private readonly List<IProperty> _propertiesList;
        private readonly List<Behaviour> _behavioursList;

        private readonly ConstructionContext _constructionContext;

        public EntityVersion Version { get; private set; }

        /// <summary>
        /// Gets the scene this entity belongs to.
        /// </summary>
        /// <value>The scene.</value>
        public Scene Scene { get; internal set; }

        /// <summary>
        /// Gets the behaviours this entity contains.
        /// </summary>
        /// <value>The behaviours.</value>
        public ReadOnlyCollection<Behaviour> Behaviours { get; private set; }

        /// <summary>
        /// Gets the properties this entity contains.
        /// </summary>
        /// <value>The properties.</value>
        public ReadOnlyCollection<IProperty> Properties { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        private INamedDataProvider _shutdownData;

        /// <summary>
        /// Gets a value indicating whether behaviours have already been shutdown
        /// </summary>
        internal bool BehavioursShutdown { get; private set; }

        internal Entity(IEnumerable<IProperty> properties, IEnumerable<Behaviour> behaviours, EntityVersion version)
        {
            Version = version;

            // create public read-only collections
            _propertiesList = new List<IProperty>(properties);
            _behavioursList = new List<Behaviour>(behaviours);
            Properties = new ReadOnlyCollection<IProperty>(_propertiesList);
            Behaviours = new ReadOnlyCollection<Behaviour>(_behavioursList);

            // add properties
            _properties = new Dictionary<String, IProperty>();
            foreach (var item in Properties)
                _properties.Add(item.Name, item);

            // sort behaviours by their type
            var catagorised = new Dictionary<Type, List<Behaviour>>();
            foreach (var item in Behaviours)
            {
                CatagoriseBehaviour(catagorised, item);
                item.Owner = this;
            }

            // add behaviours
            _behaviours = new Dictionary<Type, Behaviour[]>();
            foreach (var item in catagorised)
                _behaviours.Add(item.Key, item.Value.ToArray());

            // create initialisation context
            _constructionContext = new ConstructionContext(this);

            // allow behaviours to add their own properties
            CreateProperties();
        }

        private void CatagoriseBehaviour(Dictionary<Type, List<Behaviour>> catagorised, Behaviour behaviour)
        {
            Type type = behaviour.GetType();
            Debug.Assert(type != null);
            do
            {
                LazyGetCategoryList(type, catagorised).Add(behaviour);

                type = type.BaseType;
                Debug.Assert(type != null, "type != null");
            }
            while (type != typeof(Behaviour).BaseType);
        }

        private List<Behaviour> LazyGetCategoryList(Type type, Dictionary<Type, List<Behaviour>> catagorised)
        {
            List<Behaviour> behavioursOfType;
            if (!catagorised.TryGetValue(type, out behavioursOfType))
            {
                behavioursOfType = new List<Behaviour>();
                catagorised.Add(type, behavioursOfType);
            }

            return behavioursOfType;
        }

        private void CreateProperties()
        {
            _constructionContext.Frozen = false;

            foreach (var item in Behaviours)
            {
                item.CreateProperties(_constructionContext);
            }

            _constructionContext.Frozen = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Entity"/> is reclaimed by garbage collection.
        /// </summary>
        ~Entity()
        {
            Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose(INamedDataProvider shutdownData)
        {
            _shutdownData = shutdownData;
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(null);
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
        /// Initialises this instance.
        /// </summary>
        internal void Initialise(INamedDataProvider initialisationData)
        {
            BehavioursShutdown = false;
            IsDisposed = false;
            _shutdownData = null;

            foreach (var item in Behaviours)
            {
                if (!item.IsReady)
                    item.Initialise(initialisationData);
            }

            foreach (var item in Behaviours)
            {
                item.Initialised();
            }
        }

        /// <summary>
        /// Shuts down this instance.
        /// </summary>
        internal bool Shutdown()
        {
            _delayPropertyShutdown = false;

            if (!BehavioursShutdown)
            {
                foreach (var item in Behaviours)
                {
                    if (item.IsReady)
                        item.Shutdown(_shutdownData);
                }
                BehavioursShutdown = true;
            }

            if (!_delayPropertyShutdown)
            {
                foreach (var item in Properties)
                {
                    item.Clear();
                }
            }

            return !_delayPropertyShutdown;
        }

        bool _delayPropertyShutdown = false;
        /// <summary>
        /// Delays shutting down the properties of this entity by 1 frame
        /// </summary>
        public void DelayPropertyShutdown()
        {
            _delayPropertyShutdown = true;
        }

        /// <summary>
        /// Prepares this instance for re-use.
        /// </summary>
        public void Recycle()
        {
            if (Scene != null)
                Scene.Remove(this);

            if (Version.Creator != null)
                Version.Creator.Recycle(this);
        }

        internal void AddProperty(IProperty property)
        {
            _properties.Add(property.Name, property);
            _propertiesList.Add(property);
        }

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <param name="name">The name of the propery.</param>
        /// <returns>The property with the specified name and data type.</returns>
        public IProperty GetProperty(String name)
        {
            IProperty property;
            _properties.TryGetValue(name, out property);
            return property;
        }

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <typeparam name="T">The data type this property contains.</typeparam>
        /// <param name="name">The name of the propery.</param>
        /// <returns>The property with the specified name and data type.</returns>
        public Property<T> GetProperty<T>(String name)
        {
            return GetProperty(name) as Property<T>;
        }

        /// <summary>
        /// Gets the behaviour of the specified type and name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Behaviour GetBehaviour(Type type, string name = null)
        {
            Behaviour[] array;
            if (_behaviours.TryGetValue(type, out array))
            {
// ReSharper disable LoopCanBeConvertedToQuery
                foreach (var item in array)
// ReSharper restore LoopCanBeConvertedToQuery
                {
                    if (item.Name == name)
                        return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the behaviours of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
// ReSharper disable ReturnTypeCanBeEnumerable.Global
        public Behaviour[] GetBehaviours(Type type)
// ReSharper restore ReturnTypeCanBeEnumerable.Global
        {
            Behaviour[] array;
            _behaviours.TryGetValue(type, out array);

            return array;
        }

        /// <summary>
        /// Gets the behaviour of the specified type and name.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public T GetBehaviour<T>(string name = null)
            where T : Behaviour
        {
            return GetBehaviour(typeof(T), name) as T;
        }

        /// <summary>
        /// Gets the behaviours of the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns></returns>
        public T[] GetBehaviours<T>()
            where T : Behaviour
        {
            //return GetBehaviours(typeof(T)) as T[];
            var v = GetBehaviours(typeof(T));
            if (v != null)
                return v.Cast<T>().ToArray();
            else
                return new T[0];
        }
    }
}
