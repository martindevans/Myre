using System;
using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Myre.Extensions;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Velocity
{
    public class RandomVelocity
        :BaseParticleInitialiser
    {
        public Vector3 VelocityVariance { get; set; }

        public RandomVelocity(Vector3 variance)
        {
            VelocityVariance = variance;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            var randomVector = random.RandomNormalVector() * VelocityVariance;
            Modify(ref particle, randomVector);
        }

        public override void Maximise(ref Particle particle)
        {
            var len = particle.Velocity.Length();
            var pv = len < 0.01 ? Vector3.Normalize(VelocityVariance) : particle.Velocity / len;

            //Assume the random vector lines up with the particle velocity vector
            Modify(ref particle, pv * VelocityVariance);
        }

        private static void Modify(ref Particle particle, Vector3 velocity)
        {
            particle.Velocity += velocity;
        }

        public override object Clone()
        {
            return new RandomVelocity(VelocityVariance);
        }
    }

    public class RandomVelocityReader : ContentTypeReader<RandomVelocity>
    {
        protected override RandomVelocity Read(ContentReader input, RandomVelocity existingInstance)
        {
            return new RandomVelocity(input.ReadVector3().FromXNA());
        }
    }
}
