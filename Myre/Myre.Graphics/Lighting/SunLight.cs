using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class SunLight
        : Behaviour
    {
        private const string COLOUR_NAME = "colour";
        private const string DIRECTION_NAME = "direction";
        private const string SHADOW_RESOLUTION_NAME = "shadow_resolution";

        private Property<Vector3> _colour;
        private Property<Vector3> _direction;
        private Property<int> _shadowResolution;

        public Vector3 Colour
        {
            get { return _colour.Value; }
            set { _colour.Value = value; }
        }

        public Vector3 Direction
        {
            get { return _direction.Value; }
            set { _direction.Value = Vector3.Normalize(value); }
        }

        public int ShadowResolution
        {
            get { return _shadowResolution.Value; }
            set { _shadowResolution.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _colour = context.CreateProperty(new TypedName<Vector3>(COLOUR_NAME + AppendName()), Color.LightBlue.ToVector3());
            _direction = context.CreateProperty(new TypedName<Vector3>(DIRECTION_NAME + AppendName()), Vector3.Down);
            _shadowResolution = context.CreateProperty(new TypedName<int>(SHADOW_RESOLUTION_NAME + AppendName()));

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(COLOUR_NAME + AppendName(), _colour);
            initialisationData.TryCopyValue(DIRECTION_NAME + AppendName(), _direction);
            initialisationData.TryCopyValue(SHADOW_RESOLUTION_NAME + AppendName(), _shadowResolution);
        }
    }
}
