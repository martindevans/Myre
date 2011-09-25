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

namespace Myre.Graphics.Lighting
{
    public class SunLight
        : Behaviour
    {
        private Property<Vector3> colour;
        private Property<Vector3> direction;
        private Property<int> shadowResolution;
        private RenderTarget2D shadowMap;
        //private Matrix[] shadowViewMatrices = new Matrix[4];
        //private Matrix[] shadowProjectionMatrices = new Matrix[4];
        //private float[] farClip = new float[4];
        private Matrix view;
        private Matrix projection;
        private float farClip;
        private Vector4 nearPlane;

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
            this.colour = context.CreateProperty<Vector3>("colour");
            this.direction = context.CreateProperty<Vector3>("direction");
            this.shadowResolution = context.CreateProperty<int>("shadow_resolution");

            base.CreateProperties(context);
        }
    }
}
