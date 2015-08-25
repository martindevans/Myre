using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Myre.Extensions;

namespace Myre.Tests.Myre.Extensions
{
    [TestClass]
    public class PlaneExtensionsTest
    {
        [TestMethod]
        public void DistanceToPointFromPlane()
        {
            Plane p = PlaneExtensions.CreateFromPoints(new Vector3(0, 0.5f, 0.8660254f), new Vector3(0.75f, 0.5f, -0.4330128f), new Vector3(-0.75f, 0.5f, -0.4330126f));

            Assert.AreEqual(-0.5f, p.D);

            //Point below plane
            Assert.AreEqual(-0.5f, p.Distance(new Vector3(0, 0, 0)));

            //Point above plane
            Assert.AreEqual(0.25f, p.Distance(new Vector3(0, 0.75f, 0)));
        }

        [TestMethod]
        public void ClosestPointOnPlane()
        {
            Plane p = PlaneExtensions.CreateFromPoints(new Vector3(0, 0.5f, 0.8660254f), new Vector3(0.75f, 0.5f, -0.4330128f), new Vector3(-0.75f, 0.5f, -0.4330126f));

            //Point below plane
            Assert.AreEqual(new Vector3(0, 0.5f, 0), p.ClosestPoint(new Vector3(0, 0, 0)));

            //Point above plane
            Assert.AreEqual(new Vector3(0, 0.5f, 0), p.ClosestPoint(new Vector3(0, 1, 0)));
        }
    }
}
