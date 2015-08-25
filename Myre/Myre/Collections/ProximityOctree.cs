using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

namespace Myre.Collections
{
    /// <summary>
    /// Allows efficient position/range queries
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProximityOctreeDatabase<T>
    {
        private readonly int _xSize;
        private readonly int _ySize;
        private readonly int _zSize;
        private readonly int _octreeSplitThreshold;

        readonly ConcurrentDictionary<int, ConcurrentDictionary<int, ConcurrentDictionary<int, TokenOctree<ProximityToken>>>> _roots = new ConcurrentDictionary<int, ConcurrentDictionary<int, ConcurrentDictionary<int, TokenOctree<ProximityToken>>>>();

        /// <summary>
        /// Construct a new octree
        /// </summary>
        /// <param name="xSize">The size of the tree on the XAxis</param>
        /// <param name="ySize">The size of the tree on the YAxis</param>
        /// <param name="zSize">The size of the tree on the ZAxis</param>
        /// <param name="octreeSplitThreshold"></param>
        public ProximityOctreeDatabase(int xSize, int ySize, int zSize, int octreeSplitThreshold)
        {
            _xSize = xSize;
            _ySize = ySize;
            _zSize = zSize;
            _octreeSplitThreshold = octreeSplitThreshold;
        }

        /// <summary>
        /// Insert a new item at the given position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="item"></param>
        /// <returns>A token which represents this item in the database</returns>
        public ProximityToken Insert(Vector3 position, T item)
        {
            return new ProximityToken(item, position, this);
        }

        private TokenOctree<ProximityToken>.TokenNode Insert(Vector3 position, ProximityToken proximityToken)
        {
            var octree = GetOctree(position);
            return octree.Insert(position, proximityToken);
        }

        private Int3 GetCoordinate(Vector3 position)
        {
            int x = (int)Math.Floor(position.X / _xSize);
            int y = (int)Math.Floor(position.Y / _ySize);
            int z = (int)Math.Floor(position.Z / _zSize);

            return new Int3(x, y, z);
        }

        private TokenOctree<ProximityToken> GetOctree(Vector3 position)
        {
            Int3 coords = GetCoordinate(position);

            return GetOctree(coords);
        }

        private TokenOctree<ProximityToken> GetOctree(Int3 coords)
        {
            var yDict = _roots.GetOrAdd(coords.X, i => new ConcurrentDictionary<int, ConcurrentDictionary<int, TokenOctree<ProximityToken>>>());
            var zDict = yDict.GetOrAdd(coords.Y, i => new ConcurrentDictionary<int, TokenOctree<ProximityToken>>());

            var min = new Vector3(coords.X * _xSize, coords.Y * _ySize, coords.Z * _zSize);
            var max = min + new Vector3(_xSize, _ySize, _zSize);

            var octree = zDict.GetOrAdd(coords.Z, i => new TokenOctree<ProximityToken>(new BoundingBox(min, max), _octreeSplitThreshold));

            return octree;
        }

        /// <summary>
        /// Fetches all items in the given bounds
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<Vector3,T>> ItemsInBounds(BoundingBox box)
        {
            Int3 min = GetCoordinate(box.Min);
            Int3 max = GetCoordinate(box.Max);

            for (int i = min.X; i <= max.X; i++)
            {
                for (int j = min.Y; j <= max.Y; j++)
                {
                    for (int k = min.Z; k <= max.Z; k++)
                    {
                        var oct = GetOctree(new Int3(i, j, k));
                        if (oct == null)
                            continue;
                        foreach (var item in oct.ItemsInBounds(box))
                            yield return new KeyValuePair<Vector3, T>(item.Key, item.Value.Item);
                    }
                }
            }
        }

        /// <summary>
        /// A token which represents an item with a position
        /// </summary>
        public sealed class ProximityToken
            :IDisposable
        {
            private readonly ProximityOctreeDatabase<T> _db;
            TokenOctree<ProximityToken>.TokenNode _currentNode = null;

            private Vector3 _position;
            /// <summary>
            /// The position of the item
            /// </summary>
            public Vector3 Position
            {
                get { return _position; }
                set { UpdatePosition(value); }
            }

            /// <summary>
            /// The item
            /// </summary>
            public T Item { get; private set; }

            internal ProximityToken(T item, Vector3 initialPosition, ProximityOctreeDatabase<T> db)
            {
                _db = db;
                Item = item;
                Position = initialPosition;
            }

            private void UpdatePosition(Vector3 position)
            {
                if (_currentNode != null && position == _position)
                    return;

                if (_currentNode != null)
                    _currentNode.Remove(Position, this);

                if (_currentNode != null && _currentNode.Octree.Contains(position))
                    _currentNode = _currentNode.Octree.Insert(position, this);
                else
                {
                    _currentNode = null;
                    _currentNode = _db.Insert(position, this);
                }

                _position = position;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 
            /// </summary>
            ~ProximityToken()
            {
                Dispose(false);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="disposing"></param>
            private void Dispose(Boolean disposing)
            {
                if (disposing && _currentNode != null)
                    if (!_currentNode.Remove(Position, this))
                        throw new InvalidOperationException("Failed to remove item");
                _currentNode = null;
            }
        }
    }
}
