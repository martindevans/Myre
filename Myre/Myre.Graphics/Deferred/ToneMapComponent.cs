using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Graphics.Materials;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Myre.Debugging.Statistics;
using Myre.Collections;
using Myre.Graphics.PostProcessing;
using System.Threading;

namespace Myre.Graphics.Deferred
{
    public class ToneMapComponent
       : RendererComponent
    {
        Quad quad;
        Material calculateLuminance;
        Material readLuminance;
        Material adaptLuminance;
        Material copyLuminance;
        Material toneMap;
        Effect bloom;
        Gaussian gaussian;
        Resample scale;
        RenderTarget2D[] adaptedLuminance;
        RenderTarget2D averageLuminance;
        float[] textureData = new float[1];
        int current = 0;
        int previous = -1;

        public RenderTarget2D AdaptedLuminance
        {
            get { return adaptedLuminance[current]; }
        }

        public ToneMapComponent(GraphicsDevice device)
        {
            quad = new Quad(device);
            var effect = Content.Load<Effect>("CalculateLuminance");
            calculateLuminance = new Material(effect.Clone(), "ExtractLuminance");
            adaptLuminance = new Material(effect.Clone(), "AdaptLuminance");
            readLuminance = new Material(effect.Clone(), "ReadLuminance");
            copyLuminance = new Material(effect.Clone(), "Copy");
            toneMap = new Material(Content.Load<Effect>("ToneMap"), null);
            bloom = Content.Load<Effect>("Bloom");
            gaussian = new Gaussian(device);
            scale = new Resample(device);

            adaptedLuminance = new RenderTarget2D[2];
            adaptedLuminance[0] = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
            adaptedLuminance[1] = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            // define settings
            var settings = renderer.Settings;
            settings.Add("hdr_adaptionrate", "The rate at which the cameras' exposure adapts to changes in the scene luminance.", 1f);
            settings.Add("hdr_bloomthreshold", "The under-exposure applied during bloom thresholding.", 6f);
            settings.Add("hdr_bloommagnitude", "The overall brightness of the bloom effect.", 3f);
            settings.Add("hdr_bloomblurammount", "The amount to blur the bloom target.", 2.2f);
            settings.Add("hdr_minexposure", "The minimum exposure the camera can adapt to.", -1.1f);
            settings.Add("hdr_maxexposure", "The maximum exposure the camera can adapt to.", 1.1f);

            // define inputs
            context.DefineInput("lightbuffer");

            // define outputs
            //context.DefineOutput("luminancemap", isLeftSet: false, width: 1024, height: 1024, surfaceFormat: SurfaceFormat.Single);
            context.DefineOutput("luminance", isLeftSet: false, width: 1, height: 1, surfaceFormat: SurfaceFormat.Single);
            context.DefineOutput("bloom", isLeftSet: false, surfaceFormat: SurfaceFormat.Rgba64);
            context.DefineOutput("tonemapped", isLeftSet: true, surfaceFormat: SurfaceFormat.Color, depthFormat: DepthFormat.Depth24Stencil8);
            
            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var metadata = renderer.Data;
            var device = renderer.Device;

            var lightBuffer = metadata.Get<Texture2D>("lightbuffer").Value;
            var resolution = metadata.Get<Vector2>("resolution");

            CalculateLuminance(renderer, resolution, device, lightBuffer);
            Bloom(renderer, resolution, device, lightBuffer);
            ToneMap(renderer, resolution, device, lightBuffer);
        }

        private void CalculateLuminance(Renderer renderer, Box<Vector2> resolution, GraphicsDevice device, Texture2D lightBuffer)
        {
            if (previous == -1)
            {
                previous = 1;
                device.SetRenderTarget(adaptedLuminance[previous]);
                device.Clear(Color.Transparent);
                device.SetRenderTarget(null);
            }

            var tmp = previous;
            previous = current;
            current = tmp;

            // calculate luminance map
            var luminanceMap = RenderTargetManager.GetTarget(device, 1024, 1024, SurfaceFormat.Single, mipMap: true, name:"luminance map");
            device.SetRenderTarget(luminanceMap);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Transparent);
            calculateLuminance.Parameters["Texture"].SetValue(lightBuffer);
            quad.Draw(calculateLuminance, renderer.Data);
            Output("luminance", luminanceMap);

            // read bottom mipmap to find average luminance
            averageLuminance = RenderTargetManager.GetTarget(device, 1, 1, SurfaceFormat.Single, name: "average luminance");
            device.SetRenderTarget(averageLuminance);
            readLuminance.Parameters["Texture"].SetValue(luminanceMap);
            quad.Draw(readLuminance, renderer.Data);

            // adapt towards the current luminance
            device.SetRenderTarget(adaptedLuminance[current]);
            adaptLuminance.Parameters["Texture"].SetValue(averageLuminance);
            adaptLuminance.Parameters["PreviousAdaption"].SetValue(adaptedLuminance[previous]);
            quad.Draw(adaptLuminance, renderer.Data);

            RenderTargetManager.RecycleTarget(averageLuminance);
        }

