using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        #region nodes
        public partial class Node
        {
            public virtual void Write(Stream stream, int indent)
            {
                // write type name
                string name = GetTypeName(Type);

                Dom.Write(stream, name);
                Dom.Write(stream, " ");
            }

            private static string GetTypeName(Type type)
            {
                string name = type.Name;

                // append generic parameters
                if (type.IsGenericType)
                {
                    var sb = new StringBuilder();
                    sb.Append(name, 0, name.LastIndexOf('`'));

                    sb.Append('<');

                    var args = type.GetGenericArguments();
                    for (int i = 0; i < args.Length; i++)
                    {
                        // parameter name
                        sb.Append(GetTypeName(args[i]));

                        // comma separated list
                        if (i < args.Length - 1)
                            sb.Append(',');
                    }

                    sb.Append('>');

                    name = sb.ToString();
                }

                // prepend with declaring type if this is a nested type
                // separate with a +
                if (type.IsNested)
                    name = GetTypeName(type.DeclaringType) + "+" + name;

                return name;
            }
        }

        public partial class ListNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                // write type name
                base.Write(stream, indent);

                // start list
                Dom.Write(stream, "[");
                Dom.NewLine(stream, indent + 1);

                // write children
                for (int i = 0; i < Children.Count; i++)
                {
                    var item = Children[i];
                    item.Write(stream, indent + 1);

                    if (i < Children.Count - 1)
                    {
                        // comma separated
                        Dom.Write(stream, ",");
                        Dom.NewLine(stream, indent + 1);
                    }
                    else
                    {
                        Dom.NewLine(stream, indent);
                    }
                }

                // end list
                Dom.Write(stream, "]");
            }
        }

        public partial class DictionaryNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                // write type name
                base.Write(stream, indent);

                // start dictionary
                Dom.Write(stream, "[");
                Dom.NewLine(stream, indent + 1);

                // write children
                foreach (var item in Children.Select((kvp, i) => new { Key = kvp.Key, Value = kvp.Value, index = i }))
                {                
                    // write key:
                    item.Key.Write(stream, indent + 1);
                    Dom.Write(stream, ": ");

                    // write value
                    if (item.Value.GetNode() == null)
                        Dom.Write(stream, "null");
                    else
                        item.Value.Write(stream, indent + 1);

                    if (item.index < Children.Count - 1)
                    {
                        // comma separated
                        Dom.Write(stream, ",");
                        Dom.NewLine(stream, indent + 1);
                    }
                    else
                    {
                        Dom.NewLine(stream, indent);
                    }
                }

                // end dictionary
                Dom.Write(stream, "]");
            }
        }

        public partial class LiteralNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                base.Write(stream, indent);
                Dom.Write(stream, string.Format("\"{0}\"", Value));
            }
        }

        public partial class ObjectNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                // write type name
                base.Write(stream, indent);

                // start object
                Dom.Write(stream, "{");
                Dom.NewLine(stream, indent + 1);

                // write fields
                foreach (var item in Children.Select((kvp, i) => new { Key = kvp.Key, Value = kvp.Value, index = i }))
                {
                    // write field name:
                    Dom.Write(stream, item.Key);
                    Dom.Write(stream, ": ");

                    // write value
                    if (item.Value.GetNode() == null)
                        Dom.Write(stream, "null");
                    else
                        item.Value.Write(stream, indent + 1);

                    if (item.index < Children.Count - 1)
                    {
                        // comma separate
                        Dom.Write(stream, ",");
                        Dom.NewLine(stream, indent + 1);
                    }
                    else
                    {
                        Dom.NewLine(stream, indent);
                    }
                }

                // end object
                Dom.Write(stream, "}");
            }
        }
        #endregion

        private static byte[] buffer = new byte[100];

        public string Save()
        {
            var stream = new MemoryStream();
            Save(stream);
            stream.Flush();
            stream.Position = 0;

            var reader = new StreamReader(stream, Encoding.Unicode);
            return reader.ReadToEnd();
        }

        public void Save(Stream stream)
        {
            WriteSharedReferences(stream);
            Root.Write(stream, 0);
        }

        private void WriteSharedReferences(Stream stream)
        {
            if (sharedReferences.Count == 0)
                return;

            // start dictionary
            Dom.Write(stream, "R[");
            Dom.NewLine(stream, 1);

            // write children
            foreach (var item in sharedReferences.Select((kvp, i) => new { Key = kvp.Key, Value = kvp.Value, index = i }))
            {
                // write key:
                Dom.Write(stream, item.Key.ToString());
                Dom.Write(stream, ": ");

                // write value
                item.Value.Write(stream, 1);

                if (item.index < sharedReferences.Count - 1)
                {
                    // comma separated
                    Dom.Write(stream, ",");
                    Dom.NewLine(stream, 1);
                }
                else
                {
                    Dom.NewLine(stream, 0);
                }
            }

            // end dictionary
            Dom.Write(stream, "]");
            Dom.NewLine(stream, 0);
        }

        private static void Write(Stream stream, string value)
        {
            var length = Encoding.Unicode.GetByteCount(value);
            if (buffer.Length < length)
                buffer = new byte[length];

            Encoding.Unicode.GetBytes(value, 0, value.Length, buffer, 0);
            stream.Write(buffer, 0, length);
        }

        private static void NewLine(Stream stream, int indent)
        {
            Dom.Write(stream, "\n");
            for (int i = 0; i < indent; i++)
			    Dom.Write(stream, "  ");
        }
    }
}
