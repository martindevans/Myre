using Microsoft.Xna.Framework;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class AmbientLight
        : Behaviour
    {
        private const string SKY_COLOUR_NAME = "sky_colour";
        private const string GROUND_COLOUR_NAME = "ground_colour";
        private const string UP_NAME = "up";

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
            _skyColour = context.CreateProperty<Vector3>(SKY_COLOUR_NAME + AppendName(), Color.LightSkyBlue.ToVector3());
            _groundColour = context.CreateProperty<Vector3>(GROUND_COLOUR_NAME + AppendName(), Color.LightGray.ToVector3());
            _up = context.CreateProperty<Vector3>(UP_NAME + AppendName(), Vector3.Up);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(SKY_COLOUR_NAME + AppendName(), _skyColour);
            initialisationData.TryCopyValue(GROUND_COLOUR_NAME + AppendName(), _groundColour);
            initialisationData.TryCopyValue(UP_NAME + AppendName(), _up);
        }
    }
}
