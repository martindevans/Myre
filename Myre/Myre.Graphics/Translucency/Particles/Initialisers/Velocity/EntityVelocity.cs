using System;
using System.Numerics;
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
            Modify(ref particle, _velocity * VelocityBleedThrough);
        }

        public override void Maximise(ref Particle particle)
        {
            var len = particle.Velocity.Length();
            var pv = len < 0.01 ? Vector3.UnitX : particle.Velocity / len;

            //Assume that we accelerate the particle along the velocity vector of the particle, with a speed of 10m/s (arbitrary)
            Modify(ref particle, pv * 10 * VelocityBleedThrough);
        }

        private static void Modify(ref Particle particle, Vector3 velocity)
        {
            particle.Velocity += velocity;
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

        public override object Clone()
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
