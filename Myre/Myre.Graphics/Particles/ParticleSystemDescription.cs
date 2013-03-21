using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Particles
{
    ///// <summary>
    ///// Describes blend modes for particles.
    ///// </summary>
    //public enum ParticleBlendMode
    //{
    //    /// <summary>
    //    /// FinalColour = DestinationColour * (1 - SourceAlpha) + SourceColour
    //    /// </summary>
    //    Additive,

    //    /// <summary>
    //    /// FinalColour = DestinationColour * (1 - SourceAlpha) - SourceColour
    //    /// </summary>
    //    Subtractive
    //}

    public enum ParticleType
    {
        Hard,
        Soft
    }

    public struct ParticleSystemDescription
    {
        /// <summary>
        /// Gets or sets the type of particles.
        /// </summary>
        public ParticleType Type { get; set; }

        /// <summary>
        /// Gets or sets the texture.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets or sets the blend state.
        /// </summary>
        /// <value>The blend state.</value>
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets the lifetime (in seconds).
        /// </summary>
        /// <value>The lifetime.</value>
        public float Lifetime { get; set; }

        /// <summary>
        /// Gets or sets the proportion of a particles original linear velocity which is remaining at the end of its' lifetime.
        /// </summary>
        /// <value>The end linear velocity.</value>
        public float EndLinearVelocity { get; set; }

        /// <summary>
        /// Gets or sets the proportion of the particles original size which is remaining at the end of its' lifetime.
        /// </summary>
        public float EndScale { get; set; }

        /// <summary>
        /// Gets or sets a force to be constantly applied to particles.
        /// </summary>
        public Vector3 Gravity { get; set; }
    }
}
