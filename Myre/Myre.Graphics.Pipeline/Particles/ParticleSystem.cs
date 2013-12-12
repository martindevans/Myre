using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Pipeline.Particles
{
    public class ParticleSystem
    {
        public BlendState BlendState;
        public float EndLinearVelocity;
        public float EndScale;
        public Vector3 Gravity;
        public float Lifetime;
        public string Texture;
        public int Capacity;
    }
}
