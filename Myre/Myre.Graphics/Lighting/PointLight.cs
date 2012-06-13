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
using System.Diagnostics;
using Myre.Debugging.Statistics;

namespace Myre.Graphics.Lighting
{
    public class PointLight
        : Behaviour
    {
        private Property<Vector3> colour;
        private Property<Vector3> position;
        private Property<float> range;

        public Vector3 Colour
        {
            get { return colour.Value; }
            set { colour.Value = value; }
        }

        public Vector3 Position
        {
            get { return position.Value; }
            set { position.Value = value; }
        }

        public float Range
        {
            get { return range.Value; }
            set { range.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            colour = context.CreateProperty<Vector3>("colour");
            position = context.CreateProperty<Vector3>("position");
            range = context.CreateProperty<float>("range");
            
            base.CreateProperties(context);
        }
    }
}
