using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

namespace Myre.Graphics.Lighting
{
    public class Skybox
        : Behaviour
    {
        public static readonly TypedName<TextureCube> TextureName = new TypedName<TextureCube>("texture");
        public static readonly TypedName<float> BrightnessName = new TypedName<float>("brightness");
        public static readonly TypedName<bool> GammaCorrectName = new TypedName<bool>("gamma_correct");

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
            _texture = context.CreateProperty(TextureName);
            _brightness = context.CreateProperty(BrightnessName);
            _gammaCorrect = context.CreateProperty(GammaCorrectName);

            base.CreateProperties(context);
        }

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue(this, TextureName, _texture);
            initialisationData.TryCopyValue(this, BrightnessName, _brightness);
            initialisationData.TryCopyValue(this, GammaCorrectName, _gammaCorrect);
        }
    }
}