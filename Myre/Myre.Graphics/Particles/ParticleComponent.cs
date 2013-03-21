using System.Collections.ObjectModel;
using System.Linq;

namespace Myre.Graphics.Particles
{
    public class ParticleComponent
        : RendererComponent
    {
        private ReadOnlyCollection<ParticleEmitter.Manager> _managers;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            _managers = renderer.Scene.FindManagers<ParticleEmitter.Manager>();

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
