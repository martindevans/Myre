using System;
using Microsoft.Xna.Framework;

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
    }
}
