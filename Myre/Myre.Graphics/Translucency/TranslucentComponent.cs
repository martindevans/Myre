using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Geometry;
using Myre.Graphics.Materials;
using Vector2 = System.Numerics.Vector2;

namespace Myre.Graphics.Translucency
{
    public class TranslucentComponent
        : RendererComponent
    {
        private ReadOnlyCollection<IGeometryProvider> _geometryProviders;

        private readonly Material _copyTexture;
        private Quad _quad;

        private readonly DepthStencilState _writeDepth = new DepthStencilState {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = true,
            DepthBufferFunction = CompareFunction.Always,
        };

        public TranslucentComponent()
        {
            _copyTexture = new Material(Content.Load<Effect>("CopyTexture"));
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _quad = new Quad(renderer.Device);

            _geometryProviders = renderer.Scene.FindManagers<IGeometryProvider>();

            // define inputs
            //context.DefineInput("gbuffer_depth");
            //context.DefineInput("lightbuffer");

            //define outputs
            //context.DefineOutput("lightbuffer", isLeftSet: true, surfaceFormat: SurfaceFormat.HdrBlendable, depthFormat: DepthFormat.Depth24Stencil8);

            // define inputs
            if (context.AvailableResources.Any(r => r.Name == "gbuffer_depth"))
                context.DefineInput("gbuffer_depth");

            // define outputs
            foreach (var resource in context.SetRenderTargets)
                context.DefineOutput(resource);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            //var metadata = renderer.Data;
            //var device = renderer.Device;

            //var resolution = metadata.GetValue(new TypedName<Vector2>("resolution"));
            //var width = (int)resolution.X;
            //var height = (int)resolution.Y;

            ////Get the lightbuffer (result of deferred rendering)
            //var lightbuffer = GetResource("lightbuffer");

            ////Create a new lightbuffer to render transparencies into
            //var target = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.Color, DepthFormat.None, name: "transparencies_lightbuffer");
            //device.SetRenderTarget(target);
            //device.BlendState = BlendState.Opaque;
            //device.Clear(Color.Transparent);

            var g = new GeometryRenderer(_geometryProviders);
            g.Draw("translucent", renderer);

            //throw new NotImplementedException("Render geometry");
            //foreach (var geometryProvider in renderer.Scene.FindManagers<IGeometryProvider>())
            //    geometryProvider.Draw("gbuffer", metadata);

            //write modified pixels back into lightbuffer
           // OverwriteModifiedPixels(renderer, target, lightbuffer);

            //Now output the modified lightbuffer
            //Output("lightbuffer", lightbuffer);
        }

        private void OverwriteModifiedPixels(Renderer renderer, RenderTarget2D source, RenderTarget2D target)
        {
            //This could be made more efficient by using the stencil buffer:
            // - When rendering the source, write into the stencil buffer
            // - When performing the copy, read the stencil buffer to reject unchanged pixels
            //Unfortunately XNA4 makes stencil buffers impossible to use in this way :unimpressed:

            renderer.Device.SetRenderTarget(target);
            renderer.Device.BlendState = BlendState.AlphaBlend;
            renderer.Device.DepthStencilState = DepthStencilState.None;

            _copyTexture.Parameters["Texture"].SetValue(source);
            _quad.Draw(_copyTexture, renderer.Data);
        }
    }
}
