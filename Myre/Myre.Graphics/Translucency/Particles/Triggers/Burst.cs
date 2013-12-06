using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Translucency.Particles.Initialisers;

namespace Myre.Graphics.Translucency.Particles.Triggers
{
    public class Burst
        :ITrigger
    {
        public float BurstLength { get; set; }
        public int BurstParticles { get; set; }
        public float BurstDelay { get; set; }

        private ParticleEmitter _emitter;
        private float _time;

        public Burst(float length, int particles, float delay)
        {
            BurstLength = length;
            BurstParticles = particles;
            BurstDelay = delay;
        }

        public void Attach(ParticleEmitter emitter)
        {
            _emitter = emitter;
        }

        public void Update(float dt)
        {
            _time += dt;

            if (_time < BurstLength + BurstDelay && _time >= BurstDelay)
            {
                var particlesPerFrame = (BurstParticles / BurstLength) * dt;
                for (int i = 0; i < particlesPerFrame; i++)
                    _emitter.Spawn(new Particle());
            }
        }

        public object Copy()
        {
            return new Burst(BurstLength, BurstParticles, BurstDelay);
        }

        public void Reset()
        {
            _time = 0;
        }
    }

    public class BurstReader : ContentTypeReader<Burst>
    {
        protected override Burst Read(ContentReader input, Burst existingInstance)
        {
            return new Burst(input.ReadSingle(), input.ReadInt32(), input.ReadSingle());
        }
    }
}
