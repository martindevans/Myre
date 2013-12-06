using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Graphics.Translucency.Particles.Initialisers;
using Ninject;

namespace Myre.Graphics.Translucency.Particles
{
    public class EntityParticleEmitter
        : ParticleEmitter
    {
        private float _unspentTime;

        public float VelocityBleedThrough { get; set; }
        public float EmitPerSecond { get; set; }

        public EntityParticleEmitter(IKernel kernel)
            : base(kernel)
        {
        }

        protected override void Update(float dt)
        {
            if (System != null)
            {
                var timePerParticle = 1f / EmitPerSecond;

                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = _unspentTime + dt;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > timePerParticle)
                {
                    timeToSpend -= timePerParticle;

                    // Create the particle.
                    Spawn(new Particle(Vector3.Zero, Vector3.Zero, 0, 0, 0, Color.Transparent, Color.Transparent));
                }

                // Store any time we didn't use, so it can be part of the next update.
                _unspentTime = timeToSpend;
            }

            base.Update(dt);
        }
    }
}
