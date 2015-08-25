using System.Numerics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;
using Myre.Extensions;
using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Lighting
{
    public class AmbientLight
        : Behaviour
    {
        public static readonly TypedName<Vector3> SkyColourName = new TypedName<Vector3>("sky_colour");
        public static readonly TypedName<Vector3> GroundColourName = new TypedName<Vector3>("ground_colour");
        public static readonly TypedName<Vector3> UpName = new TypedName<Vector3>("up");

        private Property<Vector3> _skyColour;
        private Property<Vector3> _groundColour;
        private Property<Vector3> _up;

        public Vector3 SkyColour
        {
            get { return _skyColour.Value; }
            set { _skyColour.Value = value; }
        }

        public Vector3 GroundColour
        {
            get { return _groundColour.Value; }
            set { _groundColour.Value = value; }
        }

        public Vector3 Up
        {
            get { return _up.Value; }
            set { _up.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _skyColour = context.CreateProperty(SkyColourName, Color.LightSkyBlue.ToVector3().FromXNA());
            _groundColour = context.CreateProperty(GroundColourName, Color.LightGray.ToVector3().FromXNA());
            _up = context.CreateProperty(UpName, Vector3.UnitY);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, SkyColourName, _skyColour);
            initialisationData.TryCopyValue(this, GroundColourName, _groundColour);
            initialisationData.TryCopyValue(this, UpName, _up);
        }
    }
}
