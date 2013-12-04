using System;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Size
{
    public class RandomSize
        :IInitialiser
    {
        public float MinSize { get; set; }
        public float MaxSize { get; set; }

        public RandomSize(float minSize, float maxSize)
        {
            MinSize = minSize;
            MaxSize = maxSize;
        }

        public void Initialise(Random random, ref Particle particle)
        {
            var size = MathHelper.Lerp(MinSize, MaxSize, (float)random.NextDouble());
            particle.Size += size;
        }
    }
}
