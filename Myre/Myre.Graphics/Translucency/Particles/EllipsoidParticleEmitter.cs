using System;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Graphics.Translucency.Particles;
using Ninject;

namespace Myre.Graphics.Particles
{
    public class EllipsoidParticleEmitter
        : ParticleEmitter
    {
        private Property<Vector3> _position;
        private Vector3 _previousPosition;
        private Matrix _transform;
        private float _time;
        private readonly Random _random;
        private Vector3 _velocity;
        private Vector3 _direction;
        private Vector3 _tangent1;

        public int Capacity { get; set; }
        public float EmitPerSecond { get; set; }
        public Color MinStartColour { get; set; }
        public Color MaxStartColour { get; set; }
        public Color MinEndColour { get; set; }
        public Color MaxEndColour { get; set; }
        public float MinStartSize { get; set; }
        public float MaxStartSize { get; set; }
        public float HorizontalVelocityVariance { get; set; }
        public float VerticalVelocityVariance { get; set; }
        public float MinAngularVelocity { get; set; }
        public float MaxAngularVelocity { get; set; }
        public float VelocityBleedThrough { get; set; }
        public float LifetimeVariance { get; set; }
        public Vector3 Ellipsoid { get; set; }
        public float MinEmitDistance { get; set; }

        public Vector3 Velocity 
        {
            get { return _velocity; }
            set
            {
                _velocity = value;

                if (_velocity != Vector3.Zero)
                {
                    Vector3.Normalize(ref _velocity, out _direction);
                    _tangent1 = Vector3.Cross(_direction, (_velocity == Vector3.Forward) ? Vector3.Up : Vector3.Forward);
                }
                else
                {
                    _direction = Vector3.Up;
                    _tangent1 = Vector3.Forward;
                }
            }
        }

        public Matrix Transform
        {
            get { return _transform; }
            set
            {
                _transform = value;

                //if (value != Matrix.Identity && !UsingUniqueSystem)
                //    Dirty = true;
            }
        }

        public EllipsoidParticleEmitter(IKernel kernel)
            : base(kernel)
        {
            _random = new Random();
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _position = context.CreateProperty<Vector3>("position");

            base.CreateProperties(context);
        }

        protected override void Update(float dt)
        {
            if (Dirty)
            {
                CreateParticleSystem();//transform != Matrix.Identity);
                System.GrowCapacity(Capacity);
                _previousPosition = _position.Value;
                Dirty = false;
            }

            System.Transform = _transform;

            // adapted from particle 3D sample on creators.xna.com

            var emitterVelocity = (_position.Value - _previousPosition) / dt;
            var baseParticleVelocity = Velocity + emitterVelocity * VelocityBleedThrough;

            var timePerParticle = 1f / EmitPerSecond;

            // If we had any time left over that we didn't use during the
            // previous update, add that to the current elapsed time.
            float timeToSpend = _time + dt;
            float now = timeToSpend;

            // Counter for looping over the time interval.
            float currentTime = -_time;

            // Create particles as long as we have a big enough time interval.
            while (timeToSpend > timePerParticle)
            {
                currentTime += timePerParticle;
                timeToSpend -= timePerParticle;

                // Work out the optimal position for this particle. This will produce
                // evenly spaced particles regardless of the object speed, particle
                // creation frequency, or game update rate.
                var mu = currentTime / dt;
                var particlePosition = Vector3.Lerp(_previousPosition, _position.Value, mu) + RandomPositionOffset();

                var randomVector = RandomNormalVector();
                randomVector.X *= HorizontalVelocityVariance;
                randomVector.Z *= HorizontalVelocityVariance;
                randomVector.Y *= VerticalVelocityVariance;
                var particleVelocity = baseParticleVelocity + randomVector;

                var particleAngularVelocity = MathHelper.Lerp(MinAngularVelocity, MaxAngularVelocity, (float)_random.NextDouble());
                var particleSize = MathHelper.Lerp(MinStartSize, MaxStartSize, (float)_random.NextDouble());
                var particleStartColour = Color.Lerp(MinStartColour, MaxStartColour, (float)_random.NextDouble());
                var particleEndColour = Color.Lerp(MinEndColour, MaxEndColour, (float)_random.NextDouble());
                var particleLifetime = 1 - MathHelper.Lerp(0, LifetimeVariance, (float)_random.NextDouble());
                particleLifetime *= 1 - (now - currentTime) / (Lifetime * particleLifetime);

                // Create the particle.
                System.Spawn(particlePosition, particleVelocity, particleAngularVelocity, particleSize, particleLifetime, particleStartColour, particleEndColour);
            }

            // Store any time we didn't use, so it can be part of the next update.
            _time = timeToSpend;
            _previousPosition = _position.Value;
        }

        private Vector3 RandomNormalVector()
        {
            float randomA = (float)_random.NextDouble() * 2 - 1;
            float randomB = (float)_random.NextDouble() * 2 - 1;
            float randomC = (float)_random.NextDouble() * 2 - 1;
            var randomVector = Vector3.Normalize(new Vector3(randomA, randomB, randomC));
            return randomVector;
        }

        private Vector3 RandomPositionOffset()
        {
            Vector3 min;
            Vector3 max;

            do
            {
                Vector3 rand = RandomNormalVector();
                max = rand * Ellipsoid;
                min = Vector3.Normalize(max) * MinEmitDistance;
            } while (MinEmitDistance > max.Length());

            return Vector3.Lerp(min, max, (float)_random.NextDouble());
        }
    }
}
