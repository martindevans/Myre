using System;
using Microsoft.Xna.Framework;
using Myre.Extensions;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Velocity
{
    public class RandomVelocity
        :IInitialiser
    {
        public Vector3 VelocityVariance { get; set; }

        public RandomVelocity(Vector3 variance)
        {
            VelocityVariance = variance;
        }

        public void Initialise(Random random, ref Particle particle)
        {
            var randomVector = random.RandomNormalVector() * VelocityVariance;
            particle.Velocity += randomVector;
        }
    }
}
