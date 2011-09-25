using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Microsoft.Xna.Framework.Graphics;
using Ninject;
using System.Diagnostics;

namespace Myre.Graphics.Particles
{
    public class EllipsoidParticleEmitter
        : ParticleEmitter
    {
        private Property<Vector3> position;
        private Vector3 previousPosition;
        private Matrix transform;
        private float time;
        private Random random;
        private Vector3 velocity;
        private Vector3 direction;
        private Vector3 tangent1;
        private Vector3 tangent2;

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
            get { return velocity; }
            set
            {
                velocity = value;

                if (velocity != Vector3.Zero)
                {
                    Vector3.Normalize(ref velocity, out direction);
                    tangent1 = Vector3.Cross(direction, (velocity == Vector3.Forward) ? Vector3.Up : Vector3.Forward);
                    tangent2 = Vector3.Cross(direction, tangent1);
                }
                else
                {
                    direction = Vector3.Up;
                    tangent1 = Vector3.Forward;
                    tangent2 = Vector3.Right;
                }
            }
        }

        public Matrix Transform
        {
            get { return transform; }
            set
            {
                transform = value;

                //if (value != Matrix.Identity && !UsingUniqueSystem)
                //    Dirty = true;
            }
        }

        public EllipsoidParticleEmitter(IKernel kernel)
            : base(kernel)
        {
            this.random = new Random();
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            this.position = context.CreateProperty<Vector3>("position");

            base.CreateProperties(context);
        }

        protected override void Update(float dt)
        {
            if (Dirty)
            {
                CreateParticleSystem();//transform != Matrix.Identity);
                System.GrowCapacity(Capacity);
                previousPosition = position.Value;
                Dirty = false;
            }

            System.Transform = transform;

            // adapted from particle 3D sample on creators.xna.com

            var emitterVelocity = (position.Value - previousPosition) / dt;
            var baseParticleVelocity = Velocity + emitterVelocity * VelocityBleedThrough;

            var timePerParticle = 1f / EmitPerSecond;

            // If we had any time left over that we didn't use during the
            // previous update, add that to the current elapsed time.
            float timeToSpend = time + dt;
            float now = timeToSpend;

            // Counter for looping over the time interval.
            float currentTime = -time;

            // Create particles as long as we have a big enough time interval.
            while (timeToSpend > timePerParticle)
            {
                currentTime += timePerParticle;
                timeToSpend -= timePerParticle;

                // Work out the optimal position for this particle. This will produce
                // evenly spaced particles regardless of the object speed, particle
                // creation frequency, or game update rate.
                var mu = currentTime / dt;
                var particlePosition = Vector3.Lerp(previousPosition, position.Value, mu) + RandomPositionOffset();

                var randomVector = RandomNormalVector();
                randomVector.X *= HorizontalVelocityVariance;
                randomVector.Z *= HorizontalVelocityVariance;
                randomVector.Y *= VerticalVelocityVariance;
                var particleVelocity = baseParticleVelocity + randomVector;

                var particleAngularVelocity = MathHelper.Lerp(MinAngularVelocity, MaxAngularVelocity, (float)random.NextDouble());
                var particleSize = MathHelper.Lerp(MinStartSize, MaxStartSize, (float)random.NextDouble());
                var particleStartColour = Color.Lerp(MinStartColour, MaxStartColour, (float)random.NextDouble());
                var particleEndColour = Color.Lerp(MinEndColour, MaxEndColour, (float)random.NextDouble());
                var particleLifetime = 1 - MathHelper.Lerp(0, LifetimeVariance, (float)random.NextDouble());
                particleLifetime *= 1 - (now - currentTime) / (Lifetime * particleLifetime);

                // Create the particle.
                System.Spawn(particlePosition, particleVelocity, particleAngularVelocity, particleSize, particleLifetime, particleStartColour, particleEndColour);
            }

            // Store any time we didn't use, so it can be part of the next update.
            time = timeToSpend;
            previousPosition = position.Value;
        }

        private Vector3 RandomNormalVector()
        {
            float randomA = (float)random.NextDouble() * 2 - 1;
            float randomB = (float)random.NextDouble() * 2 - 1;
            float randomC = (float)random.NextDouble() * 2 - 1;
            var randomVector = Vector3.Normalize(new Vector3(randomA, randomB, randomC));
            return randomVector;
        }

        private Vector3 RandomPositionOffset()
        {
            Vector3 rand;
            Vector3 min;
            Vector3 max;

            do
            {
                rand = RandomNormalVector();
                max = rand * Ellipsoid;
                min = Vector3.Normalize(max) * MinEmitDistance;
            } while (MinEmitDistance > max.Length());

            return Vector3.Lerp(min, max, (float)random.NextDouble());
        }

        private float RandomBellCurve()
        {
            var rng = (float)random.NextDouble();
            rng *= rng;
            return rng * 2 - 1;
        }
    }
}
