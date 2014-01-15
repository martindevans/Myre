using System;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;

namespace Myre.Physics2D.Collisions
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
            _velocity = context.CreateProperty(new TypedName<Vector3>("velocity"), default(Vector3));
            _acceleration = context.CreateProperty(new TypedName<Vector3>("acceleration"), default(Vector3));
            _inverseMass = context.CreateProperty(new TypedName<float>(InverseMassCalculator.INVERSE_MASS), default(float));

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            if (Owner.GetBehaviour<InverseMassCalculator>(null) == null)
                throw new InvalidOperationException("Inverse mass calculator must be attached");
            _damping = Owner.GetProperty(new TypedName<float>("damping"));

            base.Initialise(initialisationData);
        }

        public class Manager
            : BehaviourManager<Damping>, IProcess
        {
            public const float DEFAULT_DAMPING = 0;

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
                    p.Dampen(p._damping == null ? DEFAULT_DAMPING : p._damping.Value);
            }
        }
    }
}
