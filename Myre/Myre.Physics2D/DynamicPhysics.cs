using System;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Events;
using Myre.Physics2D.Collisions;

namespace Myre.Physics2D
{
    [DefaultManager(typeof(Manager))]
    public class DynamicPhysics
        : Behaviour
    {
        private Property<Vector2> _position;
        private Property<float> _rotation;
        private Property<float> _mass;
        private Property<float> _inertiaTensor;
        private Property<Vector2> _linearVelocity;
        private Property<float> _angularVelocity;
        private Property<float> _timeMultiplier;
        private Property<bool> _sleeping;
        private Property<Vector2> _linearVelocityBias;
        private Property<float> _angularVelocityBias;
        private Property<float> _angularAcceleration;
        private Property<Vector2> _linearAcceleration;

        private Event<CollisionImpulseApplied> _collisonImpulseEvent;

        private Vector2 _force;
        private float _torque;

        private float _timeTillSleep;
        
        public Vector2 Position
        {
            get { return _position.Value; }
            set { _position.Value = value; }
        }

        public float Rotation
        {
            get { return _rotation.Value; }
            set { _rotation.Value = value; }
        }

        public float Mass
        {
            get { return _mass.Value; }
            set { _mass.Value = value; }
        }

        public float InertiaTensor
        {
            get { return _inertiaTensor.Value; }
            set { _inertiaTensor.Value = value; }
        }

        public Vector2 LinearVelocity
        {
            get { return _linearVelocity.Value; }
            set { _linearVelocity.Value = value; }
        }

        public float AngularVelocity
        {
            get { return _angularVelocity.Value; }
            set { _angularVelocity.Value = value; }
        }

        public Vector2 LinearAcceleration
        {
            get { return _linearAcceleration.Value; }
            set { _linearAcceleration.Value = value; }
        }

        public float AngularAcceleration
        {
            get { return _angularAcceleration.Value; }
            set { _angularAcceleration.Value = value; }
        }

        public float TimeMultiplier
        {
            get { return _timeMultiplier.Value; }
            set { _timeMultiplier.Value = value; }
        }

        public bool Sleeping
        {
            get { return _sleeping.Value; }
            set { _sleeping.Value = value; }
        }

        public bool IsStatic
        {
            get { return float.IsPositiveInfinity(_mass.Value) && float.IsPositiveInfinity(_inertiaTensor.Value); }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _position = context.CreateProperty<Vector2>(PhysicsProperties.POSITION, default(Vector2));
            _rotation = context.CreateProperty<float>(PhysicsProperties.ROTATION, default(float));
            _mass = context.CreateProperty<float>(PhysicsProperties.MASS, 1);
            _inertiaTensor = context.CreateProperty<float>(PhysicsProperties.INERTIA_TENSOR, 1);
            _linearVelocity = context.CreateProperty<Vector2>(PhysicsProperties.LINEAR_VELOCITY, default(Vector2));
            _angularVelocity = context.CreateProperty<float>(PhysicsProperties.ANGULAR_VELOCITY, default(float));
            _linearVelocityBias = context.CreateProperty<Vector2>(PhysicsProperties.LINEAR_VELOCITY_BIAS, default(Vector2));
            _angularVelocityBias = context.CreateProperty<float>(PhysicsProperties.ANGULAR_VELOCITY_BIAS, default(float));
            _linearAcceleration = context.CreateProperty<Vector2>(PhysicsProperties.LINEAR_ACCELERATION, default(Vector2));
            _angularAcceleration = context.CreateProperty<float>(PhysicsProperties.ANGULAR_ACCELERATION, default(float));
            _timeMultiplier = context.CreateProperty<float>(PhysicsProperties.TIME_MULTIPLIER, default(float));
            _sleeping = context.CreateProperty<bool>(PhysicsProperties.SLEEPING, default(bool));

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            _collisonImpulseEvent = Owner.Scene.GetService<EventService>().GetEvent<CollisionImpulseApplied>(null);

            _timeTillSleep = 5;
            base.Initialise(initialisationData);
        }

        public Vector2 GetVelocityAtOffset(Vector2 worldOffset)
        {
            var value = _linearVelocity.Value;
            value.X += -_angularVelocity.Value * worldOffset.Y;
            value.Y += _angularVelocity.Value * worldOffset.X;

            return value;
        }

        internal Vector2 GetVelocityBiasAtOffset(Vector2 worldOffset)
        {
            var value = _linearVelocityBias.Value;
            value.X += -_angularVelocityBias.Value * worldOffset.Y;
            value.Y += _angularVelocityBias.Value * worldOffset.X;

            return value;
        }

        public void ApplyForce(Vector2 force, Vector2 worldPosition)
        {
            Vector2.Add(ref _force, ref force, out _force);
            var pos = _position.Value;
            Vector2 r;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            _torque += r.X * force.X - r.Y * force.Y;
        }

        public void ApplyForceAtOffset(Vector2 force, Vector2 worldOffset)
        {
            Vector2.Add(ref _force, ref force, out _force);
            _torque += worldOffset.X * force.X - worldOffset.Y * force.Y;
        }

        public void ApplyForce(Vector2 force)
        {
            _force += force;
        }

        public void ApplyTorque(float torque)
        {
            _torque += torque;
        }

        public void ApplyImpulse(Vector2 impulse, Vector2 worldPosition)
        {
            var pos = _position.Value;
            Vector2 r;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            ApplyImpulseAtOffset(ref impulse, ref r);
        }

        public void ApplyImpulseAtOffset(Vector2 impulse, Vector2 worldOffset)
        {
            ApplyImpulseAtOffset(ref impulse, ref worldOffset);
        }

        internal void CollisionImpulse(Geometry geometry, Geometry other, ref Vector2 impulse, ref Vector2 worldOffset)
        {
            _collisonImpulseEvent.Send(new CollisionImpulseApplied(geometry, other, impulse));
            ApplyImpulseAtOffset(ref impulse, ref worldOffset);
        }

        public void ApplyImpulseAtOffset(ref Vector2 impulse, ref Vector2 worldOffset)
        {
            Vector2 l = _linearVelocity.Value;
            Vector2 v;

            Vector2.Multiply(ref impulse, 1f / _mass.Value, out v);
            Vector2.Add(ref l, ref v, out l);
            _linearVelocity.Value = l;

            _angularVelocity.Value += (worldOffset.X * impulse.Y - impulse.X * worldOffset.Y) / InertiaTensor;
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            _linearVelocity.Value += impulse / Mass;
        }

        internal void ApplyBiasImpulse(ref Vector2 impulse, ref Vector2 worldPosition)
        {
            var pos = _position.Value;
            Vector2 r;
            Vector2.Subtract(ref worldPosition, ref pos, out r);
            ApplyBiasImpulseAtOffset(ref impulse, ref r);
        }

        internal void ApplyBiasImpulseAtOffset(ref Vector2 impulse, ref Vector2 worldOffset)
        {
            impulse /= Mass;
            _linearVelocityBias.Value += impulse;
            _angularVelocityBias.Value += (worldOffset.X * impulse.Y - impulse.X * worldOffset.Y) / InertiaTensor;
        }

        class Manager
            : BehaviourManager<DynamicPhysics>, IActivityManager, IIntegrator, IForceApplier
        {
            public void UpdateActivityStatus(float time, float linearThreshold, float angularThreshold)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var body = Behaviours[i];

                    var linear = (body._linearVelocity.Value + body._linearVelocityBias.Value).LengthSquared();
                    var angular = Math.Abs(body._angularVelocity.Value + body._angularVelocityBias.Value);

                    if (linear <= linearThreshold
                        && angular <= angularThreshold)
                    {
                        body._timeTillSleep -= time;

                        if (!body._sleeping.Value && body._timeTillSleep <= 0)
                            body._sleeping.Value = true;
                    }
                    else
                    {
                        if (body._sleeping.Value)
                            body._sleeping.Value = false;

                        body._timeTillSleep = 5;
                    }
                }
            }

            public void FreezeSleepingObjects()
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var body = Behaviours[i];

                    if (body._sleeping.Value)
                    {
                        body._linearVelocity.Value = Vector2.Zero;
                        body._linearVelocityBias.Value = Vector2.Zero;
                        body._angularVelocity.Value = 0;
                        body._angularVelocityBias.Value = 0;
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
                    item.Position += (item.LinearVelocity + item._linearVelocityBias.Value) * elapsedTime;
                    item.Rotation += (item.AngularVelocity + item._angularVelocityBias.Value) * elapsedTime;

                    item._linearVelocityBias.Value = Vector2.Zero;
                    item._angularVelocityBias.Value = 0;
                }
            }

            #endregion

            #region IForceApplier Members

            public void CalculateAccelerations()
            {
                foreach (var item in Behaviours)
                {
                    item.LinearAcceleration = item._force / item.Mass;
                    item.AngularAcceleration = item._torque / item.InertiaTensor;
                    
                    item._force = Vector2.Zero;
                    item._torque = 0;
                }
            }

            #endregion
        }
    }
}
