using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class Skybox
        : Behaviour
    {
        private static readonly TypedName<TextureCube> _textureName = new TypedName<TextureCube>("texture");
        private static readonly TypedName<float> _brightnessName = new TypedName<float>("brightness");
        private static readonly TypedName<bool> _gammaCorrectName = new TypedName<bool>("gamma_correct");

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
            _texture = context.CreateProperty(_textureName);
            _brightness = context.CreateProperty(_brightnessName);
            _gammaCorrect = context.CreateProperty(_gammaCorrectName);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, _textureName, _texture);
            initialisationData.TryCopyValue(this, _brightnessName, _brightness);
            initialisationData.TryCopyValue(this, _gammaCorrectName, _gammaCorrect);
        }
    }
}