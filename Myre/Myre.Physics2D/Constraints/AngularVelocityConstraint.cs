using System;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Physics2D;

namespace Myre.Physics2D.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class AngularVelocityConstraint
        : Behaviour
    {
        private DynamicPhysics _body;

        private Property<float> _targetVelocity;
        private Property<float> _strength;
        private Property<float> _damping;

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            if (_body == null)
                throw new Exception("VelocityConstraint requires that the entity contain a DynamicPhysics behaviour.");

            _targetVelocity = context.CreateProperty<float>("target_angular_velocity");
            _strength = context.CreateProperty<float>("angular_velocity_constraint_strength");
            _damping = context.CreateProperty<float>("angular_velocity_constraint_damping");

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            _body = Owner.GetBehaviour<DynamicPhysics>();

            base.Initialise(initialisationData);
        }

        class Manager
            : BehaviourManager<AngularVelocityConstraint>, IForceProvider
        {
            public void Update(float elapsedTime)
            {
                for (int i = 0; i < Behaviours.Count; i++)
                {
                    var constraint = Behaviours[i];
                    var body = constraint._body;

                    var torque = (constraint._targetVelocity.Value - body.AngularVelocity) * constraint._strength.Value;
                    torque -= body.AngularAcceleration * constraint._damping.Value;
                    
                    body.ApplyTorque(torque);
                }
            }
        }
    }
}
