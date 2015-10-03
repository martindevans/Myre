using System.Numerics;
using Microsoft.Xna.Framework.Graphics;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Deferred.Decals
{
    public class DecalComponent
        : RendererComponent
    {
        private Decal.Manager _decalManager;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _decalManager = renderer.Scene.GetManager<Decal.Manager>();

            // define inputs
            context.DefineInput("gbuffer_depth");       //Used implicitly by the shader (using the GBUFFER_DEPTH semantic)

            // define outputs
            context.DefineOutput("decal_normals");
            context.DefineOutput("decal_diffuse");

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var device = renderer.Device;

            var resolution = renderer.Data.Get<Vector2>("resolution").Value;

            //Create targets to write our changes into
            var decalNormals = RenderTargetManager.GetTarget(device, (int)resolution.X, (int)resolution.Y, SurfaceFormat.Rgba1010102, usage: RenderTargetUsage.DiscardContents);
            var decalDiffuse = RenderTargetManager.GetTarget(device, (int)resolution.X, (int)resolution.Y, usage: RenderTargetUsage.DiscardContents);

            //Set targets and clear to transparent
            device.SetRenderTargets(decalNormals, decalDiffuse);
            device.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            //Setup render states
            device.BlendState = BlendState.NonPremultiplied;
            device.DepthStencilState = DepthStencilState.None;

            //Draw decals into buffers
            _decalManager.Draw(renderer);

            //Output results
            Output("decal_normals", decalNormals);
            Output("decal_diffuse", decalDiffuse);
        }
    }
}
