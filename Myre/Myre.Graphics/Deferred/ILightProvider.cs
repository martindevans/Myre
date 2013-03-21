
namespace Myre.Graphics.Deferred
{
    public interface ILightProvider
    {
        void Prepare(Renderer renderer);
        void Draw(Renderer renderer);
    }

    public interface IDirectLight
        : ILightProvider
    {
    }

    public interface IIndirectLight
        : ILightProvider
    {
    }
}