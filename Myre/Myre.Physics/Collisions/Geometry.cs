using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Myre.Entities.Behaviours;
using Myre.Entities;
using Myre.Collections;

namespace Myre.Physics.Collisions
{
    /// <summary>
    /// Represents a piece of geometry which can be used for collision detection.
    /// </summary>
    public abstract partial class Geometry
        : Behaviour
    {
        private Property<float> frictionCoefficient;
        private Property<float> restitutionCoefficient;
        private Property<bool> sleeping;
        private bool wasSleeping;

        internal List<Geometry> collidingWith;

        public DynamicPhysics Body { get; private set; }

        /// <summary>
        /// Gets or sets the restitution coefficient.
        /// This determines the 'bounciness' of the object.
        /// A value of 0 will act like a lump of clay, hitting objects and stickig to them; a value of 1 will act like a billiard ball.
        /// </summary>
        public float Restitution
        {
            get { return restitutionCoefficient.Value; }
            set { restitutionCoefficient.Value = value; }
        }

        /// <summary>
        /// Gets or sets the friction coefficient.
        /// Higher values indicate a rougher surface.
        /// </summary>
        public float FrictionCoefficient
        {
            get { return frictionCoefficient.Value; }
            set { frictionCoefficient.Value = value; }
        }

        /// <summary>
        /// Gets an axis aligned bounding box for this geometry.
        /// </summary>
        public abstract BoundingBox Bounds { get; }

        /// <summary>
        /// Gets a collection containing all geometry this geometry is colliding with.
        /// </summary>
        public ReadOnlyCollection<Geometry> CollidingWith { get; private set; }

        public Geometry()
        {
            collidingWith = new List<Geometry>();
            CollidingWith = new ReadOnlyCollection<Geometry>(collidingWith);
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            frictionCoefficient = context.CreateProperty<float>("friction_coefficient");
            restitutionCoefficient = context.CreateProperty<float>("restitution_coefficient");
            sleeping = context.CreateProperty<bool>("sleeping");

            restitutionCoefficient.PropertyChanged += ValidateRestitution;
            sleeping.PropertyChanged += WakeUp;
            
            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            Body = Owner.GetBehaviour<DynamicPhysics>();

            wasSleeping = sleeping.Value;
            base.Initialise(initialisationData);
        }

        private void ValidateRestitution(Property<float> restitution)
        {
            var value = restitution.Value;
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException("restitution must be between 0 and 1.");
        }

        private void WakeUp(Property<bool> sleeping)
        {
            if (wasSleeping && !sleeping.Value && !Body.IsStatic)
            {
                wasSleeping = false;
                for (int i = 0; i < CollidingWith.Count; i++)
                    CollidingWith[i].sleeping.Value = false;
            }
            else
                wasSleeping = sleeping.Value;
        }

        /// <summary>
        /// Gets an array of axes, each pointing out from each face on this geometry.
        /// </summary>
        /// <param name="otherObject">The other geometry instance this instance is to be tested against.</param>
        /// <returns>An array of axes, each pointing out from each face on this geometry.</returns>
        public abstract Vector2[] GetAxes(Geometry otherObject);

        /// <summary>
        /// Gets the vertices which form this geometry.
        /// </summary>
        /// <param name="axis">The axis this geometry will be projected onto.</param>
        /// <returns>The vertices which form this geometry.</returns>
        public abstract Vector2[] GetVertices(Vector2 axis);

        /// <summary>
        /// Gets the vertex on this geometry which is closest to the specified point.
        /// </summary>
        /// <param name="point">The point to be tested.</param>
        /// <returns>The vertex on this geometry which is closest to the specified point.</returns>
        public abstract Vector2 GetClosestVertex(Vector2 point);

        /// <summary>
        /// Projects this geometry onto the specified axis
        /// </summary>
        /// <param name="axis">The axis to be projected onto.</param>
        /// <returns>The projection of this geometry onto the specified axis.</returns>
        public abstract Projection Project(Vector2 axis);

        /// <summary>
        /// Determines if this geometry contains the specified point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public abstract bool Contains(Vector2 point);
    }
}