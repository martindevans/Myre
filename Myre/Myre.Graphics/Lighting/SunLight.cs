using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class SunLight
        : Behaviour
    {
        private static readonly TypedName<Vector3> _colourName = new TypedName<Vector3>("colour");
        private static readonly TypedName<Vector3> _directionName = new TypedName<Vector3>("direction");
        private static readonly TypedName<int> _shadowResolutionName = new TypedName<int>("shadow_resolution");

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
            _colour = context.CreateProperty(_colourName, Color.LightBlue.ToVector3());
            _direction = context.CreateProperty(_directionName, Vector3.Down);
            _shadowResolution = context.CreateProperty(_shadowResolutionName);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(_colourName, _colour);
            initialisationData.TryCopyValue(_directionName, _direction);
            initialisationData.TryCopyValue(_shadowResolutionName, _shadowResolution);
        }
    }
}
