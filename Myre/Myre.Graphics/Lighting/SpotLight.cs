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
    public class SpotLight
        : Behaviour
    {
        private Property<Vector3> colour;
        private Property<Vector3> position;
        private Property<Vector3> direction;
        private Property<float> angle;
        private Property<float> range;
        private Property<Texture2D> mask;
        private Property<int> shadowResolution;
        private RenderTarget2D shadowMap;
        private Matrix view;
        private Matrix projection;

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

        public Vector3 Direction
        {
            get { return direction.Value; }
            set { direction.Value = Vector3.Normalize(value); }
        }

        public float Angle
        {
            get { return angle.Value; }
            set { angle.Value = value; }
        }

        public float Range
        {
            get { return range.Value; }
            set { range.Value = value; }
        }

        public Texture2D Mask
        {
            get { return mask.Value; }
            set { mask.Value = value; }
        }

        public int ShadowResolution
        {
            get { return shadowResolution.Value; }
            set { shadowResolution.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            this.colour = context.CreateProperty<Vector3>("colour");
            this.position = context.CreateProperty<Vector3>("position");
            this.direction = context.CreateProperty<Vector3>("direction");
            this.angle = context.CreateProperty<float>("angle");
            this.range = context.CreateProperty<float>("range");
            this.mask = context.CreateProperty<Texture2D>("mask");
            this.shadowResolution = context.CreateProperty<int>("shadow_resolution");

            base.CreateProperties(context);
        }
    }
}
