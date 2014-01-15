﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

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
            particle.EndColour = new Color(particle.StartColour.ToVector4() + Color.Lerp(MinEndColour, MaxEndColour, (float)random.NextDouble()).ToVector4());
        }

        public override object Copy()
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