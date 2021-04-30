using System;
using System.Collections.Generic;
using System.Linq;
using Myre.Collections;
using Myre.Entities.Behaviours;
using Myre.Extensions;

namespace Myre.Entities
{
    /// <summary>
    /// A class which represents a collection of related properties and behaviours.
    /// </summary>
    public sealed class Entity
    {
        public sealed class ConstructionContext
            : IDisposableObject
        {
            private readonly Entity _entity;
            private Behaviour? _behaviour;

            internal ConstructionContext(Entity entity)
            {
                _entity = entity;
            }

            /// <summary>
            /// Create a new property attached to this entity
            /// </summary>
            /// <typeparam name="T">The Type of this property (usually inferred from the name parameter)</typeparam>
            /// <param name="name">The Type and name of this property</param>
            /// <param name="value">The initial value of this property</param>
            /// <returns>The property created</returns>
            public Property<T> CreateProperty<T>(TypedName<T> name, T? value = default)
            {
                if (_behaviour == null)
                    throw new InvalidOperationException("Must initialise ConstructionContext first");

                CheckFrozen();

                var fullName = name.Name;

                var property = _entity.GetProperty(new TypedName<T>(fullName));
                if (property == null)
                {
                    property = new Property<T>(fullName) { Value = value };
                    _entity.AddProperty(property);
                }

                return property;
            }

            /// <summary>
            /// There is a (notionally) a new construction context per behaviour. However we only need one at a time so we can reuse the same context object.
            /// This method mutates the context to indicate which behaviour it is currently acting for
            /// </summary>
            /// <param name="behaviour"></param>
            /// <returns></returns>
            internal ConstructionContext Next(Behaviour behaviour)
            {
                CheckFrozen();

                _behaviour = behaviour;

                return this;
            }

            #region freezing/disposal
            private void CheckFrozen()
            {
                if (IsDisposed)
                    throw new InvalidOperationException("Entity initialisation contexts can only be used during initialisation.");
            }

            public bool IsDisposed { get; private set; }            

            public void Dispose()
            {
                if (IsDisposed)
                    throw new InvalidOperationException("Context is already frozen, cannot freeze a second time");
                IsDisposed = true;
            }
            #endregion
        }

        private readonly Dictionary<NameWithType, IProperty> _properties;
        private readonly Dictionary<Type, Behaviour[]> _behaviours;

        private readonly List<IProperty> _propertiesList;

        /// <summary>
        /// Gets the scene this entity belongs to.
        /// </summary>
        /// <value>The scene.</value>
        public Scene? Scene { get; internal set; }

        /// <summary>
        /// Gets the behaviours this entity contains.
        /// </summary>
        /// <value>The behaviours.</value>
        public IReadOnlyList<Behaviour> Behaviours { get; }

        /// <summary>
        /// Gets the properties this entity contains.
        /// </summary>
        /// <value>The properties.</value>
        public IReadOnlyList<IProperty> Properties => _propertiesList;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value></value>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// A map of type to behaviour which maps from *any* type in the type hierarchy to all the behaviours which implement that type
        /// </summary>
        public IReadOnlyDictionary<Type, Behaviour[]> BehavioursIndex { get; }

        private INamedDataProvider? _shutdownData;

        /// <summary>
        /// Gets a value indicating whether behaviours have already been shutdown
        /// </summary>
        internal bool BehavioursShutdown { get; private set; }

        internal Entity(IEnumerable<IProperty> properties, IEnumerable<Behaviour> behaviours)
        {
            // create public read-only collections
            _propertiesList = new List<IProperty>(properties);
            Behaviours = new List<Behaviour>(behaviours);

            // add properties
            _properties = new Dictionary<NameWithType, IProperty>();
            foreach (var item in Properties)
                _properties.Add(new NameWithType(item.Name, item.Type), item);

            // sort behaviours by their type
            var catagorised = new Dictionary<Type, List<Behaviour>>();
            foreach (var item in Behaviours)
            {
                item.Owner = this;
                CatagoriseBehaviour(catagorised, item);
            }

            // add behaviours
            _behaviours = new Dictionary<Type, Behaviour[]>();
            foreach (var item in catagorised)
                _behaviours.Add(item.Key, item.Value.ToArray());

            BehavioursIndex = new Dictionary<Type, Behaviour[]>(_behaviours);

            // allow behaviours to add their own properties
            CreateProperties();
        }

