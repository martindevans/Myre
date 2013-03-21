using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        struct FieldsAndProperties
        {
            public PropertyInfo[] Properties;
            public FieldInfo[] Fields;
        }

        private static Dictionary<Type, bool> parsable = new Dictionary<Type, bool>();
        private static Dictionary<Type, FieldsAndProperties> properties = new Dictionary<Type, FieldsAndProperties>();

        private Dictionary<object, Node> referenceTypeNodes = new Dictionary<object, Node>();
        private Dictionary<int, Node> sharedReferences = new Dictionary<int, Node>();

        public static Dom Serialise(object value)
        {
            return Serialise(value, new StandardKernel());
        }

        public static Dom Serialise(object value, IKernel kernel)
        {
            var dom = new Dom(kernel);
            dom.Root = SerialiseObject(dom, value);
            dom.referenceTypeNodes.Clear();
            return dom;
        }

        private static NodeReference SerialiseObject(Dom dom, object item)
        {
            if (item == null)
                return new NodeReference(dom, null);

            Node node;
            if (item.GetType().IsValueType)
            {
                node = Dom.CreateNode(dom, item);
                return new NodeReference(dom, node);
            }
            else
            {
                if (dom.referenceTypeNodes.TryGetValue(item, out node))
                {
                    if (node.SharedReferenceID == null)
                    {
                        node.SharedReferenceID = dom.sharedReferences.Count;
                        dom.sharedReferences.Add(node.SharedReferenceID.Value, node);
                    }

                    return new NodeReference(dom, node);
                }
                else
                {
                    node = Dom.CreateNode(dom, item);
                    dom.referenceTypeNodes[item] = node;
                    return new NodeReference(dom, node);
                }
            }
        }

        private static Node CreateNode(Dom dom, object value)
        {
            if (value == null)
                return null;

            if (value is ICustomSerialisable)
                return (value as ICustomSerialisable).Serialise();

            var type = value.GetType();

            if (IsParsable(type))
                return CreateLiteral(value);

            if (value is IList)
                return CreateList(value as IList, type, dom);

            if (value is IDictionary)
                return CreateDictionary(value as IDictionary, type, dom);

            return CreateObject(value, type, dom);
        }

        private static Node CreateLiteral(object value)
        {
            return new LiteralNode()
            {
                Type = value.GetType(),
                Value = value.ToString().Escape()
            };
        }

        private static Node CreateList(IList list, Type type, Dom dom)
        {            
            var node = new ListNode();
            node.Type = type;
            node.Children = new List<NodeReference>();

            foreach (var item in list)
            {
                var serialisedItem = SerialiseObject(dom, item);
                node.Children.Add(serialisedItem);
            }

            return node;
        }

        private static Node CreateDictionary(IDictionary dictionary, Type type, Dom dom)
        {
            var node = new DictionaryNode();
            node.Type = type;
            node.Children = new Dictionary<NodeReference, NodeReference>();

            foreach (DictionaryEntry item in dictionary)
            {
                var serialisedKey = SerialiseObject(dom, item.Key);
                var serialisedValue = SerialiseObject(dom, item.Value);
                node.Children.Add(serialisedKey, serialisedValue);
            }

            return node;
        }

        private static Node CreateObject(object value, Type type, Dom dom)
        {
            var fap = GetFieldsAndProperties(type);
            var node = new ObjectNode();
            node.Type = type;
            node.Children = new Dictionary<string, NodeReference>();

            foreach (var field in fap.Fields)
            {
                var fieldValue = field.GetValue(value);
                var serialisedValue = SerialiseObject(dom, fieldValue);
                node.Children.Add(field.Name, serialisedValue);
            }

            foreach (var property in fap.Properties)
            {
                var fieldValue = property.GetValue(value, null);
                var serialisedValue = SerialiseObject(dom, fieldValue);
                node.Children.Add(property.Name, serialisedValue);
            }

            return node;
        }

        private static bool IsParsable(Type type)
        {
            if (type == typeof(string))
                return true;

            bool isParsable;
            if (!parsable.TryGetValue(type, out isParsable))
            {
                var parseMethod = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                isParsable = parseMethod != null && type.IsAssignableFrom(parseMethod.ReturnType);

                parsable[type] = isParsable;
            }

            return isParsable;
        }

        private static FieldsAndProperties GetFieldsAndProperties(Type type)
        {
            FieldsAndProperties fap;
            if (properties.TryGetValue(type, out fap))
                return fap;

            fap.Fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            fap.Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite).ToArray();

            properties[type] = fap;
            return fap;
        }
    }
}
