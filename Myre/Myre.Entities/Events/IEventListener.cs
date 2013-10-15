
namespace Myre.Entities.Events
{
    /// <summary>
    /// An interface which defines method for listening to events of the specified type.
    /// </summary>
    /// <typeparam name="EventData">The type of the event data.</typeparam>
    public interface IEventListener<EventData>
    {
        EventData HandleEvent(EventData data, object scope);
    }
}
