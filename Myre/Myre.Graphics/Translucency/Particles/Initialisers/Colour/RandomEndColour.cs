using System;
using Microsoft.Xna.Framework.Content;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Colour
{
    public class RandomEndColour
        :BaseParticleInitialiser
    {
        public Color MinEndColour { get; set; }
        public Color MaxEndColour { get; set; }

        public RandomEndColour(Color minEndColour, Color maxEndColour)
        {
            MinEndColour = minEndColour;
            MaxEndColour = maxEndColour;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            var startColor = Color.Lerp(MinEndColour, MaxEndColour, (float)random.NextDouble());
            Modify(ref particle, startColor);
        }

        public override void Maximise(ref Particle particle)
        {
            Modify(ref particle, MaxEndColour);
        }

        private static void Modify(ref Particle particle, Color color)
        {
            particle.EndColour = new Color(particle.StartColour.ToVector4() + color.ToVector4());
        }

        public override object Clone()
        {
            return new RandomEndColour(MinEndColour, MaxEndColour);
        }
    }

    public class RandomEndColourReader : ContentTypeReader<RandomEndColour>
    {
        protected override RandomEndColour Read(ContentReader input, RandomEndColour existingInstance)
        {
            return new RandomEndColour(input.ReadColor(), input.ReadColor());
        }
    }
}
