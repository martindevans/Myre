using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;

namespace Myre.Graphics.Deferred
{
    public class RestoreDepthPhase
        : RendererComponent
    {
        private readonly Quad _quad;
        private readonly Material _restoreDepth;

        public bool ClearDepth { get; set; }

        public RestoreDepthPhase(GraphicsDevice device)
        {
            _quad = new Quad(device);
            _restoreDepth = new Material(Content.Load<Effect>("RestoreDepth").Clone());
            ClearDepth = true;
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            context.DefineInput("gbuffer_depth");   //used implicitly with GBUFFER_DEPTH semantic

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            // work arround for a bug in xna 4.0
            renderer.Device.SamplerStates[0] = SamplerState.LinearClamp;
            renderer.Device.SamplerStates[0] = SamplerState.PointClamp;

            if (ClearDepth)
                renderer.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

            renderer.Device.DepthStencilState = DepthStencilState.Default;
            renderer.Device.BlendState = BlendState.Additive;
            _quad.Draw(_restoreDepth, renderer.Data);
        }
    }
}
