using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities;
using Microsoft.Xna.Framework;
using Myre.Collections;

namespace Myre.Physics.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class RotationConstraint
        : Behaviour
    {
        private DynamicPhysics body;

        private Property<float> targetRotation;
        private Property<float> strength;
        private Property<float> damping;

        public override void CreateProperties(Myre.Entities.Entity.ConstructionContext context)
        {
            if (body == null)
                throw new Exception("VelocityConstraint requires that the entity contain a DynamicPhysics behaviour.");

            this.targetRotation = context.CreateProperty<float>("target_rotation");
            this.strength = context.CreateProperty<float>("rotation_constraint_strength");
            this.damping = context.CreateProperty<float>("rotation_constraint_damping");

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            this.body = Owner.GetBehaviour<DynamicPhysics>();

            base.Initialise(initialisationData);
        }

        public class Manager
            : BehaviourManager<RotationConstraint>, IForceProvider
        {
            public void Update(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var constraint = Behaviours[i];
                    var body = constraint.body;

                    var torque = NormalisedDistance(constraint.targetRotation.Value, body.Rotation) * constraint.strength.Value;
                    torque -= body.AngularVelocity * constraint.damping.Value;

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
