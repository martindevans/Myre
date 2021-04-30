using System;
using System.Collections.Generic;
using System.Reflection;
using Myre.Entities.Behaviours;
using Ninject;

namespace Myre.Entities
{
    /// <summary>
    /// A struct which contains data about an entity property.
    /// </summary>
    public readonly struct PropertyData
    {
        public readonly string Name;
        public readonly Type DataType;
        public readonly object? InitialValue;

        public PropertyData(string name, Type dataType, object? initialValue)
        {
            Name = name;
            DataType = dataType;
            InitialValue = initialValue;
        }

        public override int GetHashCode()
        {
            return DataType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyData data)
                return Equals(data);
            
            return base.Equals(obj);
        }

        public bool Equals(PropertyData data)
        {
            return Name == data.Name
                && DataType == data.DataType
                && InitialValue == data.InitialValue;
        }
    }

    /// <summary>
    /// A struct which contains data about an entity behaviour.
    /// </summary>
    public class BehaviourData
        : IBehaviourFactory
    {
        public readonly Type Type;

        public BehaviourData(Type type)
        {
            Type = type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is BehaviourData a)
                return Equals(a);

            return false;
        }

        public bool Equals(BehaviourData data)
        {
            return Type == data.Type;
        }

        public Behaviour Create(IKernel kernel)
        {
            return (Behaviour)kernel.Get(Type);
        }
    }

    public interface IBehaviourFactory
    {
        Behaviour Create(IKernel kernel);
    }

    /// <summary>
    /// A class which describes the elements of an entity, and can be used to construct new entity instances.
    /// </summary>
    public class EntityDescription
    {
        private static readonly Dictionary<Type, ConstructorInfo> _propertyConstructors = new();
        private static readonly Type _genericType = Type.GetType("Myre.Entities.Property`1")!;

        private readonly IKernel _kernel;
        private readonly List<IBehaviourFactory> _behaviours;
        private readonly List<PropertyData> _properties;

        /// <summary>
        /// Gets a list of behaviours in this instance.
        /// </summary>
        /// <value>The behaviours.</value>
        public IReadOnlyList<IBehaviourFactory> Behaviours => _behaviours;

        /// <summary>
        /// Gets a list of properties in this instance.
        /// </summary>
        /// <value>The properties.</value>
        public IReadOnlyList<PropertyData> Properties => _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescription"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public EntityDescription(IKernel? kernel = null)
        {
            _kernel = kernel ?? NinjectKernel.Instance;
            _behaviours = new List<IBehaviourFactory>();
            _properties = new List<PropertyData>();
        }

        /// <summary>
        /// Resets this instance, clearing all property and behaviour deta.
        /// </summary>
        public virtual void Reset()
        {
            _behaviours.Clear();
            _properties.Clear();
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
        public bool AddBehaviour(IBehaviourFactory behaviour)
        {
            if (_behaviours.Contains(behaviour))
                return false;

            _behaviours.Add(behaviour);

            return true;
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour(Type type)
        {
            return AddBehaviour(new BehaviourData(type));
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour<T>()
            where T : Behaviour
        {
            return AddBehaviour(typeof(T));
        }

        #region properties
        /// <summary>
        /// Adds the property, provided that such a property does not already exist.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty(PropertyData property)
        {
            if (property.Name == null)
                throw new ArgumentException("property.Name", nameof(property));
            if (property.DataType == null)
                throw new ArgumentException("property.DataType", nameof(property));
            if (property.InitialValue != null && !property.DataType.IsAssignableFrom(property.InitialValue.GetType()))
                throw new ArgumentException("property.InitialValue cannot be cast to property.DataType", nameof(property));

            if (_properties.Contains(property))
                return false;

            _properties.Add(property);

            return true;
        }

        /// <summary>
        /// Adds the property, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty(Type dataType, string name, object? initialValue)
        {
            return AddProperty(new PropertyData(name, dataType, initialValue));
        }

        /// <summary>
        /// Adds the property, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty<T>(TypedName<T> name, T? initialValue = default)
        {
            return AddProperty(typeof(T), name.Name, initialValue);
        }
        #endregion

        /// <summary>
        /// Creates a new entity with the properties and behaviours described by this instance.
        /// </summary>
        /// <returns></returns>
        public virtual Entity Create()
        {
            return new Entity(CreateProperties(), CreateBehaviours());
        }

        private IEnumerable<IProperty> CreateProperties()
        {
            foreach (var item in _properties)
                yield return CreatePropertyInstance(item);
        }

        private IEnumerable<Behaviour> CreateBehaviours()
        {
            foreach (var item in _behaviours)
            {
                yield return item.Create(_kernel);
            }
        }

        private static IProperty CreatePropertyInstance(PropertyData property)
        {
            if (!_propertyConstructors.TryGetValue(property.DataType, out ConstructorInfo constructor))
            {
                var type = _genericType.MakeGenericType(property.DataType);
                constructor = type.GetConstructor(new Type[] { typeof(string) })!;
                _propertyConstructors.Add(property.DataType, constructor);
            }
            if (constructor == null)
                throw new InvalidOperationException(string.Format("Cannot find constructor(string) for {0}", property.DataType));

            if (constructor.Invoke(new object[] { property.Name }) is not IProperty prop)
                throw new InvalidOperationException(string.Format("Constructor for Property<{0}> returned null", property.DataType));

            prop.Value = property.InitialValue;
            return prop;
        }
    }
}
