using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities.Behaviours;
using Myre.Graphics.Lighting;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Deferred.LightManagers
{
    public class DeferredAmbientLightManager
            : BehaviourManager<AmbientLight>, IIndirectLight
    {
        private readonly Material _lightingMaterial;
        private readonly Quad _quad;

        public DeferredAmbientLightManager(GraphicsDevice device)
        {
            _lightingMaterial = new Material(Content.Load<Effect>("AmbientLight"));
            _quad = new Quad(device);
        }

        public void Prepare(Renderer renderer)
        {
        }

        public void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var view = metadata.GetValue(new TypedName<Matrix>("view"));
            var ssao = metadata.GetValue(new TypedName<Texture2D>("ssao"));

            _lightingMaterial.CurrentTechnique = ssao != null ? _lightingMaterial.Techniques["AmbientSSAO"] : _lightingMaterial.Techniques["Ambient"];

            foreach (var light in Behaviours)
            {
                _lightingMaterial.Parameters["Up"].SetValue(Vector3.TransformNormal(light.Up, view));
                _lightingMaterial.Parameters["SkyColour"].SetValue(light.SkyColour);
                _lightingMaterial.Parameters["GroundColour"].SetValue(light.GroundColour);

                _quad.Draw(_lightingMaterial, metadata);
            }
        }
    }
}
