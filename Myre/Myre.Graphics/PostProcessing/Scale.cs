using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Myre.Extensions;

namespace Myre.Graphics.PostProcessing
{
    public class Resample
    {
        readonly GraphicsDevice _device;
        readonly Effect _effect;
        readonly Quad _quad;

        public Resample(GraphicsDevice device)
        {
            _device = device;
            _effect = Content.Load<Effect>("Downsample");
            _quad = new Quad(device);
        }

        public void Scale(RenderTarget2D source, RenderTarget2D destination)
        {
            _effect.CurrentTechnique = source.Format.IsFloatingPoint() ? _effect.Techniques["Software"] : _effect.Techniques["Hardware"];

            Vector2 resolution = new Vector2(source.Width, source.Height);
            float scaleFactor = (destination.Width > source.Width) ? 2 : 0.5f;

            RenderTarget2D input = source;

            while (IntermediateNeeded(resolution, destination, scaleFactor))
            {
                resolution *= scaleFactor;

                RenderTarget2D output = RenderTargetManager.GetTarget(_device, (int)resolution.X, (int)resolution.Y, source.Format, name:"scaled", usage: RenderTargetUsage.DiscardContents);
                Draw(input, output);

                if (input != source)
                    RenderTargetManager.RecycleTarget(input);
                input = output;
            }

            Draw(input, destination);

            if (input != source)
                RenderTargetManager.RecycleTarget(input);
        }

        private bool IntermediateNeeded(Vector2 currentResolution, RenderTarget2D target, float scale)
        {
// ReSharper disable CompareOfFloatsByEqualityOperator
            return (scale == 2) ? (currentResolution.X * 2 < target.Width && currentResolution.Y * 2 < target.Height)
// ReSharper restore CompareOfFloatsByEqualityOperator
                                : (currentResolution.X / 2 > target.Width && currentResolution.Y / 2 > target.Height);
        }

        private void Draw(RenderTarget2D input, RenderTarget2D output)
        {
            _device.SetRenderTarget(output);

            _effect.Parameters["Resolution"].SetValue(new Vector2(output.Width, output.Height));
            _effect.Parameters["SourceResolution"].SetValue(new Vector2(input.Width, input.Height));
            _effect.Parameters["Texture"].SetValue(input);

            _quad.Draw(_effect);
        }
    }
}