        private void Bloom(Renderer renderer, Box<Vector2> resolution, GraphicsDevice device, Texture2D lightBuffer)
        {
            var screenResolution = resolution.Value;
            var halfResolution = screenResolution / 2;
            var quarterResolution = halfResolution / 2;

            // downsample the light buffer to half resolution, and threshold at the same time
            var thresholded = RenderTargetManager.GetTarget(device, (int)halfResolution.X, (int)halfResolution.Y, SurfaceFormat.Rgba64, name:"bloom thresholded");
            device.SetRenderTarget(thresholded);
            bloom.Parameters["Resolution"].SetValue(halfResolution);
            bloom.Parameters["Threshold"].SetValue(renderer.Data.Get<float>("hdr_bloomthreshold").Value);
            bloom.Parameters["MinExposure"].SetValue(renderer.Data.Get<float>("hdr_minexposure").Value);
            bloom.Parameters["MaxExposure"].SetValue(renderer.Data.Get<float>("hdr_maxexposure").Value);
            bloom.Parameters["Texture"].SetValue(lightBuffer);
            bloom.Parameters["Luminance"].SetValue(adaptedLuminance[current]);
            bloom.CurrentTechnique = bloom.Techniques["ThresholdDownsample2X"];
            quad.Draw(bloom);

            // downsample again to quarter resolution
            var downsample = RenderTargetManager.GetTarget(device, (int)quarterResolution.X, (int)quarterResolution.Y, SurfaceFormat.Rgba64, name: "bloom downsampled");
            device.SetRenderTarget(downsample);
            bloom.Parameters["Resolution"].SetValue(quarterResolution);
            bloom.Parameters["Texture"].SetValue(thresholded);
            bloom.CurrentTechnique = bloom.Techniques["Scale"];
            quad.Draw(bloom);

            // blur the target
            var blurred = RenderTargetManager.GetTarget(device, (int)quarterResolution.X, (int)quarterResolution.Y, SurfaceFormat.Rgba64, name: "bloom blurred");
            gaussian.Blur(downsample, blurred, renderer.Data.Get<float>("hdr_bloomblurammount").Value);

            // upscale back to half resolution
            device.SetRenderTarget(thresholded);
            bloom.Parameters["Resolution"].SetValue(halfResolution);
            bloom.Parameters["Texture"].SetValue(blurred);
            quad.Draw(bloom);

            // output result
            Output("bloom", thresholded);

            // cleanup temp render targets
            RenderTargetManager.RecycleTarget(downsample);
            RenderTargetManager.RecycleTarget(blurred);
        }

        private void ToneMap(Renderer renderer, Box<Vector2> resolution, GraphicsDevice device, Texture2D lightBuffer)
        {
            var toneMapped = RenderTargetManager.GetTarget(device, (int)resolution.Value.X, (int)resolution.Value.Y, SurfaceFormat.Color, depthFormat: DepthFormat.Depth24Stencil8, name:"tone mapped");
            device.SetRenderTarget(toneMapped);
            device.Clear(Color.Transparent);
            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.Opaque;

            toneMap.Parameters["Texture"].SetValue(lightBuffer);
            toneMap.Parameters["Luminance"].SetValue(adaptedLuminance[current]);
            toneMap.Parameters["MinExposure"].SetValue(renderer.Data.Get<float>("hdr_minexposure").Value);
            toneMap.Parameters["MaxExposure"].SetValue(renderer.Data.Get<float>("hdr_maxexposure").Value);
            quad.Draw(toneMap, renderer.Data);
            Output("tonemapped", toneMapped);
        }

        #region Tone Mapping Math Functions
        public static Vector3 ToneMap(Vector3 colour, float adaptedLuminance)
        {
            return ToneMapFilmic(CalcExposedColor(colour, adaptedLuminance));
        }

        // From http://mynameismjp.wordpress.com/2010/04/30/a-closer-look-at-tone-mapping
        // Applies the filmic curve from John Hable's presentation
        private static Vector3 ToneMapFilmic(Vector3 color)
        {
            color = Vector3.Max(Vector3.Zero, color - new Vector3(0.004f));
            color = (color * (6.2f * color + new Vector3(0.5f))) / (color * (6.2f * color + new Vector3(1.7f)) + new Vector3(0.06f));

            return color;
        }

        public static Vector3 InverseToneMap(Vector3 colour, float adaptedLuminance)
        {
            return InverseExposedColour(InverseToneMapFilmic(colour), adaptedLuminance);
        }

        private static Vector3 InverseToneMapFilmic(Vector3 colour)
        {
            return new Vector3(
                InverseToneMapFilmic(colour.X),
                InverseToneMapFilmic(colour.Y),
                InverseToneMapFilmic(colour.Z));
        }

        private static float InverseToneMapFilmic(float x)
        {
            var numerator = Math.Sqrt(5)
                * Math.Sqrt(701 * x * x - 106 * x + 125) - 856 * x + 25;
            var denumerator = 620 * (x - 1);

            return (float)Math.Abs(numerator / denumerator) + 0.004f;
        }

        // Determines the color based on exposure settings
        public static Vector3 CalcExposedColor(Vector3 color, float avgLuminance)
        {
            // Use geometric mean        
            avgLuminance = Math.Max(avgLuminance, 0.001f);

            float keyValue = 1.03f - (2.0f / (2 + (float)Math.Log10(avgLuminance + 1)));

            float linearExposure = (keyValue / avgLuminance);
            float exposure = Math.Max(linearExposure, 0.0001f);

            return exposure * color;
        }

        public static Vector3 InverseExposedColour(Vector3 colour, float avgLuminance)
        {
            avgLuminance = Math.Max(avgLuminance, 0.001f);

            float keyValue = 1.03f - (2.0f / (2 + (float)Math.Log10(avgLuminance + 1)));

            float linearExposure = (keyValue / avgLuminance);
            float exposure = Math.Max(linearExposure, 0.0001f);

            return colour / exposure;
        }
        #endregion
    }
}
