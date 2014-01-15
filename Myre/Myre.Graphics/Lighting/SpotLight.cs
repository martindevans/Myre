using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class SpotLight
        : Behaviour
    {
        private static readonly TypedName<Vector3> _colourName = new TypedName<Vector3>("colour");
        private static readonly TypedName<Vector3> _positionName = new TypedName<Vector3>("position");
        private static readonly TypedName<Vector3> _directionName = new TypedName<Vector3>("direction");
        private static readonly TypedName<float> _angleName = new TypedName<float>("angle");
        private static readonly TypedName<float> _rangeName = new TypedName<float>("range");
        private static readonly TypedName<Texture2D> _maskName = new TypedName<Texture2D>("mask");
        private static readonly TypedName<int> _shadowResolutionName = new TypedName<int>("shadow_resolution");
        private static readonly TypedName<bool> _activeName = new TypedName<bool>("spotlight_active");

        private Property<Vector3> _colour;
        private Property<Vector3> _position;
        private Property<Vector3> _direction;
        private Property<float> _angle;
        private Property<float> _range;
        private Property<Texture2D> _mask;
        private Property<int> _shadowResolution;
        private Property<bool> _active;

        public bool Active
        {
            get { return _active.Value; }
            set { _active.Value = value; }
        }

        public Vector3 Colour
        {
            get { return _colour.Value; }
            set { _colour.Value = value; }
        }

        public Vector3 Position
        {
            get { return _position.Value; }
            set { _position.Value = value; }
        }

        public Vector3 Direction
        {
            get { return _direction.Value; }
            set { _direction.Value = Vector3.Normalize(value); }
        }

        public float Angle
        {
            get { return _angle.Value; }
            set { _angle.Value = value; }
        }

        public float Range
        {
            get { return _range.Value; }
            set { _range.Value = value; }
        }

        public Texture2D Mask
        {
            get { return _mask.Value; }
            set { _mask.Value = value; }
        }

        public int ShadowResolution
        {
            get { return _shadowResolution.Value; }
            set { _shadowResolution.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _colour = context.CreateProperty(_colourName);
            _position = context.CreateProperty(_positionName);
            _direction = context.CreateProperty(_directionName);
            _angle = context.CreateProperty(_angleName);
            _range = context.CreateProperty(_rangeName);
            _mask = context.CreateProperty(_maskName);
            _shadowResolution = context.CreateProperty(_shadowResolutionName);
            _active = context.CreateProperty(_activeName, true);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(_colourName, _colour);
            initialisationData.TryCopyValue(_positionName, _position);
            initialisationData.TryCopyValue(_directionName, _direction);
            initialisationData.TryCopyValue(_angleName, _angle);
            initialisationData.TryCopyValue(_rangeName, _range);
            initialisationData.TryCopyValue(_maskName, _mask);
            initialisationData.TryCopyValue(_shadowResolutionName, _shadowResolution);
            initialisationData.TryCopyValue(_activeName, _active);
        }
    }
}
