using System.Collections.ObjectModel;
using System.Linq;

namespace Myre.Graphics.Translucency
{
    public class TranslucentComponent
        : RendererComponent
    {
        private ReadOnlyCollection<ITranslucencyManager> _managers;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _managers = renderer.Scene.FindManagers<ITranslucencyManager>();

            // define inputs
            if (context.AvailableResources.Any(r => r.Name == "gbuffer_depth"))
                context.DefineInput("gbuffer_depth");

            // define outputs
            foreach (var resource in context.SetRenderTargets)
                context.DefineOutput(resource);

            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            foreach (var item in _managers)
                item.Draw(renderer);
        }
    }
}
