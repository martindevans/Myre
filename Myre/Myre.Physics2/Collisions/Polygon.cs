using System;
using Microsoft.Xna.Framework;
using Myre.Collections;
using Myre.Entities;

namespace Myre.Physics2.Collisions
{
    public class Polygon
        : Geometry
    {
        private Vector2[] _localVertices;
        private Vector2[] _worldVertices;
        private Vector2[] _localAxes;
        private Vector2[] _worldAxes;
        private readonly Vector2[] _localBounds;
        private readonly Vector2[] _worldBounds;
        private BoundingBox _bounds;

        private Property<Matrix> _transformProperty;
        private Property<Vector2[]> _verticesProperty;
        private Transform _transformBehaviour;

        public Matrix Transform
        {
            get { return _transformProperty.Value; }
            set { _transformProperty.Value = value; }
        }

        public Vector2[] WorldVertices
        {
            get { return _worldVertices; }
        }

        public Vector2[] LocalVertices
        {
            get { return _localVertices; }
        }

        public override BoundingBox Bounds
        {
            get 
            {
                if (_transformBehaviour != null)
                    _transformBehaviour.CalculateTransform();

                return _bounds; 
            }
        }

        public Polygon()
        {
            _localBounds = new Vector2[4];
            _worldBounds = new Vector2[4];
            CreateArrays(0);
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            var prefix = Name != null ? Name + "_" : string.Empty;
            _transformProperty = context.CreateProperty<Matrix>("transform", Matrix.Identity);
            _verticesProperty = context.CreateProperty<Vector2[]>(prefix + "vertices");

            _transformProperty.PropertySet += (p, o, n) => ApplyTransform();
            _verticesProperty.PropertySet += (p, o, n) => ReadVertices(p);
            
            base.CreateProperties(context);
        }

        private void ReadVertices(Property<Vector2[]> p)
        {
            if (_localVertices.Length != p.Value.Length)
                CreateArrays(p.Value.Length);

            Array.Copy(p.Value, _localVertices, p.Value.Length);
            InitialiseLocalData();
            ApplyTransform();
        }

        public override void Initialise(INamedDataProvider initialisationData)
        {
            _transformBehaviour = Owner.GetBehaviour<Transform>();

            ReadVertices(_verticesProperty);
            base.Initialise(initialisationData);
        }

        private void CreateArrays(int size)
        {
            _localVertices = new Vector2[size];
            _worldVertices = new Vector2[size];
            _localAxes = new Vector2[size];
            _worldAxes = new Vector2[size];
        }

        private void InitialiseLocalData()
        {
            CalculateAxes();
            CalculateLocalBounds();
        }

        private void CalculateAxes()
        {
            for (int i = 0; i < _worldAxes.Length; i++)
            {
                var start = _localVertices[i];
                var end = _localVertices[(i + 1) % _localVertices.Length];

                var r = end - start;
                _worldAxes[i] = _localAxes[i] = new Vector2(-r.Y, r.X);
            }
        }

        private void CalculateLocalBounds()
        {
            _bounds.Min = new Vector3(float.MaxValue, float.MaxValue, 0);
            _bounds.Max = new Vector3(float.MinValue, float.MinValue, 0);
            for (int i = 0; i < _localVertices.Length; i++)
            {
                var v = _localVertices[i];
                if (_bounds.Min.X > v.X)
                    _bounds.Min.X = v.X;
                if (_bounds.Min.Y > v.Y)
                    _bounds.Min.Y = v.Y;
                if (_bounds.Max.X < v.X)
                    _bounds.Max.X = v.X;
                if (_bounds.Max.Y < v.Y)
                    _bounds.Max.Y = v.Y;
            }

            _localBounds[0] = _worldBounds[0] = new Vector2(_bounds.Min.X, _bounds.Min.Y);
            _localBounds[1] = _worldBounds[1] = new Vector2(_bounds.Max.X, _bounds.Min.Y);
            _localBounds[2] = _worldBounds[2] = new Vector2(_bounds.Max.X, _bounds.Max.Y);
            _localBounds[3] = _worldBounds[3] = new Vector2(_bounds.Min.X, _bounds.Max.Y);
        }

        private void ApplyTransform()
        {
            var transform = _transformProperty.Value;
            Vector2.Transform(_localVertices, ref transform, _worldVertices);
            Vector2.TransformNormal(_localAxes, ref transform, _worldAxes);

            CalculateWorldBounds();
        }

        private void CalculateWorldBounds()
        {
            var transform = _transformProperty.Value;
            Vector2.Transform(_localBounds, ref transform, _worldBounds);
            _bounds.Min = new Vector3(float.MaxValue, float.MaxValue, 0);
            _bounds.Max = new Vector3(float.MinValue, float.MinValue, 0);
            for (int i = 0; i < _worldBounds.Length; i++)
            {
                var v = _worldBounds[i];
                if (_bounds.Min.X > v.X)
                    _bounds.Min.X = v.X;
                if (_bounds.Min.Y > v.Y)
                    _bounds.Min.Y = v.Y;
                if (_bounds.Max.X < v.X)
                    _bounds.Max.X = v.X;
                if (_bounds.Max.Y < v.Y)
                    _bounds.Max.Y = v.Y;
            }
        }

        public override Vector2[] GetAxes(Geometry otherObject)
        {
            return _worldAxes;
        }

        public override Vector2[] GetVertices(Vector2 axis)
        {
            return _worldVertices;
        }

        public override Vector2 GetClosestVertex(Vector2 point)
        {
            // TODO: more directed search for closest vertex on a polygon
            Vector2 closest = _worldVertices[0];
            float distance = (closest - point).LengthSquared();
            for (int i = 1; i < _worldVertices.Length; i++)
            {
                var v = _worldVertices[i];
                var r = v - point;
                var d = r.LengthSquared();

                if (d < distance)
                {
                    distance = d;
                    closest = v;
                }
            }

            return closest;
        }

        public override Projection Project(Vector2 axis)
        {
            return Projection.Create(axis, _worldVertices);
        }

        // algorithm found here:
        // http://local.wasp.uwa.edu.au/~pbourke/geometry/insidepoly/
        public override bool Contains(Vector2 p)
        {
            int counter = 0;
            int i;

            Vector2 p1 = _worldVertices[0];
            for (i = 1; i <= _worldVertices.Length; i++)
            {
                Vector2 p2 = _worldVertices[i % _worldVertices.Length];
                if (p.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (p.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (p.X <= Math.Max(p1.X, p2.X))
                        {
                            if (p1.Y != p2.Y)
                            {
                                double xinters = (p.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                if (p1.X == p2.X || p.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            return (counter % 2 != 0);
        }
    }
}
