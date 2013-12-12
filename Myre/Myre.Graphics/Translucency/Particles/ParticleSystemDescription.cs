using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Translucency.Particles
{
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

        /// <summary>
        /// Gets or sets the maximum number of particles this system can hold
        /// </summary>
        public int Capacity { get; set; }
    }

    public class ParticleSystemDescriptionReader
        : ContentTypeReader<ParticleSystemDescription>
    {
        protected override ParticleSystemDescription Read(ContentReader input, ParticleSystemDescription existingInstance)
        {
            return new ParticleSystemDescription
            {
                BlendState = input.ReadObject<BlendState>(),
                EndLinearVelocity = input.ReadSingle(),
                EndScale = input.ReadSingle(),
                Gravity = input.ReadVector3(),
                Lifetime = input.ReadSingle(),
                Texture = input.ContentManager.Load<Texture2D>(input.ReadString()),
                Capacity = input.ReadInt32(),
                Type = ParticleType.Hard
            };
        }
    }
}
