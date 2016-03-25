using System;
using System.Linq;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Deferred
{
    public class Ssao
        : RendererComponent
    {
        private readonly Material _ssaoMaterial;
        private readonly Material _ssaoBlurMaterial;
        private RenderTarget2D _ssao;
        private readonly Quad _quad;

        public Ssao(GraphicsDevice device)
        {
            _ssaoMaterial = new Material(Content.Load<Effect>("SSAO"));
            _ssaoBlurMaterial = new Material(Content.Load<Effect>("BlurSSAO"));
            Random rand = new Random();
            _ssaoMaterial.Parameters["Random"].SetValue(GenerateRandomNormals(device, 4, 4, rand));//content.Load<Texture2D>("randomnormals"));
            _ssaoMaterial.Parameters["RandomResolution"].SetValue(4);
            _quad = new Quad(device);
        }

        private static Texture2D GenerateRandomNormals(GraphicsDevice device, int width, int height, Random rand)
        {
            var colours = new Color[width * height];
            for (var i = 0; i < colours.Length; i++)
            {
                var vector = new Vector2(
                    (float)rand.NextDouble(),
                    (float)rand.NextDouble());

                vector = Vector2.Normalize(vector);

                colours[i] = new Color(vector.X, vector.Y, 0);
            }

            var texture = new Texture2D(device, width, height);
            texture.SetData(colours);

            return texture;
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define settings
            var settings = renderer.Settings;
            settings.Add("ssao_radius", "SSAO sample radius", 1f);
            settings.Add("ssao_intensity", "SSAO intensity", 2.5f);
            settings.Add("ssao_scale", "Scales distance between occluders and occludee.", 1f);
            settings.Add("ssao_blur", "The amount to blur SSAO.", 1f);

            // define inputs
            context.DefineInput("gbuffer_depth_downsample");
            context.DefineInput("gbuffer_normals");
            context.DefineInput("gbuffer_diffuse");

            if (context.AvailableResources.Any(r => r.Name == "edges"))
                context.DefineInput("edges");

            // define outputs
            context.DefineOutput("ssao");

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var resolution = renderer.Data.GetValue(new TypedName<Vector2>("resolution"));

            _ssaoMaterial.CurrentTechnique = _ssaoMaterial.Techniques["SSAO"];

            var unblured = RenderTargetManager.GetTarget(renderer.Device, (int)resolution.X, (int)resolution.Y, surfaceFormat: SurfaceFormat.HalfVector4, name: "ssao unblurred", usage: RenderTargetUsage.DiscardContents);
            renderer.Device.SetRenderTarget(unblured);
            renderer.Device.Clear(Color.Transparent);
            renderer.Device.BlendState = BlendState.Opaque;
            _quad.Draw(_ssaoMaterial, renderer.Data);

            _ssao = RenderTargetManager.GetTarget(renderer.Device, (int)resolution.X, (int)resolution.Y, SurfaceFormat.HalfVector4, name: "ssao", usage: RenderTargetUsage.DiscardContents);
            renderer.Device.SetRenderTarget(_ssao);
            renderer.Device.Clear(Color.Transparent);
            _ssaoBlurMaterial.Parameters["SSAO"].SetValue(unblured);
            _quad.Draw(_ssaoBlurMaterial, renderer.Data);
            RenderTargetManager.RecycleTarget(unblured);

            Output("ssao", _ssao);
        }
    }
}
