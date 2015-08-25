using System.Numerics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;
using Myre.Extensions;
using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Lighting
{
    public class SunLight
        : Behaviour
    {
        public static readonly TypedName<Vector3> ColourName = new TypedName<Vector3>("colour");
        public static readonly TypedName<Vector3> DirectionName = new TypedName<Vector3>("direction");
        public static readonly TypedName<int> ShadowResolutionName = new TypedName<int>("shadow_resolution");
        public static readonly TypedName<bool> ActiveName = new TypedName<bool>("sunlight_active");

        private Property<Vector3> _colour;
        private Property<Vector3> _direction;
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
            _colour = context.CreateProperty(ColourName, Color.LightBlue.ToVector3().FromXNA());
            _direction = context.CreateProperty(DirectionName, -Vector3.UnitY);
            _shadowResolution = context.CreateProperty(ShadowResolutionName);
            _active = context.CreateProperty(ActiveName, true);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, ColourName, _colour);
            initialisationData.TryCopyValue(this, DirectionName, _direction);
            initialisationData.TryCopyValue(this, ShadowResolutionName, _shadowResolution);
            initialisationData.TryCopyValue(this, ActiveName, _active);
        }
    }
}
