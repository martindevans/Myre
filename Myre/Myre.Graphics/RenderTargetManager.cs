using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Myre.Graphics
{
    public struct RenderTargetInfo
    {
        public readonly int Height;
        public readonly int Width;
        public readonly SurfaceFormat SurfaceFormat;
        public readonly DepthFormat DepthFormat;
        public readonly int MultiSampleCount;
        public readonly bool MipMap;
        public readonly RenderTargetUsage Usage;

        public RenderTargetInfo(int width, int height, SurfaceFormat format, DepthFormat depthFormat, int multiSampleCount, bool mipMap, RenderTargetUsage usage)
        {
            Width = width;
            Height = height;
            SurfaceFormat = format;
            DepthFormat = depthFormat;
            MultiSampleCount = multiSampleCount;
            MipMap = mipMap;
            Usage = usage;
        }

        public bool Equals(RenderTargetInfo info)
        {
            return Height == info.Height
                && Width == info.Width
                && SurfaceFormat == info.SurfaceFormat
                && DepthFormat == info.DepthFormat
                && MultiSampleCount == info.MultiSampleCount
                && MipMap == info.MipMap
                && Usage == info.Usage;
        }

        public override bool Equals(object obj)
        {
            if (obj is RenderTargetInfo)
                return Equals((RenderTargetInfo)obj);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode()
                ^ Height
                ^ Width
                ^ SurfaceFormat.GetHashCode();
        }

        public static RenderTargetInfo FromRenderTarget(RenderTarget2D target)
        {
            return new RenderTargetInfo(target.Width, target.Height, target.Format, target.DepthStencilFormat, target.MultiSampleCount, target.LevelCount > 1, target.RenderTargetUsage);
        }
    }

    public static class RenderTargetManager
    {
#if PROFILE
        private static readonly Statistic _numRenderTargets = Statistic.Create("Graphics.RTs");
        private static readonly Statistic _renderTargetMemory = Statistic.Create("Graphics.RT_Memory", "{0:0.00}MB");
#endif

        private static readonly Dictionary<RenderTargetInfo, Stack<RenderTarget2D>> _pool = new Dictionary<RenderTargetInfo, Stack<RenderTarget2D>>();
        private static readonly Dictionary<RenderTargetInfo, RenderTargetInfo> _infoMappings = new Dictionary<RenderTargetInfo, RenderTargetInfo>();


#if DEBUG
        private static readonly List<string> _active = new List<string>();
#endif

        public static RenderTarget2D GetTarget(GraphicsDevice device, int width, int height, SurfaceFormat surfaceFormat = SurfaceFormat.Color, DepthFormat depthFormat = DepthFormat.None, int multiSampleCount = 0, bool mipMap = false, RenderTargetUsage usage = RenderTargetUsage.DiscardContents, string name = null)
        {
            var info = new RenderTargetInfo(width, height, surfaceFormat, depthFormat, multiSampleCount, mipMap, usage);

            return GetTarget(device, info, name);
        }

        public static RenderTarget2D GetTarget(GraphicsDevice device, RenderTargetInfo info, string name = null)
        {
            RenderTargetInfo mapped;
            bool wasMapped = _infoMappings.TryGetValue(info, out mapped);
            if (!wasMapped)
                mapped = info;

            var stack = GetPool(mapped);
            if (stack.Count > 0)
            {
                var t = stack.Pop();
                t.Tag = name;
#if DEBUG
                _active.Add(t.Tag as string);
#endif
                return t;
            }

            var target = new RenderTarget2D(device, mapped.Width, mapped.Height, mapped.MipMap, mapped.SurfaceFormat, mapped.DepthFormat, mapped.MultiSampleCount, mapped.Usage) { Tag = name };

            if (!wasMapped)
            {
                var targetInfo = RenderTargetInfo.FromRenderTarget(target);
                _infoMappings[info] = targetInfo;
            }

#if PROFILE
            _numRenderTargets.Add(1);

            var resolution = target.Width * target.Height;
            float size = resolution * target.Format.FormatSize();
            if (target.MultiSampleCount > 0)
                size *= target.MultiSampleCount;
            if (info.MipMap)
                size *= 1.33f;
            size += resolution * target.DepthStencilFormat.FormatSize();
            _renderTargetMemory.Add(size / (1024 * 1024));
#endif

#if DEBUG
            _active.Add(target.Tag as string);
#endif

            return target;
        }

        public static void RecycleTarget(RenderTarget2D target)
        {
            var info = RenderTargetInfo.FromRenderTarget(target);

#if DEBUG
            if (GetPool(info).Contains(target))
                throw new InvalidOperationException("Render target has already been freed.");

            _active.Remove(target.Tag as string);
#endif

            GetPool(info).Push(target);

#if DEBUG
            //Set the tag to the stacktrace of where this target was recycled
            //This helps diagnose double free bugs
            //target.Tag = new StackTrace();
#else
            //target.Tag = null;
#endif
        }

        private static Stack<RenderTarget2D> GetPool(RenderTargetInfo info)
        {
            Stack<RenderTarget2D> stack;
            if (!_pool.TryGetValue(info, out stack))
            {
                stack = new Stack<RenderTarget2D>();
                _pool.Add(info, stack);
            }

            return stack;
        }
    }
}
