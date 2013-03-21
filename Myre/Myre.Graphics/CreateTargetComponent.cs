using Microsoft.Xna.Framework;

namespace Myre.Graphics
{
    public class CreateTargetComponent
        : RendererComponent
    {
        private static int _counter;

        private readonly string _name;
        private readonly RenderTargetInfo _targetInfo;

        public CreateTargetComponent(RenderTargetInfo targetInfo, string resourceName = null)
        {
            _targetInfo = targetInfo;

            _counter = (_counter + 1) % (int.MaxValue - 1);
            _name = resourceName ?? string.Format("anonymous-{0}-{1}", _counter, targetInfo.GetHashCode());
        }

        public override void Initialise(Renderer renderer, ResourceContext context)
        {            
            // define outputs
            context.DefineOutput(_name, true, null, _targetInfo);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            var info = _targetInfo;
            if (info.Width == 0 || info.Height == 0)
            {
                var resolution = renderer.Data.Get<Vector2>("resolution").Value;
                info = new RenderTargetInfo(
                    (int) resolution.X,
                    (int) resolution.Y,
                    info.SurfaceFormat,
                    info.DepthFormat,
                    info.MultiSampleCount,
                    info.MipMap,
                    info.Usage
                );
            }

            var target = RenderTargetManager.GetTarget(renderer.Device, info);
            renderer.Device.SetRenderTarget(target);
            renderer.Device.Clear(Color.Black);

            Output(_name, target);
        }
    }
}
