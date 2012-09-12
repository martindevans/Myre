using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Ninject;
using Ninject.Parameters;
using System.Xml.Linq;
using Myre;
using Myre.Extensions;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;
using Myre.Collections;

namespace Myre.Entities
{
    /// <summary>
    /// A struct which contains data about an entity property.
    /// </summary>
    public struct PropertyData
    {
        public string Name;
        public Type DataType;
        public object InitialValue;

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
            return this.Name == data.Name
                && this.DataType == data.DataType
                && this.InitialValue == data.InitialValue;
        }
    }

    /// <summary>
    /// A struct which contains data about an entity behaviour.
    /// </summary>
    public struct BehaviourData
    {
        public string Name;
        public Type Type;
        public Func<String, Behaviour> Factory;

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
            return this.Name == data.Name
                && this.Type == data.Type;
        }
    }

    /// <summary>
    /// A class which describes the elements of an entity, and can be used to construct new entity instances.
    /// </summary>
    public class EntityDescription
    {
        private static readonly Dictionary<Type, ConstructorInfo> propertyConstructors = new Dictionary<Type, ConstructorInfo>();
        private static readonly Type genericType = Type.GetType("Myre.Entities.Property`1");

        private IKernel kernel;
        private List<BehaviourData> behaviours;
        private List<PropertyData> properties;

        private Queue<Entity> pool;
        private uint version;

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
            this.kernel = kernel ?? NinjectKernel.Instance;
            this.behaviours = new List<BehaviourData>();
            this.properties = new List<PropertyData>();
            this.pool = new Queue<Entity>();

            this.Behaviours = new ReadOnlyCollection<BehaviourData>(behaviours);
            this.Properties = new ReadOnlyCollection<PropertyData>(properties);
        }

        /// <summary>
        /// Resets this instance, clearing all property and behaviour deta.
        /// </summary>
        public virtual void Reset()
        {
            behaviours.Clear();
            properties.Clear();
            IncrementVersion();
        }

        /// <summary>
        /// Adds all the properties and behaviours from the specified entity description.
        /// </summary>
        /// <param name="entity">The entity.</param>
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

            if (behaviours.Contains(behaviour))
                return false;

            behaviours.Add(behaviour);
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
            return AddBehaviour(new BehaviourData() { Type = type, Name = name });
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

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="create">A factory function which creates an instance of this behaviour</param>
        /// <param name="name">the name.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour<T>(Func<String, T> create, string name = null)
            where T : Behaviour
        {
            return AddBehaviour(new BehaviourData()
            {
                Name = name,
                Type = typeof(T),
                Factory = (a) => create(a),
            });
        }

        /// <summary>
        /// Removes the behaviour.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveBehaviour(Type type, string name = null)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                var item = behaviours[i];
                if (item.Type == type && (name == null || item.Name == name))
                {
                    behaviours.RemoveAt(i);
                    IncrementVersion();
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
            Assert.ArgumentNotNull("property.Name", property.Name);
            Assert.ArgumentNotNull("property.DataType", property.DataType);
            Assert.IsTrue(property.InitialValue == null || property.InitialValue.GetType().IsInstanceOfType(property.DataType), "Cannot cast initial value to type of property");

            if (properties.Contains(property))
                return false;

            properties.Add(property);
            IncrementVersion();

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
            var data = new PropertyData()
            {
                Name = name,
                DataType = dataType,
                InitialValue = initialValue
            };

            return AddProperty(data);
        }

        /// <summary>
        /// Adds the property, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty<T>(string name, T initialValue = default(T))
        {
            return AddProperty(typeof(T), name, initialValue);
        }

        /// <summary>
        /// Removes the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveProperty(string name)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i].Name == name)
                {
                    properties.RemoveAt(i);
                    IncrementVersion();
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
            Entity e;

            if (pool.Count > 0)
                e = InitialisePooledEntity();
            else
                e = new Entity(CreateProperties(), CreateBehaviours(), new EntityVersion(this, version));

            return e;
        }

        /// <summary>
        /// Makes the specified entity available for re-use.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the entity was recycled; else <c>false</c>.</returns>
        public virtual bool Recycle(Entity entity)
        {
            if (entity.Version.Creator != this || entity.Version.Version != version)
                return false;

            pool.Enqueue(entity);
            return true;
        }

        private Entity InitialisePooledEntity()
        {
            var entity = pool.Dequeue();
            foreach (IProperty item in entity.Properties)
            {
                item.Clear();
                foreach (var p in properties)
                {
                    if (p.Name == item.Name && p.DataType == item.Type)
                    {
                        item.Value = p.InitialValue;
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
                version++;
            }

            pool.Clear();
        }

        private IEnumerable<IProperty> CreateProperties()
        {
            foreach (var item in properties)
                yield return CreatePropertyInstance(item);
        }

        private IEnumerable<Behaviour> CreateBehaviours()
        {
            foreach (var item in behaviours)
                yield return CreateBehaviourInstance(kernel, item);
        }

        private IProperty CreatePropertyInstance(PropertyData property)
        {
            ConstructorInfo constructor;
            if (!propertyConstructors.TryGetValue(property.DataType, out constructor))
            {
                var type = genericType.MakeGenericType(property.DataType);
                constructor = type.GetConstructor(new Type[] { typeof(string) });
                propertyConstructors.Add(property.DataType, constructor);
            }

            IProperty prop = constructor.Invoke(new object[] { property.Name }) as IProperty;
            prop.Value = property.InitialValue;

            return prop;
        }

        private Behaviour CreateBehaviourInstance(IKernel kernel, BehaviourData behaviour)
        {
            Behaviour instance;

            if (behaviour.Factory != null)
                instance = behaviour.Factory(behaviour.Name);
            else
                instance = kernel.Get(behaviour.Type, new ConstructorArgument("name", behaviour.Name)) as Behaviour;

            instance.Name = behaviour.Name;
            return instance;
        }
    }
}
