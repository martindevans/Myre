using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class SunLight
        : Behaviour
    {
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
            _colour = context.CreateProperty<Vector3>("colour");
            _direction = context.CreateProperty<Vector3>("direction");
            _shadowResolution = context.CreateProperty<int>("shadow_resolution");

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue("colour", _colour);
            initialisationData.TryCopyValue("direction", _direction);
            initialisationData.TryCopyValue("shadow_resolution", _shadowResolution);
        }
    }
}
