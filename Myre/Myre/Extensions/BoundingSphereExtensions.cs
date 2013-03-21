using System;
using Microsoft.Xna.Framework;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.BoundingSphere struct.
    /// </summary>
    public static class BoundingSphereExtensions
    {
        /// <summary>
        /// Transformes the <see cref="BoundingSphere"/> with a specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="sphere">The rectangle to transform.</param>
        /// <param name="m">The matrix with which to do the transformation.</param>
        /// <returns>The transformed <see cref="BoundingSphere"/>.</returns>
        public static BoundingSphere Transform(this BoundingSphere sphere, ref Matrix m)
        {
            return new BoundingSphere(
                Vector3.Transform(sphere.Center, m),
                sphere.Radius * Math.Max(m.M11, Math.Max(m.M22, m.M33)));
        }
    }
}
