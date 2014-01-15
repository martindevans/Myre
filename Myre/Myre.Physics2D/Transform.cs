using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Entities.Behaviours;

namespace Myre.Physics2D
{
    [DefaultManager(typeof(Manager<Transform>))]
    public class Transform
        : ProcessBehaviour
    {
        private Property<Vector2> _position;
        private Property<float> _rotation;
        private Property<Matrix> _transform;
        private Property<Matrix> _inverseTransform;
        private bool _isDirty;

        public Vector2 Position
        {
            get { return _position.Value; }
            set { _position.Value = value; }
        }

        public float Rotation
        {
            get { return _rotation.Value; }
            set { _rotation.Value = value; }
        }

        public Matrix TransformMatrix
        {
            get { return _transform.Value; }
            set { _transform.Value = value; }
        }

        public Matrix InverseTransformMatrix
        {
            get { return _inverseTransform.Value; }
            set { _inverseTransform.Value = value; }
        }
        
        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _position = context.CreateProperty(new TypedName<Vector2>(PhysicsProperties.POSITION));
            _rotation = context.CreateProperty(new TypedName<float>(PhysicsProperties.ROTATION));
            _transform = context.CreateProperty(new TypedName<Matrix>("transform"));
            _inverseTransform = context.CreateProperty(new TypedName<Matrix>("inverse_transform"));

            _position.PropertySet += (p, o, n) => _isDirty = true;
            _rotation.PropertySet += (p, o, n) => _isDirty = true;
            
            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            _isDirty = true;
        }

        public Vector2 ToWorldCoordinates(Vector2 point)
        {
            return Vector2.Transform(point, _transform.Value);
        }

        public Vector2 ToLocalCoordinates(Vector2 point)
        {
            return Vector2.Transform(point, _inverseTransform.Value);
        }

        public void CalculateTransform()
        {
            if (!_isDirty)
                return;

            var pos = new Vector3(_position.Value, 0);
            var rot = _rotation.Value;

            Matrix temp1, temp2;
            Matrix.CreateRotationZ(rot, out temp1);
            Matrix.CreateTranslation(ref pos, out temp2);
            Matrix.Multiply(ref temp1, ref temp2, out temp1);
            Matrix.Invert(ref temp1, out temp2);

            _transform.Value = temp1;
            _inverseTransform.Value = temp2;
            _isDirty = false;
        }

        protected override void Update(float elapsedTime)
        {
            CalculateTransform();
        }
    }
}
