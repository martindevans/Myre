using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Entities;
using Myre.Graphics.Deferred.LightManagers;
using Myre.Graphics.Materials;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Deferred
{
    public class LightingComponent
       : RendererComponent
    {
        readonly Quad _quad;
        readonly Material _restoreDepth;
        readonly Material _copyTexture;
        ReadOnlyCollection<IDirectLight> _directLights;
        ReadOnlyCollection<IIndirectLight> _indirectLights;

        public LightingComponent(GraphicsDevice device)
        {
            _quad = new Quad(device);
            _quad.SetPosition(depth: 0.99999f);

            _restoreDepth = new Material(Content.Load<Effect>("RestoreDepth"));
            _copyTexture = new Material(Content.Load<Effect>("CopyTexture"));
        }

        internal static void SetupScene(Scene scene, out ReadOnlyCollection<IDirectLight> directLights, out ReadOnlyCollection<IIndirectLight> indirectLights)
        {
            // define deferred light managers
            // - This is slightly odd, why do we need to define these here when we could just put [DefaultManager(DeferredAmbientLightManager)] on AmbientLight
            // - This is so that AmientLights (a generic light class) have no connection to *deferred rendering*.
            scene.GetManager<DeferredAmbientLightManager>();
            scene.GetManager<DeferredPointLightManager>();
            scene.GetManager<DeferredSkyboxManager>();
            scene.GetManager<DeferredSpotLightManager>();
            scene.GetManager<DeferredSunLightManager>();

            // get lights
            directLights = scene.FindManagers<IDirectLight>();
            indirectLights = scene.FindManagers<IIndirectLight>();
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            context.DefineInput("gbuffer_depth");
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");

            if (context.AvailableResources.Any(r => r.Name == "ssao"))
                context.DefineInput("ssao");

            // define outputs
            context.DefineOutput("lightbuffer", isLeftSet:true, surfaceFormat:SurfaceFormat.HdrBlendable, depthFormat:DepthFormat.Depth24Stencil8);
            context.DefineOutput("directlighting", isLeftSet:false, surfaceFormat: SurfaceFormat.HdrBlendable, depthFormat: DepthFormat.Depth24Stencil8);

            // Setup light managers and find lists of all lights
            SetupScene(renderer.Scene, out _directLights, out _indirectLights);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            RenderTarget2D directLightBuffer;
            RenderTarget2D indirectLightBuffer;
            PerformLightingPass(renderer, true, _quad, _restoreDepth, _copyTexture, _directLights, _indirectLights, out directLightBuffer, out indirectLightBuffer);

            Output("directlighting", directLightBuffer);
            Output("lightbuffer", indirectLightBuffer);
        }

        internal static void PerformLightingPass(Renderer renderer, bool ssao, Quad quad, Material restoreDepth, Material copyTexture, ReadOnlyCollection<IDirectLight> directLights, ReadOnlyCollection<IIndirectLight> indirectLights, out RenderTarget2D directLightBuffer, out RenderTarget2D indirectLightBuffer)
        {
            //Get some handy objects
            var device = renderer.Device;
            var resolution = renderer.Data.GetValue(new TypedName<Vector2>("resolution"));
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            //Enable or disable SSAO
            renderer.Data.Set("ssao", ssao);

            // prepare direct lights
            for (int i = 0; i < directLights.Count; i++)
                directLights[i].Prepare(renderer);

            // set and clear direct light buffer
            directLightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            device.SetRenderTarget(directLightBuffer);
            device.Clear(Color.Transparent);

            // work around for a bug in xna 4.0
            renderer.Device.SamplerStates[0] = SamplerState.LinearClamp;
            renderer.Device.SamplerStates[0] = SamplerState.PointClamp;

            // set render states to draw opaque geometry
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            // restore depth
            quad.Draw(restoreDepth, renderer.Data);

            // set render states to additive blend
            device.BlendState = BlendState.Additive;

            // draw direct lights
            foreach (IDirectLight light in directLights)
                light.Draw(renderer);

            // prepare indirect lights
            for (int i = 0; i < indirectLights.Count; i++)
                indirectLights[i].Prepare(renderer);

            // set and clear indirect light buffer
            indirectLightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            device.SetRenderTarget(indirectLightBuffer);
            device.Clear(Color.Transparent);

            //draw indirect lights
            foreach (IIndirectLight light in indirectLights)
                light.Draw(renderer);

            // blend direct lighting into the indirect light buffer
            copyTexture.Parameters["Texture"].SetValue(directLightBuffer);
            quad.Draw(copyTexture, renderer.Data);
        }
    }
}
