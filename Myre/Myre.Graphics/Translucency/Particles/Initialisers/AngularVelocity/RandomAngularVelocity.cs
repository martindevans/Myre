using System;
using Microsoft.Xna.Framework.Content;

using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Myre.Graphics.Translucency.Particles.Initialisers.AngularVelocity
{
    public class RandomAngularVelocity
        :BaseParticleInitialiser
    {
        public float MinAngularVelocity { get; set; }
        public float MaxAngularVelocity { get; set; }

        public RandomAngularVelocity(float minAngularVelocity, float maxAngularVelocity)
        {
            MinAngularVelocity = minAngularVelocity;
            MaxAngularVelocity = maxAngularVelocity;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            var angularVelocity = MathHelper.Lerp(MinAngularVelocity, MaxAngularVelocity, (float)random.NextDouble());

            Modify(ref particle, angularVelocity);
        }

        public override void Maximise(ref Particle particle)
        {
            Modify(ref particle, MaxAngularVelocity);
        }

        private static void Modify(ref Particle particle, float angularVelocity)
        {
            particle.AngularVelocity += angularVelocity;
        }

        public override void Attach(ParticleEmitter emitter)
        {
        }

        public override object Clone()
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
