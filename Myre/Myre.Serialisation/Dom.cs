using System;
using System.Collections.Generic;
using System.IO;
using Ninject;

namespace Myre.Serialisation
{
    /// <summary>
    /// A document object model which can describe a serialised object.
    /// </summary>
    public partial class Dom
    {
        public struct NodeReference
        {
            private Dom dom;
            private int? id;
            private Node node;

            public NodeReference(Dom dom, Node node)
            {
                this.dom = dom;
                this.node = node;
                this.id = null;
            }

            public NodeReference(Dom dom, int id)
            {
                this.dom = dom;
                this.node = null;
                this.id = id;
            }

            public Node GetNode()
            {
                if (node == null && id != null)
                    node = dom.sharedReferences[id.Value];

                return node;
            }

            public void Write(Stream stream, int indent)
            {
                if (id == null && node != null && node.SharedReferenceID != null)
                    id = node.SharedReferenceID;

                if (id != null)
                    Dom.Write(stream, string.Format("#{0}", id.Value));
                else
                    GetNode().Write(stream, indent);
            }
        }

        #region nodes
        public abstract partial class Node
        {
            public Type Type;
            public int? SharedReferenceID;
        }

        public sealed partial class ListNode
            : Node
        {
            public List<NodeReference> Children;
        }

        public sealed partial class DictionaryNode
            : Node
        {
            public Dictionary<NodeReference, NodeReference> Children;
        }

        public sealed partial class LiteralNode
            : Node
        {
            public string Value;
        }

        public sealed partial class ObjectNode
            : Node
        {
            public Dictionary<string, NodeReference> Children;
        }
        #endregion

        /// <summary>
        /// Gets the root DOM node.
        /// </summary>
        public NodeReference Root { get; private set; }

        /// <summary>
        /// Gets the kernel used to instantiate new objects during deserialisation.
        /// </summary>
        public IKernel Kernel { get; private set; }

        private Dom(IKernel kernel)
        {
            this.Kernel = kernel;
        }
    }
}
