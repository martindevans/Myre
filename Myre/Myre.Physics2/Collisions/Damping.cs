using System;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;

namespace Myre.Physics2.Dynamics.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class Damping
        :Behaviour
    {
        private Property<Vector3> _velocity;
        private Property<Vector3> _acceleration;
        private Property<float> _inverseMass;

        private Property<float> _damping;

        public void Dampen(float damping)
        {
            _acceleration.Value -= _velocity.Value * damping * _inverseMass.Value;
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _velocity = context.CreateProperty<Vector3>("velocity");
            _acceleration = context.CreateProperty<Vector3>("acceleration");
            _inverseMass = context.CreateProperty<float>(InverseMassCalculator.INVERSE_MASS);

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            if (Owner.GetBehaviour<InverseMassCalculator>() == null)
                throw new InvalidOperationException("Inverse mass calculator must be attached");
            _damping = Owner.GetProperty<float>("damping");

            base.Initialise(initialisationData);
        }

        public class Manager
            : BehaviourManager<Damping>, IProcess
        {
            public float DefaultDamping = 0;

            public bool IsComplete
            {
                get { return false; }
            }

            public Manager(Scene scene)
            {
                scene.GetService<ProcessService>().Add(this);
            }

            public void Update(float elapsedTime)
            {
                foreach (var p in Behaviours)
                    p.Dampen(p._damping == null ? DefaultDamping : p._damping.Value);
            }
        }
    }
}
