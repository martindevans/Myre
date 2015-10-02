using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Collections;
using Myre.Entities.Services;
using System.Collections.Generic;
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

        internal void Query(string phase, NamedBoxCollection metadata, ICollection<IGeometry> result)
        {
            //Early exit
            if (phase != "translucent")
                return;

            //Get the view matrix from the renderer
            var view = metadata.Get<Matrix4x4>("view").Value;

            //Add all particle systems to the output buffer
            foreach (var particleSystem in _particleSystems.Values)
            {
                //Calculate the world view of this system for depth sorting
                particleSystem.CalculateWorldView(ref view);

                result.Add(particleSystem);
            }
        }
    }
}
