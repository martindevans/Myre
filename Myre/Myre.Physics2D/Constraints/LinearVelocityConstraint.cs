using System;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Physics2D;

namespace Myre.Physics2D.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class LinearVelocityConstraint
        : Behaviour
    {
        private DynamicPhysics _body;

        private Property<Vector2> _axis;
        private Property<Vector2> _targetVelocity;
        private Property<float> _strength;
        private Property<float> _damping;

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            if (_body == null)
                throw new Exception("VelocityConstraint requires that the entity contain a DynamicPhysics behaviour.");

            _targetVelocity = context.CreateProperty<Vector2>("target_linear_velocity");
            _strength = context.CreateProperty<float>("linear_velocity_constraint_strength");
            _damping = context.CreateProperty<float>("linear_velocity_constraint_damping");
            _axis = context.CreateProperty<Vector2>("linear_velocity_constraint_axis");

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            _body = Owner.GetBehaviour<DynamicPhysics>();

            base.Initialise(initialisationData);
        }

        class Manager
            : BehaviourManager<LinearVelocityConstraint>, IForceProvider
        {
            public void Update(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var constraint = Behaviours[i];
                    var body = constraint._body;

                    var force = (constraint._targetVelocity.Value - body.LinearVelocity) * constraint._strength.Value;
                    force -= body.LinearAcceleration * constraint._damping.Value;

                    var axis = constraint._axis.Value;
                    if (axis != Vector2.Zero)
                        force = axis * Vector2.Dot(axis, force);

                    body.ApplyForce(force);
                }
            }
        }
    }
}
