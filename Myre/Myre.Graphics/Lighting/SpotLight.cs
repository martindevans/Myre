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
        public static readonly TypedName<Vector3> ColourName = new TypedName<Vector3>("colour");
        public static readonly TypedName<Vector3> PositionName = new TypedName<Vector3>("position");
        public static readonly TypedName<Vector3> DirectionName = new TypedName<Vector3>("direction");
        public static readonly TypedName<Vector3> UpName = new TypedName<Vector3>("up");
        public static readonly TypedName<float> AngleName = new TypedName<float>("angle");
        public static readonly TypedName<float> RangeName = new TypedName<float>("range");
        public static readonly TypedName<Texture2D> MaskName = new TypedName<Texture2D>("mask");
        public static readonly TypedName<int> ShadowResolutionName = new TypedName<int>("shadow_resolution");
        public static readonly TypedName<bool> ActiveName = new TypedName<bool>("spotlight_active");
        public static readonly TypedName<float> FalloffName = new TypedName<float>("falloff");

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
            _colour = context.CreateProperty(ColourName);
            _position = context.CreateProperty(PositionName);
            _direction = context.CreateProperty(DirectionName);
            _up = context.CreateProperty(UpName, Vector3.UnitY);
            _angle = context.CreateProperty(AngleName);
            _range = context.CreateProperty(RangeName);
            _mask = context.CreateProperty(MaskName);
            _shadowResolution = context.CreateProperty(ShadowResolutionName);
            _active = context.CreateProperty(ActiveName, true);
            _falloff = context.CreateProperty(FalloffName, 1);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, ColourName, _colour);
            initialisationData.TryCopyValue(this, PositionName, _position);
            initialisationData.TryCopyValue(this, DirectionName, _direction);
            initialisationData.TryCopyValue(this, UpName, _up);
            initialisationData.TryCopyValue(this, AngleName, _angle);
            initialisationData.TryCopyValue(this, RangeName, _range);
            initialisationData.TryCopyValue(this, MaskName, _mask);
            initialisationData.TryCopyValue(this, ShadowResolutionName, _shadowResolution);
            initialisationData.TryCopyValue(this, ActiveName, _active);
            initialisationData.TryCopyValue(this, FalloffName, _falloff);
        }
    }
}
