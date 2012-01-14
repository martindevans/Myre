using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Collections;

namespace Myre.Physics.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class LinearVelocityConstraint
        : Behaviour
    {
        private DynamicPhysics body;

        private Property<Vector2> axis;
        private Property<Vector2> targetVelocity;
        private Property<float> strength;
        private Property<float> damping;

        public override void CreateProperties(Myre.Entities.Entity.ConstructionContext context)
        {
            if (body == null)
                throw new Exception("VelocityConstraint requires that the entity contain a DynamicPhysics behaviour.");

            this.targetVelocity = context.CreateProperty<Vector2>("target_linear_velocity");
            this.strength = context.CreateProperty<float>("linear_velocity_constraint_strength");
            this.damping = context.CreateProperty<float>("linear_velocity_constraint_damping");
            this.axis = context.CreateProperty<Vector2>("linear_velocity_constraint_axis");

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            this.body = Owner.GetBehaviour<DynamicPhysics>();

            base.Initialise(initialisationData);
        }

        public class Manager
            : BehaviourManager<LinearVelocityConstraint>, IForceProvider
        {
            public void Update(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var constraint = Behaviours[i];
                    var body = constraint.body;

                    var force = (constraint.targetVelocity.Value - body.LinearVelocity) * constraint.strength.Value;
                    force -= body.LinearAcceleration * constraint.damping.Value;

                    var axis = constraint.axis.Value;
                    if (axis != Vector2.Zero)
                        force = axis * Vector2.Dot(axis, force);

                    body.ApplyForce(force);
                }
            }
        }
    }
}
