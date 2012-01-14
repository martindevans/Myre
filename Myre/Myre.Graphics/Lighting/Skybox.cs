using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Ninject;
using Microsoft.Xna.Framework.Content;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework;

namespace Myre.Graphics.Lighting
{
    public class Skybox
        : Behaviour
    {
        private Property<TextureCube> texture;
        private Property<float> brightness;
        private Property<bool> gammaCorrect;

        public TextureCube Texture
        {
            get { return texture.Value; }
            set { texture.Value = value; }
        }

        public float Brightness
        {
            get { return brightness.Value; }
            set { brightness.Value = value; }
        }

        public bool GammaCorrect
        {
            get { return gammaCorrect.Value; }
            set { gammaCorrect.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            this.texture = context.CreateProperty<TextureCube>("texture");
            this.brightness = context.CreateProperty<float>("brightness");
            this.gammaCorrect = context.CreateProperty<bool>("gamma_correct");

            base.CreateProperties(context);
        }
    }
}