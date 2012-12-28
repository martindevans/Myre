using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Myre.Graphics.Geometry;
using TestAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Myre.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DepthSort()
        {
            var view = Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitX, Vector3.Up);

            Matrix worldViewA = Matrix.CreateTranslation(10, 0, 0) * view;
            Matrix worldViewB = Matrix.CreateTranslation(20, 0, 0) * view;

            TestAssert.AreEqual(-1, ModelInstance.Manager.CompareWorldViews(ref worldViewA, ref worldViewB));
        }
    }
}
