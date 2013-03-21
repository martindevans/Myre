using Microsoft.Xna.Framework.Graphics;

namespace Myre.Extensions
{
    /// <summary>
    /// A static class containing extension methods for the Microsoft.Xna.Framework.Graphics.SurfaceFormat enum.
    /// </summary>
    public static class SurfaceFormatExtensions
    {
        /// <summary>
        /// Determines whether the specified format is floating point.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// 	<c>true</c> if the specified format is floating point; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFloatingPoint(this SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Single:
                case SurfaceFormat.Vector2:
                case SurfaceFormat.Vector4:
                case SurfaceFormat.HdrBlendable:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the byte size of a render target format
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static int FormatSize(this SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Color:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.Single:
                    return 4;
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.HalfSingle:
                    return 2;
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.Vector2:
                    return 8;
                case SurfaceFormat.Vector4:
                    return 16;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the byte size of a render target format
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static int FormatSize(this DepthFormat format)
        {
            switch (format)
            {
                case DepthFormat.Depth16:
                    return 2;
                case DepthFormat.Depth24:
                    return 3;
                case DepthFormat.Depth24Stencil8:
                    return 4;
                default:
                    return 0;
            }
        }
    }
}
