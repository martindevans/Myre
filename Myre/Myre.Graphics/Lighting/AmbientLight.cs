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
using Myre.Graphics.PostProcessing;

namespace Myre.Graphics.Lighting
{
    public class AmbientLight
        : Behaviour
    {
        private Property<Vector3> skyColour;
        private Property<Vector3> groundColour;
        private Property<Vector3> up;

        public Vector3 SkyColour
        {
            get { return skyColour.Value; }
            set { skyColour.Value = value; }
        }

        public Vector3 GroundColour
        {
            get { return groundColour.Value; }
            set { groundColour.Value = value; }
        }

        public Vector3 Up
        {
            get { return up.Value; }
            set { up.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            this.skyColour = context.CreateProperty<Vector3>("sky_colour");
            this.groundColour = context.CreateProperty<Vector3>("ground_colour");
            this.up = context.CreateProperty<Vector3>("up");

            base.CreateProperties(context);
        }
    }
}
