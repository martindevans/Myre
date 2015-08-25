using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Myre.Collections
{
    /// <summary>
    /// An octree which allows inserting { Position, Object } tuples and supports efficient range queries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TokenOctree<T>
        : Octree
    {
        private readonly int _splitThreshold;

        private new TokenNode Root { get { return (TokenNode)base.Root; } }

        /// <summary>
        /// Constructs a new TokenOctree
        /// </summary>
        /// <param name="bounds">the bounds of the root of this tree</param>
        /// <param name="splitThreshold">the number of tokens in a node which causes it to automatically split</param>
        public TokenOctree(BoundingBox bounds, int splitThreshold)
            : base(bounds, (t, p, b) => new TokenNode(t, (TokenNode)p, b))
        {
            _splitThreshold = splitThreshold;
        }

        /// <summary>
        /// Insert a new token into the octree, possibly triggering Octree splitting
        /// </summary>
        /// <param name="position"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public TokenNode Insert(Vector3 position, T item)
        {
            return Root.Insert(position, item);
        }

        /// <summary>
        /// Remove a token from the octree
        /// </summary>
        /// <param name="position"></param>
        /// <param name="item"></param>
        public void Remove(Vector3 position, T item)
        {
            Root.Remove(position, item);
        }

        /// <summary>
        /// Gets all the items in the given bounding box
        /// </summary>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<Vector3, T>> ItemsInBounds(BoundingBox bounds)
        {
            return Root.ItemsInBounds(bounds);
        }

        /// <summary>
        /// An entry in a token octree
        /// </summary>
        public class TokenNode
            : Node
        {
            private readonly List<KeyValuePair<Vector3, T>> _tokens = new List<KeyValuePair<Vector3, T>>();

            /// <summary>
            /// Gets the children of this node (if any)
            /// </summary>
            public new IEnumerable<TokenNode> Children { get { return base.Children.Cast<TokenNode>(); } }

            /// <summary>
            /// Gets the tree which contains this node
            /// </summary>
            public new TokenOctree<T> Octree
            {
                get { return (TokenOctree<T>)base.Octree; }
            }

            /// <summary>
            /// Construct a new node
            /// </summary>
            /// <param name="tree"></param>
            /// <param name="parent"></param>
            /// <param name="bounds"></param>
            internal TokenNode(Octree tree, TokenNode parent, BoundingBox bounds)
                : base(tree, parent, bounds)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            protected override void Split()
            {
                base.Split();

                foreach (var token in _tokens)
                    Insert(token.Key, token.Value);
                _tokens.Clear();
            }

            /// <summary>
            /// Insert a token into this node
            /// </summary>
            /// <param name="position"></param>
            /// <param name="item"></param>
            /// <returns>The node the token was inserted into (if it was inserted into this/a child node) or null if the token was not inserted</returns>
            /// <exception cref="InvalidOperationException"></exception>
            public TokenNode Insert(Vector3 position, T item)
            {
                if (Bounds.Contains(position) == Microsoft.Xna.Framework.ContainmentType.Disjoint)
                    return null;

                if (IsSubdivided)
                {
                    foreach (var child in Children)
                    {
                        var i = child.Insert(position, item);
                        if (i != null)
                            return i;
                    }
                }
                else if (_tokens.Count + 1 > Octree._splitThreshold)
                {
                    Split();
                    return Insert(position, item);
                }
                else
                {
                    _tokens.Add(new KeyValuePair<Vector3, T>(position, item));
                    NotifyNodeModified();
                    return this;
                }

                throw new InvalidOperationException("Item is in octree bounds but was not inserted");
            }

            /// <summary>
            /// Try to remove an item at the given position from the tree
            /// </summary>
            /// <param name="position"></param>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Remove(Vector3 position, T item)
            {
                if (Bounds.Contains(position) == Microsoft.Xna.Framework.ContainmentType.Disjoint)
                    return false;

                if (IsSubdivided)
                {
                    if (Children.Any(child => child.Remove(position, item)))
                        return true;
                }
                else
                {
                    int index = _tokens.FindIndex(a => a.Key == position && a.Value.Equals(item));
                    if (index != -1)
                    {
                        _tokens.RemoveAt(index);
                        NotifyNodeModified();
                        return true;
                    }
                    return false;
                }

                return false;
            }

            /// <summary>
            /// Gets all the items in the given bounding box
            /// </summary>
            /// <param name="bounds"></param>
            /// <returns></returns>
            public IEnumerable<KeyValuePair<Vector3, T>> ItemsInBounds(BoundingBox bounds)
            {
                return NodesOverlappingBounds(bounds)
                    .Cast<TokenNode>()
                    .SelectMany(a => a._tokens
                        .Where(t => bounds.Contains(t.Key) != Microsoft.Xna.Framework.ContainmentType.Disjoint));
            }
        }
    }
}
