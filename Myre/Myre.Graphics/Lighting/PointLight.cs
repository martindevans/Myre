using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class PointLight
        : Behaviour
    {
        private const string COLOUR_NAME = "colour";
        private const string POSITION_NAME = "position";
        private const string RANGE_NAME = "range";

        private Property<Vector3> _colour;
        private Property<Vector3> _position;
        private Property<float> _range;

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
            _colour = context.CreateProperty<Vector3>(COLOUR_NAME + AppendName());
            _position = context.CreateProperty<Vector3>(POSITION_NAME + AppendName());
            _range = context.CreateProperty<float>(RANGE_NAME + AppendName());
            
            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(COLOUR_NAME + AppendName(), _colour);
            initialisationData.TryCopyValue(POSITION_NAME + AppendName(), _position);
            initialisationData.TryCopyValue(RANGE_NAME + AppendName(), _range);
        }
    }
}
