using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

        public object Clone()
        {
            return new RandomVelocity(VelocityVariance);
        }
    }

    public class RandomVelocityReader : ContentTypeReader<RandomVelocity>
    {
        protected override RandomVelocity Read(ContentReader input, RandomVelocity existingInstance)
        {
            return new RandomVelocity(input.ReadVector3());
        }
    }
}
