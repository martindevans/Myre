using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;

namespace Myre.Graphics.Lighting
{
    public class Skybox
        : Behaviour
    {
        private Property<TextureCube> _texture;
        private Property<float> _brightness;
        private Property<bool> _gammaCorrect;

        public TextureCube Texture
        {
            get { return _texture.Value; }
            set { _texture.Value = value; }
        }

        public float Brightness
        {
            get { return _brightness.Value; }
            set { _brightness.Value = value; }
        }

        public bool GammaCorrect
        {
            get { return _gammaCorrect.Value; }
            set { _gammaCorrect.Value = value; }
        }

        public override void CreateProperties(Entity.ConstructionContext context)
        {
            _texture = context.CreateProperty<TextureCube>("texture");
            _brightness = context.CreateProperty<float>("brightness");
            _gammaCorrect = context.CreateProperty<bool>("gamma_correct");

            base.CreateProperties(context);
        }
    }
}