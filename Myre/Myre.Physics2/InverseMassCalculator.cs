using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Myre.Entities;

namespace Myre.Physics2
{
    public class InverseMassCalculator
        :Behaviour
    {
        public const String MASS = PhysicsProperties.MASS;
        public const String INVERSE_MASS = PhysicsProperties.INVERSE_MASS;

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            Property<float> mass = context.CreateProperty<float>(MASS);
            Property<float> invMass = context.CreateProperty<float>(INVERSE_MASS);

            mass.PropertySet += (p, o, n) => { invMass.Value = 1 / mass.Value; };

            //trigger the changed property to initialise inverse mass to a useful value
            mass.Value = mass.Value;

            base.CreateProperties(context);
        }
    }
}
