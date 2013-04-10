using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Myre.Collections
{
    /// <summary>
    /// An 8-Way tree which allows nodes to be split
    /// </summary>
    public class Octree
    {
        protected Node Root { get; private set; }

        /// <summary>
        /// Triggered every time a node is split
        /// </summary>
        public event Action<Node> NodeSplit;
        /// <summary>
        /// Triggered every time a node is modified
        /// </summary>
        public event Action<Node> NodeModified;

        private readonly Func<Octree, Node, BoundingBox, Node> _childFactory;

        /// <summary>
        /// Construct a new octree with a single root node covering the given bounds
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="childFactory"></param>
        public Octree(BoundingBox bounds, Func<Octree, Node, BoundingBox, Node> childFactory)
        {
            _childFactory = childFactory;
            Root = _childFactory(this, null, bounds);
        }

        /// <summary>
        /// Checks if the root of the tree contains the given point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Vector3 point)
        {
            return Root.Contains(point);
        }

        /// <summary>
        /// Get all nodes overlapping the bounds
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public IEnumerable<Node> NodesOverlappingBounds(BoundingBox bounds)
        {
            return Root.NodesOverlappingBounds(bounds);
        }

        private Node CreateChild(Node parent, BoundingBox bounds)
        {
            return _childFactory(this, parent, bounds);
        }

        private void InvokeNodeSplit(Node n)
        {
            if (NodeSplit != null)
                NodeSplit(n);
        }

        private void InvokeNodeModified(Node n)
        {
            if (NodeModified != null)
                NodeModified(n);
        }

        /// <summary>
        /// A node in an octree
        /// </summary>
        public class Node
        {
            /// <summary>
            /// The bounds of this node
            /// </summary>
            public BoundingBox Bounds { get; private set; }

            /// <summary>
            /// The tree which contains this node
            /// </summary>
            public Octree Octree
            {
                get;
                private set;
            }

            protected Node Parent { get; private set; }

            Node[] _children = null;

            protected IEnumerable<Node> Children
            {
                get { return _children ?? new Node[0]; }
            }

            protected bool IsSubdivided
            {
                get { return _children != null; }
            }

            internal Node(Octree octree, Node parent, BoundingBox bounds)
            {
                Octree = octree;
                Parent = parent;
                Bounds = bounds;
            }

            /// <summary>
            /// Checks if this node contains this point
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public bool Contains(Vector3 point)
            {
                return Bounds.Contains(point) != ContainmentType.Disjoint;
            }

            /// <summary>
            /// Split this node into 8 children. If this node is already split it will replace all the children with new children
            /// </summary>
            protected virtual void Split()
            {
                _children = new Node[8];
                int childIndex = 0;
                var min = Bounds.Min;
                var size = (Bounds.Max - Bounds.Min) / 2;
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            var positionOffset = size * new Vector3(x, y, z);
                            _children[childIndex++] = Octree.CreateChild(this, new BoundingBox(min + positionOffset, min + size + positionOffset));
                        }
                    }
                }

                Octree.InvokeNodeSplit(this);
            }

            /// <summary>
            /// Fetches all nodes (including non leaf nodes) which intersect the given bounds
            /// </summary>
            /// <param name="bounds"></param>
            /// <returns></returns>
            public IEnumerable<Node> NodesOverlappingBounds(BoundingBox bounds)
            {
                if (!bounds.Intersects(Bounds))
                    yield break;

                yield return this;

                if (_children != null)
                    foreach (var descendant in _children.SelectMany(c => c.NodesOverlappingBounds(bounds)))
                        yield return descendant;
            }

            protected void NotifyNodeModified()
            {
                Octree.InvokeNodeModified(this);
            }
        }
    }
}
