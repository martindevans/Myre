using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;
using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Ninject;
using Myre.Entities.Services;
using Myre.Collections;

namespace Myre.Physics
{
    [DefaultManager(typeof(Manager))]
    public class DynamicPhysics
        : Behaviour
    {
        private Property<Vector2> position;
        private Property<float> rotation;
        private Property<float> mass;
        private Property<float> inertiaTensor;
        private Property<Vector2> linearVelocity;
        private Property<float> angularVelocity;
        private Property<float> timeMultiplier;
        private Property<bool> sleeping;
        private Property<Vector2> linearVelocityBias;
        private Property<float> angularVelocityBias;
        private Property<float> angularAcceleration;
        private Property<Vector2> linearAcceleration;

        private Vector2 force;
        private float torque;

        private float timeTillSleep;

        #region temp
        private Vector2 r;
        #endregion
        
        public Vector2 Position
        {
            get { return position.Value; }
            set { position.Value = value; }
        }

        public float Rotation
        {
            get { return rotation.Value; }
            set { rotation.Value = value; }
        }

        public float Mass
        {
            get { return mass.Value; }
            set { mass.Value = value; }
        }

        public float InertiaTensor
        {
            get { return inertiaTensor.Value; }
            set { inertiaTensor.Value = value; }
        }

        public Vector2 LinearVelocity
        {
            get { return linearVelocity.Value; }
            set { linearVelocity.Value = value; }
        }

        public float AngularVelocity
        {
            get { return angularVelocity.Value; }
            set { angularVelocity.Value = value; }
        }

        public Vector2 LinearAcceleration
        {
            get { return linearAcceleration.Value; }
            set { linearAcceleration.Value = value; }
        }

        public float AngularAcceleration
        {
            get { return angularAcceleration.Value; }
            set { angularAcceleration.Value = value; }
        }

        public float TimeMultiplier
        {
            get { return timeMultiplier.Value; }
            set { timeMultiplier.Value = value; }
        }

        public bool Sleeping
        {
            get { return sleeping.Value; }
            set { sleeping.Value = value; }
        }

        public bool IsStatic
        {
            get { return mass.Value == float.PositiveInfinity && inertiaTensor.Value == float.PositiveInfinity; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            this.position = context.CreateProperty<Vector2>(PhysicsProperties.POSITION);
            this.rotation = context.CreateProperty<float>(PhysicsProperties.ROTATION);
            this.mass = context.CreateProperty<float>(PhysicsProperties.MASS);
            this.inertiaTensor = context.CreateProperty<float>(PhysicsProperties.INERTIA_TENSOR);
            this.linearVelocity = context.CreateProperty<Vector2>(PhysicsProperties.LINEAR_VELOCITY);
            this.angularVelocity = context.CreateProperty<float>(PhysicsProperties.ANGULAR_VELOCITY);
            this.linearVelocityBias = context.CreateProperty<Vector2>(PhysicsProperties.LINEAR_VELOCITY_BIAS);
            this.angularVelocityBias = context.CreateProperty<float>(PhysicsProperties.ANGULAR_VELOCITY_BIAS);
            this.linearAcceleration = context.CreateProperty<Vector2>(PhysicsProperties.LINEAR_ACCELERATION);
            this.angularAcceleration = context.CreateProperty<float>(PhysicsProperties.ANGULAR_ACCELERATION);
            this.timeMultiplier = context.CreateProperty<float>(PhysicsProperties.TIME_MULTIPLIER);
            this.sleeping = context.CreateProperty<bool>(PhysicsProperties.SLEEPING);

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            timeTillSleep = 5;
            base.Initialise(initialisationData);
        }

        public Vector2 GetVelocityAtOffset(Vector2 worldOffset)
        {
            var value = linearVelocity.Value;
            value.X += -angularVelocity.Value * worldOffset.Y;
            value.Y += angularVelocity.Value * worldOffset.X;

            return value;
        }

        internal Vector2 GetVelocityBiasAtOffset(Vector2 worldOffset)
        {
            var value = linearVelocityBias.Value;
            value.X += -angularVelocityBias.Value * worldOffset.Y;
            value.Y += angularVelocityBias.Value * worldOffset.X;

            return value;
        }

        public void ApplyForce(Vector2 force, Vector2 worldPosition)
        {
            Vector2.Add(ref this.force, ref force, out this.force);
            var pos = position.Value;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            torque += r.X * force.X - r.Y * force.Y;
        }

        public void ApplyForceAtOffset(Vector2 force, Vector2 worldOffset)
        {
            Vector2.Add(ref this.force, ref force, out this.force);
            torque += worldOffset.X * force.X - worldOffset.Y * force.Y;
        }

        public void ApplyForce(Vector2 force)
        {
            this.force += force;
        }

        public void ApplyTorque(float torque)
        {
            this.torque += torque;
        }

        public void ApplyImpulse(Vector2 impulse, Vector2 worldPosition)
        {
            var pos = position.Value;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            ApplyImpulseAtOffset(ref impulse, ref r);
        }

        public void ApplyImpulseAtOffset(ref Vector2 impulse, ref Vector2 worldOffset)
        {
            Vector2 l = linearVelocity.Value;
            Vector2 v;

            Vector2.Multiply(ref impulse, 1f / mass.Value, out v);
            Vector2.Add(ref l, ref v, out l);
            linearVelocity.Value = l;

            angularVelocity.Value += (worldOffset.X * impulse.Y - impulse.X * worldOffset.Y) / InertiaTensor;
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            linearVelocity.Value += impulse / Mass;
        }

        internal void ApplyBiasImpulse(ref Vector2 impulse, ref Vector2 worldPosition)
        {
            var pos = position.Value;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            ApplyBiasImpulseAtOffset(ref impulse, ref r);
        }

        internal void ApplyBiasImpulseAtOffset(ref Vector2 impulse, ref Vector2 worldOffset)
        {
            impulse /= Mass;
            linearVelocityBias.Value += impulse;
            angularVelocityBias.Value += (worldOffset.X * impulse.Y - impulse.X * worldOffset.Y) / InertiaTensor;
        }

        public class Manager
            : BehaviourManager<DynamicPhysics>, IActivityManager, IIntegrator, IForceApplier
        {
            public Manager()
            {
            }

            public void Update()
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var body = Behaviours[i];

                    body.linearAcceleration.Value = body.force / body.Mass;
                    body.angularAcceleration.Value = body.torque / body.InertiaTensor;
                }
            }

            public void UpdateActivityStatus(float time, float linearThreshold, float angularThreshold)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var body = Behaviours[i];

                    var linear = (body.linearVelocity.Value + body.linearVelocityBias.Value).LengthSquared();
                    var angular = Math.Abs(body.angularVelocity.Value + body.angularVelocityBias.Value);

                    if (linear <= linearThreshold
                        && angular <= angularThreshold)
                    {
                        body.timeTillSleep -= time;

                        if (!body.sleeping.Value && body.timeTillSleep <= 0)
                            body.sleeping.Value = true;
                    }
                    else
                    {
                        if (body.sleeping.Value)
                            body.sleeping.Value = false;

                        body.timeTillSleep = 5;
                    }
                }
            }

            public void FreezeSleepingObjects()
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var body = Behaviours[i];

                    if (body.sleeping.Value)
                    {
                        body.linearVelocity.Value = Vector2.Zero;
                        body.linearVelocityBias.Value = Vector2.Zero;
                        body.angularVelocity.Value = 0;
                        body.angularVelocityBias.Value = 0;
                    }
                }
            }

            #region IIntegrator Members

            void IIntegrator.UpdateVelocity(float elapsedTime)
            {
                foreach (var item in Behaviours)
                {
                    item.LinearVelocity += item.LinearAcceleration * elapsedTime;
                    item.AngularVelocity += item.AngularAcceleration * elapsedTime;
                }
            }

            void IIntegrator.UpdatePosition(float elapsedTime)
            {
                foreach (var item in Behaviours)
                {
                    item.Position += (item.LinearVelocity + item.linearVelocityBias.Value) * elapsedTime;
                    item.Rotation += (item.AngularVelocity + item.angularVelocityBias.Value) * elapsedTime;

                    item.linearVelocityBias.Value = Vector2.Zero;
                    item.angularVelocityBias.Value = 0;
                }
            }

            #endregion

            #region IForceApplier Members

            public void CalculateAccelerations()
            {
                foreach (var item in Behaviours)
                {
                    item.LinearAcceleration = item.force / item.Mass;
                    item.AngularAcceleration = item.torque / item.InertiaTensor;
                    
                    item.force = Vector2.Zero;
                    item.torque = 0;
                }
            }

            #endregion
        }
    }
}
