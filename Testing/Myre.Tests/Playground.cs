using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace Myre.Tests
{
    [TestClass]
    public class Playground
    {
        private static Func<Vector3, Vector3> Swizzler(string str)
        {
            str = str.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(str) || str == "null" || str == "none" || str == "x,y,z")
                return a => a;

            var parts = str.Split(',');
            if (parts.Length != 3)
                throw new Exception(string.Format("Swizzle vector '{0}' has {1} elements; expected three", str, parts.Length));

            var vectors = new Vector4[4];
            for (int i = 0; i < 3; i++)
            {
                float p = parts[i].StartsWith("-") ? -1 : 1;

                if (parts[i].Contains("x"))
                    vectors[i].X = p;
                else if (parts[i].Contains("y"))
                    vectors[i].Y = p;
                else if (parts[i].Contains("z"))
                    vectors[i].Z = p;
            }

            var swizzle = new Matrix(
                vectors[0].X, vectors[1].X, vectors[2].X, vectors[3].X,
                vectors[0].Y, vectors[1].Y, vectors[2].Y, vectors[3].Y,
                vectors[0].Z, vectors[1].Z, vectors[2].Z, vectors[3].Z,
                vectors[0].W, vectors[1].W, vectors[2].W, vectors[3].W
            );

            return a => Vector3.Transform(a, swizzle);
        }

        [TestMethod]
        public void MethodName()
        {
            var a = new Vector3(1, 2, 3);
            var b = Swizzler("-z,+x,-x")(a);

            Assert.AreEqual(new Vector3(-3, 1, -1), b);
        }
    }
}