        private static void CatagoriseBehaviour(IDictionary<Type, List<Behaviour>> catagorised, Behaviour behaviour)
        {
            var type = behaviour.GetType();

            foreach (var t in type.GetImplementedTypes().Distinct())
                LazyGetCategoryList(t, catagorised).Add(behaviour);
        }

        private static List<Behaviour> LazyGetCategoryList(Type type, IDictionary<Type, List<Behaviour>> catagorised)
        {
            if (!catagorised.TryGetValue(type, out var behavioursOfType))
            {
                behavioursOfType = new List<Behaviour>();
                catagorised.Add(type, behavioursOfType);
            }

            return behavioursOfType;
        }

        private void CreateProperties()
        {
            using var context = new ConstructionContext(this);

            foreach (var behaviour in Behaviours)
                behaviour.CreateProperties(context.Next(behaviour));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose(INamedDataProvider shutdownData)
        {
            _shutdownData = shutdownData;
            IsDisposed = true;
        }

        /// <summary>
        /// Initialises this instance. Automatically assigns initialisation values to properties with the same name and an assignable type
        /// </summary>
        /// <param name="initialisationData">Initialisation data to pass to behaviours</param>
        internal void Initialise(INamedDataProvider? initialisationData)
        {
            BehavioursShutdown = false;
            IsDisposed = false;
            _shutdownData = null;

            ////Automatically assign properties
            //if (initialisationData != null)
            //{
            //    foreach (var item in initialisationData)
            //    {
            //        var prop = GetProperty(item.Key);
            //        if (prop != null && prop.Type.IsAssignableFrom(item.Value.Type))
            //            prop.Value = item.Value.Value;
            //    }
            //}

            //Initialise behaviours (potentially overwriting the auto assign)
            foreach (var item in Behaviours)
            {
                if (!item.IsReady)
                    item.Initialise(initialisationData);
            }
        }

        internal void Initialised()
        {
            //Ok everything has been initialised
            foreach (var item in Behaviours)
                item.Initialised();
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

        private bool _delayPropertyShutdown;
        /// <summary>
        /// Delays shutting down the properties of this entity by 1 frame
        /// </summary>
        public void DelayPropertyShutdown()
        {
            _delayPropertyShutdown = true;
        }

        private void AddProperty(IProperty property)
        {
            _properties.Add(new NameWithType(property.Name, property.Type), property);
            _propertiesList.Add(property);
        }

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <param name="name">The name of the propery.</param>
        /// <param name="type">The type of the value contained in this property</param>
        /// <returns>The property with the specified name and data type.</returns>
        public IProperty? GetProperty(string name, Type type)
        {
            _properties.TryGetValue(new NameWithType(name, type), out var property);
            return property;
        }

        /// <summary>
        /// Gets the property with the specified name.
        /// </summary>
        /// <typeparam name="T">The data type this property contains.</typeparam>
        /// <param name="name">The name of the propery.</param>
        /// <returns>The property with the specified name and data type.</returns>
        public Property<T>? GetProperty<T>(TypedName<T> name)
        {
            return (Property<T>?)GetProperty(name.Name, typeof(T));
        }

        /// <summary>
        /// Gets the first behaviour of the specified type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Behaviour? GetBehaviour(Type type)
        {
            if (_behaviours.TryGetValue(type, out var array))
                if (array.Length > 0)
                    return array[0];

            return null;
        }

        /// <summary>
        /// Gets the behaviours of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
// ReSharper disable ReturnTypeCanBeEnumerable.Global
        public IReadOnlyList<Behaviour> GetBehaviours(Type type)
// ReSharper restore ReturnTypeCanBeEnumerable.Global
        {
            _behaviours.TryGetValue(type, out var array);

            if (array != null)
                return array;

            var empty = Array.Empty<Behaviour>();
            return empty;
        }

        /// <summary>
        /// Gets the first behaviour of the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns></returns>
        public T? GetBehaviour<T>()
        {
            var b = GetBehaviour(typeof(T));
            return (T?)(object?)b;
        }

        /// <summary>
        /// Gets the behaviours of the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns></returns>
        public IReadOnlyList<T> GetBehaviours<T>()
        {
            return GetBehaviours(typeof(T))
                   .Cast<T>()
                   .ToArray();
        }
    }
}
