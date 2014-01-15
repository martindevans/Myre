using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Deferred
{
    public class EdgeDetectComponent
        : RendererComponent
    {
        private readonly Material _edgeDetect;
        private readonly Quad _quad;

        public EdgeDetectComponent(GraphicsDevice device)
        {
            _edgeDetect = new Material(Content.Load<Effect>("EdgeDetect"));
            _quad = new Quad(device);
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            context.DefineInput("gbuffer_depth");
            context.DefineInput("gbuffer_normals");

            // define outputs
            context.DefineOutput("edges", isLeftSet: true, surfaceFormat: SurfaceFormat.Color);

            // define settings
            var settings = renderer.Settings;
            settings.Add("edge_normalthreshold", "Threshold used to decide between an edge and a non-edge by normal.", 0.5f);
            settings.Add("edge_depththreshold", "Threshold used to decide between an edge and a non-edge by depth.", 0.01f);
            settings.Add("edge_normalweight", "Weighting used for edges detected via normals in the output.", 0.15f);
            settings.Add("edge_depthweight", "Weighting used for edges detected via depth in the output.", 0.2f);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.GetValue(new TypedName<Vector2>("resolution"));
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var target = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color, DepthFormat.None, name: "edges");

            device.SetRenderTarget(target);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Black);

            _edgeDetect.Parameters["TexelSize"].SetValue(new Vector2(1f / width, 1f / height));
            _quad.Draw(_edgeDetect, metadata);

            Output("edges", target);
        }
    }
}