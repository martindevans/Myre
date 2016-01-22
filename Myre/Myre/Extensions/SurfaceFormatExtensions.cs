using System;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;

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
                case SurfaceFormat.Color:
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.NormalizedByte2:
                case SurfaceFormat.NormalizedByte4:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.Alpha8:
                    return false;
                default:
                    throw new ArgumentException(string.Format("Unknown format '{0}'", format), "format");
            }
        }

        /// <summary>
        /// Gets the byte size of a render target format
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static float FormatSize(this SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Dxt3:        //1 byte per pixel, 16 pixels in a 128 bit (8 byte) block
                case SurfaceFormat.Dxt5:        //1 byte per pixel, 16 pixels in a 128 bit (8 byte) block. Arranged differently to DXT3
                case SurfaceFormat.Alpha8:
                    return 1;

                case SurfaceFormat.Color:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.Single:
                case SurfaceFormat.NormalizedByte4:
                    return 4;

                case SurfaceFormat.NormalizedByte2:
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.HalfSingle:
                    return 2;

                case SurfaceFormat.Rgba64:
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.HdrBlendable:
                case SurfaceFormat.Vector2:
                    return 8;

                case SurfaceFormat.Vector4:
                    return 16;

                case SurfaceFormat.Dxt1:
                    return 0.5f;            //Half a bit per pixel!? DXT1 stores 16 pixels in a 64 bit (8 byte) block

                default:
                    throw new ArgumentException(string.Format("Unknown format '{0}'", format), "format");
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
                case DepthFormat.None:
                    return 0;
                case DepthFormat.Depth16:
                    return 2;
                case DepthFormat.Depth24:
                    return 3;
                case DepthFormat.Depth24Stencil8:
                    return 4;
                default:
                    throw new ArgumentException(string.Format("Unknown format '{0}'", format), "format");
            }
        }
    }
}
