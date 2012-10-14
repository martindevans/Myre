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
    public sealed class Entity
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

            public TProperty CreateProperty<TProperty, TData>(String name = null, TData value = default(TData)) where TProperty : Property<TData>, new()
            {
                CheckFrozen();

                var property = _entity.GetProperty<TProperty, TData>(name);
                if (property == null)
                {
                    property = new TProperty { Name = name, Value = value };
                    _entity.AddProperty<TProperty, TData>(property);
                }

                return property;
            }
            
            /// <summary>
            /// Create a new property on this entity with the default property type
            /// </summary>
            /// <typeparam name="TProperty">Type of the data in this property</typeparam>
            /// <param name="name">name of this property</param>
            /// <param name="value">default value for this property</param>
            /// <returns></returns>
            public Property<TProperty> CreateProperty<TProperty>(String name, TProperty value = default(TProperty))
            {
                return CreateProperty<DefaultProperty<TProperty>, TProperty>(name, value);
            }

            private void CheckFrozen()
            {
                if (Frozen)
                    throw new InvalidOperationException("Entity initialisation contexts can only be used during initialisation.");
            }
        }

        private readonly Dictionary<KeyValuePair<String, Type>, BaseUntypedProperty> _properties;
        private readonly Dictionary<Type, Behaviour[]> _behaviours;

        private readonly List<BaseUntypedProperty> _propertiesList;
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
        public ReadOnlyCollection<BaseUntypedProperty> Properties { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        internal Entity(IEnumerable<BaseUntypedProperty> properties, IEnumerable<Behaviour> behaviours, EntityVersion version)
        {
            Version = version;

            // create public read-only collections
            _propertiesList = new List<BaseUntypedProperty>(properties);
            _behavioursList = new List<Behaviour>(behaviours);
            Properties = new ReadOnlyCollection<BaseUntypedProperty>(_propertiesList);
            Behaviours = new ReadOnlyCollection<Behaviour>(_behavioursList);

            // add properties
            _properties = new Dictionary<KeyValuePair<String, Type>, BaseUntypedProperty>();
            foreach (var item in Properties)
                _properties.Add(new KeyValuePair<String, Type>(item.Name, item.GetType()), item);

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
        public void Dispose()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposeManagedResources"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposeManagedResources)
        {
            IsDisposed = true;
        }

        /// <summary>
        /// Initialises this instance.
        /// </summary>
        internal void Initialise(INamedDataProvider initialisationData)
        {
            foreach (var item in Behaviours)
            {
                if (!item.IsReady)
                    item.Initialise(initialisationData);
            }
        }

        /// <summary>
        /// Shuts down this instance.
        /// </summary>
        internal void Shutdown()
        {
            foreach (var item in Behaviours)
            {
                if (item.IsReady)
                    item.Shutdown();
            }

            foreach (var item in Properties)
            {
                item.Clear();
            }
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

        /// <summary>
        /// Add a property with a given implementing type
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="property"></param>
        private void AddProperty<TProperty, TData>(TProperty property) where TProperty : Property<TData>
        {
            AddProperty(property.Name, typeof(TProperty), property);    
        }

        /// <summary>
        /// Add a property with a specific name/type pair.
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="propertyType">Type of the property</param>
        /// <param name="property">The property instance</param>
        private void AddProperty(String name, Type propertyType, BaseUntypedProperty property)
        {
            _properties.Add(new KeyValuePair<String, Type>(name, propertyType), property);
            _propertiesList.Add(property);
        }

        /// <summary>
        /// Get a property with a specific implementing type and contained data type
        /// </summary>
        /// <typeparam name="TProperty">Type of the property</typeparam>
        /// <typeparam name="TData">Type of the data</typeparam>
        /// <param name="name">The name of this property</param>
        /// <returns>A property</returns>
        public TProperty GetProperty<TProperty, TData>(String name = null) where TProperty : Property<TData>
        {
            return GetProperty(name, typeof(TProperty)) as TProperty;
        }

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <typeparam name="T">The data type this property contains.</typeparam>
        /// <param name="name">The name of the propery.</param>
        /// <returns>The property with the specified name and data type.</returns>
        public Property<T> GetProperty<T>(String name)
        {
            return GetProperty(name, typeof(DefaultProperty<T>)) as Property<T>;
        }

        /// <summary>
        /// Get a property of a specific type
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="propertyType">type of the property (*not* the data contained within)</param>
        /// <returns>The property with the specified name and type</returns>
        private BaseUntypedProperty GetProperty(String name, Type propertyType)
        {
            BaseUntypedProperty property;
            _properties.TryGetValue(new KeyValuePair<String, Type>(name, propertyType), out property);
            return property;
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
                foreach (var item in array)
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
        public Behaviour[] GetBehaviours(Type type)
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
