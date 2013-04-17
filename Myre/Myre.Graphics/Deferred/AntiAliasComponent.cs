using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;
using Ninject;

namespace Myre.Graphics.Deferred
{
    public class AntiAliasComponent
        : RendererComponent
    {
        private readonly Material _edgeBlur;
        private readonly Quad _quad;
        private string _inputResource;

        [Inject]
// This method is needed for dependency injection
// ReSharper disable RedundantOverload.Global
        public AntiAliasComponent(GraphicsDevice device)
// ReSharper restore RedundantOverload.Global
            : this(device, null)
        {
        }

        public AntiAliasComponent(GraphicsDevice device, string inputResource = null)
        {
            _edgeBlur = new Material(Content.Load<Effect>("EdgeBlur"));
            _quad = new Quad(device);
            _inputResource = inputResource;
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            if (_inputResource == null)
                _inputResource = context.SetRenderTargets[0].Name;
            
            context.DefineInput(_inputResource);
            context.DefineInput("edges");

            // define outputs
            context.DefineOutput("antialiased", isLeftSet: true, surfaceFormat: SurfaceFormat.Color);
            
            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.Get<Vector2>("resolution").Value;
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var target = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color, DepthFormat.None, name: "antialiased");

            device.SetRenderTarget(target);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Black);

            _edgeBlur.Parameters["Texture"].SetValue(GetResource(_inputResource));
            _edgeBlur.Parameters["TexelSize"].SetValue(new Vector2(1f / width, 1f / height));
            _quad.Draw(_edgeBlur, metadata);

            Output("antialiased", target);
        }
    }
}