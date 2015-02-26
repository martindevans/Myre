using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Extensions;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Deferred.Decals
{
    /// <summary>
    /// Mix decal buffers into the gbuffers
    /// </summary>
    public class DecalMixComponent
        : RendererComponent
    {
        private Material _mixDecalMaterial;
        private Quad _quad;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _mixDecalMaterial = new Material(Content.Load<Effect>("DecalBufferMix").Clone(), "MixDecalBuffers");
            _quad = new Quad(renderer.Device);

            // define inputs
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");
            context.DefineInput("decal_normals");
            context.DefineInput("decal_diffuse");

            // define outputs
            context.DefineOutput("gbuffer_normals");
            context.DefineOutput("gbuffer_diffuse");

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var inputGbufferNormals = renderer.Plan.GetResource("gbuffer_normals");
            var inputGbufferDiffuse = renderer.Plan.GetResource("gbuffer_diffuse");

            var inputDecalNormals = renderer.Plan.GetResource("decal_normals");
            var inputDecalDiffuse = renderer.Plan.GetResource("decal_diffuse");

            var outputGbufferNormals = RenderTargetManager.GetTarget(renderer.Device, inputGbufferNormals.RenderTargetInfo());
            var outputGbufferDiffuse = RenderTargetManager.GetTarget(renderer.Device, inputGbufferDiffuse.RenderTargetInfo());

            var device = renderer.Device;
            device.SetRenderTargets(outputGbufferNormals, outputGbufferDiffuse);

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.None;
            device.Clear(Color.Black);

            _mixDecalMaterial.Parameters["GbufferNormals"].SetValue(inputGbufferNormals);
            _mixDecalMaterial.Parameters["GbufferDiffuse"].SetValue(inputGbufferDiffuse);
            _mixDecalMaterial.Parameters["DecalNormals"].SetValue(inputDecalNormals);
            _mixDecalMaterial.Parameters["DecalDiffuse"].SetValue(inputDecalDiffuse);

            _quad.Draw(_mixDecalMaterial, renderer.Data);

            //output new gbuffer
            Output("gbuffer_normals", outputGbufferNormals);
            Output("gbuffer_diffuse", outputGbufferDiffuse);

            //Recycle old gbuffer
            RenderTargetManager.RecycleTarget(inputGbufferNormals);
            RenderTargetManager.RecycleTarget(inputGbufferDiffuse);
        }
    }
}
