using System;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Translucency.Particles.Initialisers;

namespace Myre.Graphics.Translucency.Particles.Triggers
{
    public class Continuous
        :ITrigger
    {
        public float EmitsPerSecond { get; set; }

        private ParticleEmitter _emitter;
        private float _unspentTime = 0;

        public Continuous(float emitsPerSecond)
        {
            EmitsPerSecond = emitsPerSecond;
        }

        public void Attach(ParticleEmitter emitter)
        {
            _emitter = emitter;
        }

        public void Update(float dt)
        {
            var emitsPerSecond = Math.Min(_emitter.Capacity / _emitter.Lifetime, EmitsPerSecond);
            var timePerParticle = 1f / emitsPerSecond;

            // If we had any time left over that we didn't use during the
            // previous update, add that to the current elapsed time.
            float timeToSpend = _unspentTime + dt;

            // Create particles as long as we have a big enough time interval.
            while (timeToSpend > timePerParticle)
            {
                timeToSpend -= timePerParticle;

                // Create the particle.
                _emitter.Spawn(new Particle());
            }

            // Store any time we didn't use, so it can be part of the next update.
            _unspentTime = timeToSpend;
        }

        public object Copy()
        {
            return new Continuous(EmitsPerSecond);
        }

        public void Reset()
        {
        }
    }

    public class ContinuousReader : ContentTypeReader<Continuous>
    {
        protected override Continuous Read(ContentReader input, Continuous existingInstance)
        {
            return new Continuous(input.ReadSingle());
        }
    }
}
