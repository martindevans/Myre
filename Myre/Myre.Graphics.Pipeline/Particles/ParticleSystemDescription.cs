using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Pipeline.Particles.Initialisers;
using Myre.Graphics.Pipeline.Particles.Triggers;

namespace Myre.Graphics.Pipeline.Particles
{
    public class ParticleSystemDescription
    {
        public BlendState BlendState;
        public float EndLinearVelocity;
        public float EndScale;
        public Vector3 Gravity;
        public float Lifetime;
        public string Texture;
        public int Capacity;

        public ITrigger[] Triggers;

        public IInitialiser[] Initialisers;
    }
}
