
namespace Myre.Serialisation
{
    public interface ICustomSerialisable
    {
        Dom.Node Serialise();
        void Deserialise(Dom.Node node);
    }
}
