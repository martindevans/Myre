using System;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Physics2;

namespace Myre.Physics2.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class RotationConstraint
        : Behaviour
    {
        private DynamicPhysics _body;

        private Property<float> _targetRotation;
        private Property<float> _strength;
        private Property<float> _damping;

        public override void CreateProperties(Myre.Entities.Entity.ConstructionContext context)
        {
            if (_body == null)
                throw new Exception("VelocityConstraint requires that the entity contain a DynamicPhysics behaviour.");

            _targetRotation = context.CreateProperty<float>("target_rotation");
            _strength = context.CreateProperty<float>("rotation_constraint_strength");
            _damping = context.CreateProperty<float>("rotation_constraint_damping");

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            _body = Owner.GetBehaviour<DynamicPhysics>();

            base.Initialise(initialisationData);
        }

        class Manager
            : BehaviourManager<RotationConstraint>, IForceProvider
        {
            public void Update(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var constraint = Behaviours[i];
                    var body = constraint._body;

                    var torque = NormalisedDistance(constraint._targetRotation.Value, body.Rotation) * constraint._strength.Value;
                    torque -= body.AngularVelocity * constraint._damping.Value;

                    System.Diagnostics.Debug.WriteLine(torque);
                    body.ApplyTorque(torque);
                }
            }

            private float NormaliseRotation(float rotation)
            {
                //while (rotation < 0)
                //    rotation += MathHelper.TwoPi;

                //rotation %= MathHelper.TwoPi;

                return rotation;
            }

            private float NormalisedDistance(float a, float b)
            {
                var distance = NormaliseRotation(a) - NormaliseRotation(b);

                //if (distance > MathHelper.Pi)
                //    distance = -(MathHelper.TwoPi - distance);
                //else if (distance < -MathHelper.Pi)
                //    distance = MathHelper.TwoPi - distance;

                return distance;
            }
        }
    }
}
