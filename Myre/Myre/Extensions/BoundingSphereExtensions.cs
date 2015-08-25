using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.BoundingSphere struct.
    /// </summary>
    public static class BoundingSphereExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Xna.Framework.BoundingSphere ToXNA(this BoundingSphere sphere)
        {
            return new Microsoft.Xna.Framework.BoundingSphere(sphere.Center.ToXNA(), sphere.Radius);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoundingSphere FromXNA(this Microsoft.Xna.Framework.BoundingSphere sphere)
        {
            return new BoundingSphere(sphere.Center.FromXNA(), sphere.Radius);
        }

        /// <summary>
        /// Transformes the <see cref="BoundingSphere"/> with a specified <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="sphere">The rectangle to transform.</param>
        /// <param name="m">The matrix with which to do the transformation.</param>
        /// <returns>The transformed <see cref="BoundingSphere"/>.</returns>
        public static BoundingSphere Transform(this BoundingSphere sphere, ref Matrix4x4 m)
        {
            return new BoundingSphere(
                Vector3.Transform(sphere.Center, m),
                sphere.Radius * Math.Max(m.M11, Math.Max(m.M22, m.M33)));
        }

        /// <summary>
        /// Transformes the <see cref="BoundingSphere"/> with a specified <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="sphere">The rectangle to transform.</param>
        /// <param name="m">The matrix with which to do the transformation.</param>
        /// <returns>The transformed <see cref="BoundingSphere"/>.</returns>
        public static BoundingSphere Transform(this BoundingSphere sphere, Matrix4x4 m)
        {
            return sphere.Transform(ref m);
        }
    }
}
