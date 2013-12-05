using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Graphics.Translucency.Particles.Initialisers;
using Ninject;

namespace Myre.Graphics.Translucency.Particles
{
    public class EntityParticleEmitter
        : ParticleEmitter
    {
        private Property<Vector3> _position;
        private Vector3 _previousPosition;

        private float _unspentTime;

        public float VelocityBleedThrough { get; set; }
        public float EmitPerSecond { get; set; }

        public EntityParticleEmitter(IKernel kernel)
            : base(kernel)
        {
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _position = context.CreateProperty<Vector3>("position");

            base.CreateProperties(context);
        }

        protected override void Update(float dt)
        {
            base.Update(dt);

            _previousPosition = _position.Value;

            var emitterVelocity = (_position.Value - _previousPosition) / dt;
            var baseParticleVelocity = emitterVelocity * VelocityBleedThrough;

            var timePerParticle = 1f / EmitPerSecond;

            // If we had any time left over that we didn't use during the
            // previous update, add that to the current elapsed time.
            float timeToSpend = _unspentTime + dt;

            // Counter for looping over the time interval.
            float currentTime = -_unspentTime;

            // Create particles as long as we have a big enough time interval.
            while (timeToSpend > timePerParticle)
            {
                currentTime += timePerParticle;
                timeToSpend -= timePerParticle;

                // Work out the optimal position for this particle. This will produce
                // evenly spaced particles regardless of the object speed, particle
                // creation frequency, or game update rate.
                var mu = currentTime / dt;
                var particlePosition = Vector3.Lerp(_previousPosition, _position.Value, mu);

                // Create the particle.
                Spawn(new Particle(particlePosition, baseParticleVelocity, 0, 0, 0, Color.Transparent, Color.Transparent));
            }

            // Store any time we didn't use, so it can be part of the next update.
            _unspentTime = timeToSpend;
            _previousPosition = _position.Value;
        }
    }
}
