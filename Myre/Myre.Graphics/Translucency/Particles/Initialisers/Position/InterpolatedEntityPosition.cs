using System;
using System.Numerics;
using Microsoft.Xna.Framework.Content;
using Myre.Entities;

namespace Myre.Graphics.Translucency.Particles.Initialisers.Position
{
    /// <summary>
    /// Create entities interpolated along the line between the previous and current particle positions
    /// </summary>
    public class InterpolatedEntityPosition
        :BaseParticleInitialiser
    {
        public int BatchSize { get; set; }
        private int _spawnCount;

        private Vector3 _previousPosition;
        private Property<Vector3> _position;

        public InterpolatedEntityPosition(int batchSize = 1)
        {
            BatchSize = batchSize;
        }

        public override object Clone()
        {
            return new InterpolatedEntityPosition(BatchSize);
        }

        public override void Initialise(Random random, ref Particle particle)
        {
            float mu = (_spawnCount + 1) / (float)BatchSize;
            _spawnCount = (_spawnCount + 1) % BatchSize;

            particle.Position += Vector3.Lerp(_previousPosition, _position.Value, mu);
        }

        public override void Update(float dt)
        {
            _previousPosition = _position.Value;

            base.Update(dt);
        }

        public override void Attach(ParticleEmitter emitter)
        {
            _position = emitter.Owner.GetProperty(new TypedName<Vector3>("position"));
            _previousPosition = _position.Value;
        }
    }

    public class InterpolatedEntityPositionReader : ContentTypeReader<InterpolatedEntityPosition>
    {
        protected override InterpolatedEntityPosition Read(ContentReader input, InterpolatedEntityPosition existingInstance)
        {
            return new InterpolatedEntityPosition(input.ReadInt32());
        }
    }
}
