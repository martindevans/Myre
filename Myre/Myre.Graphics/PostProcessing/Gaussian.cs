using System;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;

namespace Myre.Graphics.PostProcessing
{
    public class Gaussian
    {
        readonly Effect _effect;
        readonly Quad _quad;
        readonly GraphicsDevice _device;
        readonly float[] _weights;
        readonly float[] _offsets;

        int _height;
        int _width;
        float _sigma;

        public Gaussian(GraphicsDevice device)
        {
            _effect = Content.Load<Effect>("Gaussian");
            _quad = new Quad(device);
            _device = device;

            var sampleCount = _effect.Parameters["Weights"].Elements.Count;
            _weights = new float[sampleCount];
            _offsets = new float[sampleCount];
        }

        public void Blur(RenderTarget2D source, RenderTarget2D destination, float sigma)
        {
// ReSharper disable CompareOfFloatsByEqualityOperator
            if (_width != source.Width || _height != source.Height || _sigma != sigma)
// ReSharper restore CompareOfFloatsByEqualityOperator
                CalculateWeights(source.Width, source.Height, sigma);

            _effect.Parameters["Resolution"].SetValue(new Vector2(_width, _height));

            var intermediate = RenderTargetManager.GetTarget(_device, _width, _height, destination.Format, name: "gaussian intermediate", usage: RenderTargetUsage.DiscardContents);
            _device.SetRenderTarget(intermediate);

            _effect.Parameters["Texture"].SetValue(source);
            _effect.CurrentTechnique = _effect.Techniques["BlurHorizontal"];
            _quad.Draw(_effect);

            _device.SetRenderTarget(destination);

            _effect.Parameters["Texture"].SetValue(intermediate);
            _effect.CurrentTechnique = _effect.Techniques["BlurVertical"];
            _quad.Draw(_effect);

            RenderTargetManager.RecycleTarget(intermediate);
        }

        // from the bloom sample on creators.xna.com
        private void CalculateWeights(int width, int height, float sigma)
        {
            _width = width;
            _height = height;
            _sigma = sigma;

            // The first sample always has a zero offset.
            _weights[0] = ComputeGaussian(0);
            _offsets[0] = 0;

            // Maintain a sum of all the weighting values.
            float totalWeights = _weights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < _weights.Length / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                _weights[i * 2 + 1] = weight;
                _weights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float offset = i * 2 + 1.5f;

                // Store texture coordinate offsets for the positive and negative taps.
                _offsets[i * 2 + 1] = offset;
                _offsets[i * 2 + 2] = -offset;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            _effect.Parameters["Weights"].SetValue(_weights);
            _effect.Parameters["Offsets"].SetValue(_offsets);
        }

        private float ComputeGaussian(float n)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * _sigma)) *
                           Math.Exp(-(n * n) / (2 * _sigma * _sigma)));
        }
    }
}
