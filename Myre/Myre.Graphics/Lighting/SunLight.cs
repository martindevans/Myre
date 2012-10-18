using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework;
using Myre.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Materials;
using Ninject;
using Myre.Graphics.Geometry;
using Myre.Debugging;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class SunLight
        : Behaviour
    {
        private Property<Vector3> colour;
        private Property<Vector3> direction;
        private Property<int> shadowResolution;

        public Vector3 Colour
        {
            get { return colour.Value; }
            set { colour.Value = value; }
        }

        public Vector3 Direction
        {
            get { return direction.Value; }
            set { direction.Value = Vector3.Normalize(value); }
        }

        public int ShadowResolution
        {
            get { return shadowResolution.Value; }
            set { shadowResolution.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            colour = context.CreateProperty<Vector3>("colour");
            direction = context.CreateProperty<Vector3>("direction");
            shadowResolution = context.CreateProperty<int>("shadow_resolution");

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue("colour", colour);
            initialisationData.TryCopyValue("direction", direction);
            initialisationData.TryCopyValue("shadow_resolution", shadowResolution);
        }
    }
}
