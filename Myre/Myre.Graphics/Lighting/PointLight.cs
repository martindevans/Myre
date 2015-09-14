using System.Numerics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class PointLight
        : Behaviour
    {
        public static readonly TypedName<Vector3> ColourName = new TypedName<Vector3>("colour");
        public static readonly TypedName<Vector3> PositionName = new TypedName<Vector3>("position");
        public static readonly TypedName<float> RangeName = new TypedName<float>("range");
        public static readonly TypedName<bool> ActiveName = new TypedName<bool>("pointlight_active");

        private Property<Vector3> _colour;
        private Property<Vector3> _position;
        private Property<float> _range;
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

        public float Range
        {
            get { return _range.Value; }
            set { _range.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _colour = context.CreateProperty(ColourName);
            _position = context.CreateProperty(PositionName);
            _range = context.CreateProperty(RangeName);
            _active = context.CreateProperty(ActiveName, true);
            
            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, ColourName, _colour);
            initialisationData.TryCopyValue(this, PositionName, _position);
            initialisationData.TryCopyValue(this, RangeName, _range);
            initialisationData.TryCopyValue(this, ActiveName, _active);
        }
    }
}
