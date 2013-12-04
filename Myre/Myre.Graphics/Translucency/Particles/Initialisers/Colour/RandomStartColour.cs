using System;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Colour
{
    public class RandomStartColour
        :IInitialiser
    {
        public Color MinStartColour { get; set; }
        public Color MaxStartColour { get; set; }

        public RandomStartColour(Color minStartColour, Color maxStartColour)
        {
            MinStartColour = minStartColour;
            MaxStartColour = maxStartColour;
        }

        public void Initialise(Random random, ref Particle particle)
        {
            particle.StartColour = new Color(particle.StartColour.ToVector4() + Color.Lerp(MinStartColour, MaxStartColour, (float) random.NextDouble()).ToVector4());
        }
    }
}
