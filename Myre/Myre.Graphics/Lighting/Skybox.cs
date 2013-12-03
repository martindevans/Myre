using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Entities.Behaviours;
using Myre.Entities.Extensions;

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

        public override void Initialise(Collections.INamedDataProvider initialisationData)
        {
            base.Initialise(initialisationData);

            initialisationData.TryCopyValue("texture" + AppendName(), _texture);
            initialisationData.TryCopyValue("brightness" + AppendName(), _brightness);
            initialisationData.TryCopyValue("gamma_correct" + AppendName(), _gammaCorrect);
        }
    }
}