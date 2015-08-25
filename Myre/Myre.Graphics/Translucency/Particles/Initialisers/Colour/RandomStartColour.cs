using System;
using Microsoft.Xna.Framework.Content;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Colour
{
    public class RandomStartColour
        :BaseParticleInitialiser
    {
        public Color MinStartColour { get; set; }
        public Color MaxStartColour { get; set; }

        public RandomStartColour(Color minStartColour, Color maxStartColour)
        {
            MinStartColour = minStartColour;
            MaxStartColour = maxStartColour;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            particle.StartColour = new Color(particle.StartColour.ToVector4() + Color.Lerp(MinStartColour, MaxStartColour, (float) random.NextDouble()).ToVector4());
        }

        public override void Attach(ParticleEmitter emitter)
        {
        }

        public override object Clone()
        {
            return new RandomStartColour(MinStartColour, MaxStartColour);
        }
    }

    public class RandomStartColourReader : ContentTypeReader<RandomStartColour>
    {
        protected override RandomStartColour Read(ContentReader input, RandomStartColour existingInstance)
        {
            return new RandomStartColour(input.ReadColor(), input.ReadColor());
        }
    }
}
