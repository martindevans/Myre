using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Collections;

namespace Myre.Physics.Collisions
{
    public class Circle
        : Geometry
    {
        private Vector2[] axes;
        private Vector2[] vertices;
        private Property<float> radius;
        private Property<Vector2> centre;
        private Property<Matrix> transform;
        
        private BoundingBox bounds;
        private float transformedRadius;
        private Vector2 transformedCentre;

        public override BoundingBox Bounds
        {
            get { return bounds; }
        }

        public float Radius 
        {
            get { return radius.Value; }
            set { radius.Value = value; }
        }

        public Vector2 Centre 
        {
            get { return centre.Value; }
            set { centre.Value = value; }
        }

        public Circle()
        {
            axes = new Vector2[1];
            vertices = new Vector2[2];
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            var prefix = Name != null ? Name + "_" : string.Empty;
            radius = context.CreateProperty<float>(prefix + "radius");
            centre = context.CreateProperty<Vector2>(prefix + "centre");
            transform = context.CreateProperty<Matrix>("transform", Matrix.Identity);
            
            radius.PropertyChanged += _ => UpdateBounds();
            centre.PropertyChanged += _ => UpdateBounds();
            transform.PropertyChanged += _ => UpdateBounds();
            
            base.CreateProperties(context);
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            var transform = Owner.GetBehaviour<Transform>();
            if (transform != null)
                transform.CalculateTransform();
            
            base.Initialise(initialisationData);
        }

        private void UpdateBounds()
        {
            transformedCentre = Vector2.Transform(centre.Value, transform.Value);
            transformedRadius = radius.Value;// *transform.Value.M11;

            Vector3 c = new Vector3(transformedCentre, 0);
            Vector3 extents = new Vector3(transformedRadius, transformedRadius, 0);
            bounds.Min = c - extents;
            bounds.Max = c + extents;
        }

        public override Projection Project(Vector2 axis)
        {
            Vector2 axisNormalised = Vector2.Normalize(axis);
            Vector2 minIntersection = axisNormalised * -transformedRadius + transformedCentre;
            Vector2 maxIntersection = axisNormalised * transformedRadius + transformedCentre;

            return new Projection(Vector2.Dot(axis, minIntersection), Vector2.Dot(axis, maxIntersection), minIntersection, maxIntersection);
        }

        public override Vector2[] GetAxes(Geometry otherObject)
        {
            axes[0] = otherObject.GetClosestVertex(transformedCentre) - transformedCentre;
            if (axes[0] == Vector2.Zero)
                axes[0] = Vector2.UnitY;

            return axes;
        }

        public override Vector2[] GetVertices(Vector2 axis)
        {
            vertices[0] = transformedCentre + axis * transformedRadius;
            vertices[1] = transformedCentre - axis * transformedRadius;

            return vertices;
        }

        public override Vector2 GetClosestVertex(Vector2 point)
        {
            Vector2 r = point - transformedCentre;
            r *= transformedRadius / r.Length();

            return transformedCentre + r;
        }

        public override bool Contains(Vector2 point)
        {
            Vector2 r = point - transformedCentre;
            return r.LengthSquared() < (transformedRadius * transformedRadius);
        }
    }
}
