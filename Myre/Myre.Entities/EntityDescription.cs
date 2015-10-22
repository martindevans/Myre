using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                && Type == data.Type
                && Factory == data.Factory;
        }
    }

    /// <summary>
    /// A class which describes the elements of an entity, and can be used to construct new entity instances.
    /// </summary>
    public class EntityDescription
    {
        private static readonly Dictionary<Type, ConstructorInfo> _propertyConstructors = new Dictionary<Type, ConstructorInfo>();
        private static readonly Type _genericType = Type.GetType("Myre.Entities.Property`1");

        private readonly IKernel _kernel;
        private readonly List<BehaviourData> _behaviours;
        private readonly List<PropertyData> _properties;

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
            if (((object)behaviour.Type ?? behaviour.Factory) == null)
                throw new ArgumentException("behaviour.TypeAndFactory", "behaviour");

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
        public bool AddBehaviour<T>(TypedName<T> name = default(TypedName<T>))
            where T : Behaviour
        {
            return AddBehaviour(typeof(T), name.Name);
        }

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

        /// <summary>
        /// Removes the behaviour.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveBehaviour(Type type, string name = null)
        {
            for (int i = 0; i < _behaviours.Count; i++)
            {
                var item = _behaviours[i];
                if (item.Type == type && (name == null || item.Name == name))
                {
                    _behaviours.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the behaviour.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveBehaviour<T>(string name = null)
            where T : Behaviour
        {
            return RemoveBehaviour(typeof(T), name);
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

        /// <summary>
        /// Removes the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveProperty(string name)
        {
            for (int i = 0; i < _properties.Count; i++)
            {
                if (_properties[i].Name == name)
                {
                    _properties.RemoveAt(i);
                    return true;
                }
            }

            return false;
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
                yield return CreateBehaviourInstance(_kernel, item);
        }

        private IProperty CreatePropertyInstance(PropertyData property)
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

        private Behaviour CreateBehaviourInstance(IKernel kernel, BehaviourData behaviour)
        {
            Behaviour instance;

            if (behaviour.Factory != null)
                instance = behaviour.Factory(behaviour.Name);
            else
                instance = (Behaviour)kernel.Get(behaviour.Type, new ConstructorArgument("name", behaviour.Name));

            instance.Name = behaviour.Name;
            return instance;
        }
    }
}
