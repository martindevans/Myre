using System;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;

namespace Myre.Physics2D.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class PositionConstraint
        : Behaviour
    {
        private DynamicPhysics _body;

        private Property<Vector2> _axis;
        private Property<Vector2> _targetPosition;
        private Property<float> _strength;
        private Property<float> _damping;

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            if (_body == null)
                throw new Exception("VelocityConstraint requires that the entity contain a DynamicPhysics behaviour.");

            _targetPosition = context.CreateProperty(new TypedName<Vector2>("target_position"));
            _strength = context.CreateProperty(new TypedName<float>("position_constraint_strength"));
            _damping = context.CreateProperty(new TypedName<float>("position_constraint_damping"));
            _axis = context.CreateProperty(new TypedName<Vector2>("position_constraint_axis"));

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            _body = Owner.GetBehaviour<DynamicPhysics>();

            base.Initialise(initialisationData);
        }

        class Manager
            : BehaviourManager<PositionConstraint>, IForceProvider
        {
            public void Update(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var constraint = Behaviours[i];
                    var body = constraint._body;

                    var force = (constraint._targetPosition.Value - body.Position) * constraint._strength.Value;
                    force -= body.LinearVelocity * constraint._damping.Value;

                    var axis = constraint._axis.Value;
                    if (axis != Vector2.Zero)
                        force = axis * Vector2.Dot(axis, force);

                    body.ApplyForce(force);
                }
            }
        }
    }
}
