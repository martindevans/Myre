using System;
using MathHelperRedux;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Lifetime
{
    public class RandomLifetime
        :BaseParticleInitialiser
    {
        public float MinLifetimeScale { get; set; }
        public float MaxLifetimeScale { get; set; }

        public RandomLifetime(float minLifetimeScale, float maxLifetimeScale)
        {
            MinLifetimeScale = minLifetimeScale;
            MaxLifetimeScale = maxLifetimeScale;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            var lifetime = MathHelper.Lerp(MinLifetimeScale, MaxLifetimeScale, (float)random.NextDouble());
            Modify(ref particle, lifetime);
        }

        public override void Maximise(ref Particle particle)
        {
            Modify(ref particle, MaxLifetimeScale);
        }

        private static void Modify(ref Particle particle, float lifetime)
        {
            particle.LifetimeScale += lifetime;
        }

        public override void Attach(ParticleEmitter emitter)
        {
        }

        public override object Clone()
        {
            return new RandomLifetime(MinLifetimeScale, MaxLifetimeScale);
        }
    }

    public class RandomLifetimeReader : ContentTypeReader<RandomLifetime>
    {
        protected override RandomLifetime Read(ContentReader input, RandomLifetime existingInstance)
        {
            return new RandomLifetime(input.ReadSingle(), input.ReadSingle());
        }
    }
}
