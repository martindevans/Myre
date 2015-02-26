using Microsoft.Xna.Framework.Graphics;

namespace Myre.Graphics.Extensions
{
    public static class RenderTarget2DExtensions
    {
        public static RenderTargetInfo RenderTargetInfo(this RenderTarget2D target)
        {
            return new RenderTargetInfo(
                target.Width,
                target.Height,
                target.Format,
                target.DepthStencilFormat,
                target.MultiSampleCount,
                target.LevelCount > 1,
                target.RenderTargetUsage
            );
        }
    }
}
