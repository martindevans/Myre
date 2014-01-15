using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Deferred.LightManagers;
using Myre.Graphics.Materials;

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

        RenderTarget2D _directLightBuffer;
        RenderTarget2D _indirectLightBuffer;

        public LightingComponent(GraphicsDevice device)
        {
            _quad = new Quad(device);
            _quad.SetPosition(depth: 0.99999f);

            _restoreDepth = new Material(Content.Load<Effect>("RestoreDepth"));
            _copyTexture = new Material(Content.Load<Effect>("CopyTexture"));
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define inputs
            context.DefineInput("gbuffer_depth");
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");
            //context.DefineInput("gbuffer_depth_downsample");

            if (context.AvailableResources.Any(r => r.Name == "ssao"))
                context.DefineInput("ssao");

            // define outputs
            context.DefineOutput("lightbuffer", isLeftSet:true, surfaceFormat:SurfaceFormat.HdrBlendable, depthFormat:DepthFormat.Depth24Stencil8);
            context.DefineOutput("directlighting", isLeftSet:false, surfaceFormat: SurfaceFormat.HdrBlendable, depthFormat: DepthFormat.Depth24Stencil8);

            // define default light managers
            var scene = renderer.Scene;
            scene.GetManager<DeferredAmbientLightManager>();
            scene.GetManager<DeferredPointLightManager>();
            scene.GetManager<DeferredSkyboxManager>();
            scene.GetManager<DeferredSpotLightManager>();
            scene.GetManager<DeferredSunLightManager>();

            // get lights
            _directLights = scene.FindManagers<IDirectLight>();
            _indirectLights = scene.FindManagers<IIndirectLight>();

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var resolution = metadata.GetValue(new TypedName<Vector2>("resolution"));
            var width = (int)resolution.X;
            var height = (int)resolution.Y;

            // prepare direct lights
            for (int i = 0; i < _directLights.Count; i++)
                _directLights[i].Prepare(renderer);

            // set and clear direct light buffer
            _directLightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            device.SetRenderTarget(_directLightBuffer);
            device.Clear(Color.Transparent);

            // work around for a bug in xna 4.0
            renderer.Device.SamplerStates[0] = SamplerState.LinearClamp;
            renderer.Device.SamplerStates[0] = SamplerState.PointClamp;

            // set render states to draw opaque geometry
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            // restore depth
            _quad.Draw(_restoreDepth, metadata);
            
            // set render states to additive blend
            device.BlendState = BlendState.Additive;

            // draw direct lights
            for (int i = 0; i < _directLights.Count; i++)
                _directLights[i].Draw(renderer);

            // output direct lighting
            Output("directlighting", _directLightBuffer);

            // prepare indirect lights
            for (int i = 0; i < _indirectLights.Count; i++)
                _indirectLights[i].Prepare(renderer);

            // set and clear indirect light buffer
            _indirectLightBuffer = RenderTargetManager.GetTarget(device, width, height, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            device.SetRenderTarget(_indirectLightBuffer);
            device.Clear(Color.Transparent);

            //draw indirect lights
            for (int i = 0; i < _indirectLights.Count; i++)
                _indirectLights[i].Draw(renderer);

            // blend direct lighting into the indirect light buffer
            _copyTexture.Parameters["Texture"].SetValue(_directLightBuffer);
            _quad.Draw(_copyTexture, metadata);

            // output resulting light buffer
            Output("lightbuffer", _indirectLightBuffer);
        }
    }
}
