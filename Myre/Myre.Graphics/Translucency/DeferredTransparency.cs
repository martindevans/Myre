using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Deferred;
using Myre.Graphics.Geometry;
using Myre.Graphics.Materials;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Translucency
{
    public class DeferredTransparency
        : RendererComponent
    {
        private ReadOnlyCollection<IGeometryProvider> _geometryProviders;

        private readonly List<IGeometry> _geometry = new List<IGeometry>();
        private readonly List<List<IGeometry>> _layers = new List<List<IGeometry>>();
        private DepthPeel _depthPeeler;

        private readonly Material _copyTexture;
        private readonly Material _copyTextureWithStencil;
        private readonly Material _clearGBuffer;
        private readonly Material _restoreDepth;
        private Quad _quad;

        private ReadOnlyCollection<IDirectLight> _directLights;
        private ReadOnlyCollection<IIndirectLight> _indirectLights;

        private readonly DepthStencilState _depthReadGreaterThan = new DepthStencilState {
            DepthBufferEnable = true,
            DepthBufferFunction = CompareFunction.GreaterEqual,
            DepthBufferWriteEnable = false
        };

        public DeferredTransparency()
        {
            _copyTexture = new Material(Content.Load<Effect>("CopyTexture").Clone(), "Copy");
            _copyTextureWithStencil = new Material(Content.Load<Effect>("CopyTexture").Clone(), "CopyWithStencil");
            _clearGBuffer = new Material(Content.Load<Effect>("ClearGBuffer").Clone(), "Clear_NormalsDiffuse");
            _restoreDepth = new Material(Content.Load<Effect>("RestoreDepth"));
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _quad = new Quad(renderer.Device);

            //Create geometry management objects
            _geometryProviders = renderer.Scene.FindManagers<IGeometryProvider>();
            _depthPeeler = new DepthPeel();

            var settings = renderer.Settings;

            // 1 - Min
            // 5 - Default
            // 10 - Extreme
            settings.Add("transparency_deferred_layers", "the max number of depth peeled layers to use for deferred transparency", 5);

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

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            //Create layers
            var layersCount = renderer.Data.Get<int>("transparency_deferred_layers", 5, true).Value;
            while (_layers.Count > layersCount)
                _layers.RemoveAt(_layers.Count - 1);
            while (_layers.Count < layersCount)
                _layers.Add(new List<IGeometry>());
            foreach (var layer in _layers)
                layer.Clear();

            //Find geometry to draw in this phase
            _geometry.Clear();
            foreach (var geometryProvider in _geometryProviders)
                geometryProvider.Query("translucent", renderer.Data, _geometry);

            //Peel geometry into separate layers
            _depthPeeler.Peel(_geometry, _layers, renderer.Data.Get<View>("activeview").Value);

            //Get the lightbuffer (result of opaque deferred rendering)
            var lightbuffer = GetResource("lightbuffer");

            //Create a new GBuffer to render transparencies into
            var depth = GetResource("gbuffer_depth");
            var normals = GetResource("gbuffer_normals");
            var diffuse = GetResource("gbuffer_diffuse");

            renderer.Data.Set<bool>("render_translucent", true);

            //Render each peeled depth layer
            for (int i = 0; i < _layers.Count; i++)
            {
                if (_layers[i].Count == 0)
                    continue;

                //Clear the transparent gbuffer
                ClearGBuffer(renderer, normals, diffuse);

                //Render the layer gbuffer
                renderer.Device.SetRenderTargets(depth, normals, diffuse);
                renderer.Device.DepthStencilState = DepthStencilState.Default;
                renderer.Device.BlendState = BlendState.Opaque;
                GeometryRenderer.Draw(_layers[i], DepthSort.FrontToBack, "gbuffer", renderer);

                //Do a lighting pass for this gbuffer
                //This leaves the lightbuffer set as the target
                var tempLightbuffer = PerformLightingPass(renderer, depth, normals, diffuse);

                //Blend tempLightbuffer into lightbuffer
                BlendTransparencies(renderer, _layers[i], normals, tempLightbuffer, lightbuffer);

                //Render the layer (alpha blended particles into the lightbuffer)
                GeometryRenderer.Draw(_layers[i], DepthSort.BackToFront, "translucent_alpha", renderer);

                //Recycle the temp lightbuffer
                RenderTargetManager.RecycleTarget(tempLightbuffer);
            }

            renderer.Data.Set<bool>("render_translucent", false);

            //Now output the modified lightbuffer
            Output("lightbuffer", lightbuffer);
        }

        /// <summary>
        /// Blend the lightbuffer resulting from transparency into the primary scene lightbuffer
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="geometry"></param>
        /// <param name="gbufferNormals">The normal gbuffer component for the transparencies (used as a stencil buffer)</param>
        /// <param name="transparencyLightbuffer"></param>
        /// <param name="lightbuffer"></param>
        private void BlendTransparencies(Renderer renderer, List<IGeometry> geometry, Texture gbufferNormals, RenderTarget2D transparencyLightbuffer, RenderTarget2D lightbuffer)
        {
            //Write the new pixels into the temp light buffers
            var tempLightBuffer = RenderTargetManager.GetTarget(renderer.Device, lightbuffer.Width, lightbuffer.Height, lightbuffer.Format, lightbuffer.DepthStencilFormat);

            //Restore depth from gbuffer_depth into the lightbuffer depth buffer
            renderer.Device.SetRenderTargets(tempLightBuffer);
            RestoreDepthPhase.RestoreDepth(renderer, _quad, _restoreDepth, false);

            //Put the transparency lightbuffer into the metadata so pixel shader semantics can access it
            renderer.Data.Set<Texture2D>("transparency_lightbuffer", transparencyLightbuffer);

            //Draw the transparent geometry again, but this time draw the back side and set depth read to greaterequal
            //The material applied here has the distance to the front (read from gbuffer_depth) and the distance to the back (calculate from geometry being rendered)
            //This means we can apply transmittance fx based on material thickness
            renderer.Device.DepthStencilState = _depthReadGreaterThan;
            renderer.Device.BlendState = BlendState.Opaque;
            renderer.Device.RasterizerState = RasterizerState.CullClockwise;
            GeometryRenderer.Draw(geometry, DepthSort.None, "translucent", renderer);
            renderer.Device.RasterizerState = RasterizerState.CullCounterClockwise;

            //Remove transparency lightbuffer from the metadata
            renderer.Data.Set<Texture2D>("transparency_lightbuffer", null);

            //Blend the temp lightbuffer into the real lightbuffer
            renderer.Device.SetRenderTarget(lightbuffer);
            renderer.Device.BlendState = BlendState.Opaque;
            renderer.Device.DepthStencilState = DepthStencilState.None;
            _copyTextureWithStencil.Parameters["Texture"].SetValue(tempLightBuffer);
            _copyTextureWithStencil.Parameters["Stencil"].SetValue(gbufferNormals);
            _quad.Draw(_copyTextureWithStencil, renderer.Data);

            //Discard the temp light buffer
            RenderTargetManager.RecycleTarget(tempLightBuffer);
        }

        private void ClearGBuffer(Renderer renderer, RenderTarget2D normals, RenderTarget2D diffuse)
        {
            //Clear GBuffer to black but *keep* the depth buffer set
            renderer.Device.SetRenderTargets(normals, diffuse);
            renderer.Device.BlendState = BlendState.Opaque;
            renderer.Device.Clear(Color.Black);

            //Perform GBuffer specific clearing (clears different buffers to different initial values)
            renderer.Device.DepthStencilState = DepthStencilState.None;
            _quad.Draw(_clearGBuffer, renderer.Data);
        }

        private RenderTarget2D PerformLightingPass(Renderer renderer, RenderTarget2D depth, RenderTarget2D normals, RenderTarget2D diffuse)
        {
            RenderTarget2D directLightBuffer;
            RenderTarget2D indirectLightBuffer;
            LightingComponent.PerformLightingPass(renderer, false, _quad, _restoreDepth, _copyTexture, _directLights, _indirectLights, out directLightBuffer, out indirectLightBuffer);

            RenderTargetManager.RecycleTarget(directLightBuffer);
            return indirectLightBuffer;
        }
    }
}
