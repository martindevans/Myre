using System.Numerics;
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
        private static readonly TypedName<Vector3> _upName = new TypedName<Vector3>("up");
        private static readonly TypedName<float> _angleName = new TypedName<float>("angle");
        private static readonly TypedName<float> _rangeName = new TypedName<float>("range");
        private static readonly TypedName<Texture2D> _maskName = new TypedName<Texture2D>("mask");
        private static readonly TypedName<int> _shadowResolutionName = new TypedName<int>("shadow_resolution");
        private static readonly TypedName<bool> _activeName = new TypedName<bool>("spotlight_active");
        private static readonly TypedName<float> _falloffName = new TypedName<float>("falloff");

        private Property<Vector3> _colour;
        private Property<Vector3> _position;
        private Property<Vector3> _direction;
        private Property<Vector3> _up;
        private Property<float> _angle;
        private Property<float> _range;
        private Property<Texture2D> _mask;
        private Property<int> _shadowResolution;
        private Property<bool> _active;
        private Property<float> _falloff;

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

        public Vector3 Up
        {
            get { return _up.Value; }
            set { _up.Value = value; }
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

        public float Falloff
        {
            get { return _falloff.Value; }
            set { _falloff.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _colour = context.CreateProperty(_colourName);
            _position = context.CreateProperty(_positionName);
            _direction = context.CreateProperty(_directionName);
            _up = context.CreateProperty(_upName, Vector3.UnitY);
            _angle = context.CreateProperty(_angleName);
            _range = context.CreateProperty(_rangeName);
            _mask = context.CreateProperty(_maskName);
            _shadowResolution = context.CreateProperty(_shadowResolutionName);
            _active = context.CreateProperty(_activeName, true);
            _falloff = context.CreateProperty(_falloffName, 1);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, _colourName, _colour);
            initialisationData.TryCopyValue(this, _positionName, _position);
            initialisationData.TryCopyValue(this, _directionName, _direction);
            initialisationData.TryCopyValue(this, _upName, _up);
            initialisationData.TryCopyValue(this, _angleName, _angle);
            initialisationData.TryCopyValue(this, _rangeName, _range);
            initialisationData.TryCopyValue(this, _maskName, _mask);
            initialisationData.TryCopyValue(this, _shadowResolutionName, _shadowResolution);
            initialisationData.TryCopyValue(this, _activeName, _active);
            initialisationData.TryCopyValue(this, _falloffName, _falloff);
        }
    }
}
