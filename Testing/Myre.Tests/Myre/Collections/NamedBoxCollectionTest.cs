using Microsoft.VisualStudio.TestTools.UnitTesting;
using Myre.Collections;
using Assert2 = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Myre.Tests.Myre.Collections
{
    /// <summary>
    /// Summary description for BoxedDataCollection
    /// </summary>
    [TestClass]
    public class NamedBoxCollectionTest
    {
        [TestMethod]
        public void CollectionInitializer()
        {
            NamedBoxCollection store = new NamedBoxCollection {
                { new TypedName<int>("a"), 1 },
                { new TypedName<float>("b"), 2f },
                { new TypedName<string>("c"), "3" },
            };

            Assert2.AreEqual(1, store.GetValue(new TypedName<int>("a")));
            Assert2.AreEqual(2, store.GetValue(new TypedName<float>("b")));
            Assert2.AreEqual("3", store.GetValue(new TypedName<string>("c")));
        }
    }
}
