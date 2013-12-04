using System;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Colour
{
    public class RandomEndColour
        :IInitialiser
    {
        public Color MinEndColour { get; set; }
        public Color MaxEndColour { get; set; }

        public RandomEndColour(Color minEndColour, Color maxEndColour)
        {
            MinEndColour = minEndColour;
            MaxEndColour = maxEndColour;
        }

        public void Initialise(Random random, ref Particle particle)
        {
            particle.StartColour = new Color(particle.StartColour.ToVector4() + Color.Lerp(MinEndColour, MaxEndColour, (float)random.NextDouble()).ToVector4());
        }
    }
}
