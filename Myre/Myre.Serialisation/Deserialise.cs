using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Ninject;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        public abstract partial class Node
        {
            public abstract object Deserialise(Dom dom, IKernel kernel, Type expectedType);
        }

        public partial class LiteralNode
        {
            private MethodInfo parseMethod;
            private static Dictionary<Type, MethodInfo> parseMethods = new Dictionary<Type, MethodInfo>();

            public override object Deserialise(Dom dom, IKernel kernel, Type expectedType)
            {
                if (Type == typeof(string))
                    return Value;

                FindParseMethod();
                return parseMethod.Invoke(null, new object[] { Value.Unescape() });
            }

            private void FindParseMethod()
            {
                if (parseMethod != null)
                    return;
                
                if (!parseMethods.TryGetValue(Type, out parseMethod))
                {
                    parseMethod = Type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                    parseMethods[Type] = parseMethod;
                }
            }
        }

        public partial class ListNode
        {
            public override object Deserialise(Dom dom, IKernel kernel, Type expectedType)
            {
                var type = Type ?? expectedType;

                if (type.IsArray)
                {
                    var instance = Array.CreateInstance(type.GetElementType(), Children.Count);

                    for (int i = 0; i < Children.Count; i++)
                    {
                        var child = Children[i].GetNode().Deserialise(dom, kernel, type.GetElementType());
                        instance.SetValue(child, i);
                    }

                    return instance;
                }
                else
                {
                    var instance = CreateInstance(type, kernel);

                    IList list = instance as IList;
                    foreach (var item in Children)
                    {
                        var child = DeserialiseNode(dom, item.GetNode(), kernel, typeof(object));
                        list.Add(child);
                    }

                    return list;
                }
            }
        }

        public partial class DictionaryNode
        {
            public override object Deserialise(Dom dom, IKernel kernel, Type expectedType)
            {
                var type = Type ?? expectedType;
                var instance = CreateInstance(type, kernel);

                IDictionary dictionary = instance as IDictionary;
                foreach (var item in Children)
                {
                    var key = DeserialiseNode(dom, item.Key.GetNode(), kernel, typeof(object));
                    var value = DeserialiseNode(dom, item.Value.GetNode(), kernel, typeof(object));
                    dictionary.Add(key, value);
                }

                return dictionary;
            }
        }

        public partial class ObjectNode
        {
            private struct FieldOrProperty
            {
                public FieldInfo Field;
                public PropertyInfo Property;
                public string Name;
                public Type Type;

                public bool IsValid
                {
                    get { return Field != null || Property != null; }
                }

                public void Set(object instance, object value)
                {
                    if (Field != null)
                        Field.SetValue(instance, value);
                    if (Property != null)
                        Property.SetValue(instance, value, null);
                }
            }

            public override object Deserialise(Dom dom, IKernel kernel, Type expectedType)
            {
                var type = Type ?? expectedType;
                var instance = CreateInstance(type, kernel);

                foreach (var item in Children)
                {
                    if (item.Value.GetNode() == null)
                        continue;

                    var field = FindField(type, item.Key);

                    if (!field.IsValid)
                        continue;

                    var value = DeserialiseNode(dom, item.Value.GetNode(), kernel, field.Type);
                    field.Set(instance, value);
                }

                return instance;
            }

            private FieldOrProperty FindField(Type type, string name)
            {
                FieldOrProperty f = new FieldOrProperty();
                f.Name = name;

                var field = type.GetField(name);
                if (field != null)
                {
                    f.Field = field;
                    f.Property = null;
                    f.Type = field.FieldType;
                }
                else
                {
                    var property = type.GetProperty(name);
                    if (property != null)
                    {
                        f.Property = property;
                        f.Field = null;
                        f.Type = property.PropertyType;
                    }
                }

                return f;
            }
        }

        private Dictionary<int, object> sharedInstances = new Dictionary<int, object>();

        /// <summary>
        /// Deserialises the object described by this DOM instance into the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to instantiate.</typeparam>
        /// <returns>The runtime object described by this DOM.</returns>
        public T Deserialise<T>()
        {
            return (T)DeserialiseNode(this, Root.GetNode(), Kernel, typeof(T));
        }

        private static object CreateInstance(Dom dom, Node node, IKernel kernel, Type expectedType)
        {
            if (typeof(ICustomSerialisable).IsAssignableFrom(node.Type))
            {
                var type = node.Type ?? expectedType;
                var instance = CreateInstance(type, kernel) as ICustomSerialisable;
                instance.Deserialise(node);
                return instance;
            }
            else
                return node.Deserialise(dom, kernel, expectedType);
        }

        private static object CreateInstance(Type type, IKernel kernel)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                return kernel.Get(type);
        }

        private static object DeserialiseNode(Dom dom, Node node, IKernel kernel, Type expectedType)
        {
            if (node.SharedReferenceID == null)
                return CreateInstance(dom, node, kernel, expectedType);

            var id = node.SharedReferenceID.Value;
            object value;
            if (!dom.sharedInstances.TryGetValue(id, out value))
            {
                value = CreateInstance(dom, node, kernel, expectedType);
                dom.sharedInstances[id] = value;
            }

            return value;
        }
    }
}
