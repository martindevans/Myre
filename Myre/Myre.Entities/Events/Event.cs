using System.Collections.Generic;
using Myre.Collections;

namespace Myre.Entities.Events
{
    /// <summary>
    /// A class which represents an event for a specified data type.
    /// Instances of this type can be used to send events to listeners which have registered with this event.
    /// </summary>
    /// <typeparam name="Data">The type of payload data this event sends.</typeparam>
    public class Event<Data>
        : IEvent
    {
        class Invocation
            : IEventInvocation
        {
            public Data Data;
            public Event<Data> Event;

            public void Execute()
            {
                //Loop over event listeners backwards so most recent handlers are executed first
                //This makes events compatible with using them as a chained system where more recent handlers can temporarily block lower handlers (by modifying the event data)
                for (int i = Event._listeners.Count - 1; i >= 0; i--)
                    Data = Event._listeners[i].HandleEvent(Data, Event._scope);
            }


            public void Recycle()
            {
                Data = default(Data);
                Pool<Invocation>.Instance.Return(this);
            }
        }

        private readonly EventService _service;
        private readonly object _scope;
        private readonly Event<Data> _global;
        private readonly List<IEventListener<Data>> _listeners;

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <value>The service.</value>
        public EventService Service { get { return _service; } }

        internal Event(EventService service, object scope = null, Event<Data> globalScoped = null)
        {
            _service = service;
            _scope = scope;
            _global = globalScoped;
            _listeners = new List<IEventListener<Data>>();
        }

        /// <summary>
        /// Adds a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void AddListener(IEventListener<Data> listener)
        {
            _listeners.Add(listener);
        }

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        /// <returns></returns>
        public bool RemoveListener(IEventListener<Data> listener)
        {
            return _listeners.Remove(listener);
        }

        /// <summary>
        /// Sends the specified data to all registered listeners.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(Data data)
        {
            Send(data, this);

            if (_global != null)
                Send(data, _global);
        }

        private void Send(Data data, Event<Data> channel)
        {
            Invocation invocation = Pool<Invocation>.Instance.Get();
            invocation.Event = channel;
            invocation.Data = data;

            _service.Queue(invocation);
        }
    }
}
