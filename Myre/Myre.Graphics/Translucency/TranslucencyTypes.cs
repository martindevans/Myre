
using System;

namespace Myre.Graphics.Translucency
{
    [Flags]
    public enum TranslucencyTypes
        : byte
    {
        /// <summary>
        /// Alpha is ignored, item is not transparent
        /// </summary>
        Opaque = 1,

        /// <summary>
        /// Render into the gbuffer as normal, reject pixels below 50% opacity
        /// </summary>
        Cutout = 2,

        /// <summary>
        /// Alpha blend over the top of the lightbuffer
        /// </summary>
        AlphaBlend = 4,

        /// <summary>
        /// Render transparent items as opaque, layer by layer, into a new gbuffer and then blend the resulting lightbuffers
        /// </summary>
        DepthPeel = 8,
    }
}
