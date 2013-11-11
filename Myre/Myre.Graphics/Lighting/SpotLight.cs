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
        private const string COLOUR_NAME = "colour";
        private const string POSITION_NAME = "position";
        private const string DIRECTION_NAME = "direction";
        private const string ANGLE_NAME = "angle";
        private const string RANGE_NAME = "range";
        private const string MASK_NAME = "mask";
        private const string SHADOW_RESOLUTION_NAME = "shadow_resolution";
        private const string ACTIVE_NAME = "spotlight_active";

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
            _colour = context.CreateProperty<Vector3>(COLOUR_NAME + AppendName());
            _position = context.CreateProperty<Vector3>(POSITION_NAME + AppendName());
            _direction = context.CreateProperty<Vector3>(DIRECTION_NAME + AppendName());
            _angle = context.CreateProperty<float>(ANGLE_NAME + AppendName());
            _range = context.CreateProperty<float>(RANGE_NAME + AppendName());
            _mask = context.CreateProperty<Texture2D>(MASK_NAME + AppendName());
            _shadowResolution = context.CreateProperty<int>(SHADOW_RESOLUTION_NAME + AppendName());
            _active = context.CreateProperty<bool>(ACTIVE_NAME + AppendName(), true);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(COLOUR_NAME + AppendName(), _colour);
            initialisationData.TryCopyValue(POSITION_NAME + AppendName(), _position);
            initialisationData.TryCopyValue(DIRECTION_NAME + AppendName(), _direction);
            initialisationData.TryCopyValue(ANGLE_NAME + AppendName(), _angle);
            initialisationData.TryCopyValue(RANGE_NAME + AppendName(), _range);
            initialisationData.TryCopyValue(MASK_NAME + AppendName(), _mask);
            initialisationData.TryCopyValue(SHADOW_RESOLUTION_NAME + AppendName(), _shadowResolution);
            initialisationData.TryCopyValue(ACTIVE_NAME + AppendName(), _active);
        }
    }
}
