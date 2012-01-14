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
    public class AngularVelocityConstraint
        : Behaviour
    {
        private DynamicPhysics body;

        private Property<float> targetVelocity;
        private Property<float> strength;
        private Property<float> damping;

        public override void CreateProperties(Myre.Entities.Entity.ConstructionContext context)
        {
            if (body == null)
                throw new Exception("VelocityConstraint requires that the entity contain a DynamicPhysics behaviour.");

            this.targetVelocity = context.CreateProperty<float>("target_angular_velocity");
            this.strength = context.CreateProperty<float>("angular_velocity_constraint_strength");
            this.damping = context.CreateProperty<float>("angular_velocity_constraint_damping");

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            this.body = Owner.GetBehaviour<DynamicPhysics>();

            base.Initialise(initialisationData);
        }

        public class Manager
            : BehaviourManager<AngularVelocityConstraint>, IForceProvider
        {
            public void Update(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var constraint = Behaviours[i];
                    var body = constraint.body;

                    var torque = (constraint.targetVelocity.Value - body.AngularVelocity) * constraint.strength.Value;
                    torque -= body.AngularAcceleration * constraint.damping.Value;
                    
                    body.ApplyTorque(torque);
                }
            }
        }
    }
}
