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
        private Material lightingMaterial;
        private Quad quad;

        public DeferredAmbientLightManager(GraphicsDevice device)
        {
            lightingMaterial = new Material(Content.Load<Effect>("AmbientLight"));
            quad = new Quad(device);
        }

        public void Prepare(Renderer renderer)
        {
        }

        public void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var view = metadata.Get<Matrix>("view").Value;
            var ssao = metadata.Get<Texture2D>("ssao").Value;

            if (ssao != null)
                lightingMaterial.CurrentTechnique = lightingMaterial.Techniques["AmbientSSAO"];
            else
                lightingMaterial.CurrentTechnique = lightingMaterial.Techniques["Ambient"];

            foreach (var light in Behaviours)
            {
                lightingMaterial.Parameters["Up"].SetValue(Vector3.TransformNormal(light.Up, view));
                lightingMaterial.Parameters["SkyColour"].SetValue(light.SkyColour);
                lightingMaterial.Parameters["GroundColour"].SetValue(light.GroundColour);

                quad.Draw(lightingMaterial, metadata);
            }
        }
    }
}
