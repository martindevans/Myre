using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities;
using Microsoft.Xna.Framework;
using Myre.Entities.Behaviours;
using Myre.Entities.Services;
using Myre.Collections;

namespace Myre.Physics.Dynamics.Constraints
{
    [DefaultManager(typeof(Manager))]
    public class Damping
        :Behaviour
    {
        private Property<Vector3> velocity;
        private Property<Vector3> acceleration;
        private Property<float> inverseMass;

        private Property<float> damping;

        public void Dampen(float damping)
        {
            acceleration.Value -= velocity.Value * damping * inverseMass.Value;
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            velocity = context.CreateProperty<Vector3>("velocity");
            acceleration = context.CreateProperty<Vector3>("acceleration");
            inverseMass = context.CreateProperty<float>(InverseMassCalculator.INVERSE_MASS);

            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            if (Owner.GetBehaviour<InverseMassCalculator>() == null)
                throw new InvalidOperationException("Inverse mass calculator must be attached");
            damping = Owner.GetProperty<float>("damping");

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
                    p.Dampen(p.damping == null ? DefaultDamping : p.damping.Value);
            }
        }
    }
}
