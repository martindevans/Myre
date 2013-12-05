using System;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Translucency.Particles.Initialisers
{
    public interface IInitialiser : ICloneable
    {
        void Initialise(Random random, ref Particle particle);
    }

    public struct Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float AngularVelocity;
        public float Size;
        public float LifetimeScale;
        public Color StartColour;
        public Color EndColour;

        public Particle(Vector3 position, Vector3 velocity, float angularVelocity, float size, float lifetimeScale, Color startColour, Color endColour)
        {
            Position = position;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            Size = size;
            LifetimeScale = lifetimeScale;
            StartColour = startColour;
            EndColour = endColour;
        }
    }
}
