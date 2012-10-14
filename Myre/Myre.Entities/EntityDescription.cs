using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Myre.Entities.Behaviours;
using Ninject;
using Ninject.Parameters;

namespace Myre.Entities
{
    /// <summary>
    /// A struct which contains data about an entity property.
    /// </summary>
    public struct PropertyData
    {
        public readonly string Name;
        public readonly Type PropertyType;
        public readonly Type DataType;
        public readonly object InitialValue;

        public PropertyData(string name, Type propertyType, Type dataType, object initialValue)
        {
            Name = name;
            PropertyType = propertyType;
            DataType = dataType;
            InitialValue = initialValue;
        }

        public override int GetHashCode()
        {
            return PropertyType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyData)
                return Equals((PropertyData)obj);
            
            return base.Equals(obj);
        }

        public bool Equals(PropertyData data)
        {
            return Name == data.Name
                && PropertyType == data.PropertyType
                && InitialValue == data.InitialValue;
        }
    }

    /// <summary>
    /// A struct which contains data about an entity behaviour.
    /// </summary>
    public struct BehaviourData
    {
        public readonly string Name;
        public readonly Type Type;
        public readonly Func<String, Behaviour> Factory;

        public BehaviourData(string name, Type type, Func<String, Behaviour> factory)
        {
            Name = name;
            Type = type;
            Factory = factory;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is BehaviourData)
                return Equals((BehaviourData)obj);

            return base.Equals(obj);
        }

        public bool Equals(BehaviourData data)
        {
            return Name == data.Name
                && Type == data.Type;
        }
    }

    /// <summary>
    /// A class which describes the elements of an entity, and can be used to construct new entity instances.
    /// </summary>
    public class EntityDescription
    {
        private readonly IKernel _kernel;
        private readonly List<BehaviourData> _behaviours;
        private readonly List<PropertyData> _properties;

        private readonly Queue<Entity> _pool;
        private uint _version;

        /// <summary>
        /// Gets a list of behaviours in this instance.
        /// </summary>
        /// <value>The behaviours.</value>
        public ReadOnlyCollection<BehaviourData> Behaviours { get; private set; }
        
        /// <summary>
        /// Gets a list of properties in this instance.
        /// </summary>
        /// <value>The properties.</value>
        public ReadOnlyCollection<PropertyData> Properties { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescription"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public EntityDescription(IKernel kernel = null)
        {
            _kernel = kernel ?? NinjectKernel.Instance;
            _behaviours = new List<BehaviourData>();
            _properties = new List<PropertyData>();
            _pool = new Queue<Entity>();

            Behaviours = new ReadOnlyCollection<BehaviourData>(_behaviours);
            Properties = new ReadOnlyCollection<PropertyData>(_properties);
        }

        /// <summary>
        /// Resets this instance, clearing all property and behaviour deta.
        /// </summary>
        public virtual void Reset()
        {
            _behaviours.Clear();
            _properties.Clear();
            IncrementVersion();
        }

        /// <summary>
        /// Adds all the properties and behaviours from the specified entity description.
        /// </summary>
        /// <param name="description">The entity description to copy from</param>
        public void AddFrom(EntityDescription description)
        {
            foreach (var item in description.Behaviours)
                AddBehaviour(item);

            foreach (var item in description.Properties)
                AddProperty(item);
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour(BehaviourData behaviour)
        {
            Assert.ArgumentNotNull("behaviour.Type", behaviour.Type);

            if (_behaviours.Contains(behaviour))
                return false;

            _behaviours.Add(behaviour);
            IncrementVersion();

            return true;
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour(Type type, string name = null)
        {
            return AddBehaviour(new BehaviourData(name, type, null));
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour<T>(string name = null)
            where T : Behaviour
        {
            return AddBehaviour(typeof(T), name);
        }

#if WINDOWS
        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="create">A factory function which creates an instance of this behaviour</param>
        /// <param name="name">the name.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour<T>(Func<String, T> create, string name = null) where T : Behaviour
        {
            return AddBehaviour(new BehaviourData(name, typeof(T), create));
        }
#endif

        #region properties
        /// <summary>
        /// Adds the property, provided that such a property does not already exist.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        private bool AddProperty(PropertyData property)
        {
            Assert.ArgumentNotNull("property.Name", property.Name);
            Assert.ArgumentNotNull("property.PropertyType", property.PropertyType);

            if (property.InitialValue != null)
            {
                var initialType = property.InitialValue.GetType();
                Assert.IsTrue(property.DataType.IsAssignableFrom(initialType), "Cannot cast initial value to type of property");
            }

            if (_properties.Contains(property))
                return false;

            _properties.Add(property);
            IncrementVersion();

            return true;
        }

        /// <summary>
        /// Adds the property, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="dataType">Type of the data</param>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        private bool AddProperty(Type propertyType, Type dataType, string name, object initialValue)
        {
            return AddProperty(new PropertyData(name, propertyType, dataType, initialValue));
        }

        /// <summary>
        /// Adds the property, provided that such a property does not already exist. Using the default Property&lt;T&gt; implementation
        /// </summary>
        /// <typeparam name="TData">The type of the data in this property</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty<TData>(string name, TData initialValue = default(TData))
        {
            return AddProperty(typeof(DefaultProperty<TData>), typeof(TData), name, initialValue);
        }

        /// <summary>
        /// Adds the property, provided that such a property does not already exist.
        /// </summary>
        /// <typeparam name="TProperty">The type of this property</typeparam>
        /// <typeparam name="TData">The type of the data in this property</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty<TProperty, TData>(string name = null, TData initialValue = default(TData)) where TProperty : Property<TData>
        {
            return AddProperty(typeof(TProperty), typeof(TData), name, initialValue);
        }
        #endregion

        /// <summary>
        /// Creates a new entity with the properties and behaviours described by this instance.
        /// </summary>
        /// <returns></returns>
        public virtual Entity Create()
        {
            Entity e;

            if (_pool.Count > 0)
                e = InitialisePooledEntity();
            else
                e = new Entity(CreateProperties(), CreateBehaviours(), new EntityVersion(this, _version));

            return e;
        }

        /// <summary>
        /// Makes the specified entity available for re-use.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the entity was recycled; else <c>false</c>.</returns>
        protected internal virtual bool Recycle(Entity entity)
        {
            if (entity.Version.Creator != this || entity.Version.Version != _version)
                return false;

            _pool.Enqueue(entity);
            return true;
        }

        private Entity InitialisePooledEntity()
        {
            var entity = _pool.Dequeue();
            foreach (BaseUntypedProperty item in entity.Properties)
            {
                item.Clear();
                foreach (var p in _properties)
                {
                    if (p.Name == item.Name && p.PropertyType == item.GetType())
                    {
                        item.SetBoxedValue(p.InitialValue);
                        break;
                    }
                }
            }

            return entity;
        }

        private void IncrementVersion()
        {
            unchecked
            {
                _version++;
            }

            _pool.Clear();
        }

        private IEnumerable<BaseUntypedProperty> CreateProperties()
        {
            return _properties.Select(CreatePropertyInstance);
        }

        private IEnumerable<Behaviour> CreateBehaviours()
        {
            return _behaviours.Select(CreateBehaviourInstance);
        }

        private BaseUntypedProperty CreatePropertyInstance(PropertyData property)
        {
            BaseUntypedProperty instance = _kernel.Get(property.PropertyType) as BaseUntypedProperty;

            instance.SetBoxedValue(property.InitialValue);
            instance.Name = property.Name;
            
            return instance;
        }

        private Behaviour CreateBehaviourInstance(BehaviourData behaviour)
        {
            Behaviour instance;

            if (behaviour.Factory != null)
                instance = behaviour.Factory(behaviour.Name);
            else
                instance = _kernel.Get(behaviour.Type, new ConstructorArgument("name", behaviour.Name)) as Behaviour;

            instance.Name = behaviour.Name;
            return instance;
        }
    }
}
