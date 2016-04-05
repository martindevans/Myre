using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
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
        public readonly Type DataType;
        public readonly object InitialValue;

        public PropertyData(string name, Type dataType, object initialValue)
        {
            Contract.Requires(name != null);
            Contract.Requires(dataType != null);

            Name = name;
            DataType = dataType;
            InitialValue = initialValue;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Name != null);
            Contract.Invariant(DataType != null);
        }

        public override int GetHashCode()
        {
            return DataType.GetHashCode();
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
        public readonly NameWithType Name;

        public BehaviourData(NameWithType name)
        {
            Name = name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var a = obj as BehaviourData;
            if (a != null)
                return Equals(a);

            return false;
        }

        public bool Equals(BehaviourData data)
        {
            Contract.Requires(data != null);

            return Name.Equals(data.Name);
        }

        public Behaviour Create(IKernel kernel)
        {
            var instance = (Behaviour)kernel.Get(Name.Type, new ConstructorArgument("name", Name.Name));
            Contract.Assume(instance != null);

            instance.Name = Name.Name;
            return instance;
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
        private static readonly Dictionary<Type, ConstructorInfo> _propertyConstructors = new Dictionary<Type, ConstructorInfo>();
        private static readonly Type _genericType = Type.GetType("Myre.Entities.Property`1");

        private readonly IKernel _kernel;
        private readonly List<IBehaviourFactory> _behaviours;
        private readonly List<PropertyData> _properties;

        /// <summary>
        /// Gets a list of behaviours in this instance.
        /// </summary>
        /// <value>The behaviours.</value>
        public IReadOnlyList<IBehaviourFactory> Behaviours { get
        {
            Contract.Ensures(Contract.Result<IReadOnlyList<IBehaviourFactory>>() != null);
            return _behaviours;
        }
        }
        
        /// <summary>
        /// Gets a list of properties in this instance.
        /// </summary>
        /// <value>The properties.</value>
        public IReadOnlyList<PropertyData> Properties { get
        {
            Contract.Ensures(Contract.Result<IReadOnlyList<PropertyData>>() != null);
            return _properties;
        }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescription"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public EntityDescription(IKernel kernel = null)
        {
            _kernel = kernel ?? NinjectKernel.Instance;
            _behaviours = new List<IBehaviourFactory>();
            _properties = new List<PropertyData>();
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_kernel != null);
            Contract.Invariant(_behaviours != null);
            Contract.Invariant(_properties != null);
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
            Contract.Requires(description != null);

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
            Contract.Requires(behaviour != null);

            if (_behaviours.Contains(behaviour))
                return false;

            _behaviours.Add(behaviour);

            return true;
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour(Type type, string name = "")
        {
            Contract.Requires(type != null);
            Contract.Requires(name != null);

            return AddBehaviour(new BehaviourData(new NameWithType(name, type)));
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour<T>(TypedName<T> name)
            where T : Behaviour
        {
            return AddBehaviour(typeof(T), name.Name);
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist. (Uses default name "")
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour<T>()
            where T : Behaviour
        {
            return AddBehaviour(typeof(T), "");
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
                throw new ArgumentException("property.Name", "property");
            if (property.DataType == null)
                throw new ArgumentException("property.DataType", "property");
            if (property.InitialValue != null && !property.DataType.IsAssignableFrom(property.InitialValue.GetType()))
                throw new ArgumentException("property.InitialValue cannot be cast to property.DataType", "property");

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
        public bool AddProperty(Type dataType, string name, object initialValue)
        {
            Contract.Requires(dataType != null);
            Contract.Requires(name != null);

            return AddProperty(new PropertyData(name, dataType, initialValue));
        }

        /// <summary>
        /// Adds the property, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty<T>(TypedName<T> name, T initialValue = default(T))
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
            Contract.Ensures(Contract.Result<Entity>() != null);

            return new Entity(CreateProperties(), CreateBehaviours());
        }

        private IEnumerable<IProperty> CreateProperties()
        {
            Contract.Ensures(Contract.Result<IEnumerable<IProperty>>() != null);

            foreach (var item in _properties)
                yield return CreatePropertyInstance(item);
        }

        private IEnumerable<Behaviour> CreateBehaviours()
        {
            Contract.Ensures(Contract.Result<IEnumerable<Behaviour>>() != null);

            foreach (var item in _behaviours)
                yield return item.Create(_kernel);
        }

        private static IProperty CreatePropertyInstance(PropertyData property)
        {
            ConstructorInfo constructor;
            if (!_propertyConstructors.TryGetValue(property.DataType, out constructor))
            {
                var type = _genericType.MakeGenericType(property.DataType);
                constructor = type.GetConstructor(new Type[] { typeof(string) });
                _propertyConstructors.Add(property.DataType, constructor);
            }
            if (constructor == null)
                throw new InvalidOperationException(string.Format("Cannot find constructor(string) for {0}", property.DataType));

            IProperty prop = constructor.Invoke(new object[] { property.Name }) as IProperty;
            if (prop == null)
                throw new InvalidOperationException(string.Format("Constructor for Property<{0}> returned null", property.DataType));

            prop.Value = property.InitialValue;
            return prop;
        }
    }
}
