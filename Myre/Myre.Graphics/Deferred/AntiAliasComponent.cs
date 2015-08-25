using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;
using Myre.Graphics.Materials;
using Ninject;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Deferred
{
    public class AntiAliasComponent
        : RendererComponent
    {
        private readonly Material _fxaa;
        private readonly Quad _quad;
        private string _inputResource;
        private readonly GraphicsDevice _device;

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
            _device = device;

            _fxaa = new Material(Content.Load<Effect>("FXAA"), "FXAA");
            _quad = new Quad(device);
            _inputResource = inputResource;
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            if (_inputResource == null)
                _inputResource = context.SetRenderTargets[0].Name;
            
            context.DefineInput(_inputResource);

            // define outputs
            context.DefineOutput("antialiased", isLeftSet: true, surfaceFormat: SurfaceFormat.Color);

            // define settings
            var settings = renderer.Settings;

            //   1.00 - upper limit (softer)
            //   0.75 - default amount of filtering
            //   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
            //   0.25 - almost off
            //   0.00 - completely off
            settings.Add("fxaa_subpixelaliasingremoval", "the amount of sub-pixel aliasing removal. This can effect sharpness.", 0.75f);

            //   0.333 - too little (faster)
            //   0.250 - low quality
            //   0.166 - default
            //   0.125 - high quality 
            //   0.063 - overkill (slower)
            settings.Add("fxaa_edgethreshold", "The minimum amount of local contrast required to apply algorithm.", 0.166f);

            //   0.0833 - upper limit (default, the start of visible unfiltered edges)
            //   0.0625 - high quality (faster)
            //   0.0312 - visible limit (slower)
            // Special notes when using FXAA_GREEN_AS_LUMA,
            //   Likely want to set this to zero.
            //   As colors that are mostly not-green
            //   will appear very dark in the green channel!
            //   Tune by looking at mostly non-green content,
            //   then start at zero and increase until aliasing is a problem.
            settings.Add("fxaa_edgethresholdmin", "Trims the algorithm from processing darks.", 0.0f);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.GetValue(new TypedName<Vector2>("resolution"));
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            var target = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color, DepthFormat.None, name: "antialiased");

            device.SetRenderTarget(target);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Black);

            Viewport viewport = _device.Viewport;

            _fxaa.Parameters["InverseViewportSize"].SetValue(new Vector2(1f / viewport.Width, 1f / viewport.Height));
            _fxaa.Parameters["Texture"].SetValue(GetResource(_inputResource));
            _quad.Draw(_fxaa, metadata);

            Output("antialiased", target);
        }
    }
}