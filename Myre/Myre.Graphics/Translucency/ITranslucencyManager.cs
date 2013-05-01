using Myre.Entities.Behaviours;

namespace Myre.Graphics.Translucency
{
    /// <summary>
    /// Marks a entity manager as managing entities which need to be drawn as translucent objects
    /// </summary>
    public interface ITranslucencyManager
        : IBehaviourManager
    {
        void Draw(Renderer renderer);
    }
}
