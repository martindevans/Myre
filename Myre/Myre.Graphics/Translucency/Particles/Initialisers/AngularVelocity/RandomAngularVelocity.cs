using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Myre.Graphics.Translucency.Particles.Initialisers.AngularVelocity
{
    public class RandomAngularVelocity
        :IInitialiser
    {
        public float MinAngularVelocity { get; set; }
        public float MaxAngularVelocity { get; set; }

        public RandomAngularVelocity(float minAngularVelocity, float maxAngularVelocity)
        {
            MinAngularVelocity = minAngularVelocity;
            MaxAngularVelocity = maxAngularVelocity;
        }

        public void Initialise(Random random, ref Particle particle)
        {
            var angularVelocity = MathHelper.Lerp(MinAngularVelocity, MaxAngularVelocity, (float)random.NextDouble());
            particle.AngularVelocity += angularVelocity;
        }

        public object Clone()
        {
            return new RandomAngularVelocity(MinAngularVelocity, MaxAngularVelocity);
        }
    }

    public class RandomAngularVelocityReader : ContentTypeReader<RandomAngularVelocity>
    {
        protected override RandomAngularVelocity Read(ContentReader input, RandomAngularVelocity existingInstance)
        {
            return new RandomAngularVelocity(input.ReadSingle(), input.ReadSingle());
        }
    }
}
