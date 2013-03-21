using System.Collections.ObjectModel;

namespace Myre.Graphics
{
    /// <summary>
    /// A renderer component which draws IRenderable objects.
    /// </summary>
    public class RenderableComponent
        : RendererComponent
    {
        private ReadOnlyCollection<IRenderable> _renderables;

        public override void Initialise(Renderer renderer, ResourceContext context)
        {
            foreach (var resource in context.SetRenderTargets)
            {
                context.DefineInput(resource.Name);
                context.DefineOutput(resource);
            }

            _renderables = renderer.Scene.FindManagers<IRenderable>();
            base.Initialise(renderer, context);
        }

        public override void Draw(Renderer renderer)
        {
            foreach (var item in _renderables)
                item.Draw(renderer);
        }
    }
}
