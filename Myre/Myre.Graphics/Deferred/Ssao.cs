using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myre.Graphics.Materials;

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
            _ssaoMaterial.Parameters["Samples"].SetValue(GenerateRandomSamplePositions(16, rand));
            _quad = new Quad(device);
        }

        private Texture2D GenerateRandomNormals(GraphicsDevice device, int width, int height, Random rand)
        {
            Color[] colours = new Color[width * height];
            for (int i = 0; i < colours.Length; i++)
            {
                var vector = new Vector2(
                    (float)rand.NextDouble(),
                    (float)rand.NextDouble());

                Vector2.Normalize(ref vector, out vector);

                colours[i] = new Color(vector.X, vector.Y, 0);
            }

            var texture = new Texture2D(device, width, height);
            texture.SetData(colours);

            return texture;
        }

        private Vector2[] GenerateRandomSamplePositions(int numSamples, Random rand)
        {
            Vector2[] samples = new Vector2[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                var angle = rand.NextDouble() * MathHelper.TwoPi;
                var vector = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));

                var length = i / (float)numSamples;
                length = MathHelper.Lerp(0.1f, 1.0f, length * length);
                samples[i] = vector * length;
            }

            return samples;
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define settings
            var settings = renderer.Settings;
            //settings.Add("ssao_enabled", "Determines if Screen Space Ambient Occlusion is enabled.", true);
            //settings.Add("ssao_halfres", "Determines if SSAO will run at full of half screen resolution.", true);
            settings.Add("ssao_radius", "SSAO sample radius", 1f);
            settings.Add("ssao_intensity", "SSAO intensity", 2.5f);
            settings.Add("ssao_scale", "Scales distance between occluders and occludee.", 1f);
            //settings.Add("ssao_detailradius", "SSAO sample radius", 2.3f);
            //settings.Add("ssao_detailintensity", "SSAO intensity", 15f);
            //settings.Add("ssao_detailscale", "Scales distance between occluders and occludee.", 1.5f);
            settings.Add("ssao_blur", "The amount to blur SSAO.", 1f);
            //settings.Add("ssao_radiosityintensity", "The intensity of local radiosity colour transfer.", 0.0f);
            settings.Add("ssao_highquality", "Switches between high and low quality SSAO sampling pattern.", false);

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
            var resolution = renderer.Data.Get<Vector2>("resolution").Value;

            //if (renderer.Data.Get<float>("ssao_radiosityintensity").Value > 0)
            //    ssaoMaterial.CurrentTechnique = ssaoMaterial.Techniques["SSGI"];
            //else
            //{
                _ssaoMaterial.CurrentTechnique = renderer.Data.Get<bool>("ssao_highquality").Value ? _ssaoMaterial.Techniques["HQ_SSAO"] : _ssaoMaterial.Techniques["LQ_SSAO"];
            //}

            var unblured = RenderTargetManager.GetTarget(renderer.Device, (int)resolution.X, (int)resolution.Y, surfaceFormat: SurfaceFormat.HalfVector4, name: "ssao unblurred");//, SurfaceFormat.HalfVector4);
            renderer.Device.SetRenderTarget(unblured);
            renderer.Device.Clear(Color.Transparent);
            renderer.Device.BlendState = BlendState.Opaque;
            _quad.Draw(_ssaoMaterial, renderer.Data);

            _ssao = RenderTargetManager.GetTarget(renderer.Device, (int)resolution.X, (int)resolution.Y, SurfaceFormat.HalfVector4, name: "ssao");
            renderer.Device.SetRenderTarget(_ssao);
            renderer.Device.Clear(Color.Transparent);
            _ssaoBlurMaterial.Parameters["SSAO"].SetValue(unblured);
            _quad.Draw(_ssaoBlurMaterial, renderer.Data);
            RenderTargetManager.RecycleTarget(unblured);

            Output("ssao", _ssao);
        }
    }
}
