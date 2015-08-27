using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Myre.Collections;
using SwizzleMyVectors.Geometry;

namespace Myre.Tests.Myre.Collections
{
    [TestClass]
    public class Octree
    {
        [TestMethod]
        public void Construct()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ProximityOctreeDatabase<object>(100, 100, 100, 10);
        }

        [TestMethod]
        public void InsertItem()
        {
            ProximityOctreeDatabase<object> db = new ProximityOctreeDatabase<object>(100, 100, 100, 1);

            var o = new object();
            var token = db.Insert(new Vector3(0), o);

            Assert.IsNotNull(token);
            Assert.AreEqual(o, token.Item);
            Assert.AreEqual(new Vector3(0), token.Position);
        }

        [TestMethod]
        public void InsertItems()
        {
            ProximityOctreeDatabase<object> db = new ProximityOctreeDatabase<object>(100, 100, 100, 1);

            var o = new object();
            var token = db.Insert(new Vector3(20, 20, 20), o);

            Assert.IsNotNull(token);
            Assert.AreEqual(o, token.Item);
            Assert.AreEqual(new Vector3(20), token.Position);

            var o2 = new object();
            var token2 = db.Insert(new Vector3(10, 10, 10), o2);

            Assert.IsNotNull(token2);
            Assert.AreEqual(o2, token2.Item);
            Assert.AreEqual(new Vector3(10), token2.Position);
        }

        [TestMethod]
        public void RemoveItem()
        {
            ProximityOctreeDatabase<object> db = new ProximityOctreeDatabase<object>(100, 100, 100, 1);

            const string O = "a";
            var token = db.Insert(new Vector3(20, 20, 20), O);

            Assert.IsNotNull(token);
            Assert.AreEqual("a", token.Item);
            Assert.AreEqual(new Vector3(20), token.Position);

            token.Dispose();
        }

        [TestMethod]
        public void RemoveItems()
        {
            ProximityOctreeDatabase<object> db = new ProximityOctreeDatabase<object>(100, 100, 100, 1);

            var token = new[]
            {
                db.Insert(new Vector3(0, 0, 1), "a"),
                db.Insert(new Vector3(0, 0, 10), "b"),
                db.Insert(new Vector3(0, 0, 20), "c"),
                db.Insert(new Vector3(0, 0, 40), "d"),
                db.Insert(new Vector3(0, 0, 50), "e"),
                db.Insert(new Vector3(0, 0, 60), "f"),
                db.Insert(new Vector3(0, 0, 70), "g"),
            };

            token[0].Dispose();
        }

        [TestMethod]
        public void MoveItem()
        {
            ProximityOctreeDatabase<object> db = new ProximityOctreeDatabase<object>(10, 10, 10, 1);

            var o = new object();
            var token = db.Insert(new Vector3(5), o);

            Assert.IsNotNull(token);
            Assert.AreEqual(o, token.Item);
            Assert.AreEqual(new Vector3(5), token.Position);

            token.Position = new Vector3(10);

            Assert.IsNotNull(token);
            Assert.AreEqual(o, token.Item);
            Assert.AreEqual(new Vector3(10), token.Position);
        }

        [TestMethod]
        public void FindNearbyItems()
        {
            ProximityOctreeDatabase<object> db = new ProximityOctreeDatabase<object>(5, 5, 5, 1);

            // ReSharper disable once UnusedVariable
            var token = new[]
            {
                db.Insert(new Vector3(1), "a"),
                db.Insert(new Vector3(4), "b"),
                db.Insert(new Vector3(8), "c"),
                db.Insert(new Vector3(12), "d"),
                db.Insert(new Vector3(16), "e"),
                db.Insert(new Vector3(20), "f"),
                db.Insert(new Vector3(24), "g"),
            };

            var inBounds = db.ItemsInBounds(new BoundingBox(new Vector3(0), new Vector3(12))).ToArray();

            Assert.AreEqual(4, inBounds.Length);
            Assert.AreEqual(1, inBounds.Count(a => (string)a.Value == "a"));
            Assert.AreEqual(1, inBounds.Count(a => (string)a.Value == "b"));
            Assert.AreEqual(1, inBounds.Count(a => (string)a.Value == "c"));
            Assert.AreEqual(1, inBounds.Count(a => (string)a.Value == "d"));
        }
    }
}
