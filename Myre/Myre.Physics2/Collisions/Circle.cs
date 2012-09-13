using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;

namespace Myre.Physics2.Collisions
{
    public class Circle
        : Geometry
    {
        private readonly Vector2[] _axes;
        private readonly Vector2[] _vertices;
        private Property<float> _radius;
        private Property<Vector2> _centre;
        private Property<Matrix> _transform;
        
        private BoundingBox _bounds;
        private float _transformedRadius;
        private Vector2 _transformedCentre;

        public override BoundingBox Bounds
        {
            get { return _bounds; }
        }

        public float Radius 
        {
            get { return _radius.Value; }
            set { _radius.Value = value; }
        }

        public Vector2 Centre 
        {
            get { return _centre.Value; }
            set { _centre.Value = value; }
        }

        public Circle()
        {
            _axes = new Vector2[1];
            _vertices = new Vector2[2];
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            var prefix = Name != null ? Name + "_" : string.Empty;
            _radius = context.CreateProperty<float>(prefix + "radius");
            _centre = context.CreateProperty<Vector2>(prefix + "centre");
            _transform = context.CreateProperty<Matrix>("transform", Matrix.Identity);
            
            _radius.PropertySet += (p, o, n) => UpdateBounds();
            _centre.PropertySet += (p, o, n) => UpdateBounds();
            _transform.PropertySet += (p, o, n) => UpdateBounds();
            
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
            _transformedCentre = Vector2.Transform(_centre.Value, _transform.Value);
            _transformedRadius = _radius.Value;// *transform.Value.M11;

            Vector3 c = new Vector3(_transformedCentre, 0);
            Vector3 extents = new Vector3(_transformedRadius, _transformedRadius, 0);
            _bounds.Min = c - extents;
            _bounds.Max = c + extents;
        }

        public override Projection Project(Vector2 axis)
        {
            Vector2 axisNormalised = Vector2.Normalize(axis);
            Vector2 minIntersection = axisNormalised * -_transformedRadius + _transformedCentre;
            Vector2 maxIntersection = axisNormalised * _transformedRadius + _transformedCentre;

            return new Projection(Vector2.Dot(axis, minIntersection), Vector2.Dot(axis, maxIntersection), minIntersection, maxIntersection);
        }

        public override Vector2[] GetAxes(Geometry otherObject)
        {
            _axes[0] = otherObject.GetClosestVertex(_transformedCentre) - _transformedCentre;
            if (_axes[0] == Vector2.Zero)
                _axes[0] = Vector2.UnitY;

            return _axes;
        }

        public override Vector2[] GetVertices(Vector2 axis)
        {
            _vertices[0] = _transformedCentre + axis * _transformedRadius;
            _vertices[1] = _transformedCentre - axis * _transformedRadius;

            return _vertices;
        }

        public override Vector2 GetClosestVertex(Vector2 point)
        {
            Vector2 r = point - _transformedCentre;
            r *= _transformedRadius / r.Length();

            return _transformedCentre + r;
        }

        public override bool Contains(Vector2 point)
        {
            Vector2 r = point - _transformedCentre;
            return r.LengthSquared() < (_transformedRadius * _transformedRadius);
        }
    }
}
