using SwizzleMyVectors.Geometry;
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
    }
}
