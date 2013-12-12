using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Entities.Services;
using Myre.Graphics.Geometry;

namespace Myre.Graphics.Translucency.Particles
{
    public class ParticleSystemService
        :Service
    {
        private readonly GraphicsDevice _device;

        private readonly Dictionary<ParticleSystemDescription, ParticleSystem> _particleSystems = new Dictionary<ParticleSystemDescription, ParticleSystem>();

        public ParticleSystemService(GraphicsDevice device)
        {
            _device = device;
        }

        public ParticleSystem Get(ParticleSystemDescription description)
        {
            ParticleSystem system;
            if (!_particleSystems.TryGetValue(description, out system))
            {
                system = new ParticleSystem(_device);
                system.Initialise(description);
                _particleSystems.Add(description, system);
            }

            return system;
        }

        internal void Draw(string phase, NamedBoxCollection metadata)
        {
            if (phase == "translucent")
                foreach (var particleEmitter in _particleSystems.Values)
                    particleEmitter.Draw(metadata);
        }
    }
}
