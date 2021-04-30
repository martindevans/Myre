
namespace Myre.Entities.Events
{
    /// <summary>
    /// An interface which defines method for listening to events of the specified type.
    /// </summary>
    /// <typeparam name="TEventData">The type of the event data.</typeparam>
    public interface IEventListener<TEventData>
    {
        TEventData HandleEvent(TEventData? data, object? scope);
    }
}
