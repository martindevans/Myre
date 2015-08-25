using System;
using System.Numerics;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.Rectangle struct.
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        /// Transforms the <see cref=" Microsoft.Xna.Framework.Rectangle"/> with a specified <see cref="Matrix4x4"/>.
        /// </summary>
        /// <param name="rect">The rectangle to transform.</param>
        /// <param name="m">The matrix with which to do the transformation.</param>
        /// <returns>The transformed rectangle.</returns>
        public static Microsoft.Xna.Framework.Rectangle Transform(this  Microsoft.Xna.Framework.Rectangle rect, ref Matrix4x4 m)
        {
            // get corners
            Vector2 topLeft = new Vector2(rect.X, rect.Y);
            Vector2 topRight = new Vector2(rect.X + rect.Width, rect.Y);
            Vector2 bottomLeft = new Vector2(rect.X, rect.Y + rect.Height);
            Vector2 bottomRight = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

            // transform corners
            var newTopLeft = Vector2.Transform(topLeft, m);
            var newTopRight = Vector2.Transform(topRight, m);
            var newBottomRight = Vector2.Transform(bottomRight, m);
            var newBottomLeft = Vector2.Transform(bottomLeft, m);

            // find new extents
            Vector2 min = new Vector2(
                Math.Min(newTopLeft.X, Math.Min(newTopRight.X, Math.Min(newBottomLeft.X, newBottomRight.X))),
                Math.Min(newTopLeft.Y, Math.Min(newTopRight.Y, Math.Min(newBottomLeft.Y, newBottomRight.Y))));
            Vector2 max = new Vector2(
                Math.Max(newTopLeft.X, Math.Max(newTopRight.X, Math.Max(newBottomLeft.X, newBottomRight.X))),
                Math.Max(newTopLeft.Y, Math.Max(newTopRight.Y, Math.Max(newBottomLeft.Y, newBottomRight.Y))));

            // create and return rectangle
            return new Microsoft.Xna.Framework.Rectangle(
                (int)min.X,
                (int)min.Y,
                (int)(max.X - min.X),
                (int)(max.Y - min.Y));
        }
    }
}
