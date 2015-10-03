using System;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;
using Myre.Graphics.Materials;
using Myre.Graphics.PostProcessing;

using Color = Microsoft.Xna.Framework.Color;

namespace Myre.Graphics.Deferred
{
    public class ToneMapComponent
       : RendererComponent
    {
        readonly Quad _quad;
        readonly Material _calculateLuminance;
        readonly Material _readLuminance;
        readonly Material _adaptLuminance;
        readonly Material _toneMap;
        readonly Effect _bloom;
        readonly Gaussian _gaussian;
        readonly RenderTarget2D[] _adaptedLuminance;
        RenderTarget2D _averageLuminance;
        int _current = 0;
        int _previous = -1;

        public RenderTarget2D AdaptedLuminance
        {
            get { return _adaptedLuminance[_current]; }
        }

        public ToneMapComponent(GraphicsDevice device)
        {
            _quad = new Quad(device);
            var effect = Content.Load<Effect>("CalculateLuminance");
            _calculateLuminance = new Material(effect.Clone(), "ExtractLuminance");
            _adaptLuminance = new Material(effect.Clone(), "AdaptLuminance");
            _readLuminance = new Material(effect.Clone(), "ReadLuminance");
            _toneMap = new Material(Content.Load<Effect>("ToneMap"), null);
            _bloom = Content.Load<Effect>("Bloom");
            _gaussian = new Gaussian(device);

            _adaptedLuminance = new RenderTarget2D[2];
            _adaptedLuminance[0] = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
            _adaptedLuminance[1] = new RenderTarget2D(device, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
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

            var lightBuffer = metadata.GetValue(new TypedName<Texture2D>("lightbuffer"));
            var resolution = metadata.GetValue(new TypedName<Vector2>("resolution"));

            CalculateLuminance(renderer, resolution, device, lightBuffer);
            Bloom(renderer, resolution, device, lightBuffer);
            ToneMap(renderer, resolution, device, lightBuffer);
        }

        private void CalculateLuminance(Renderer renderer, Vector2 resolution, GraphicsDevice device, Texture2D lightBuffer)
        {
            if (_previous == -1)
            {
                _previous = 1;
                device.SetRenderTarget(_adaptedLuminance[_previous]);
                device.Clear(Color.Transparent);
                device.SetRenderTarget(null);
            }

            var tmp = _previous;
            _previous = _current;
            _current = tmp;

            // calculate luminance map
            var luminanceMap = RenderTargetManager.GetTarget(device, 1024, 1024, SurfaceFormat.Single, mipMap: true, name: "luminance map", usage: RenderTargetUsage.DiscardContents);
            device.SetRenderTarget(luminanceMap);
            device.BlendState = BlendState.Opaque;
            device.Clear(Color.Transparent);
            _calculateLuminance.Parameters["Texture"].SetValue(lightBuffer);
            _quad.Draw(_calculateLuminance, renderer.Data);
            Output("luminance", luminanceMap);

            // read bottom mipmap to find average luminance
            _averageLuminance = RenderTargetManager.GetTarget(device, 1, 1, SurfaceFormat.Single, name: "average luminance", usage: RenderTargetUsage.DiscardContents);
            device.SetRenderTarget(_averageLuminance);
            _readLuminance.Parameters["Texture"].SetValue(luminanceMap);
            _quad.Draw(_readLuminance, renderer.Data);

            // adapt towards the current luminance
            device.SetRenderTarget(_adaptedLuminance[_current]);
            _adaptLuminance.Parameters["Texture"].SetValue(_averageLuminance);
            _adaptLuminance.Parameters["PreviousAdaption"].SetValue(_adaptedLuminance[_previous]);
            _quad.Draw(_adaptLuminance, renderer.Data);

            RenderTargetManager.RecycleTarget(_averageLuminance);
        }

        private void Bloom(Renderer renderer, Vector2 resolution, GraphicsDevice device, Texture2D lightBuffer)
        {
            var screenResolution = resolution;
            var halfResolution = screenResolution / 2;
            var quarterResolution = halfResolution / 2;

            // downsample the light buffer to half resolution, and threshold at the same time
            var thresholded = RenderTargetManager.GetTarget(device, (int)halfResolution.X, (int)halfResolution.Y, SurfaceFormat.Rgba64, name: "bloom thresholded", usage: RenderTargetUsage.DiscardContents);
            device.SetRenderTarget(thresholded);
            _bloom.Parameters["Resolution"].SetValue(halfResolution);
            _bloom.Parameters["Threshold"].SetValue(renderer.Data.GetValue(new TypedName<float>("hdr_bloomthreshold")));
            _bloom.Parameters["MinExposure"].SetValue(renderer.Data.GetValue(new TypedName<float>("hdr_minexposure")));
            _bloom.Parameters["MaxExposure"].SetValue(renderer.Data.GetValue(new TypedName<float>("hdr_maxexposure")));
            _bloom.Parameters["Texture"].SetValue(lightBuffer);
            _bloom.Parameters["Luminance"].SetValue(_adaptedLuminance[_current]);
            _bloom.CurrentTechnique = _bloom.Techniques["ThresholdDownsample2X"];
            _quad.Draw(_bloom);

            // downsample again to quarter resolution
            var downsample = RenderTargetManager.GetTarget(device, (int)quarterResolution.X, (int)quarterResolution.Y, SurfaceFormat.Rgba64, name: "bloom downsampled", usage: RenderTargetUsage.DiscardContents);
            device.SetRenderTarget(downsample);
            _bloom.Parameters["Resolution"].SetValue(quarterResolution);
            _bloom.Parameters["Texture"].SetValue(thresholded);
            _bloom.CurrentTechnique = _bloom.Techniques["Scale"];
            _quad.Draw(_bloom);

            // blur the target
            var blurred = RenderTargetManager.GetTarget(device, (int)quarterResolution.X, (int)quarterResolution.Y, SurfaceFormat.Rgba64, name: "bloom blurred", usage: RenderTargetUsage.DiscardContents);
            _gaussian.Blur(downsample, blurred, renderer.Data.GetValue(new TypedName<float>("hdr_bloomblurammount")));

            // upscale back to half resolution
            device.SetRenderTarget(thresholded);
            _bloom.Parameters["Resolution"].SetValue(halfResolution);
            _bloom.Parameters["Texture"].SetValue(blurred);
            _quad.Draw(_bloom);

            // output result
            Output("bloom", thresholded);

            // cleanup temp render targets
            RenderTargetManager.RecycleTarget(downsample);
            RenderTargetManager.RecycleTarget(blurred);
        }

        private void ToneMap(Renderer renderer, Vector2 resolution, GraphicsDevice device, Texture2D lightBuffer)
        {
            var toneMapped = RenderTargetManager.GetTarget(device, (int)resolution.X, (int)resolution.Y, SurfaceFormat.Color, depthFormat: DepthFormat.Depth24Stencil8, name: "tone mapped", usage: RenderTargetUsage.DiscardContents);
            device.SetRenderTarget(toneMapped);
            device.Clear(Color.Transparent);
            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.Opaque;

            _toneMap.Parameters["Texture"].SetValue(lightBuffer);
            _toneMap.Parameters["Luminance"].SetValue(_adaptedLuminance[_current]);
            _toneMap.Parameters["MinExposure"].SetValue(renderer.Data.GetValue(new TypedName<float>("hdr_minexposure")));
            _toneMap.Parameters["MaxExposure"].SetValue(renderer.Data.GetValue(new TypedName<float>("hdr_maxexposure")));
            _quad.Draw(_toneMap, renderer.Data);
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
