using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Geometry;
using Myre.Graphics.Materials;
using System.Collections.ObjectModel;

using Vector2 = System.Numerics.Vector2;
using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Translucency
{
    public class DeferredTransparency
        : RendererComponent
    {
        private ReadOnlyCollection<IGeometryProvider> _geometryProviders;

        private readonly List<IGeometry> _geometry = new List<IGeometry>();
        private List<IGeometry>[] _layers;  
        private DepthPeel _depthPeeler;

        private readonly Material _copyTexture;
        private readonly Material _clearGBuffer;
        private readonly Material _restoreDepth;
        private Quad _quad;

        private ReadOnlyCollection<IDirectLight> _directLights;
        private ReadOnlyCollection<IIndirectLight> _indirectLights;

        public DeferredTransparency()
        {
            _copyTexture = new Material(Content.Load<Effect>("CopyTexture"));
            _clearGBuffer = new Material(Content.Load<Effect>("ClearGBuffer"));
            _restoreDepth = new Material(Content.Load<Effect>("RestoreDepth"));
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _quad = new Quad(renderer.Device);

            //Create geometry management objects
            _geometryProviders = renderer.Scene.FindManagers<IGeometryProvider>();
            _depthPeeler = new DepthPeel();

            //create layers to peel geometry into
            _layers = new List<IGeometry>[] {
                new List<IGeometry>(),
                //new List<IGeometry>(),
                //new List<IGeometry>(),
            };

            //Make sure deferred lighting is enabled
            LightingComponent.SetupScene(renderer.Scene, out _directLights, out _indirectLights);

            // define inputs
            context.DefineInput("gbuffer_depth");
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");
            context.DefineInput("lightbuffer");

            //define outputs
            context.DefineOutput("gbuffer_depth");
            context.DefineOutput("lightbuffer", isLeftSet: true, surfaceFormat: SurfaceFormat.HdrBlendable, depthFormat: DepthFormat.Depth24Stencil8);
            context.DefineOutput("demo", surfaceFormat: SurfaceFormat.HdrBlendable, depthFormat: DepthFormat.Depth24Stencil8);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.GetValue(new TypedName<Vector2>("resolution"));
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            //Find geometry to draw in this phase
            _geometry.Clear();
            foreach (var geometryProvider in _geometryProviders)
                geometryProvider.Query("translucent", renderer.Data, _geometry);

            //Peel geometry into separate layers
            foreach (var layer in _layers)
                layer.Clear();
            _depthPeeler.Peel(_geometry, _layers);

            //Get the lightbuffer (result of opaque deferred rendering)
            var lightbuffer = GetResource("lightbuffer");

            //Create a new GBuffer to render transparencies into
            var depth = GetResource("gbuffer_depth");
            var normals = GetResource("gbuffer_normals");
            var diffuse = GetResource("gbuffer_diffuse");

            //Render each peeled depth layer
            for (int i = 0; i < _layers.Length; i++)
            {
                //Clear the transparent gbuffer
                ClearGBuffer(renderer, depth, normals, diffuse);

                //Render the layer gbuffer
                GeometryRenderer.Draw(_layers[i], false, "translucent", renderer);

                //Do a lighting pass for this gbuffer
                //This leaves the lightbuffer set as the target
                var tempLightbuffer = PerformLightingPass(renderer, depth, normals, diffuse);

                //Render the layer (alpha blended particles into the lightbuffer)
                GeometryRenderer.Draw(_layers[i], false, "translucent_alpha", renderer);

                //Blend tempLightbuffer into lightbuffer
                renderer.Device.SetRenderTarget(lightbuffer);
                //blend!

                //Recycle the temp lightbuffer
                //RenderTargetManager.RecycleTarget(tempLightbuffer);
                Output("demo", tempLightbuffer);
            }

            //Now output the modified lightbuffer
            Output("lightbuffer", lightbuffer);
        }

        private void ClearGBuffer(Renderer renderer, RenderTarget2D depth, RenderTarget2D normals, RenderTarget2D diffuse)
        {
            //Clear GBuffer to black (this clears associated stencil and depth buffers)
            renderer.Device.SetRenderTargets(depth, normals, diffuse);
            renderer.Device.BlendState = BlendState.Opaque;
            renderer.Device.Clear(Color.Black);

            //Perform GBuffer specific clearing (clears different buffers to different initial values)
            renderer.Device.DepthStencilState = DepthStencilState.None;
            _quad.Draw(_clearGBuffer, renderer.Data);

            renderer.Device.DepthStencilState = DepthStencilState.Default;
            renderer.Device.BlendState = BlendState.Opaque;
        }

        private RenderTarget2D PerformLightingPass(Renderer renderer, RenderTarget2D depth, RenderTarget2D normals, RenderTarget2D diffuse)
        {
            RenderTarget2D directLightBuffer;
            RenderTarget2D indirectLightBuffer;
            LightingComponent.PerformLightingPass(renderer, false, _quad, _restoreDepth, _copyTexture, _directLights, _indirectLights, out directLightBuffer, out indirectLightBuffer);

            RenderTargetManager.RecycleTarget(directLightBuffer);
            return indirectLightBuffer;
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
