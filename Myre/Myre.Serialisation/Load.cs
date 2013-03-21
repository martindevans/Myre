using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Ninject;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        struct CollectionNode
        {
            public NodeReference First;
            public NodeReference Second;
        }

        public static Dom Load(string input)
        {
            return Load(input, new StandardKernel());
        }

        public Dom Load(Stream stream)
        {
            return Load(stream, new StandardKernel());
        }

        public static Dom Load(string input, IKernel kernel)
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(input)))
                return Load(stream, kernel);
        }

        public static Dom Load(Stream stream, IKernel kernel)
        {
            using (var reader = new StreamReader(stream, Encoding.Unicode))
            {
                var dom = new Dom(kernel);
                ParseSharedReferences(reader, dom);
                dom.Root = ParseItem(reader, dom);
                return dom;
            }
        }

        private static void ParseSharedReferences(StreamReader reader, Dom dom)
        {
            if (reader.Peek() != 'R')
                return;

            // skip over R[
            reader.Read();
            reader.Read();

            SkipWhitespace(reader);

            while ((char)reader.Peek() != ']')
            {
                // read first element
                var key = ParseIdentifier(reader);
                var id = int.Parse(key);

                //skip over separator :
                var separator = (char)reader.Read();

                // read node
                var value = ParseItem(reader, dom);

                dom.sharedReferences.Add(id, value.GetNode());
                SkipWhitespace(reader);
            }

            // skip over ]
            reader.Read();
        }

        private static NodeReference ParseItem(StreamReader reader, Dom dom)
        {
            SkipWhitespace(reader);

            var type = ParseTypeName(reader);

            SkipWhitespace(reader);

            if (reader.Peek() == '#')
            {
                reader.Read();
                var value = ParseIdentifier(reader);
                var id = int.Parse(value);
                return new NodeReference(dom, id);
            }
            else
            {
                var node = ParseNode(reader, type, dom);
                return new NodeReference(dom, node);
            }
        }

        private static Node ParseNode(StreamReader reader, Type type, Dom dom)
        {
            SkipWhitespace(reader);

            switch (reader.Peek())
            {
                case '"':
                    return ParseLiteral(reader, type);

                case '{':
                    return ParseObject(reader, type, dom);

                case '[':
                    return ParseCollection(reader, type, dom);

                default:
                    return null;
            }
        }

        private static void SkipWhitespace(StreamReader reader)
        {
            while (char.IsWhiteSpace((char)reader.Peek()))
                reader.Read();
        }

        private static Node ParseCollection(StreamReader reader, Type type, Dom dom)
        {
            List<CollectionNode> nodes = new List<CollectionNode>();

            // skip over [
            reader.Read();
            SkipWhitespace(reader);

            // read children
            bool isDictionary = false;
            while ((char)reader.Peek() != ']')
            {
                var node = new CollectionNode();

                // read first element
                node.First = ParseItem(reader, dom);

                var seperator = (char)reader.Peek();
                if (seperator == ':')
                {
                    // found dictionary key/value separator
                    reader.Read();

                    // read value
                    node.Second = ParseItem(reader, dom);
                    isDictionary = true;
                }

                // skip over ,
                if ((char)reader.Peek() == ',')
                    reader.Read();

                nodes.Add(node);
                SkipWhitespace(reader);
            }

            // skip over ]
            reader.Read();

            if (isDictionary)
                return CreateDictionary(type, nodes);
            else
                return CreateList(type, nodes);
        }

        private static Node CreateDictionary(Type type, List<CollectionNode> children)
        {
            // create a dictionary to store children
            var c = new Dictionary<NodeReference, NodeReference>(children.Count);

            for (int i = 0; i < children.Count; i++)
                c.Add(children[i].First, children[i].Second);

            // create dictionary node
            return new DictionaryNode() { Type = type, Children = c };
        }

        private static Node CreateList(Type type, List<CollectionNode> children)
        {
            // create a list to store children
            var c = new List<NodeReference>(children.Count);

            for (int i = 0; i < children.Count; i++)
                c.Add(children[i].First);

            // create list node
            return new ListNode() { Type = type, Children = c };
        }

        private static Node ParseObject(StreamReader reader, Type type, Dom dom)
        {
            var children = new Dictionary<string, NodeReference>();

            // skip over {
            reader.Read();
            SkipWhitespace(reader);

            // read members
            while ((char)reader.Peek() != '}')
            {
                // read member name
                var ident = ParseIdentifier(reader);

                // skip over :
                reader.Read();

                // read value
                var value = ParseItem(reader, dom);
                SkipWhitespace(reader);
                
                // skip over ,
                if ((char)reader.Peek() == ',')
                    reader.Read();

                children.Add(ident, value);
                SkipWhitespace(reader);
            }

            // skip over }
            reader.Read();

            // create object node
            return new ObjectNode() { Type = type, Children = children };
        }

        private static Node ParseLiteral(StreamReader reader, Type type)
        {
            var sb = new StringBuilder();
            var escaped = true;
            int character;

            reader.Read();

            // read until we get to the closing "
            while ((character = reader.Read()) != '"' || escaped)
            {
                sb.Append((char)character);
                escaped = character == '\\';
            }
            
            // create literal node
            return new LiteralNode() { Type = type, Value = sb.ToString() };
        }
        
        private static Type ParseTypeName(StreamReader stream)
        {
            // type name can be an identifier, followed by generic parameters in < >
            // nested types are prepended with their declaring type and a separating +
            // generic type arguments must be collected together

            var genericParameters = new List<Type>();
            string identifier = string.Empty;
            bool nestedType;

            // loop while we are going through nested types
            do
            {
                nestedType = false;

                // append the type name
                identifier += ParseIdentifier(stream);

                if (identifier.Length == 0)
                    return null;

                // parse generic type parameter list
                if ((char)stream.Peek() == '<')
                {
                    // skip over <
                    stream.Read();

                    var numParameters = 0;
                    while (true)
                    {
                        // parse parameter
                        var parameter = ParseTypeName(stream);
                        genericParameters.Add(parameter);
                        numParameters++;
                        
                        // stop if we reach the end of the param list
                        var seperator = stream.Read();
                        if ((char)seperator == '>')
                            break;

                        SkipWhitespace(stream);
                    }

                    // append the identifier with the number of generic parameters
                    identifier = string.Format("{0}`{1}", identifier, numParameters);
                }

                // check if we have a nested type
                if ((char)stream.Peek() == '+')
                {
                    stream.Read();
                    identifier += "+";
                    nestedType = true;
                }
            } while (nestedType);

            // check if we are creating an array
            bool isArray = false;
            if ((char)stream.Peek() == '[')
            {
                stream.Read();
                stream.Read();
                isArray = true;
            }

            // find Type instance
            return FindTypeByName(identifier, genericParameters.ToArray(), isArray);
        }

        private static string ParseIdentifier(StreamReader stream)
        {
            var sb = new StringBuilder();

            char c;
            while (((c = (char)stream.Peek()) >= 'A' && c <= 'Z')
                || (c >= 'a' && c <= 'z')
                || (c >= '0' && c <= '9')
                || c == '_'
                || c == '.')
            {
                sb.Append((char)stream.Read());
            }

            return sb.ToString();
        }

        private static Dictionary<string, Type> types = new Dictionary<string, Type>();
        private static Dictionary<Type, Type> arrayTypes = new Dictionary<Type, Type>();
        private static Type FindTypeByName(string name, Type[] genericParameters, bool isArray)
        {
            // check cache first
            Type type;
            if (!types.TryGetValue(name, out type))
            {
                // search through each assembly for the type
                type = (from a in GetAssemblies()
                        from t in EnumerateTypes(a)
                        where t.FullName.EndsWith(name) && (t.FullName.Length == name.Length || t.FullName[t.FullName.Length - name.Length - 1] == '.')
                        select t).FirstOrDefault();

                // throw exception if we cannot find it
                if (type == null)
                    throw new Exception(string.Format("Type {0} not found", name));

                // convert into generic type
                if (genericParameters.Length > 0)
                    type = type.MakeGenericType(genericParameters);
                
                types[name] = type;
            }

            // convert into array
            if (isArray)
            {
                // check cache first
                Type arrayType;
                if (!arrayTypes.TryGetValue(type, out arrayType))
                {
                    arrayType = type.MakeArrayType();
                    arrayTypes[type] = arrayType;
                }

                type = arrayType;
            }

            return type;
        }

        private static Type[] EnumerateTypes(Assembly a)
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                // log exception and swollow
                Trace.Write(string.Format("Myre.Serialisation: Error loading types:\n{0}", e.LoaderExceptions));

                return new Type[0];
            }
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            // queue of loaded assemblies to search
            var assemblies = new Queue<Assembly>();

            // queue of assemblies yet to be loaded
            var toLoad = new Queue<string>();

            // set of assemblies which we can ignore
            var ignore = new HashSet<string>();

            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                // get loaded assemblies, and ignore them in the future
                assemblies.Enqueue(item);
                ignore.Add(item.GetName().FullName);
            }

            while (assemblies.Count > 0)
            {
                // return assembly on the top of the queue
                var assem = assemblies.Dequeue();
                yield return assem;

                foreach (var child in assem.GetReferencedAssemblies())
                {
                    // add child assemblies to toLoad, and ignore them in the future
                    if (ignore.Contains(child.FullName))
                        continue;

                    ignore.Add(child.FullName);
                    toLoad.Enqueue(child.FullName);
                }

                // load more assemblies
                if (assemblies.Count == 0 && toLoad.Count > 0)
                {
                    try
                    {
                        assemblies.Enqueue(Assembly.Load(toLoad.Dequeue()));
                    }
                    catch (FileNotFoundException e)
                    {
                        // log exception and swollow
                        Trace.Write(string.Format("Myre.Serialisation: Error loading assembly:\n{0}", e.Message));
                    }
                }
            }
        }
    }
}
