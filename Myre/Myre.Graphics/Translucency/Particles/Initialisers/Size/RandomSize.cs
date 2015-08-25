using System;
using Microsoft.Xna.Framework.Content;

using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Size
{
    public class RandomSize
        :BaseParticleInitialiser
    {
        public float MinSize { get; set; }
        public float MaxSize { get; set; }

        public RandomSize(float minSize, float maxSize)
        {
            MinSize = minSize;
            MaxSize = maxSize;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            var size = MathHelper.Lerp(MinSize, MaxSize, (float)random.NextDouble());
            particle.Size += size;
        }

        public override object Clone()
        {
            return new RandomSize(MinSize, MaxSize);
        }

        public override void Attach(ParticleEmitter emitter)
        {
        }
    }

    public class RandomSizeReader : ContentTypeReader<RandomSize>
    {
        protected override RandomSize Read(ContentReader input, RandomSize existingInstance)
        {
            return new RandomSize(input.ReadSingle(), input.ReadSingle());
        }
    }
}
