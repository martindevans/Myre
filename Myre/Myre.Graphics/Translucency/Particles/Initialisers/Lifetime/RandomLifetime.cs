using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Lifetime
{
    public class RandomLifetime
        :IInitialiser
    {
        public float MinLifetimeScale { get; set; }
        public float MaxLifetimeScale { get; set; }

        public RandomLifetime(float minLifetimeScale, float maxLifetimeScale)
        {
            MinLifetimeScale = minLifetimeScale;
            MaxLifetimeScale = maxLifetimeScale;
        }

        public void Initialise(Random random, ref Particle particle)
        {
            var lifetime = MathHelper.Lerp(MinLifetimeScale, MaxLifetimeScale, (float)random.NextDouble());
            particle.LifetimeScale += lifetime;
        }

        public object Clone()
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
