using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Myre.Entities;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Velocity
{
    public class EntityVelocity
        :BaseParticleInitialiser
    {
        private Vector3 _velocity;
        private Vector3 _previousPosition;
        private Property<Vector3> _position;

        public float VelocityBleedThrough { get; set; }

        public EntityVelocity(float velocityBleedThrough)
        {
            VelocityBleedThrough = velocityBleedThrough;
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            particle.Velocity += _velocity * VelocityBleedThrough;
        }

        public override void Attach(ParticleEmitter emitter)
        {
            _position = emitter.Owner.GetProperty(new TypedName<Vector3>("position"));
            _previousPosition = _position.Value;
        }

        public override void Update(float dt)
        {
            _velocity = (_position.Value - _previousPosition) / dt;
            _previousPosition = _position.Value;
            base.Update(dt);
        }

        public override object Copy()
        {
            return new EntityVelocity(VelocityBleedThrough);
        }
    }

    public class EntityVelocityReader : ContentTypeReader<EntityVelocity>
    {
        protected override EntityVelocity Read(ContentReader input, EntityVelocity existingInstance)
        {
            return new EntityVelocity(input.ReadSingle());
        }
    }
}
